# MongoDB.Extensions.Migration

MongoDB.Extensions.Migration is a library which supports writing migrations for MongoDB in c#.
Simple changes in the data model can often be accommodated by using data annotations from the MongoDB c# driver like `[BsonDefaultValue(...)]` but for more complicated changes the ability to write migrations is helpful.

## Concept

Traditionally in relational databases, migration have been applied to all documents at once and caused downtime.
With document databases which have a schema on read not on write we can do better.
Introducing a version field on each document and having migration logic which performs the needed migrations when reading a document allows for a downtime free migrations.
This pattern allows for multiple versions of an application to use the same database, for example in the case of a rolling update.

## Api

If used in combination with ASP.NET Core register your migrations with the `UseMongoMigration` extension method on the `IApplicationBuilder`.
Either in Program.cs or in the Configure method of Startup.cs.

```csharp
...
var app = builder.Build();

app.UseMongoMigration(m => m
    .ForEntity<Customer>(e => e
        .AtVersion(1)
        .WithMigration(new ExampleMigration())));


public record Customer : IVersioned {...}
public class ExampleMigration : IMigration {...}
```

Entities for which a migration is needed must implement IVersioned and Migrations must implement IMigration.
The optional method AtVersion allows setting the version of the data the application currently supports.
This allows for scenarios where a migration down to a specific version is needed.

### Writing Migrations

Each migration has a version.
The versions of all registered migrations must be continuously incrementing without a gap.
We recommend to never change a already deployed migration.

The up and down method of a migration act directly on the BsonDocument and allows making the needed changes to get from one version to another.
This example shows how a typo in a field name is fixed.

```csharp
public class ExampleMigration : IMigration
{
    public int Version => 1;

    public void Up(BsonDocument document)
    {
        document["Name"] = document["Namee"];
    }

    public void Down(BsonDocument document)
    {
        document["Namee"] = document["Name"];
    }
}
```
