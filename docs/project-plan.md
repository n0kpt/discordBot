# Discord bot plan

Status: Discord-independent foundation is established and tested. The approved
guild-restricted `/hello` implementation and offline checks are complete on
`feature/hello-command`; the live check is deferred by the user.
Docker deployment and integrations remain future work requiring separate
plans.

## Confirmed requirements

- Build a Discord bot in C# / .NET, starting with hello-world behavior.
- Keep the first design small and add later functionality as modules.
- Candidate future modules are entry sounds and Steam game statistics.
- Entry sounds will play a personal sound when a user enters a voice channel.
- CS2 is the first planned game integration; specific statistics are deferred.
- The audience is one private Discord server; commands use slash commands.
- Hosting is planned in Docker on a VM on the user's Proxmox home server.
- Modules may be compiled into the application; restarting for changes is fine.
- A Discord application and test server already exist.
- Keep credentials outside source control, documentation, and logs.

## Decisions recorded for the initial scaffold

- Target .NET 10 (`net10.0`), supported by the installed .NET 10 SDKs.
- Keep greeting behavior in a Discord-independent `DiscordBot.Core` class
  library so it can be tested without a bot connection.
- Use xUnit for the initial unit test project.
- The current greeting contract is the exact string `Hello, world!`.
- The root solution is `DiscordBot.slnx`.

These choices establish the testable foundation; they do not select a Discord
library or implement a live bot.

## Approved `/hello` milestone

Approved and implemented on 2026-11-12. The selected app design
is a .NET 10 Generic Host executable (`DiscordBot.App`) using Discord.Net
WebSocket and Interactions packages. The command uses the existing compiled
greeting service. Configuration keys are `Discord__Token` and
`Discord__GuildId`; local development supports .NET User Secrets. The command
is registered for and handled only in the configured server. It responds once
in the invoking channel with `Hello, world!`.

The app should validate configuration before login, avoid logging secrets,
load modules once, avoid duplicate command registration after repeated ready
events, and stop cleanly on cancellation. See [next-milestone.md](next-milestone.md)
for approved acceptance gates and [testing.md](testing.md) for stage tests.

The Release build and 18 offline tests pass. An invalid-configuration process
check also confirmed startup rejection without printing a sentinel token.

The live Discord check is pending at the user's request. No live connection, command registration,
restart check, or Ctrl+C shutdown check has been performed for this milestone.

## Current foundation acceptance

- `DiscordBot.Core` builds for .NET 10.
- Its greeting service returns exactly `Hello, world!`.
- The xUnit test asserts that exact result.
- `dotnet test` from the repository root discovers and runs the solution tests.
- No Discord credentials, connection, or server are needed for this foundation.

Verified on 2026-11-12: Release build succeeded and the greeting test passed
(1 passed, 0 failed, 0 skipped).

## Proposed stage order

See [testing.md](testing.md) for the cumulative stage gates and the tests to
pass at each stage. The order is a proposal; future integration details depend
on requirements that are still unanswered.

## Unanswered questions

- VM guest operating system and CPU architecture.
- Bot/project name.
- Voice compatibility of the selected Discord library still needs a practical
  check before entry-sound work.
- Which CS2 statistics to show; no API behavior or statistic availability has
  been confirmed.
- Entry-sound channel selection, sound storage, cooldown behavior, and other
  details.
