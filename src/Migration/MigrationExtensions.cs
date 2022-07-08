using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using Microsoft.Extensions.Logging;

namespace MongoDB.Extensions.Migration;

public static class MigrationExtensions
{
    public static IApplicationBuilder UseMongoMigration(
        this IApplicationBuilder app,
        Func<MigrationOptionBuilder, MigrationOptionBuilder> builderAction)
    {
        var builder = new MigrationOptionBuilder();
        MigrationOption options = builderAction(builder).Build();

        MigrationContext context = new(
            options,
            app.ApplicationServices.GetRequiredService<ILoggerFactory>());

        BsonSerializer.RegisterSerializationProvider(new MigrationSerializerProvider(context));

        return app;
    }
}
