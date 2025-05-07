using Microsoft.Extensions.DependencyInjection;

namespace EventSource.Test.Performance;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var startup = new EventSource.Test.Utilities.Startup();
        startup.ConfigureServices(services);
    }
}
