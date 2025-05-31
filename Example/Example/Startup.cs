using EventSourcingFramework.Infrastructure.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Example;

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
                (typeof(Order), "Orders")
            );
            
            schema.Register(typeof(Customer), 2);
            
            migrations.Register<Customer>(1, typeof(CustomerV1));
            migrations.Register<Customer>(2, typeof(Customer));
            
            migrator.Register<CustomerV1, Customer>(1, v1 => new Customer
            {
                Id = v1.Id,
                Name = $"{v1.FirstName} {v1.LastName}"
            });
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
    }
}