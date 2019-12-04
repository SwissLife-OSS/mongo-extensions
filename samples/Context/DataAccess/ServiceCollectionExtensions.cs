using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Extensions.Context;

namespace DataAccess
{
    public static class ServiceCollectionExtensions
    {
        private static IServiceCollection AddShopDatabase(
            this IServiceCollection services, IConfiguration configuration)
        {
            MongoOptions shopDbOptions = configuration
                .GetSection("Shop:Database")
                .Get<MongoOptions>();

            services.AddSingleton<MongoOptions>(sp => shopDbOptions);
            services.AddSingleton<ShopDbContext>();
            services.AddSingleton<ProductRepository>();

            return services;
        }
    }
}
