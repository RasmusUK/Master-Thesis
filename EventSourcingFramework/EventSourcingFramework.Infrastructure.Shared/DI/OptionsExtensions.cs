using EventSourcing.Framework.Infrastructure.Shared.Configuration.Options;
using EventSourcingFramework.Application.Abstractions.EventSourcingSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EventSourcing.Framework.Infrastructure.Shared.DI;

public static class OptionsExtensions
{
    public static IServiceCollection AddFrameworkOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .Configure<MongoDbOptions>(configuration.GetSection(MongoDbOptions.MongoDb))
            .Configure<EventSourcingOptions>(configuration.GetSection(EventSourcingOptions.EventSourcing))
            .AddSingleton<IEventSourcingSettings>(sp =>
                sp.GetRequiredService<IOptions<EventSourcingOptions>>().Value);
    }
}