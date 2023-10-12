using FromSpotifyToYoutube.Models.MongoDB;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Interfaces.MongoDB
{
    public interface IVideosContext
    {
        IMongoCollection<Video> Videos { get; }
    }
}
