using FromSpotifyToYoutube.Models.Spotify;

namespace FromSpotifyToYoutube.Interfaces.Spotify
{
    public interface IGetSpotifyPlaylistService
    {
        Task<SpotifyPlaylist> GetPlaylist(string accessToken, string playlistUrl);
    }
}