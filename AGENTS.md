# Project working agreement

## Current stage

The user approved the live `/hello` milestone in `docs/next-milestone.md`.
Implement it on `feature/hello-command`, including configuration validation,
guild-restricted command handling, lifecycle handling, tests, and documentation.
Docker, entry sounds, and CS2 remain future work requiring separate plans.
Record requirements and decisions in `docs/project-plan.md`.

## Product direction

- Build a Discord bot in C# / .NET.
- Start with a small hello-world capability.
- Add future features as modules, including entry sounds and Steam game stats.
- Keep the initial design small and easy to extend.
- Distinguish confirmed requirements from proposals and unanswered questions.

## Model and effort preferences

- Use more capable models for complex architecture, integration investigation,
  difficult debugging, and consequential reviews.
- Prefer less expensive models for well-specified, routine implementation.
- Choose models from those actually available; do not claim a model switch
  happened unless it did.
- Avoid unnecessary agent fan-out and repeated context to conserve usage.

## Subagent workflow

- Project agent definitions live in `.codex/agents/`.
- Delegate substantial architecture, integration investigation, and difficult
  debugging to `architect` (gpt-6-astra, high reasoning).
- Delegate bounded implementation with agreed acceptance criteria to
  `implementer` (gpt-6-luna, medium reasoning).
- Delegate substantial documentation work to `documentation` (gpt-6-luna,
  medium reasoning), using confirmed requirements and verified behavior.
- Keep trivial edits in the main agent to avoid unnecessary token overhead.
- Use at most two subagents concurrently. Give each an explicit scope and
  avoid overlapping file edits. Run dependent work sequentially.
- The parent reviews results and owns integration and final verification.
- If named roles cannot be selected by the current tool interface, pass their
  instructions and configured model explicitly to an available subagent.
- Never silently substitute a more expensive model if a configured model is
  unavailable; report the limitation.

## Implementation principles

- Keep credentials out of source control, documentation, and logs.
- Keep feature-specific behavior within its module.
- Add abstractions and dependencies when justified by an actual requirement.
- Verify changes with checks appropriate to their impact.

## Planning and Git workflow

- Present the next functionality's concrete plan and acceptance criteria to the
  user for validation before implementing it.
- Create a dedicated `feature/<short-description>` branch before changing code
  for each new functionality. Do not implement new features directly on `main`.
- Keep commits focused and exclude credentials, build output, and local IDE files.
- Push when authorized. Never force-push or overwrite remote history to resolve
  an unexpected divergence.
