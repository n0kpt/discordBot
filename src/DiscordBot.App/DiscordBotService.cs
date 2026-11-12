using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordBot.App;

public sealed class DiscordBotService(
    DiscordSocketClient client,
    InteractionService interactionService,
    DiscordBotSettings settings,
    IServiceProvider services,
    GuildReadyRegistrar registrar,
    SlashCommandResultReporter resultReporter,
    ILogger<DiscordBotService> logger) : BackgroundService
{
    private readonly GuildCommandGuard _guildGuard = new(settings.GuildId);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await interactionService.AddModuleAsync<HelloModule>(services).ConfigureAwait(false);
        client.Ready += HandleReadyAsync;
        client.InteractionCreated += HandleInteractionAsync;
        client.Log += HandleDiscordLogAsync;
        client.Connected += HandleConnectedAsync;
        client.Disconnected += HandleDisconnectedAsync;
        interactionService.SlashCommandExecuted += HandleSlashCommandExecutedAsync;

        try
        {
            await client.LoginAsync(TokenType.Bot, settings.Token).ConfigureAwait(false);
            await client.StartAsync().ConfigureAwait(false);
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal host shutdown.
        }
        catch (Exception exception)
        {
            logger.LogError("Discord connection failed ({ErrorType}).", exception.GetType().Name);
            throw new InvalidOperationException($"Discord connection failed ({exception.GetType().Name}).");
        }
        finally
        {
            client.InteractionCreated -= HandleInteractionAsync;
            client.Ready -= HandleReadyAsync;
            client.Log -= HandleDiscordLogAsync;
            client.Connected -= HandleConnectedAsync;
            client.Disconnected -= HandleDisconnectedAsync;
            interactionService.SlashCommandExecuted -= HandleSlashCommandExecutedAsync;
            if (client.ConnectionState != ConnectionState.Disconnected)
            {
                await client.StopAsync().ConfigureAwait(false);
                await client.LogoutAsync().ConfigureAwait(false);
            }
        }
    }

    private Task HandleSlashCommandExecutedAsync(SlashCommandInfo command, IInteractionContext context, IResult result) =>
        resultReporter.HandleAsync(result);

    private async Task HandleReadyAsync()
    {
        try
        {
            await registrar.OnReadyAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError("Guild command registration failed ({ErrorType}); it will be retried after the next ready event.", exception.GetType().Name);
        }
    }

    private Task HandleDiscordLogAsync(LogMessage message)
    {
        if (message.Exception is not null)
        {
            logger.Log(LogLevel.Warning, "Discord.Net {Source} reported an error ({ErrorType}).", message.Source, message.Exception.GetType().Name);
        }
        else
        {
            logger.Log(MapLogLevel(message.Severity), "Discord.Net {Source}.", message.Source);
        }

        return Task.CompletedTask;
    }

    private Task HandleConnectedAsync()
    {
        logger.LogInformation("Discord connection established.");
        return Task.CompletedTask;
    }

    private Task HandleDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
        {
            logger.LogWarning("Discord connection closed.");
        }
        else
        {
            logger.LogWarning("Discord connection closed ({ErrorType}); the client will reconnect if possible.", exception.GetType().Name);
        }

        return Task.CompletedTask;
    }

    private static LogLevel MapLogLevel(LogSeverity severity) => severity switch
    {
        LogSeverity.Critical => LogLevel.Critical,
        LogSeverity.Error => LogLevel.Error,
        LogSeverity.Warning => LogLevel.Warning,
        LogSeverity.Info => LogLevel.Information,
        LogSeverity.Verbose => LogLevel.Debug,
        _ => LogLevel.Trace
    };

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        if (!_guildGuard.Allows(interaction.GuildId))
        {
            if (!interaction.HasResponded)
            {
                await interaction.RespondAsync(
                    "This command is not available here.", ephemeral: true).ConfigureAwait(false);
            }

            return;
        }

        var context = new SocketInteractionContext(client, interaction);
        var result = await interactionService.ExecuteCommandAsync(context, services).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Interaction handling failed ({ErrorType}).", result.Error?.ToString() ?? "Unknown");
        }
    }
}
