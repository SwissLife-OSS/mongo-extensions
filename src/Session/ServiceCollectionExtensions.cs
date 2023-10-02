using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Extensions.Context;

namespace MongoDB.Extensions.Session;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoSessionProvider<TContext, TScope>(
        this IServiceCollection services)
        where TContext : class, IMongoDbContext
    {
        services.TryAddSingleton<TContext>();
        services.TryAddSingleton<ISessionProvider<TScope>, MongoSessionProvider<TContext, TScope>>();

        return services;
    }
}
