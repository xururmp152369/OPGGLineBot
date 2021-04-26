using LinBotDBClass;
using LineBotCP.Class;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;

namespace LineBotCP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

            // Replace 'YourDbContext' with the name of your own DbContext derived class.
            services.AddDbContextPool<CoreDbContext>(
                dbContextOptions => dbContextOptions
                    .UseMySql(Configuration.GetConnectionString("SQLConnectionString"), serverVersion)
                    .EnableSensitiveDataLogging() // These two calls are optional but help
                    .EnableDetailedErrors()       // with debugging (remove for production).
            );


            services.AddSingleton<LineBotConfig, LineBotConfig>((s) => new LineBotConfig
            {
                channelSecret = Configuration["LineBot:channelSecret"],
                accessToken = Configuration["LineBot:accessToken"]
            });

            services.AddHttpContextAccessor();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "api",
                    pattern: "api/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
