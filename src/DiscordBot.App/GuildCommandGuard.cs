namespace DiscordBot.App;

public sealed class GuildCommandGuard(ulong configuredGuildId)
{
    public bool Allows(ulong? guildId) => guildId == configuredGuildId;
}
