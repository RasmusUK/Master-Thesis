using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Test.Performance;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var startup = new EventSourcingFramework.Test.Utilities.Startup();
        startup.ConfigureServices(services);
    }
}
