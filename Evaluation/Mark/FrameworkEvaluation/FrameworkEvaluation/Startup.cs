using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventSourcingFramework.Infrastructure.DI;
using Castle.Core.Configuration;


namespace FrameworkEvaluation;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", false, true)
          .Build();

        services.AddEventSourcing(configuration, (schema, migrations, migrator, mongoDbRegistrationService) =>
        {
            mongoDbRegistrationService.Register(
            (typeof(Customer), "Customers"),
            (typeof(SpotQuote), "SpotQuotes")
            );
            // Register migrations
        });
    }
}