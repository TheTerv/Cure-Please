namespace CurePlease
{
    using CurePlease.Engine;
    using CurePlease.Infrastructure;
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
            var engineManager = ActivatorUtilities.CreateInstance<EngineManager>(host.Services);
            var processManager = ActivatorUtilities.CreateInstance<ProcessManager>(host.Services);


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(processManager, engineManager));
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
                            services.AddSingleton<IProcessManager, ProcessManager>();
                            services.AddSingleton<IProcessUtilities, ProcessUtilities>();
                            services.AddSingleton<IEngineManager, EngineManager>();
                            services.AddScoped<IGeoEngine, GeoEngine>();
                            services.AddSingleton<IFollowEngine, FollowEngine>();
                            services.AddScoped<IBuffEngine, BuffEngine>();
                            services.AddScoped<ICureEngine, CureEngine>();
                            services.AddScoped<IDebuffEngine, DebuffEngine>();
                            services.AddScoped<IPLEngine, PLEngine>();
                            services.AddScoped<ISongEngine, SongEngine>();
                            services.AddScoped<IAddonEngine, AddonEngine>();
                        })
                        .UseSerilog()
                        .Build();

            return host;
        }
    }
}
