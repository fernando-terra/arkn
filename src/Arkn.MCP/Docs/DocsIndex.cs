namespace Arkn.MCP.Docs;

/// <summary>Static documentation index embedded in the binary — no HTTP required.</summary>
public static class DocsIndex
{
    private static readonly Dictionary<string, string> _entries = new(StringComparer.OrdinalIgnoreCase)
    {
        ["result"] = """
            # Arkn.Results — Result Pattern

            ## Creating results
            Result<User> success = Result.Success(user);
            Result<User> failure = Result.Failure<User>(Error.NotFound("User.NotFound", "User not found"));

            // Implicit conversions:
            Result<User> r = user;                                   // T → Result<T>
            Result<User> r = Error.NotFound("User.NotFound", "msg"); // Error → Result<T>

            ## Functional API
            result
              .Map(u => u.Name)              // transform value
              .Bind(name => Validate(name))  // chain operations
              .Ensure(n => n.Length > 0, Error.Validation("Name.Empty", "Name is required"))
              .Tap(n => logger.Info(n))      // side effect
              .Match(
                  onSuccess: name  => Results.Ok(name),
                  onFailure: error => Results.Problem(error.Message));

            ## Multiple errors
            Result.Failure<T>(errors[]) — result.Errors: IReadOnlyList<Error>

            ## Error types
            Error.Failure / NotFound / Validation / Conflict / Unauthorized / Forbidden
            """,

        ["error"] = """
            # Arkn Error Codes (ARK002)

            Error codes must follow: Namespace.Reason (dot-separated PascalCase)
            Examples: "User.NotFound", "Order.AlreadyProcessed", "Payment.InvalidCard"

            ## Factories
            Error.NotFound("User.NotFound", "User not found")
            Error.Validation("Email.Invalid", "Email is not valid")
            Error.Conflict("User.EmailConflict", "Email already exists")
            Error.Unauthorized("Auth.Required", "Authentication required")
            Error.Forbidden("User.Forbidden", "Access denied")
            Error.Failure("Order.Failed", "Order processing failed")

            ## With Arkn.SourceGen (eliminates boilerplate)
            [ArknErrors]
            public static partial class UserErrors
            {
                [ArknErrorCode("NotFound", "User was not found")]
                public static partial Error NotFound(string? detail = null);
            }
            """,

        ["iarknjob"] = """
            # Arkn.Jobs — IArknJob

            ## Interface
            public interface IArknJob
            {
                Task<Result> ExecuteAsync(ArknJobContext ctx);
            }

            ## Context
            ctx.RunId          — Guid unique per execution
            ctx.JobName        — registered name
            ctx.ScheduledAt    — when it was due
            ctx.CancellationToken
            ctx.Log(msg)       — info log scoped to this run
            ctx.LogWarning(msg)
            ctx.LogError(msg, ex)

            ## Registration
            services.AddArknJobs(jobs =>
            {
                jobs.Add<MyJob>("0 2 * * *")
                    .WithName("my-job")
                    .WithTimeout(TimeSpan.FromMinutes(10))
                    .WithRetry(maxAttempts: 3)
                    .NotifyOn(JobEvent.Failed | JobEvent.TimedOut);

                jobs.OnFailure<SlackNotifier>(); // global fallback
            });

            ## Cron operators: * , - /
            Examples: "* * * * *", "0 2 * * *", "*/5 * * * *", "0 9,17 * * 1-5"
            """,

        ["iarknlogger"] = """
            # Arkn.Logging — IArknLogger

            ## Methods
            logger.Trace(msg, context?)
            logger.Debug(msg, context?)
            logger.Info(msg, context?)
            logger.Warning(msg, context?)
            logger.Error(msg, exception?, context?)
            logger.Fatal(msg, exception?, context?)

            ## Context (scoped, immutable)
            var ctx = ArknLogContext.ForScope("job-run-123")
                .With("UserId", 42)
                .With("Action", "ProcessOrder");

            logger.Info("Processing", ctx);

            ## Setup
            services.AddArknLogging(logging =>
            {
                logging.SetMinimumLevel(ArknLogLevel.Info);
                logging.AddConsoleSink();
                logging.AddFileSink("logs/app.log");
                logging.AddInMemorySink();
            });

            ## Available sinks
            Console / File / InMemory / Seq / Elasticsearch
            """,

        ["addarknhttp"] = """
            # Arkn.Http — Typed HTTP Clients

            ## Define a typed client
            public sealed class PaymentClient(IArknHttp http)
                : ArknHttpClient(http, "https://api.payments.com")
            {
                public Task<Result<PaymentDto>> GetAsync(Guid id) =>
                    GetAs<PaymentDto>("/payments/{id}", id);

                public Task<Result<PaymentDto>> CreateAsync(PaymentRequest req) =>
                    PostAs<PaymentDto>("/payments", req);

                public Task<Result> CancelAsync(Guid id) =>
                    Delete("/payments/{id}", id);
            }

            ## Register with DI
            services.AddArknHttp<PaymentClient>("https://api.payments.com")
                .WithRetry(maxAttempts: 3)
                .WithTimeout(TimeSpan.FromSeconds(30));
            """,

        ["analyzers"] = """
            # Arkn.Analyzers — Compile-time Rules

            ARK001 (Warning) — Domain methods returning void/Task instead of Result
            ARK002 (Warning) — Error code not following Namespace.Reason convention
            ARK003 (Warning) — Result silently discarded (not assigned or matched)
            ARK004 (Error)   — IArknJob.ExecuteAsync not returning Task<Result>

            ## Install
            dotnet add package Arkn.Analyzers
            (DevelopmentDependency=true — does not ship in output)

            ## Suppress (if needed)
            #pragma warning disable ARK001
            """,

        ["sourcegen"] = """
            # Arkn.SourceGen — Error Factory Generator

            ## Usage
            [ArknErrors]
            public static partial class UserErrors
            {
                [ArknErrorCode("NotFound", "User was not found")]
                public static partial Error NotFound(string? detail = null);

                [ArknErrorCode("Validation", "Email is invalid")]
                public static partial Error InvalidEmail(string? detail = null);
            }

            // Generated:
            public static partial class UserErrors
            {
                public static partial Error NotFound(string? detail = null) =>
                    Error.NotFound("UserErrors.NotFound", detail ?? "User was not found");

                public static partial Error InvalidEmail(string? detail = null) =>
                    Error.Validation("UserErrors.InvalidEmail", detail ?? "Email is invalid");
            }

            ## Error codes
            Auto-generated as ClassName.MethodName — always ARK002 compliant.

            ## Install
            dotnet add package Arkn.SourceGen
            """,

        ["templates"] = """
            # Arkn.Templates — dotnet new

            ## Install
            dotnet new install Arkn.Templates

            ## Available templates
            arkn-api  — Minimal API + Arkn.Results + error→HTTP + CI
            arkn-job  — Worker Service + Arkn.Jobs + SampleJob
            arkn-lib  — Class library + Arkn.Core + Arkn.Analyzers + SampleEntity

            ## Usage
            dotnet new arkn-api -n MyApi
            dotnet new arkn-job -n MyWorker
            dotnet new arkn-lib -n MyLibrary
            """,
    };

    public static string Lookup(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return "Please provide a search query.";

        // Direct match
        if (_entries.TryGetValue(query.Trim(), out var direct))
            return direct;

        // Keyword search
        var q = query.ToLowerInvariant();
        var hits = _entries
            .Where(kv => kv.Key.Contains(q, StringComparison.OrdinalIgnoreCase)
                      || kv.Value.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (hits.Count == 0)
            return $"No documentation found for '{query}'. Available topics: {string.Join(", ", _entries.Keys)}";

        return string.Join("\n\n---\n\n", hits.Select(h => h.Value));
    }
}
