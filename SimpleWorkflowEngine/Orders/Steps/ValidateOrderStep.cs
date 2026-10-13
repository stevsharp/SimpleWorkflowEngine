using System.Net.Mail;

namespace SimpleWorkflowEngine.Orders.Steps;

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

