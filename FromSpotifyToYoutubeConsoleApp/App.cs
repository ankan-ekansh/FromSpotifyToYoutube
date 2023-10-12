using Microsoft.Extensions.Logging;
using FromSpotifyToYoutube.Interfaces.FromSpotifyToYoutube;

namespace FromSpotifyToYoutubeConsoleApp;

public class App
{
    private readonly ILogger<App> _logger;
    private readonly IFromSpotifyToYoutube _fromSpotifyToYoutube;

    public App(ILogger<App> logger, IFromSpotifyToYoutube fromSpotifyToYoutube) 
    {
        _logger = logger;
        _fromSpotifyToYoutube = fromSpotifyToYoutube;
    }

    public async Task Run(string[] args)
    {
        _logger.LogInformation("App is running");

        string? input;

        Console.WriteLine("Enter inputs. To exit, press enter");

        input = Console.ReadLine();
        if (input != null && input != string.Empty)
        {
            Console.WriteLine($"Entered input was: {input}");
            await _fromSpotifyToYoutube.ConvertSpotifyToYoutube(input);
        }
        else
        {
            Console.WriteLine($"Invalid input");
        }
    }
}
