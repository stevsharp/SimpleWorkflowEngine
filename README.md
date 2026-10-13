# SimpleWorkflowEngine

A tiny, strongly-typed workflow engine in C# / .NET 9. Steps are plain classes, transitions are either linear (`AddStep(step, nextStep)`) or conditional (`AddRoute(from, predicate, to)`), and the whole thing composes through DI.

The sample domain is an order-processing workflow: validate → check inventory → either charge payment (and send confirmation) or notify backorder, with a failure branch for declined payments.

## Repository layout

```
SimpleWorkflowEngine.sln
├── SimpleWorkflowEngine/           # console host + engine + sample domain
│   ├── Workflow/                   # generic engine (reusable)
│   │   ├── WorkflowEngine.cs
│   │   ├── WorkflowContext.cs
│   │   ├── IWorkflowStep.cs
│   │   └── StepResult.cs
│   ├── Orders/                     # sample domain
│   │   ├── Order.cs / OrderItem.cs / OrderContext.cs
│   │   └── Steps/                  # one class per step
│   ├── Factory/
│   │   └── OrderWorkflowFactory.cs
│   ├── IEmailService.cs / EmailMessage.cs
│   └── Program.cs                  # DI composition root + interactive menu
└── SimpleWorkflowEngine.Tests/     # xUnit tests
```

## Build and run

```bash
dotnet build SimpleWorkflowEngine.sln
dotnet run --project SimpleWorkflowEngine/SimpleWorkflowEngine.csproj
dotnet test SimpleWorkflowEngine.sln
```

## How the engine works

A workflow is a graph of named steps. Each step implements `IWorkflowStep<TContext>`:

```csharp
public interface IWorkflowStep<TContext> where TContext : WorkflowContext
{
    string Name { get; }
    Task<StepResult> ExecuteAsync(TContext context, CancellationToken ct = default);
}
```

A step returns one of three results:

```csharp
StepResult.Success();            // follow routes/transitions to pick the next step
StepResult.GoTo("SomeStep");     // override routing and jump to a specific step
StepResult.Failure("reason");    // stop the workflow and mark it Failed
```

`WorkflowEngine<TContext>.RunAsync` executes the current step, records it in `ExecutedSteps`, raises `OnStepStarted` / `OnStepCompleted`, and resolves the next step in this order:

1. The step's own `StepResult.NextStep` (via `GoTo`)
2. The first matching `AddRoute` predicate for the current step
3. The linear `nextStep` passed to `AddStep`

The run ends when a step returns `Failure`, the resolved next step is `null`, or the cancellation token fires.

## Example 1 — a validation step

```csharp
public class ValidateOrderStep : IWorkflowStep<OrderContext>
{
    public string Name => "ValidateOrder";

    public Task<StepResult> ExecuteAsync(OrderContext ctx, CancellationToken ct = default)
    {
        if (!ctx.Order.Items.Any())
            return Task.FromResult(StepResult.Failure("Order has no items."));

        if (ctx.Order.TotalAmount <= 0)
            return Task.FromResult(StepResult.Failure("Invalid order amount."));

        if (string.IsNullOrWhiteSpace(ctx.Order.CustomerEmail))
            return Task.FromResult(StepResult.Failure("Customer email is required."));

        return Task.FromResult(StepResult.Success());
    }
}
```

## Example 2 — a routing step (no failure, just sets a flag)

```csharp
public class CheckInventoryStep : IWorkflowStep<OrderContext>
{
    public string Name => "CheckInventory";

    public Task<StepResult> ExecuteAsync(OrderContext ctx, CancellationToken ct = default)
    {
        ctx.IsBackorder = ctx.Order.Items.Any(i => !i.StockAvailable);
        return Task.FromResult(StepResult.Success());
    }
}
```

The *routing* lives in the factory, not the step. The engine inspects `ctx.IsBackorder` after this step runs and chooses the next step based on `AddRoute` predicates.

## Example 3 — a step with an injected dependency

```csharp
public class SendConfirmationStep(IEmailService email) : IWorkflowStep<OrderContext>
{
    public string Name => "SendConfirmation";

    public async Task<StepResult> ExecuteAsync(OrderContext ctx, CancellationToken ct = default)
    {
        await email.SendAsync(new EmailMessage(
            To: ctx.Order.CustomerEmail,
            Subject: $"Order {ctx.Order.Id} Confirmed - Payment {ctx.PaymentId}",
            HtmlBody: "<h1>Hello</h1>"), ct);

        return StepResult.Success();
    }
}
```

## Example 4 — composing the workflow

The factory takes each step as a constructor dependency (no service locator, no magic strings — route targets use `step.Name` so renames are refactor-safe):

```csharp
public sealed class OrderWorkflowFactory
{
    private readonly ValidateOrderStep _validate;
    private readonly CheckInventoryStep _checkInventory;
    private readonly ChargePaymentStep _chargePayment;
    private readonly SendConfirmationStep _sendConfirmation;
    private readonly NotifyBackorderStep _notifyBackorder;
    private readonly HandleDeclinedPaymentStep _handleDeclinedPayment;

    public OrderWorkflowFactory(
        ValidateOrderStep validate,
        CheckInventoryStep checkInventory,
        ChargePaymentStep chargePayment,
        SendConfirmationStep sendConfirmation,
        NotifyBackorderStep notifyBackorder,
        HandleDeclinedPaymentStep handleDeclinedPayment)
    {
        _validate = validate;
        _checkInventory = checkInventory;
        _chargePayment = chargePayment;
        _sendConfirmation = sendConfirmation;
        _notifyBackorder = notifyBackorder;
        _handleDeclinedPayment = handleDeclinedPayment;
    }

    public WorkflowEngine<OrderContext> Build() =>
        new WorkflowEngine<OrderContext>()
            .AddStep(_validate, nextStep: _checkInventory.Name)
            .AddStep(_checkInventory)
                .AddRoute(_checkInventory.Name,
                    when: ctx => ctx.IsBackorder,
                    to: _notifyBackorder.Name)
                .AddRoute(_checkInventory.Name,
                    when: ctx => !ctx.IsBackorder,
                    to: _chargePayment.Name)
            .AddStep(_chargePayment)
                .AddRoute(_chargePayment.Name,
                    when: ctx => ctx.PaymentDeclined,
                    to: _handleDeclinedPayment.Name)
                .AddRoute(_chargePayment.Name,
                    when: ctx => !ctx.PaymentDeclined,
                    to: _sendConfirmation.Name)
            .AddStep(_sendConfirmation)
            .AddStep(_notifyBackorder)
            .AddStep(_handleDeclinedPayment);
}
```

## Example 5 — DI composition and running a workflow

```csharp
var services = new ServiceCollection();

services.AddSingleton<IEmailService, SmtpEmailService>();

services.AddTransient<ValidateOrderStep>();
services.AddTransient<CheckInventoryStep>();
services.AddTransient<ChargePaymentStep>();
services.AddTransient<SendConfirmationStep>();
services.AddTransient<NotifyBackorderStep>();
services.AddTransient<HandleDeclinedPaymentStep>();
services.AddTransient<OrderWorkflowFactory>();

await using var sp = services.BuildServiceProvider();
var engine = sp.GetRequiredService<OrderWorkflowFactory>().Build();

engine.OnStepStarted += name => Console.WriteLine($"-> {name}");
engine.OnStepCompleted += (name, result, ms) =>
{
    Console.WriteLine($"<- {name} ({ms} ms) success={result.IsSuccess}");
    return Task.CompletedTask;
};

var ctx = new OrderContext
{
    Order = new Order
    {
        Id = "ORD-1001",
        CustomerId = "CUST-1",
        CustomerEmail = "customer@example.com",
        CustomerName = "Jane Doe",
        TotalAmount = 150m,
        Items =
        {
            new OrderItem { ProductId = "P-1", Quantity = 2, UnitPrice = 75m, StockAvailable = true },
        },
    },
};

await engine.RunAsync(ctx);

Console.WriteLine($"Status: {ctx.Status}");
Console.WriteLine($"Path:   {string.Join(" -> ", ctx.ExecutedSteps)}");
```

Expected output:

```
-> ValidateOrder
<- ValidateOrder (0 ms) success=True
-> CheckInventory
<- CheckInventory (0 ms) success=True
-> ChargePayment
<- ChargePayment (82 ms) success=True
-> SendConfirmation
<- SendConfirmation (0 ms) success=True
Status: Completed
Path:   ValidateOrder -> CheckInventory -> ChargePayment -> SendConfirmation
```

## Built-in scenarios

`Program.cs` ships an interactive menu that lets you run any of the following orders through the engine:

| # | Scenario                            | Expected path                                                                       |
|---|-------------------------------------|-------------------------------------------------------------------------------------|
| 1 | Happy path (in stock, payment ok)   | Validate → CheckInventory → ChargePayment → SendConfirmation                        |
| 2 | Backorder (one item out of stock)   | Validate → CheckInventory → NotifyBackorder                                         |
| 3 | Payment declined (amount > 9999)    | Validate → CheckInventory → ChargePayment → HandleDeclinedPayment *(Failed)*        |
| 4 | Validation: no items                | Validate *(Failed)*                                                                 |
| 5 | Validation: zero amount             | Validate *(Failed)*                                                                 |
| 6 | Validation: missing email           | Validate *(Failed)*                                                                 |

Enter `1`-`6` to run one, `A` to run all, `Q` to quit.

## Example 6 — unit test

Steps are plain classes with explicit dependencies, so tests just instantiate what they need:

```csharp
public class OrderWorkflowFactoryTests
{
    private static (WorkflowEngine<OrderContext> Engine, FakeEmailService Email) BuildEngine()
    {
        var email = new FakeEmailService();
        var factory = new OrderWorkflowFactory(
            validate: new ValidateOrderStep(),
            checkInventory: new CheckInventoryStep(),
            chargePayment: new ChargePaymentStep(),
            sendConfirmation: new SendConfirmationStep(email),
            notifyBackorder: new NotifyBackorderStep(email),
            handleDeclinedPayment: new HandleDeclinedPaymentStep(email));

        return (factory.Build(), email);
    }

    [Fact]
    public async Task HappyPath_sends_confirmation_and_completes()
    {
        var (engine, email) = BuildEngine();
        var ctx = ValidOrder();   // helper that builds a valid in-stock order

        await engine.RunAsync(ctx);

        Assert.Empty(ctx.Errors);
        Assert.Equal(WorkflowStatus.Completed, ctx.Status);
        Assert.Equal(
            new[] { "ValidateOrder", "CheckInventory", "ChargePayment", "SendConfirmation" },
            ctx.ExecutedSteps);
        Assert.Single(email.Sent);
        Assert.Contains("Confirmed", email.Sent[0].Subject);
    }
}

internal sealed class FakeEmailService : IEmailService
{
    public List<EmailMessage> Sent { get; } = new();

    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        Sent.Add(message);
        return Task.CompletedTask;
    }
}
```

## Adding a new step

1. Create a class under `Orders/Steps/` that implements `IWorkflowStep<OrderContext>` and sets `Name`.
2. Register it in DI (`services.AddTransient<MyNewStep>()`).
3. Add it as a constructor parameter on `OrderWorkflowFactory` and wire it into `Build()` via `AddStep` and (if needed) `AddRoute`.
4. If the step needs new state (flags, IDs), add properties to `OrderContext`.

## Notes

- The console host targets `net9.0` and takes a `FrameworkReference` on `Microsoft.AspNetCore.App` purely for `Microsoft.Extensions.DependencyInjection`. It's a console app, not a web app.
- `WorkflowContext` also defines `WorkflowRun` and `WorkflowStepLog` DTOs that look like a persistence seam — nothing currently reads or writes them.
