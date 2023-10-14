using FromSpotifyToYoutube.Interfaces.Spotify;
using FromSpotifyToYoutube.Models.Spotify;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;

namespace FromSpotifyToYoutube.Services.Spotify
{
    public class GetSpotifyPlaylistService : IGetSpotifyPlaylistService
    {
#nullable disable
        private readonly string _apiUrl;
        private readonly ILogger<GetSpotifyPlaylistService> _logger;
        private readonly IConfiguration _configuration;
#nullable enable
        public GetSpotifyPlaylistService(ILogger<GetSpotifyPlaylistService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration.GetSection("Spotify");
            _apiUrl = _configuration["ApiUrl"];
        }

        public async Task<SpotifyPlaylist> GetPlaylist(string accessToken, string playlistUrl)
        {
            _logger.LogInformation("Getting playlist using access token");

            var tmp = playlistUrl.Split("/");
            var tmp2 = tmp[tmp.Length - 1];
            var tmp3 = tmp2.Split("?");
            string playlistId = tmp3[0];

            using (HttpClient httpClient = new HttpClient())
            {
                // Add authorization header with bearer token
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                try
                {
                    // Send GET request to the API endpoint
                    HttpResponseMessage response = await httpClient.GetAsync(_apiUrl + "/playlists/" + playlistId);

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read and process the response content as needed
                        string responseBody = await response.Content.ReadAsStringAsync();

                        JsonSerializerOptions options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                        };

                        SpotifyPlaylist? playlist = JsonSerializer.Deserialize<SpotifyPlaylist>(responseBody, options);

                        if (playlist == null)
                        {
                            _logger.LogInformation("/playlists/{playlistId} returned invalid or empty response", playlistId);
                            return new SpotifyPlaylist();
                        }

                        Console.WriteLine($"Received {playlist.Tracks.PlaylistTrackListItems.Length} tracks in the Spotify playlist");
                        _logger.LogInformation("Received {count} tracks in the Spotify playlist", playlist.Tracks.PlaylistTrackListItems.Length);
                        return playlist;
                    }
                    else if(response.StatusCode.Equals(HttpStatusCode.NotFound))
                    {
                        _logger.LogInformation("No results found for the search {playlistUrl}", playlistUrl);
                        throw new Exception($"No results found for the search {playlistUrl}");
                    }
                    else
                    {
                        _logger.LogError("Request failed with status code: {stausCode}", response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("An error occurred: {errorMessage}", ex.Message);
                    throw;
                }
            }
            return new SpotifyPlaylist();
        }
    }
}
