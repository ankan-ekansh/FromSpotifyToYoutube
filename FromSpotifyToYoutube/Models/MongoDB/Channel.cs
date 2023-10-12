using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.MongoDB
{
    [BsonNoId]
    [BsonIgnoreExtraElements]
    public class Channel
    {
        public string? Id { get; set; }

        [BsonElement("ChannelName")]
        public string? ChannelName { get; set; }
        [BsonElement("ChannelId")]
        public string? ChannelId { get; set; }
    }
}
