using Discord.Interactions;
using DiscordBot.Core;

namespace DiscordBot.App;

public interface IGreetingResponder
{
    Task RespondAsync(string message);
}

public sealed class HelloCommandHandler(GreetingService greetingService)
{
    public Task HandleAsync(IGreetingResponder responder) =>
        responder.RespondAsync(greetingService.GetGreeting());
}

public sealed class HelloModule(HelloCommandHandler handler) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("hello", "Get a greeting.")]
    public Task HelloAsync() => handler.HandleAsync(new ModuleGreetingResponder(this));

    private sealed class ModuleGreetingResponder(HelloModule module) : IGreetingResponder
    {
        public Task RespondAsync(string message) => module.RespondAsync(message);
    }
}
