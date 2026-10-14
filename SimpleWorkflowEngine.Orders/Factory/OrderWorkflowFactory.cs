using SimpleWorkflowEngine.Orders;
using SimpleWorkflowEngine.Orders.Steps;

namespace SimpleWorkflowEngine.Factory;

/// <summary>
/// 
/// </summary>
/// <param name="validate"></param>
/// <param name="checkInventory"></param>
/// <param name="chargePayment"></param>
/// <param name="sendConfirmation"></param>
/// <param name="notifyBackorder"></param>
/// <param name="handleDeclinedPayment"></param>
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
