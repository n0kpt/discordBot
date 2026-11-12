using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace DiscordBot.App;

public sealed class SlashCommandResultReporter(ILogger<SlashCommandResultReporter> logger)
{
    public Task HandleAsync(IResult result)
    {
        if (!result.IsSuccess)
        {
            var errorType = result is ExecuteResult { Exception: { } exception }
                ? exception.GetType().Name
                : result.Error?.ToString() ?? "Unknown";
            logger.LogWarning("Slash command execution failed ({ErrorType}).", errorType);
        }

        return Task.CompletedTask;
    }
}
