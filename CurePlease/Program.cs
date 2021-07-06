namespace CurePlease
{
    using CurePlease.Engine;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using System;
    using System.IO;
    using System.Windows.Forms;

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application. 
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var host = AppStartup();
            var geoEngine = ActivatorUtilities.CreateInstance<GeoEngine>(host.Services);
            var followEngine = ActivatorUtilities.CreateInstance<FollowEngine>(host.Services);


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(geoEngine, followEngine));
        }

        static void ConfigSetup(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .AddEnvironmentVariables();
        }

        static IHost AppStartup()
        {
            var builder = new ConfigurationBuilder();
            ConfigSetup(builder);

            Log.Logger = new LoggerConfiguration()
                                .MinimumLevel.Information()
                                .WriteTo.File("Logs/CP-.log", rollingInterval: RollingInterval.Day)
                                .CreateLogger();

            var host = Host.CreateDefaultBuilder()
                        .ConfigureServices((context, services) =>
                        {
                            // here's where we can wire up dependency injection
                            services.AddScoped<IGeoEngine, GeoEngine>();
                            services.AddSingleton<IFollowEngine, FollowEngine>();
                        })
                        .UseSerilog()
                        .Build();

            return host;
        }
    }
}
