using FromSpotifyToYoutube.Models.MongoDB;
using MongoDB.Driver;

namespace FromSpotifyToYoutube.Interfaces.MongoDB
{
    public interface IChannelsContext
    {
        IMongoCollection<Channel> Channels { get; }
    }
}