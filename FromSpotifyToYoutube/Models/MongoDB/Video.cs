using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.MongoDB
{
    [BsonNoId]
    [BsonIgnoreExtraElements]
    public class Video
    {
        public string? Id { get; set; }

        [BsonElement("VideoName")]
        public string? VideoName { get; set; }
        [BsonElement("VideoId")]
        public string? VideoId { get; set; }
    }
}
