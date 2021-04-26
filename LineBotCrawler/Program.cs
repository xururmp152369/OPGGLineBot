using LinBotDBClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;

namespace LineBotCrawler
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                //將 appsettings.json 加入 Configuration
                config.AddJsonFile("appsettings.json", optional: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

                // Replace 'YourDbContext' with the name of your own DbContext derived class.
                services.AddDbContextPool<CoreDbContext>(
                    dbContextOptions => dbContextOptions
                        .UseMySql(hostContext.Configuration.GetConnectionString("SQLConnectionString"), serverVersion, options =>{
                            options.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null);})
                        .EnableSensitiveDataLogging() // These two calls are optional but help
                        .EnableDetailedErrors()       // with debugging (remove for production).
                        
                );
                services.AddHostedService<Worker>();
            });
    }
}
