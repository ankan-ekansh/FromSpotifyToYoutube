
namespace FromSpotifyToYoutube.Interfaces.Spotify
{
    public interface ISpotifyAccessTokenService
    {
        Task<string> GetAccessToken();
    }
}