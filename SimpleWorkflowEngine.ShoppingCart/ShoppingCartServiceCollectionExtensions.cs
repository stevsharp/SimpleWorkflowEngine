using Microsoft.Extensions.DependencyInjection;

using SimpleWorkflowEngine.ShoppingCart.Factory;
using SimpleWorkflowEngine.ShoppingCart.Steps;

namespace SimpleWorkflowEngine.ShoppingCart;

public static class ShoppingCartServiceCollectionExtensions
{
    public static IServiceCollection AddShoppingCart(this IServiceCollection services)
    {
        services.AddTransient<ValidateCartStep>();
        services.AddTransient<CheckInventoryStep>();
        services.AddTransient<ReviewOrderStep>();
        services.AddTransient<VerifyFundsStep>();
        services.AddTransient<ChargePaymentStep>();
        services.AddTransient<SendReceiptStep>();
        services.AddTransient<HandleDeclinedPaymentStep>();
        services.AddTransient<NotifyOutOfStockStep>();

        services.AddTransient<CartWorkflowFactory>();
        return services;
    }
}
