using Microsoft.Extensions.DependencyInjection;

using SimpleWorkflowEngine;
using SimpleWorkflowEngine.CustomerFeature;
using SimpleWorkflowEngine.CustomerFeature.Factory;
using SimpleWorkflowEngine.Factory;
using SimpleWorkflowEngine.Orders;

try
{
    var services = new ServiceCollection();

    services
        .AddEmail()
        .AddOrders()
        .AddCustomerFeature();

    await using var sp = services.BuildServiceProvider();

    var engine = sp.GetRequiredService<OrderWorkflowFactory>().Build();
    var customerEngine = sp.GetRequiredService<CustomerFeatureWorkflowFactory>().Build();

    engine.OnStepStarted += name => Console.WriteLine($"  -> {name}");
    engine.OnStepCompleted += (name, result, ms) =>
    {
        Console.WriteLine($"  <- {name} ({ms} ms) success={result.IsSuccess}");
        return Task.CompletedTask;
    };

    customerEngine.OnStepStarted += name => Console.WriteLine($"  -> {name}");
    customerEngine.OnStepCompleted += (name, result, ms) =>
    {
        Console.WriteLine($"  <- {name} ({ms} ms) success={result.IsSuccess}");
        return Task.CompletedTask;
    };

    // Each scenario below exercises a distinct branch of the CustomerFeature graph:
    //   CheckExisting
    //     ├─ AlreadyCompleted → SkipOnboarding → Finish
    //     ├─ IsVip            → VipWelcome    → ProvisionAccount → Finish
    //     └─ otherwise        → Welcome       → ProvisionAccount → Finish
    //   (empty CustomerId fails at CheckExisting and the workflow terminates.)
    var customerScenarios = new (string Label, Func<CustomerFeatureContext> Build)[]
    {
        // Standard onboarding path: CheckExisting -> Welcome -> ProvisionAccount -> Finish.
        ("New standard customer", () => new CustomerFeatureContext
        {
            CustomerId = "CUST-NEW-1",
        }),

        // VIP branch: CheckExisting -> VipWelcome -> ProvisionAccount -> Finish.
        ("VIP customer", () => new CustomerFeatureContext
        {
            CustomerId = "CUST-VIP-1",
            IsVip = true,
        }),

        // Short-circuit for already-onboarded customers: CheckExisting -> SkipOnboarding -> Finish.
        // AlreadyCompleted takes priority over IsVip in the route order.
        ("Returning (already onboarded)", () => new CustomerFeatureContext
        {
            CustomerId = "CUST-RET-1",
            AlreadyCompleted = true,
        }),

        // CheckExisting fails fast on empty CustomerId; status ends as Failed with an error.
        ("Validation failure: missing CustomerId", () => new CustomerFeatureContext
        {
            CustomerId = "",
        }),
    };

    var scenarios = new (string Label, Func<OrderContext> Build)[]
    {
        ("Happy path (in stock, payment ok)", () => new OrderContext
        {
            Order = new Order
            {
                Id = "ORD-1001",
                CustomerId = "CUST-1",
                CustomerEmail = "customer@example.com",
                CustomerName = "Jane Doe",
                TotalAmount = 150m,
                Items =
                [
                    new() { ProductId = "P-1", Quantity = 2, UnitPrice = 75m, StockAvailable = true },
                ],
            },
        }),

        ("Backorder (one item out of stock)", () => new OrderContext
        {
            Order = new Order
            {
                Id = "ORD-1002",
                CustomerId = "CUST-2",
                CustomerEmail = "backorder@example.com",
                CustomerName = "Mark Twain",
                TotalAmount = 300m,
                Items =
                [
                    new() { ProductId = "P-1", Quantity = 1, UnitPrice = 100m, StockAvailable = true },
                    new() { ProductId = "P-2", Quantity = 1, UnitPrice = 200m, StockAvailable = false },
                ],
            },
        }),

        ("Payment declined (amount > 9999)", () => new OrderContext
        {
            Order = new Order
            {
                Id = "ORD-1003",
                CustomerId = "CUST-3",
                CustomerEmail = "bigspender@example.com",
                CustomerName = "Big Spender",
                TotalAmount = 15000m,
                Items =
                [
                    new() { ProductId = "P-3", Quantity = 1, UnitPrice = 15000m, StockAvailable = true },
                ],
            },
        }),

        ("Validation failure: no items", () => new OrderContext
        {
            Order = new Order
            {
                Id = "ORD-1004",
                CustomerId = "CUST-4",
                CustomerEmail = "empty@example.com",
                CustomerName = "Empty Cart",
                TotalAmount = 50m,
            },
        }),

        ("Validation failure: zero amount", () => new OrderContext
        {
            Order = new Order
            {
                Id = "ORD-1005",
                CustomerId = "CUST-5",
                CustomerEmail = "freebie@example.com",
                CustomerName = "Free Rider",
                TotalAmount = 0m,
                Items =
                [
                    new() { ProductId = "P-4", Quantity = 1, UnitPrice = 0m, StockAvailable = true },
                ],
            },
        }),

        ("Validation failure: missing email", () => new OrderContext
        {
            Order = new Order
            {
                Id = "ORD-1006",
                CustomerId = "CUST-6",
                CustomerEmail = "",
                CustomerName = "Anonymous",
                TotalAmount = 99m,
                Items =
                [
                    new() { ProductId = "P-5", Quantity = 1, UnitPrice = 99m, StockAvailable = true },
                ],
            },
        }),
    };

    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("============ Simple Workflow Engine ============");
        for (var i = 0; i < scenarios.Length; i++)
            Console.WriteLine($"  {i + 1}. {scenarios[i].Label}");
        Console.WriteLine("  A. Run all order scenarios");
        Console.WriteLine("  C. Run all customer scenarios");
        Console.WriteLine("  Q. Quit");

        Console.Write("Select an option: ");

        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input))
            continue;

        if (input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            break;

        if (input.Equals("A", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var scenario in scenarios)
                await RunScenario(engine, scenario.Label, scenario.Build());
            continue;
        }

        if (input.Equals("C", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var scenario in customerScenarios)
                await RunCustomerScenario(customerEngine, scenario.Label, scenario.Build());
            continue;
        }

        if (int.TryParse(input, out var choice) && choice >= 1 && choice <= scenarios.Length)
        {
            var scenario = scenarios[choice - 1];
            await RunScenario(engine, scenario.Label, scenario.Build());
        }
        else
        {
            Console.WriteLine("Invalid option.");
        }
    }
}
catch (Exception)
{

    throw;
}

static async Task RunCustomerScenario(WorkflowEngine<CustomerFeatureContext> engine, string label, CustomerFeatureContext context)
{
    Console.WriteLine();
    Console.WriteLine($"=== {label} ===");

    await engine.RunAsync(context);

    Console.WriteLine($"  Status: {context.Status}");
    Console.WriteLine($"  Path: {string.Join(" -> ", context.ExecutedSteps)}");
    if (context.Errors.Count > 0)
        Console.WriteLine($"  Errors: {string.Join("; ", context.Errors)}");
}

static async Task RunScenario(WorkflowEngine<OrderContext> engine, string label, OrderContext context)
{
    Console.WriteLine();
    Console.WriteLine($"=== {label} ===");

    await engine.RunAsync(context);

    Console.WriteLine($"  Status: {context.Status}");
    Console.WriteLine($"  Path: {string.Join(" -> ", context.ExecutedSteps)}");
    if (context.Errors.Count > 0)
        Console.WriteLine($"  Errors: {string.Join("; ", context.Errors)}");
}
