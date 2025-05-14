using EventSourcingFramework.Application.Abstractions.ApiGateway;
using EventSourcingFramework.Infrastructure.Http.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Infrastructure.Http.DI;

public static class HttpExtensions
{
    public static IServiceCollection AddHttp(this IServiceCollection services)
    {
        services.AddHttpClient<IApiGateway, ApiGateway>();

        return services;
    }
}