using Microsoft.Extensions.DependencyInjection;

namespace EventSource.Persistence.Test.Integration;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var startup = new EventSource.Test.Utilities.Startup();
        startup.ConfigureServices(services);
    }
}
