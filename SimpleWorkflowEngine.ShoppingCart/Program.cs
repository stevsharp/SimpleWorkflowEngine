using Microsoft.Extensions.DependencyInjection;

using SimpleWorkflowEngine;
using SimpleWorkflowEngine.ShoppingCart;
using SimpleWorkflowEngine.ShoppingCart.Factory;

var services = new ServiceCollection();

services
    .AddEmail()
    .AddShoppingCart();

await using var sp = services.BuildServiceProvider();

var engine = sp.GetRequiredService<CartWorkflowFactory>().Build();

engine.OnStepStarted += name => Console.WriteLine($"  -> {name}");
engine.OnStepCompleted += (name, result, ms) =>
{
    Console.WriteLine($"  <- {name} ({ms} ms) success={result.IsSuccess}");
    return Task.CompletedTask;
};

// Each scenario below exercises a distinct branch of the cart checkout graph:
//   ValidateCart
//     └─ CheckInventory
//          ├─ IsOutOfStock       -> NotifyOutOfStock
//          └─ otherwise          -> ReviewOrder -> VerifyFunds
//                                                   ├─ InsufficientFunds -> HandleDeclinedPayment
//                                                   └─ otherwise          -> ChargePayment
//                                                                              ├─ PaymentDeclined -> HandleDeclinedPayment
//                                                                              └─ otherwise       -> SendReceipt
var scenarios = new (string Label, Func<CartContext> Build)[]
{
    ("Happy path (in stock, funds ok, payment ok)", () => new CartContext
    {
        UserAccountBalance = 200m,
        Cart = new Cart
        {
            Id = "CART-1001",
            CustomerId = "CUST-1",
            CustomerEmail = "customer@example.com",
            CustomerName = "Jane Doe",
            Total = 149.99m,
            Items =
            [
                new() { ProductId = "P-1", Quantity = 1, UnitPrice = 149.99m, InStock = true },
            ],
        },
    }),

    ("Out of stock (one item unavailable)", () => new CartContext
    {
        UserAccountBalance = 500m,
        Cart = new Cart
        {
            Id = "CART-1002",
            CustomerId = "CUST-2",
            CustomerEmail = "backorder@example.com",
            CustomerName = "Mark Twain",
            Total = 300m,
            Items =
            [
                new() { ProductId = "P-1", Quantity = 1, UnitPrice = 100m, InStock = true },
                new() { ProductId = "P-2", Quantity = 1, UnitPrice = 200m, InStock = false },
            ],
        },
    }),

    ("Insufficient funds (balance < total)", () => new CartContext
    {
        UserAccountBalance = 50m,
        Cart = new Cart
        {
            Id = "CART-1003",
            CustomerId = "CUST-3",
            CustomerEmail = "broke@example.com",
            CustomerName = "Low Balance",
            Total = 149.99m,
            Items =
            [
                new() { ProductId = "P-3", Quantity = 1, UnitPrice = 149.99m, InStock = true },
            ],
        },
    }),

    ("Payment declined (amount > 9999)", () => new CartContext
    {
        UserAccountBalance = 20000m,
        Cart = new Cart
        {
            Id = "CART-1004",
            CustomerId = "CUST-4",
            CustomerEmail = "bigspender@example.com",
            CustomerName = "Big Spender",
            Total = 15000m,
            Items =
            [
                new() { ProductId = "P-4", Quantity = 1, UnitPrice = 15000m, InStock = true },
            ],
        },
    }),

    ("Validation failure: no items", () => new CartContext
    {
        UserAccountBalance = 100m,
        Cart = new Cart
        {
            Id = "CART-1005",
            CustomerId = "CUST-5",
            CustomerEmail = "empty@example.com",
            CustomerName = "Empty Cart",
            Total = 50m,
        },
    }),

    ("Validation failure: missing email", () => new CartContext
    {
        UserAccountBalance = 100m,
        Cart = new Cart
        {
            Id = "CART-1006",
            CustomerId = "CUST-6",
            CustomerEmail = "",
            CustomerName = "Anonymous",
            Total = 99m,
            Items =
            [
                new() { ProductId = "P-5", Quantity = 1, UnitPrice = 99m, InStock = true },
            ],
        },
    }),
};

while (true)
{
    Console.WriteLine();
    Console.WriteLine("========= Shopping Cart Checkout Simulation =========");
    for (var i = 0; i < scenarios.Length; i++)
        Console.WriteLine($"  {i + 1}. {scenarios[i].Label}");
    Console.WriteLine("  A. Run all scenarios");
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

static async Task RunScenario(WorkflowEngine<CartContext> engine, string label, CartContext context)
{
    Console.WriteLine();
    Console.WriteLine($"=== {label} ===");

    await engine.RunAsync(context);

    Console.WriteLine($"  Status: {context.Status}");
    Console.WriteLine($"  Path: {string.Join(" -> ", context.ExecutedSteps)}");
    if (context.Errors.Count > 0)
        Console.WriteLine($"  Errors: {string.Join("; ", context.Errors)}");
}
