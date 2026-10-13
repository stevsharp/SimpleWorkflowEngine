using SimpleWorkflowEngine.Factory;
using SimpleWorkflowEngine.Orders;
using SimpleWorkflowEngine.Orders.Steps;

namespace SimpleWorkflowEngine.Tests;

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

    private static OrderContext ValidOrder(decimal total = 150m, bool inStock = true) => new()
    {
        Order = new Order
        {
            Id = "ORD-TEST",
            CustomerId = "CUST-TEST",
            CustomerEmail = "customer@example.com",
            CustomerName = "Test Customer",
            TotalAmount = total,
            Items = new OrderItem[]
            {
                new() { ProductId = "P-1", Quantity = 1, UnitPrice = total, StockAvailable = inStock },
            },
        },
    };

    [Fact]
    public async Task HappyPath_sends_confirmation_and_completes()
    {
        var (engine, email) = BuildEngine();
        var ctx = ValidOrder();

        await engine.RunAsync(ctx);

        Assert.Empty(ctx.Errors);
        Assert.Equal(WorkflowStatus.Completed, ctx.Status);
        Assert.Equal(
            new[] { "ValidateOrder", "CheckInventory", "ChargePayment", "SendConfirmation" },
            ctx.ExecutedSteps);
        Assert.False(ctx.PaymentDeclined);
        Assert.False(ctx.IsBackorder);
        Assert.StartsWith("PAY-", ctx.PaymentId);
        Assert.Single(email.Sent);
        Assert.Contains("Confirmed", email.Sent[0].Subject);
    }

    [Fact]
    public async Task Backorder_routes_to_NotifyBackorder_and_skips_payment()
    {
        var (engine, email) = BuildEngine();
        var ctx = ValidOrder(total: 300m, inStock: false);

        await engine.RunAsync(ctx);

        Assert.Equal(WorkflowStatus.Completed, ctx.Status);
        Assert.Equal(
            new[] { "ValidateOrder", "CheckInventory", "NotifyBackorder" },
            ctx.ExecutedSteps);
        Assert.True(ctx.IsBackorder);
        Assert.Null(ctx.PaymentId);
        Assert.Single(email.Sent);
        Assert.Contains("Backorder", email.Sent[0].Subject);
    }

    [Fact]
    public async Task PaymentDeclined_routes_to_HandleDeclinedPayment_and_fails()
    {
        var (engine, email) = BuildEngine();
        var ctx = ValidOrder(total: 15000m);

        await engine.RunAsync(ctx);

        Assert.Equal(WorkflowStatus.Failed, ctx.Status);
        Assert.Equal(
            new[] { "ValidateOrder", "CheckInventory", "ChargePayment", "HandleDeclinedPayment" },
            ctx.ExecutedSteps);
        Assert.True(ctx.PaymentDeclined);
        Assert.Null(ctx.PaymentId);
        Assert.Contains("Payment was declined.", ctx.Errors);
        Assert.Single(email.Sent);
        Assert.Contains("Payment Failed", email.Sent[0].Subject);
    }

    [Fact]
    public async Task Validation_fails_when_no_items()
    {
        var (engine, email) = BuildEngine();
        var ctx = ValidOrder();
        ctx.Order = ctx.Order with { Items = Array.Empty<OrderItem>() };

        await engine.RunAsync(ctx);

        Assert.Equal(WorkflowStatus.Failed, ctx.Status);
        Assert.Equal(new[] { "ValidateOrder" }, ctx.ExecutedSteps);
        Assert.Contains("Order has no items.", ctx.Errors);
        Assert.Empty(email.Sent);
    }

    [Fact]
    public async Task Validation_fails_when_amount_is_zero()
    {
        var (engine, _) = BuildEngine();
        var ctx = ValidOrder(total: 0m);

        await engine.RunAsync(ctx);

        Assert.Equal(WorkflowStatus.Failed, ctx.Status);
        Assert.Equal(new[] { "ValidateOrder" }, ctx.ExecutedSteps);
        Assert.Contains("Invalid order amount.", ctx.Errors);
    }

    [Fact]
    public async Task Validation_fails_when_email_is_missing()
    {
        var (engine, _) = BuildEngine();
        var ctx = ValidOrder();
        ctx.Order = ctx.Order with { CustomerEmail = "" };

        await engine.RunAsync(ctx);

        Assert.Equal(WorkflowStatus.Failed, ctx.Status);
        Assert.Equal(new[] { "ValidateOrder" }, ctx.ExecutedSteps);
        Assert.Contains("Customer email is required.", ctx.Errors);
    }
}
