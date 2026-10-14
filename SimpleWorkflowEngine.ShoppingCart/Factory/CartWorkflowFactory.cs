using SimpleWorkflowEngine.ShoppingCart.Steps;

namespace SimpleWorkflowEngine.ShoppingCart.Factory;

/// <summary>
/// Builds the shopping-cart checkout <see cref="WorkflowEngine{TContext}"/>.
/// Steps are resolved from DI and composed into the graph below:
/// <code>
/// ValidateCart -> CheckInventory
///                    ├─ IsOutOfStock       -> NotifyOutOfStock
///                    └─ otherwise          -> ReviewOrder -> VerifyFunds
///                                                              ├─ InsufficientFunds -> HandleDeclinedPayment
///                                                              └─ otherwise          -> ChargePayment
///                                                                                        ├─ PaymentDeclined -> HandleDeclinedPayment
///                                                                                        └─ otherwise       -> SendReceipt
/// </code>
/// </summary>
public sealed class CartWorkflowFactory(
    ValidateCartStep validate,
    CheckInventoryStep checkInventory,
    ReviewOrderStep reviewOrder,
    VerifyFundsStep verifyFunds,
    ChargePaymentStep chargePayment,
    SendReceiptStep sendReceipt,
    HandleDeclinedPaymentStep handleDeclinedPayment,
    NotifyOutOfStockStep notifyOutOfStock)
{
    public WorkflowEngine<CartContext> Build() =>
        new WorkflowEngine<CartContext>()
            .AddStep(validate, nextStep: checkInventory.Name)
            .AddStep(checkInventory)
                .AddRoute(checkInventory.Name,
                    when: ctx => ctx.IsOutOfStock,
                    to: notifyOutOfStock.Name)
                .AddRoute(checkInventory.Name,
                    when: ctx => !ctx.IsOutOfStock,
                    to: reviewOrder.Name)
            .AddStep(reviewOrder, nextStep: verifyFunds.Name)
            .AddStep(verifyFunds)
                .AddRoute(verifyFunds.Name,
                    when: ctx => ctx.InsufficientFunds,
                    to: handleDeclinedPayment.Name)
                .AddRoute(verifyFunds.Name,
                    when: ctx => !ctx.InsufficientFunds,
                    to: chargePayment.Name)
            .AddStep(chargePayment)
                .AddRoute(chargePayment.Name,
                    when: ctx => ctx.PaymentDeclined,
                    to: handleDeclinedPayment.Name)
                .AddRoute(chargePayment.Name,
                    when: ctx => !ctx.PaymentDeclined,
                    to: sendReceipt.Name)
            .AddStep(sendReceipt)
            .AddStep(handleDeclinedPayment)
            .AddStep(notifyOutOfStock);
}
