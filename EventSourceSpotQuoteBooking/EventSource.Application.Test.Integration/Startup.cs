using Microsoft.Extensions.DependencyInjection;

namespace EventSource.Application.Integration.Test;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var startup = new EventSource.Test.Utilities.Startup();
        startup.ConfigureServices(services);
    }
}
