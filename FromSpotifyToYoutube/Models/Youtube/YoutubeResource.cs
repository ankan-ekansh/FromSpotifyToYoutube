using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.Youtube
{
    public class YoutubeResource
    {
        [JsonPropertyName("kind")]
        public string? Kind { get; set; }
        [JsonPropertyName("videoId")]
        public string? VideoId { get; set; }
    }
}
