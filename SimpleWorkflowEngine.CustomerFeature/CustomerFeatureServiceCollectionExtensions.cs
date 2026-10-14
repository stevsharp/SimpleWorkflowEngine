using Microsoft.Extensions.DependencyInjection;

using SimpleWorkflowEngine.CustomerFeature.Factory;
using SimpleWorkflowEngine.CustomerFeature.Steps;

namespace SimpleWorkflowEngine.CustomerFeature;

public static class CustomerFeatureServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerFeature(this IServiceCollection services)
    {
        services.AddTransient<CheckExistingStep>();
        services.AddTransient<WelcomeStep>();
        services.AddTransient<VipWelcomeStep>();
        services.AddTransient<ProvisionAccountStep>();
        services.AddTransient<SkipOnboardingStep>();
        services.AddTransient<FinishStep>();
        services.AddTransient<CustomerFeatureWorkflowFactory>();
        return services;
    }
}
