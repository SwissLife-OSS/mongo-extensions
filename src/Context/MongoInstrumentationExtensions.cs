using System;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace MongoDB.Extensions.Context;

public static class MongoInstrumentationExtensions
{
    public static IMongoDatabaseBuilder AddInstrumentation(
        this IMongoDatabaseBuilder mongoDatabaseBuilder,
        Action<InstrumentationOptions>? configureInstrumentation = default,
        Action<ClusterBuilder>? configureCluster = default)
    {
        return mongoDatabaseBuilder
            .ConfigureConnection(s => s
                .AddInstrumentation(configureInstrumentation, configureCluster));
    }

    public static MongoClientSettings AddInstrumentation(
        this MongoClientSettings mongoClientSettings,
        Action<InstrumentationOptions>? configureInstrumentation = default,
        Action<ClusterBuilder>? configureCluster = default)
    {
        var instrumentationOptions = new InstrumentationOptions { CaptureCommandText = true };
        configureInstrumentation?.Invoke(instrumentationOptions);

        mongoClientSettings.ClusterConfigurator = builder =>
        {
            builder.Subscribe(new DiagnosticsActivityEventSubscriber(instrumentationOptions));
            configureCluster?.Invoke(builder);
        };

        return mongoClientSettings;
    }
}
