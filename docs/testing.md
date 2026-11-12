# Test plan by stage

Run automated checks from the repository root. Offline unit tests must not need
Discord credentials. Record automated results separately from live integration
checks. Later stages retain the earlier gates; “planned” checks are not
implemented or passing until verified.

## Stage 1: Discord-independent foundation — complete

**Automated unit test**

- `dotnet test` builds the solution and runs the xUnit tests.
- `GreetingService` returns exactly `Hello, world!`.

Verified on 2026-11-12: Release build succeeded; 1 test passed, 0 failed,
0 skipped.

## Stage 2: guild-restricted `/hello` bot — offline checks passed, live check pending

Verified on 2026-11-12 with
`dotnet test DiscordBot.slnx --configuration Release --disable-build-servers -m:1`:
Release build succeeded; 18 tests passed, 0 failed, 0 skipped.
The live Discord check remains pending at the user's request.

**Automated tests present in the implementation**

- Release build and all Stage 1 tests pass.
- Configuration validation rejects a missing or blank token and missing,
  malformed, zero, or overflowing server IDs before attempting login.
- Validation and startup errors do not include a sentinel token value.
- Offline command tests verify command discovery, the intended name and
  description, server restrictions, and exactly one `Hello, world!` response.
- Registration boundary tests cover guild-only command registration and avoid
  duplicate registration on repeated and overlapping initial ready events.
- Service resolution verifies that the hosted bot and registrar can be constructed.
- Asynchronous slash-command failures are reported without logging exception
  messages or their embedded secret values.

An additional offline process check supplied a dummy token and invalid server ID:
startup failed with configuration guidance and did not print the dummy token.

The offline tests also cover retrying command registration after a transient
registration failure. Command discovery currently expects `hello` with the
description `Get a greeting.`.
Retry occurs after a subsequent ready event, not on a background retry timer;
restart after correcting registration failures if the connection stays up.

**Live integration check — pending**

- Run with the existing test server's local credentials and verify connection,
  `/hello` registration, and one greeting response in the invoking channel.
- Restart and confirm the command remains available without duplicate entries.
- Press Ctrl+C and confirm clean shutdown.
- Do not count a missing live check as a pass. No live Discord check has been
  performed for this milestone.

For local secrets, use the setup in [README.md](../README.md). The test-server
application needs the `bot` and `applications.commands` install scopes. No
additional bot permission bitfield, Administrator permission, or Message
Content intent is needed for this command. Discord includes
`applications.commands` when `bot` is selected, though both scopes may be
selected explicitly. See [Discord's application command authorization documentation](https://docs.discord.com/developers/interactions/application-commands#authorizing-your-application).

## Later stages — not yet approved or implemented

Each requires its own plan and user validation before implementation.

### Docker deployment on the Proxmox VM

After confirming the guest OS and CPU architecture, build the container for
that target; verify runtime secret injection, startup, shutdown, and restart in
the VM. Docker implementation and deployment checks are not part of the
current milestone.

### Entry sounds

After selecting the voice library and agreeing on triggers and sound policy,
test the chosen event filtering, queue/cooldown/cancellation behavior, and
failure handling as applicable. Add a test-server voice integration check.
These are candidate checks; the behavior remains undecided.

### CS2 statistics

After agreeing on specific statistics and verifying a data source, test its
adapter with controlled valid, missing, private/unavailable, rate-limited,
timed-out, and malformed responses when applicable. A live smoke check should
be limited and opt-in. No API or statistic availability is assumed.
