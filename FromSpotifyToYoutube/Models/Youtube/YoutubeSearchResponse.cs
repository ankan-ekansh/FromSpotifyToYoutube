﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Models.Youtube
{
    public class YoutubeSearchResponse
    {
        [JsonPropertyName("items")]
        public List<YoutubeSearchItem>? Items { get; set; }
    }
}
