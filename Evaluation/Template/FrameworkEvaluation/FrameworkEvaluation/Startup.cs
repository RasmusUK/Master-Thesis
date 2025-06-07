using EventSourcingFramework.Infrastructure.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrameworkEvaluation;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();
        
        
    }
}