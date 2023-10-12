using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.Spotify
{
    public class SpotifyPlaylist
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("tracks")]
        public SpotifyPlaylistTrackList? Tracks { get; set; }
    }
}
