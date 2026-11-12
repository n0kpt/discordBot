using Discord.Interactions;

namespace DiscordBot.App;

public sealed class DiscordGuildCommandRegistry(InteractionService interactionService) : IGuildCommandRegistry
{
    public async Task RegisterAsync(ulong guildId, bool deleteMissing)
    {
        await interactionService.RegisterCommandsToGuildAsync(guildId, deleteMissing).ConfigureAwait(false);
    }
}
