using System.Net.Mail;

namespace SimpleWorkflowEngine.Orders.Steps;

public class ChargePaymentStep : IWorkflowStep<OrderContext>
{
    public string Name => "ChargePayment";

    public async Task<StepResult> ExecuteAsync(OrderContext ctx, CancellationToken ct = default)
    {
        await Task.Delay(80, ct); // simulate payment gateway call

        // Simulate decline for amounts over 9999
        ctx.PaymentDeclined = ctx.Order.TotalAmount > 9999;

        if (!ctx.PaymentDeclined)
            ctx.PaymentId = $"PAY-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        return StepResult.Success();
    }
}
