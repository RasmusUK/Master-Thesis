using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventSourcingFramework.Infrastructure.DI;

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
            mongoDbRegistrationService.Register((typeof(SpotQuote), "Spotquotes"), (typeof(Customer), "Customers"));

            // Register entities
            // Register migrations
        });
    }
}