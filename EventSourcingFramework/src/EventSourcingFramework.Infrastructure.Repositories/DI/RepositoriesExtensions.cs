using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Repositories.Interfaces;
using EventSourcingFramework.Infrastructure.Repositories.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Infrastructure.Repositories.DI;

public static class RepositoriesExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton(typeof(Repository<>))
            .AddScoped<ITransactionManager, TransactionManager>()
            .AddScoped(typeof(IRepository<>), typeof(SmartRepository<>));

        return services;
    }
}