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

        services.AddEventSourcing(configuration, (schema, migrations, migrator, mongoDbRegistrationService) =>
        {
            mongoDbRegistrationService.Register(
               (typeof(Car), "Cars"),
               (typeof(Order), "Orders")
             );

            schema.Register(typeof(Car), 2);

            migrations.Register<Car>(1, typeof(CarV1));
            migrations.Register<Car>(2, typeof(Car));

            migrator.Register<CarV1, Car>(1, v1 => new Car(Model.ParseFromString(v1.Model), v1.Year));
        });
    }
}