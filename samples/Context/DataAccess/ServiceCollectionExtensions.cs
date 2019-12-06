using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Extensions.Context;

namespace SimpleBlog.DataAccess
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlogDatabase(
            this IServiceCollection services, IConfiguration configuration)
        {
            MongoOptions blogDbOptions = configuration
                .GetMongoOptions(WellKnown.Path.SimpleBlogDB);

            services.AddSingleton(blogDbOptions);
            services.AddSingleton<ISimpleBlogDbContext, SimpleBlogDbContext>();
            services.AddSingleton<IBlogRepository, BlogRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<ITagRepository, TagRepository>();

            return services;
        }
    }
}
