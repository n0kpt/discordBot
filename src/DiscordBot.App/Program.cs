using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.App;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

var settings = DiscordBotSettings.FromConfiguration(builder.Configuration);
builder.Services.AddDiscordBot(settings);

await builder.Build().RunAsync();

public partial class Program { }
