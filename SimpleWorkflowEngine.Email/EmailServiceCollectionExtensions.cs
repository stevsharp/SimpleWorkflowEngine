using Microsoft.Extensions.DependencyInjection;

namespace SimpleWorkflowEngine;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmail(this IServiceCollection services)
    {
        services.AddSingleton<IEmailService, SmtpEmailService>();
        return services;
    }
}
