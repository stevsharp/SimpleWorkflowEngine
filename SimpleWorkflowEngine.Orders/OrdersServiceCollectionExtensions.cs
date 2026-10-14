using Microsoft.Extensions.DependencyInjection;

using SimpleWorkflowEngine.Factory;
using SimpleWorkflowEngine.Orders.Steps;

namespace SimpleWorkflowEngine.Orders;

public static class OrdersServiceCollectionExtensions
{
    public static IServiceCollection AddOrders(this IServiceCollection services)
    {
        services.AddTransient<ValidateOrderStep>();
        services.AddTransient<CheckInventoryStep>();
        services.AddTransient<ChargePaymentStep>();
        services.AddTransient<SendConfirmationStep>();
        services.AddTransient<NotifyBackorderStep>();
        services.AddTransient<HandleDeclinedPaymentStep>();

        services.AddTransient<OrderWorkflowFactory>();
        return services;
    }
}
