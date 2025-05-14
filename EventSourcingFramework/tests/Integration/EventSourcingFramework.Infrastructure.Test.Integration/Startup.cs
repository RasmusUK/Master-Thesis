using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Infrastructure.Test.Integration;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var startup = new EventSourcingFramework.Test.Utilities.Startup();
        startup.ConfigureServices(services);
    }
}