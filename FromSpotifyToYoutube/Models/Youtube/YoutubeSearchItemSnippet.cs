using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.Youtube
{
    public class YoutubeSearchItemSnippet
    {
        [JsonPropertyName("channelId")]
        public string? ChannelId { get; set; }
        [JsonPropertyName("channelTitle")]
        public string? ChannelTitle { get; set; }
        [JsonPropertyName("channelApproval")]
        public string? ChannelApproval { get; set; }
    }
}
