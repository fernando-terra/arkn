# P04 — IArknJob implementation

**Context:** You are working in a .NET 10 project using the Arkn framework.

**Task:** Write a class `InvoiceProcessorJob` that implements `IArknJob`. The job should:
1. Log "Starting invoice processing" at the beginning
2. Simulate processing (just a `Task.Delay(100)`)
3. Return success

**Expected patterns:**
- Implements `IArknJob`
- `ExecuteAsync` returns `Task<Result>` (not `Task` or `void`)
- Uses `IArknLogger` injected via constructor
- Returns `Result.Success()` at the end
- No exceptions thrown for control flow
