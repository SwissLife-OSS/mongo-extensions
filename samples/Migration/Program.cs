using Migration;
using MongoDB.Extensions.Migration;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton(_ => new MongoClient("mongodb://localhost:27017"))
    .AddTransient<Repository>();

var app = builder.Build();

app.UseMongoMigration(m => m.ForEntity<Customer>(e => e
    .AtVersion(1)
    .WithMigration(new ExampleMigration())));

app.MapGet("/customer/{id}", (string id, Repository repo) => repo.GetAsync(id));
app.MapPost("/customer/", (Customer customer, Repository repo) => repo.AddAsync(customer));

app.Run();
