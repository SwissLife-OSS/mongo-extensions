using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDbContext<TContext, TInterface>(
        this IServiceCollection services,
        string configurationSection = "MongoDb",
        string? connectionStringName = null)
        where TContext : class, TInterface
        where TInterface : class, IMongoDbContext
    {
        var optionsBuilder = services
            .AddOptions<MongoOptions<TInterface>>()
            .BindConfiguration(configurationSection);

        if (connectionStringName is not null)
        {
            optionsBuilder.Configure<IConfiguration>((options, config) =>
            {
                var connectionString = config.GetConnectionString(connectionStringName);
                if (string.IsNullOrEmpty(connectionString))
                {
                    return;
                }

                options.ConnectionString = connectionString;

                if (string.IsNullOrEmpty(options.DatabaseName))
                {
                    var mongoUrl = new MongoUrl(connectionString);
                    if (!string.IsNullOrEmpty(mongoUrl.DatabaseName))
                    {
                        options.DatabaseName = mongoUrl.DatabaseName;
                    }
                }
            });
        }

        services.TryAddSingleton<TInterface, TContext>();

        return services;
    }
}
