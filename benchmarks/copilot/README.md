# Arkn.Copilot Benchmark

Measures how often an LLM generates correct Arkn code **without manual correction**.

## How it works

1. Send each prompt in `prompts/` to a model (with or without instructions)
2. Save the generated `.cs` file to `results/<model>/<prompt-id>/`
3. Run `score.py` — it compiles the code and checks for ARK analyzer warnings
4. Compare **with-instructions** vs **without-instructions** hit rate

## Running

```bash
# Score all results in results/
python3 score.py

# Score a specific model
python3 score.py --model gpt-4o

# Detailed output
python3 score.py --verbose
```

## Scoring criteria

A result is **PASS** when:
- `dotnet build` succeeds (zero errors)
- Zero ARK001–ARK004 warnings from Arkn.Analyzers

A result is **FAIL** when any of the above is violated.

## Target

| Condition | Target |
|-----------|--------|
| Without instructions | baseline (expected ~40-60%) |
| With `copilot-instructions.md` | > 90% |

## Prompts

| ID | Scenario |
|----|----------|
| P01 | Create a domain service method that finds a user by ID |
| P02 | Create a domain service method that creates a user |
| P03 | Create a Minimal API endpoint that calls a service and maps errors to HTTP |
| P04 | Create an IArknJob that processes a queue |
| P05 | Create an error code for a not-found scenario |
| P06 | Chain two Result operations with Map and Bind |
| P07 | Return multiple validation errors from a service method |
| P08 | Create an HTTP client call using Arkn.Http |
| P09 | Register a job with retry and notification on failure |
| P10 | Create a ValueObject with validation returning Result |
| P11 | Write a unit test for a method that returns Result<T> |
| P12 | Create a Slack notifier registration |
| P13 | Map a Result<T> failure to an HTTP 409 Conflict response |
| P14 | Consume a Result with Match in a background service |
| P15 | Create an Entity base class implementation |
| P16 | Add structured logging to a job execution |
| P17 | Create a typed HTTP client for an external API |
| P18 | Handle a Result that may contain multiple validation errors |
| P19 | Write a CRUD service using Result<T> throughout |
| P20 | Configure Arkn.Jobs in Program.cs with DI |
