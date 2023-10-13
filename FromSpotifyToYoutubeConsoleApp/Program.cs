using FromSpotifyToYoutube.Interfaces.FromSpotifyToYoutube;
using FromSpotifyToYoutube.Interfaces.MongoDB;
using FromSpotifyToYoutube.Interfaces.Spotify;
using FromSpotifyToYoutube.Interfaces.Youtube;
using FromSpotifyToYoutube.Models.MongoDB;
using FromSpotifyToYoutube.Repositories.MongoDB;
using FromSpotifyToYoutube.Services;
using FromSpotifyToYoutube.Services.Spotify;
using FromSpotifyToYoutube.Services.Youtube;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FromSpotifyToYoutubeConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            using IHost host = CreateHostBuilder(args, configuration).Build();
            using var scope = host.Services.CreateScope();

            var services = scope.ServiceProvider;

            try
            {
                await services.GetRequiredService<App>().Run(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static IHostBuilder CreateHostBuilder(string[] args, IConfiguration config)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<App>();
                    services.RegisterServices(config);
                })
                .ConfigureLogging(opt =>
                {
                    opt.AddConfiguration(config.GetSection("Logging"));
                    opt.AddConsole();
                })
                .ConfigureHostConfiguration((options) =>
                {
                    options.AddConfiguration(config.GetSection("Spotify"));
                    options.AddConfiguration(config.GetSection("YoutubeOperationalApi"));
                    options.AddConfiguration(config.GetSection("MongoDB"));
                    options.AddConfiguration(config.GetSection("Youtube"));
                });
        }
    }
}