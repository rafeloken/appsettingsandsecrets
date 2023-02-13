using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


/*
 * 
 * Example of using appsettings and secrets in a .net Console application.
 * 
 */

namespace AppSettingsAndSecrets;
internal class Program {        
    private static IHost ConfigureHost() {
        // See https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.host.createdefaultbuilder?view=dotnet-plat-ext-7.0#microsoft-extensions-hosting-host-createdefaultbuilder
        // for more details on what is configured by calling Host.CreateDefaultBuilder.
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices((context, services) => {
            IConfiguration config = context.Configuration;
            services.Configure<AppSettings>(config.GetSection("App"));
            services.Configure<AppSecrets>(config.GetSection("AppSecrets"));

            services.AddSingleton<App>();
            // Add other services to be available via the host/DI
        });

        return builder.Build();
    }

    static async Task Main(string[] args) {

        var host = ConfigureHost();
        var logger = host.Services.GetRequiredService<ILogger<App>>()!;

        try {            
            await host.Services.GetRequiredService<App>().Run(args);
        } catch (Exception ex) {
            logger.LogDebug(ex.InnerException?.Message);
        } finally {
            logger.LogDebug("[Main] exiting program");
        }
        await Console.Out.WriteLineAsync("\nPress any key to exit\b").ContinueWith(c => Console.ReadKey()); 
    }

    protected class App {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        private readonly AppSecrets _appSecrets;

        private readonly CancellationTokenSource _token;

        public App(IServiceProvider services, IOptions<AppSettings> appSettings, IOptions<AppSecrets> appSecrets, ILogger<App> logger) {
            _services = services;
            // Note: If these weren't being DI'd in, could call something like _services.GetRequiredService<IOptions<AppSettings>>().Value
            _logger = logger;
            _appSettings = appSettings?.Value ?? throw new ArgumentNullException(nameof(appSettings));
            _appSecrets = appSecrets?.Value ?? throw new ArgumentNullException(nameof(appSecrets));
            _token = new CancellationTokenSource();
        }

        public async Task Run(string[] args) {
            string env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            await Console.Out.WriteLineAsync(env);
            await Console.Out.WriteLineAsync($"{_appSettings.Demo}");
            await Console.Out.WriteLineAsync($"{_appSettings.OnlyBase}");
            await Console.Out.WriteLineAsync($"{_appSecrets.SuperSecret}");                       

            Console.CancelKeyPress += (sender, eventArgs) => {                
                _logger.LogInformation("cancel event triggered");                
                _token.Cancel();
                eventArgs.Cancel = true;
            };
             
            await Task.Delay(-1, _token.Token).ContinueWith(task => { });
        }
    }
}