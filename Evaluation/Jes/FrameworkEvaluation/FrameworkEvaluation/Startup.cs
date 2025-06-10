using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventSourcingFramework.Infrastructure.DI;
using Castle.Core.Resource;
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
            (typeof(Customer), "Customer"),
            (typeof(SpotQuote), "SpotQuote")
            );

            schema.Register(typeof(Customer), 2);

            migrations.Register<Customer>(1, typeof(Customerv1));
            migrations.Register<Customer>(2, typeof(Customer));

            migrator.Register<Customerv1, Customer>(1, v1 =>
            new Customer(v1.FirstName)
            { Id = v1.Id }
            );
        });
    }
}