using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CoinBinanceApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Enable Serilog
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger(); 

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var ConfigSettings = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", optional: false)
                .Build();

            Log.Logger = new LoggerConfiguration()
                //.ReadFrom.Configuration(ConfigSettings)
                .Enrich.FromLogContext()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)                
                .CreateLogger();

            Log.Information("test");

            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.AddConfiguration(ConfigSettings);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.CaptureStartupErrors(true);
                    webBuilder.UseStartup<Startup>();
                });
        }
            //Host.CreateDefaultBuilder(args)
            //.ConfigureWebHostDefaults(webBuilder =>
            //{
            //    webBuilder.UseStartup<Startup>();
            //});
    }
}
