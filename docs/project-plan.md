# Discord bot plan

Status: initial Discord-independent foundation is established and tested. The live
bot, integrations, and deployment are future work.

## Confirmed requirements

- Build a Discord bot in C# / .NET, starting with hello-world behavior.
- Keep the first design small and add later functionality as modules.
- Candidate future modules are entry sounds and Steam game statistics.
- Entry sounds will play a personal sound when a user enters a voice channel.
- CS2 is the first planned game integration; specific statistics are deferred.
- The audience is one private Discord server; commands will use slash commands.
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
- Discord .NET library, to be selected after checking current slash-command
  and voice support.
- Which CS2 statistics to show; no API behavior or statistic availability has
  been confirmed.
- Entry-sound channel selection, sound storage, cooldown behavior, and other
  details.
