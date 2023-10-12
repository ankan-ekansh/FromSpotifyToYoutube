using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.Spotify
{
    public class SpotifyPlaylistTrackListItem
    {
        [JsonPropertyName("track")]
        public SpotifyPlaylistTrack? Track { get; set; }
    }
}
