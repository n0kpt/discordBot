using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.App;

public sealed record DiscordBotSettings(string Token, ulong GuildId)
{
    public static DiscordBotSettings FromConfiguration(IConfiguration configuration)
    {
        var token = configuration["Discord:Token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Discord token is missing or blank. Set Discord__Token.");
        }

        var guildIdValue = configuration["Discord:GuildId"];
        if (!ulong.TryParse(guildIdValue, NumberStyles.None, CultureInfo.InvariantCulture, out var guildId) || guildId == 0)
        {
            throw new InvalidOperationException("Discord server ID must be a positive unsigned integer. Set Discord__GuildId.");
        }

        return new DiscordBotSettings(token, guildId);
    }
}
