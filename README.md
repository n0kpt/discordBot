# Discord bot

A C# / .NET 10 bot for one private Discord server. The approved first live
milestone implements a guild-restricted `/hello` command. The Release build and
18 offline tests pass. The live test-server check is deferred at the user's
request and has not been performed.

## Prerequisites

- .NET 10 SDK
- A Discord application with a bot user, plus a test server where you can
  install the application

## Configure local credentials

The app reads the bot token and server ID from `Discord__Token` and
`Discord__GuildId`. For local development, .NET User Secrets keeps these
settings in your user profile rather than in the repository. Set them on your
machine, replacing the angle-bracketed values locally; do not commit or share
the values:

```sh
dotnet user-secrets set --project src/DiscordBot.App "Discord:Token" "<BOT_TOKEN>"
dotnet user-secrets set --project src/DiscordBot.App "Discord:GuildId" "<TEST_SERVER_ID>"
```

User Secrets are a development convenience, not encrypted storage. The
equivalent environment variables are `Discord__Token` and `Discord__GuildId`.
User Secrets are loaded in the Development environment; environment variables
override them. The app does not load a repository `.env` file. See [Microsoft's User Secrets guidance](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0).

## Install the app in a test server

Use the Discord Developer Portal's installation/OAuth2 settings to install the
application into the test server. Select the `bot` and
`applications.commands` scopes. This app uses a bot account for its gateway
connection, but the slash command needs no additional permission bitfield.
Administrator permission and the Message Content intent are not needed.
Discord automatically includes
`applications.commands` with `bot`, but selecting both makes the intended
command scope explicit. See [Discord's application command authorization
guide](https://docs.discord.com/developers/interactions/application-commands#authorizing-your-application).

## Run and test

Start the app from the repository root in Development so it loads User Secrets:

```sh
DOTNET_ENVIRONMENT=Development dotnet run --project src/DiscordBot.App
```

Run the offline automated checks:

```sh
dotnet test
```

The unit tests do not need Discord credentials. The live check requires the
local test-server configuration and is tracked separately in
[docs/testing.md](docs/testing.md).

Command registration failures are logged and retried on the next connection-ready
event. If the connection stays up, restart the bot after fixing the cause.
