using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Squadron;

namespace SimpleBlog.Api
{
    public class Program
    {
        private static MongoReplicaSetResource _mongoResource;

        public static async Task Main(string[] args)
        {
            _mongoResource = new MongoReplicaSetResource();

            await _mongoResource.InitializeAsync();
            
            CreateHostBuilder(args).Build().Run();

            await _mongoResource.DisposeAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            string mongoConnectionString = _mongoResource.ConnectionString;
            string mongoDatabaseName = _mongoResource
                .CreateDatabase()
                .DatabaseNamespace
                .DatabaseName;

            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(configure =>
                {
                    configure.AddConsole();
                })
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile("appsettings.json");
                    builder.AddEnvironmentVariables();
                    builder.AddInMemoryCollection(new Dictionary<string, string>()
                    { 
                        { "SimpleBlog:Database:ConnectionString", mongoConnectionString },
                        {"SimpleBlog:Database:DatabaseName", mongoDatabaseName } 
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
            
    }
}
