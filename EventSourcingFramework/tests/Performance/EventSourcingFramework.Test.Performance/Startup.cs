using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Test.Performance;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var startup = new Utilities.Startup();
        startup.ConfigureServices(services);
    }
}