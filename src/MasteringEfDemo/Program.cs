using MasteringEfDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MasteringEfDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Setup Serilog for outputting log items
            //Build Config
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            //Configure logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var connection = configuration.GetConnectionString("DefaultConnection");

            //Setup DI
            var host = new HostBuilder()
                .UseSerilog()
                .ConfigureServices(services =>
                {
                    services.AddTransient<IDemoEngine, DemoEngine>();
                    services.AddTransient<IDemoRunner, DemoRunner>();
                    services.AddDbContextPool<AdventureWorks2022Context>(options =>
                        options
                            .UseLazyLoadingProxies() //Allows for lazy loading of entites
                            .UseSqlServer(connection));
                })
                .Build();
            //var builder = Host.CreateApplicationBu
            
            using var scope = host.Services.CreateScope();

            try
            {
                Log.Logger.Information("Starting Demo Runner");
                var runner = scope.ServiceProvider.GetRequiredService<IDemoRunner>();
                runner.RunDemo();
                Log.Logger.Information("Demo Runner Exited");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Unknown error");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
