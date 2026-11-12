using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscordBot.App;

public static class DiscordBotServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordBot(this IServiceCollection services, DiscordBotSettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
        }));
        services.AddSingleton(provider => new InteractionService(
            provider.GetRequiredService<DiscordSocketClient>(),
            new InteractionServiceConfig { DefaultRunMode = RunMode.Async }));
        services.AddSingleton<GreetingService>();
        services.AddSingleton<HelloCommandHandler>();
        services.AddSingleton<SlashCommandResultReporter>();
        services.AddSingleton<IGuildCommandRegistry, DiscordGuildCommandRegistry>();
        services.AddSingleton(provider => new GuildReadyRegistrar(
            provider.GetRequiredService<IGuildCommandRegistry>(), settings.GuildId));
        services.AddHostedService<DiscordBotService>();
        return services;
    }
}
