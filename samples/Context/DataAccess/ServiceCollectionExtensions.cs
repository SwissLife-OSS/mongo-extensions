using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Extensions.Context;

namespace DataAccess
{
    public class ServiceCollectionExtensions
    {
        private static IServiceCollection AddShopDatabase(
            this IServiceCollection services, IConfiguration configuration)
        {
            MongoOptions dbOptions = configuration
                .GetSection(Wellknown.Configuration.Sections.Database)
                .Get<MongoOptions<IDocuStoreDbContext>>()

            return services;
        }
    }
}
