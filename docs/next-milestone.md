# Next milestone: a working /hello bot

Status: approved and implemented on 2026-11-12; Release build and 18 offline
tests passed. Live check deferred by the user.
Implementation branch: `feature/hello-command`.

The user explicitly deferred the live Discord check. Complete offline checks
and document the live smoke test as pending; do not request or use credentials.

## Scope

- Add a .NET 10 executable, `DiscordBot.App`, using Discord.Net WebSocket and
  Interactions packages. Select a current stable version at implementation.
- Use the existing `GreetingService` from a compiled greeting command module.
- Add normal dependency injection, configuration validation, and a hosted
  Discord connection service. No dynamic plugin framework is required.
- Read `Discord__Token` and `Discord__GuildId` from environment configuration.
  Validate settings before connection and never print secret values.
- Register `/hello` only in the configured server in both Debug and Release.
  Preserve unrelated commands, load modules once, and prevent duplicate
  registration on repeated connection-ready events.
- Restrict command handling to the configured server. Respond once in the
  invoking channel with `Hello, world!`. No message-content intent required.
- Provide useful startup/connection errors and clean cancellation/shutdown.
- Update local setup and test instructions. Credentials stay local.

## Acceptance gates

1. Release build and existing tests pass.
2. Configuration tests cover missing/blank token and missing, malformed, zero,
   or overflowing server ID. Errors never include a sentinel secret value.
3. An offline command test verifies exactly one greeting response.
4. Offline command discovery verifies the intended name and description.
5. Tests verify server restrictions and registration behavior through a small
   boundary where needed; avoid mocking the whole Discord SDK.
6. A live check on the existing test server verifies connection, registration,
   response, restart without duplicate commands, and Ctrl+C shutdown.
7. Record automated and live results separately; a missing live check is not
   a passing check. Token provisioning and live checks need local credentials.

## Later functionality branches

- `feature/docker-deployment`: container and deployment instructions after
  confirming VM guest OS and CPU architecture.
- `feature/entry-sounds`: first prove voice connection/playback and native
  dependencies on the target platform, then agree triggers and sound policy.
- `feature/cs2-stats`: agree specific statistics and verify the data source
  before designing the integration.

Each functionality requires user validation of its plan before implementation.

## Research behind the proposal

- [Discord.Net InteractionService](https://docs.discordnet.dev/guides/int_framework/intro)
  supports compiled modules and guild command registration.
- [Discord.Net package](https://www.nuget.org/packages/Discord.Net) is the
  proposed library family; the architecture review found stable 3.20.2.
- Discord.Net's [3.19.0 release](https://github.com/discord-net/Discord.Net/releases/tag/3.19.0)
  introduced libdave support. Discord's [voice specification](https://github.com/discord/discord-api-docs/blob/main/developers/topics/voice-connections.mdx)
  requires DAVE support. Voice still needs a separate practical compatibility
  check; selecting the library for slash commands does not validate audio.
