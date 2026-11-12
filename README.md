# Discord bot

A C# / .NET project for a private Discord server. The current scope is a small,
Discord-independent greeting component and its unit test. A live bot and
integrations are future work; this repository does not yet connect to Discord.

## Prerequisites

- .NET 10 SDK

## Run the tests

From the repository root:

```sh
dotnet test
```

The current test checks that the core greeting is exactly `Hello, world!`.
This foundation test does not require Discord credentials. See
[docs/testing.md](docs/testing.md) for the proposed cumulative test gates at
each development stage, including future integration and manual checks.
