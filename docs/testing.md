# Test plan by stage

Run the implemented repository checks from the project root. The gates below
are cumulative: later stages keep passing the earlier gates. “Proposed” means
the stage is future work and its checks are not implemented yet. Unit tests run
without Discord or Steam credentials; integration and manual checks are labeled
separately.

## Stage 1: foundation — current scope

**Implemented unit test**

- `dotnet test` — restore/build the solution and run the xUnit tests.
- Greeting contract: the core greeting service returns exactly
  `Hello, world!`.

No Discord connection or credentials are required.

Verified on 2026-11-12 with `dotnet test DiscordBot.slnx --configuration Release`:
build succeeded; 1 test passed, 0 failed, 0 skipped. Re-run the checks after changes.

## Stage 2: live `/hello` bot — proposed

Keep all Stage 1 checks passing. Once a Discord adapter and command handler
exist, add:

**Unit tests**

- Configuration validation accepts the required settings and reports missing
  or invalid settings without including secret values in errors or logs.
- The command handler produces the agreed greeting response and handles its
  defined error cases.

**Integration / manual checks**

- A test-server smoke check confirms the bot connects and `/hello` responds.
- Verify clean shutdown and that invalid configuration prevents startup with a
  useful, credential-safe error.

Credentials must be supplied outside source control. The Discord library and
exact configuration contract are still undecided, so these checks are plans,
not runnable tests today.

## Stage 3: module structure — proposed

Keep prior gates passing. As module registration and boundaries are designed,
add tests for module registration, disabled modules, and failure isolation at
the boundaries the design actually promises. Add integration checks that the
application starts with the intended module set. Do not assert behavior for
module controls that have not been selected or implemented.

## Stage 4: Docker on the Proxmox VM — proposed

Keep prior gates passing. After a container image and target VM are selected:

**Build / configuration checks**

- Build the image for the confirmed guest OS and CPU architecture.
- Confirm required settings and secrets are injected at runtime and are not
  baked into the image or emitted in logs.

**Manual integration checks in the VM**

- Start the container, confirm the bot reaches the test server, stop it, and
  confirm clean shutdown.
- Exercise the intended restart behavior and confirm it returns to service.

The VM guest OS and CPU architecture remain unknown, so no target image check
is runnable yet.

## Stage 5: entry sounds — proposed

Keep prior gates passing. Once voice behavior and policy are decided, use unit
tests around the module's event filtering, queue behavior, cooldowns,
cancellation, and failure handling as applicable to the chosen design. Add a
manual test-server voice check for the selected channel and sound behavior.
Channel selection, cooldowns, sound storage, voice library, and expected
failure behavior remain undecided; these are candidate coverage areas, not
promised features or existing tests.

## Stage 6: CS2 statistics — proposed

Keep prior gates passing. Once the desired statistics and data source are
known, isolate external data access so tests can supply controlled responses.
Cover valid and missing data, private/unavailable profiles where applicable,
rate limiting, timeouts, and malformed responses according to the provider's
documented behavior. Add only a limited, opt-in live smoke check if a suitable
source is selected. Do not assert that particular statistics or API access
exist until verified; no source or statistics have been chosen.
