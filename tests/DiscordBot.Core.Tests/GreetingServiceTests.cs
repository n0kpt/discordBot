using DiscordBot.Core;

namespace DiscordBot.Core.Tests;

public class GreetingServiceTests
{
    [Fact]
    public void GetGreeting_ReturnsHelloWorld()
    {
        var service = new GreetingService();

        var greeting = service.GetGreeting();

        Assert.Equal("Hello, world!", greeting);
    }
}
