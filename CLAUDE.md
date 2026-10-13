# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build / run

- `dotnet build SimpleWorkflowEngine.sln` — build the solution (net9.0, nullable enabled).
- `dotnet run --project SimpleWorkflowEngine/SimpleWorkflowEngine.csproj` — run the console host (`Program.cs`).

The csproj is `OutputType=Exe` and takes a `FrameworkReference` on `Microsoft.AspNetCore.App` purely to get `Microsoft.Extensions.DependencyInjection` and related host primitives — this is a console app, not a web app. `Program.cs` is still the stock `"Hello, World!"` stub: there is no composition root yet, so the workflow pieces below are not wired up at startup.

No test project exists.

## Architecture

Generic, in-memory workflow engine with two layers:

**Engine (`Workflow/`)** — reusable, domain-agnostic:
- `WorkflowEngine<TContext>` where `TContext : WorkflowContext`. Steps are registered via `AddStep(step, nextStep?)`; conditional branches via `AddRoute(from, predicate, to)`. `RunAsync` walks the step graph, timing each step with `Stopwatch` and firing `OnStepStarted` / `OnStepCompleted` events. The first `AddStep` call becomes the entry point.
- Next-step resolution order inside `RunAsync` → `ResolveNext`: (1) the step's own `StepResult.NextStep` (via `StepResult.GoTo`) wins, (2) otherwise the first matching `AddRoute` predicate, (3) otherwise the linear `nextStep` from `AddStep`. Keep this precedence in mind when adding routing — a step that returns `GoTo` bypasses all registered routes.
- `IWorkflowStep<TContext>` has `Name` + `ExecuteAsync`; `StepResult` is `Success()` / `GoTo(step)` / `Failure(error)`.
- `WorkflowContext` is abstract and tracks `Status`, `CurrentStep`, `ExecutedSteps`, `Errors`, `StartedAt/CompletedAt`, plus a loose `Data` bag. `WorkflowRun` and `WorkflowStepLog` in the same file look like persistence DTOs but nothing currently reads or writes them.

**Order domain (`Orders/`, `Factory/`)** — concrete workflow built on the engine:
- `OrderContext : WorkflowContext` carries the `Order` plus flags (`IsBackorder`, `PaymentDeclined`, `PaymentId`) that routing predicates read.
- Steps live in `Orders/Steps/` and each hard-codes its `Name` string (`"ValidateOrder"`, `"CheckInventory"`, `"ChargePayment"`, `"SendConfirmation"`, `"NotifyBackorder"`, `"HandleDeclinedPayment"`). `OrderWorkflowFactory.Build(IServiceProvider)` composes them: linear Validate → CheckInventory, then branches on `IsBackorder` and `PaymentDeclined`. If you rename a step, update the matching `AddRoute` string in the factory — there is no compile-time check.
- Steps are resolved from DI (`sp.GetRequiredService<...>()`), so adding a step means registering its concrete type in the service collection as well as adding it to the factory.

**Side-effect services (root namespace)**: `IEmailService` with a no-op `SmtpEmailService`. Steps like `SendConfirmationStep` take it via constructor injection.

## Notes

- `.github/copilot-instructions.md` only contains Azure MCP tool rules — not relevant to the C# code itself.
