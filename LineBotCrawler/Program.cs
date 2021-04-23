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
                //將 CoreDbContext 註冊 DI
                services.AddDbContext<CoreDbContext>(options =>
                {
                    options.UseMySql(hostContext.Configuration.GetConnectionString("SQLConnectionString"),
                        mySqlOptions =>
                        {
                            mySqlOptions.ServerVersion(new Version(5, 7, 17), ServerType.MySql)
                            .EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                        });
                });
                services.AddHostedService<Worker>();
            });
    }
}
