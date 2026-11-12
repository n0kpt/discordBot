using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.App;
using DiscordBot.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscordBot.Core.Tests;

public class DiscordBotSettingsTests
{
    private const string SentinelToken = "test-secret-token-do-not-print";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromConfiguration_RejectsMissingOrBlankToken(string? token)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => CreateSettings(token, "123"));

        Assert.Contains("Discord__Token", exception.Message);
        Assert.DoesNotContain(SentinelToken, exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("18446744073709551616")]
    public void FromConfiguration_RejectsInvalidGuildIdWithoutRevealingToken(string? guildId)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => CreateSettings(SentinelToken, guildId));

        Assert.Contains("Discord__GuildId", exception.Message);
        Assert.DoesNotContain(SentinelToken, exception.Message);
    }

    [Fact]
    public void FromConfiguration_AcceptsValidSettings()
    {
        var settings = CreateSettings(SentinelToken, "123456789012345678");

        Assert.Equal(SentinelToken, settings.Token);
        Assert.Equal(123456789012345678UL, settings.GuildId);
    }

    private static DiscordBotSettings CreateSettings(string? token, string? guildId)
    {
        var values = new Dictionary<string, string?>
        {
            ["Discord:Token"] = token,
            ["Discord:GuildId"] = guildId
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        return DiscordBotSettings.FromConfiguration(configuration);
    }
}

public class HelloCommandTests
{
    [Fact]
    public async Task HandleAsync_SendsExactlyOneExactGreeting()
    {
        var responder = new RecordingResponder();
        var handler = new HelloCommandHandler(new GreetingService());

        await handler.HandleAsync(responder);

        Assert.Equal(new[] { "Hello, world!" }, responder.Messages);
    }

    [Fact]
    public async Task ModuleDiscovery_ExposesExpectedHelloCommandMetadata()
    {
        var client = new DiscordSocketClient();
        var interactionService = new InteractionService(client);
        using var services = new ServiceCollection()
            .AddSingleton<GreetingService>()
            .AddSingleton<HelloCommandHandler>()
            .BuildServiceProvider();

        await interactionService.AddModuleAsync<HelloModule>(services);

        var command = Assert.Single(interactionService.Modules.SelectMany(module => module.SlashCommands));
        Assert.Equal("hello", command.Name);
        Assert.Equal("Get a greeting.", command.Description);
        client.Dispose();
    }

    [Fact]
    public void GuildCommandGuard_OnlyAllowsConfiguredGuild()
    {
        var guard = new GuildCommandGuard(123);

        Assert.True(guard.Allows(123));
        Assert.False(guard.Allows(456));
        Assert.False(guard.Allows(null));
    }

    [Fact]
    public void ServiceRegistration_ResolvesHostedBotAndGuildRegistrar()
    {
        using var services = new ServiceCollection()
            .AddLogging()
            .AddDiscordBot(new DiscordBotSettings("test-token", 123))
            .BuildServiceProvider(validateScopes: true);

        Assert.Single(services.GetServices<IHostedService>());
        Assert.NotNull(services.GetRequiredService<GuildReadyRegistrar>());
    }

    [Fact]
    public async Task ReadyRegistrar_RegistersOnceAndPreservesUnrelatedCommands()
    {
        var registry = new RecordingRegistry();
        var registrar = new GuildReadyRegistrar(registry, 123);

        await registrar.OnReadyAsync();
        await registrar.OnReadyAsync();
        await Task.WhenAll(registrar.OnReadyAsync(), registrar.OnReadyAsync());

        var call = Assert.Single(registry.Calls);
        Assert.Equal(123UL, call.GuildId);
        Assert.False(call.DeleteMissing);
    }

    [Fact]
    public async Task ReadyRegistrar_RetriesAfterRegistrationFailure()
    {
        var registry = new RecordingRegistry { FailNext = true };
        var registrar = new GuildReadyRegistrar(registry, 123);

        await Assert.ThrowsAsync<InvalidOperationException>(() => registrar.OnReadyAsync());
        await registrar.OnReadyAsync();

        Assert.Equal(2, registry.Calls.Count);
    }

    [Fact]
    public async Task ReadyRegistrar_SerializesConcurrentFirstReadyEvents()
    {
        var registry = new BlockingRegistry();
        var registrar = new GuildReadyRegistrar(registry, 123);
        var readyCalls = Enumerable.Range(0, 4).Select(_ => registrar.OnReadyAsync()).ToArray();

        await registry.RegistrationStarted.Task;
        Assert.Single(registry.Calls);
        registry.ReleaseRegistration.TrySetResult();
        await Task.WhenAll(readyCalls);

        Assert.Single(registry.Calls);
    }

    [Fact]
    public async Task SlashCommandResultReporter_LogsExceptionTypeWithoutExceptionMessage()
    {
        const string sentinel = "secret-token-sentinel-do-not-log";
        var logger = new RecordingLogger<SlashCommandResultReporter>();
        var reporter = new SlashCommandResultReporter(logger);

        await reporter.HandleAsync(ExecuteResult.FromError(new InvalidOperationException(sentinel)));

        var message = Assert.Single(logger.Messages);
        Assert.Contains(nameof(InvalidOperationException), message);
        Assert.DoesNotContain(sentinel, message);
    }

    private sealed class RecordingResponder : IGreetingResponder
    {
        public List<string> Messages { get; } = [];

        public Task RespondAsync(string message)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingRegistry : IGuildCommandRegistry
    {
        public List<(ulong GuildId, bool DeleteMissing)> Calls { get; } = [];
        public bool FailNext { get; set; }

        public Task RegisterAsync(ulong guildId, bool deleteMissing)
        {
            Calls.Add((guildId, deleteMissing));
            if (FailNext)
            {
                FailNext = false;
                throw new InvalidOperationException("Transient registration failure.");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class BlockingRegistry : IGuildCommandRegistry
    {
        public List<(ulong GuildId, bool DeleteMissing)> Calls { get; } = [];
        public TaskCompletionSource RegistrationStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseRegistration { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task RegisterAsync(ulong guildId, bool deleteMissing)
        {
            Calls.Add((guildId, deleteMissing));
            RegistrationStarted.TrySetResult();
            await ReleaseRegistration.Task;
        }
    }

    private sealed class RecordingLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            Microsoft.Extensions.Logging.EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
