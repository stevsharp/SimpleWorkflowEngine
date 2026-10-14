using SimpleWorkflowEngine.Orders;
using SimpleWorkflowEngine.Orders.Steps;

namespace SimpleWorkflowEngine.Factory;

/// <summary>
/// Builds the order-processing <see cref="WorkflowEngine{TContext}"/>.
/// Steps are resolved from DI and composed into the graph below:
/// <code>
/// ValidateOrder -> CheckInventory
///                    ├─ IsBackorder    -> NotifyBackorder
///                    └─ otherwise      -> ChargePayment
///                                           ├─ PaymentDeclined -> HandleDeclinedPayment
///                                           └─ otherwise       -> SendConfirmation
/// </code>
/// A validation failure in <c>ValidateOrder</c> terminates the run with <see cref="WorkflowStatus.Failed"/>.
/// </summary>
/// <param name="validate">Entry step; rejects empty/zero-amount/missing-email orders.</param>
/// <param name="checkInventory">Sets <c>IsBackorder</c> based on item stock availability.</param>
/// <param name="chargePayment">Attempts to charge the order total; sets <c>PaymentDeclined</c>.</param>
/// <param name="sendConfirmation">Happy-path terminal step for successful orders.</param>
/// <param name="notifyBackorder">Terminal step when any item is out of stock.</param>
/// <param name="handleDeclinedPayment">Terminal step when payment is declined.</param>
public sealed class OrderWorkflowFactory(
    ValidateOrderStep validate,
    CheckInventoryStep checkInventory,
    ChargePaymentStep chargePayment,
    SendConfirmationStep sendConfirmation,
    NotifyBackorderStep notifyBackorder,
    HandleDeclinedPaymentStep handleDeclinedPayment)
{
    private readonly ValidateOrderStep _validate = validate;
    private readonly CheckInventoryStep _checkInventory = checkInventory;
    private readonly ChargePaymentStep _chargePayment = chargePayment;
    private readonly SendConfirmationStep _sendConfirmation = sendConfirmation;
    private readonly NotifyBackorderStep _notifyBackorder = notifyBackorder;
    private readonly HandleDeclinedPaymentStep _handleDeclinedPayment = handleDeclinedPayment;

    /// <summary>
    /// Composes a new <see cref="WorkflowEngine{OrderContext}"/> wired with the order steps and routes.
    /// Call once per host; the returned engine is reusable across <c>RunAsync</c> invocations.
    /// </summary>
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
