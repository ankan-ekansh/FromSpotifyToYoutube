using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.Youtube
{
    public class YoutubePlaylistItemSnippet
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("resourceId")]
        public YoutubeResource? ResourceId { get; set; }
    }
}
