using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("username");
var password = builder.AddParameter("password", secret: true);

var mongo = builder.AddMongoDB("mongo", 27019, username, password)
                   .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("mongodb");

builder.AddProject<Projects.SpotQuoteApp_Web>("web").WithReference(mongodb)
       .WaitFor(mongodb);

builder.Build().Run();
