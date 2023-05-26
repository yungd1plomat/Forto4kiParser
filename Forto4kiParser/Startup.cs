using Forto4kiParser.Abstractions;
using Forto4kiParser.Data;
using Forto4kiParser.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Forto4kiParser
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionStr = Configuration["FORTO4KI_CONNECTION"] ?? throw new InvalidOperationException("connection string not found");
            var botToken = Configuration["BOT_TOKEN"] ?? throw new InvalidOperationException("bot token not found");
            var channelId = Configuration["CHANNEL_ID"] ?? throw new InvalidOperationException("channel id not found");

            services.AddLogging();
            services.AddMemoryCache();
            services.AddDbContext<AppDb>(options => options.UseMySql(connectionStr, new MySqlServerVersion(new Version(8, 0, 27))));

            services.AddSingleton<IParserService, ParserService>();
            services.AddSingleton<ITelegramProvider, TelegramProvider>(service => new TelegramProvider(channelId));
            services.AddHostedService(service => new TelegramService(botToken, service.GetRequiredService<ITelegramProvider>()));
            services.AddHostedService<MonitorService>();
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            app.UseHsts();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallback(async context =>
                {
                    context.Response.Redirect("/");
                });
            });
        }
    }
}
