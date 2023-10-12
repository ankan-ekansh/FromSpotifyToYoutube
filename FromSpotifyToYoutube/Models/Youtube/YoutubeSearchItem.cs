using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.Youtube
{
    public class YoutubeSearchItem
    {
        [JsonPropertyName("snippet")]
        public YoutubeSearchItemSnippet? Snippet { get; set; }
        [JsonPropertyName("id")]
        public YoutubeResource? Id { get; set; }
    }
}
