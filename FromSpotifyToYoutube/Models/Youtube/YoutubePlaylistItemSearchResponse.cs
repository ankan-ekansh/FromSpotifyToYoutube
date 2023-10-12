using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.Youtube
{
    public class YoutubePlaylistItemSearchResponse
    {
        [JsonPropertyName("nextPageToken")]
        public string? NextPageToken { get; set; }
        [JsonPropertyName("items")]
        public List<YoutubePlaylistItem>? Items { get; set; }
    }
}
