using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayTR.PosSelection.Application.SeedWork.PipelineBehaviours;
using PayTR.PosSelection.Domain;
using PayTR.PosSelection.Shared.DistributedCache;
using System.Reflection;

namespace PayTR.PosSelection.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR();
        services.AddDomain();
        services.AddCaching(configuration);
        return services;
    }


    public static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Add the security behavior to the pipeline
            
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionPipelineBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehaviour<,>));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register required services

        return services;
    }
}
