using FromSpotifyToYoutube.Interfaces.FromSpotifyToYoutube;
using FromSpotifyToYoutube.Interfaces.Spotify;
using FromSpotifyToYoutube.Interfaces.Youtube;
using FromSpotifyToYoutube.Models.Spotify;
using Microsoft.Extensions.Logging;

namespace FromSpotifyToYoutube
{
    public class FromSpotifyToYoutube : IFromSpotifyToYoutube
    {
        private readonly ILogger<FromSpotifyToYoutube> _logger;
        private readonly ISpotifyAccessTokenService _tokenService;
        private readonly IGetSpotifyPlaylistService _getSpotifyPlaylistService;
        private readonly IYoutubeClient _youtubeClient;

        public FromSpotifyToYoutube(ILogger<FromSpotifyToYoutube> logger,
            ISpotifyAccessTokenService tokenService,
            IGetSpotifyPlaylistService getSpotifyPlaylistService,
            IYoutubeClient youtubeClient)
        {
            _logger = logger;
            _tokenService = tokenService;
            _getSpotifyPlaylistService = getSpotifyPlaylistService;
            _youtubeClient = youtubeClient;
        }

        public async Task ConvertSpotifyToYoutube(string playlistUrl)
        {
            _logger.LogInformation("Converting playlist");

            string accessToken = await _tokenService.GetAccessToken();

            _logger.LogInformation("Received access token");

            _logger.LogInformation("Retrieving spotify playlist items");

            SpotifyPlaylist playlist = await _getSpotifyPlaylistService.GetPlaylist(accessToken, playlistUrl);

            _logger.LogInformation("Retrieved spotify playlist items");

            await _youtubeClient.ConvertPlaylist(playlist);

            _logger.LogInformation("Converted playlist");
        }
    }
}