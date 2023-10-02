using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Extensions.Context;

namespace MongoDB.Extensions.Session;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoSessionProvider<TContext>(
        this IServiceCollection services)
        where TContext : class, IMongoDbContext
    {
        services.TryAddSingleton<TContext>();
        services.TryAddSingleton<ISessionProvider<TContext>, MongoSessionProvider<TContext>>();

        return services;
    }
}
