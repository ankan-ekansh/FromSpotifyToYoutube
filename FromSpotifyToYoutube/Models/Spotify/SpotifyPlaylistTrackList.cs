using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.Spotify
{
    public class SpotifyPlaylistTrackList
    {
        [JsonPropertyName("items")]
        public SpotifyPlaylistTrackListItem[]? PlaylistTrackListItems { get; set; }
    }
}
