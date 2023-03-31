using MongoDB.Extensions.Context;
using SwissLife.MongoDB.Extensions.Outbox.Core;
using SwissLife.MongoDB.Extensions.Outbox.Core.Pipeline;
using SwissLife.MongoDB.Extensions.Outbox.Persistence;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OutboxServiceCollectionExtensions
    {
        public static OutboxBuilder AddMongoTransactionalOutbox(
            this IServiceCollection services,
            OutboxOptions<DatabaseOptions> options)
        {
            OutboxBuilder builder = services.AddTransactionalOutbox(options);

            services
                .AddSingleton(options)
                .AddSingleton<ILockProvider, MongoDBLockProvider> ()
                .AddSingleton<IOutboxDbContext, OutboxDbContext>(
                    ctx =>
                    {
                        MongoOptions mongoOptions =
                            new MongoOptions
                            {
                                ConnectionString = options.Persistence.ConnectionString,
                                DatabaseName = options.Persistence.DatabaseName
                            };

                        var context = new OutboxDbContext(mongoOptions, options.Message);
                        context.Initialize();

                        return context;
                    })
                .AddSingleton<IOutboxRepository, OutboxRepository>()
                .AddSingleton<ISessionProvider, MongoSessionProvider>();

            return builder;
        }
    }
}
