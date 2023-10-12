using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.Youtube
{
    public class YoutubePlaylistItem
    {
        [JsonPropertyName("snippet")]
        public YoutubePlaylistItemSnippet? Snippet { get; set; }
    }
}
