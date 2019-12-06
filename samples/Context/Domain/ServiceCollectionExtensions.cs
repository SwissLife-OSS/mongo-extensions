using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Extensions.Context;

namespace SimpleBlog.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlogDomain(
            this IServiceCollection services)
        {
            services.AddSingleton<IBlogService, BlogService>();
            services.AddSingleton<IUserService, UserService>();

            return services;
        }
    }
}
