using FromSpotifyToYoutube.Interfaces.MongoDB;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.MongoDB
{
    public class VideosContext : IVideosContext
    {
        private readonly IMongoDatabase _db;
        private readonly IConfiguration _configuration;

        public VideosContext(IConfiguration configuration)
        {
            _configuration = configuration.GetSection("MongoDB");

            var mongoClient = new MongoClient(_configuration["ConnectionString"]);

            _db = mongoClient.GetDatabase(_configuration["DatabaseName"]);
        }
        public IMongoCollection<Video> Videos => _db.GetCollection<Video>("Videos");
    }
}
