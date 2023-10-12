using FromSpotifyToYoutube.Models.Spotify;

namespace FromSpotifyToYoutube.Interfaces.Youtube
{
    public interface IYoutubeClient
    {
        Task ConvertPlaylist(SpotifyPlaylist spotifyPlaylist);
    }
}