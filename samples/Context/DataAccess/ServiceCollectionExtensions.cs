using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    public static class ServiceCollectionExtensions
    {
        private static IServiceCollection AddShopDatabase(
            this IServiceCollection services, IConfiguration configuration)
        {
            //MongoOptions dbOptions = configuration
            //    .GetSection(Wellknown.Configuration.Sections.Database)
            //    .Get<MongoOptions<IDocuStoreDbContext>>()

            return services;
        }
    }
}
