using EventSourcingFramework.Application.Abstractions.EntityHistory;
using EventSourcingFramework.Application.Abstractions.PersonalData;
using EventSourcingFramework.Application.Abstractions.Replay;
using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Application.UseCases.EntityHistory;
using EventSourcingFramework.Application.UseCases.PersonalData;
using EventSourcingFramework.Application.UseCases.Replay;
using EventSourcingFramework.Application.UseCases.ReplayContext;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Application.DI;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplications(this IServiceCollection services)
    {
        services
            .AddSingleton<IPersonalDataService, PersonalDataService>()
            .AddSingleton<IEntityHistoryService, EntityHistoryService>()
            .AddSingleton<IReplayService, ReplayService>()
            .AddSingleton<IGlobalReplayContext, GlobalReplayContext>();

        return services;
    }
}