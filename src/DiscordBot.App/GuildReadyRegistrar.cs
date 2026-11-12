namespace DiscordBot.App;

public interface IGuildCommandRegistry
{
    Task RegisterAsync(ulong guildId, bool deleteMissing);
}

public sealed class GuildReadyRegistrar(IGuildCommandRegistry registry, ulong guildId)
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _registered;

    public async Task OnReadyAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_registered)
            {
                return;
            }

            await registry.RegisterAsync(guildId, deleteMissing: false).ConfigureAwait(false);
            _registered = true;
        }
        finally
        {
            _gate.Release();
        }
    }
}
