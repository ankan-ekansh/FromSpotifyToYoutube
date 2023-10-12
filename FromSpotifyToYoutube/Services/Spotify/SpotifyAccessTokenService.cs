using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FromSpotifyToYoutube.Interfaces.Spotify;
using FromSpotifyToYoutube.Models.Spotify;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace FromSpotifyToYoutube.Services.Spotify
{
    public class SpotifyAccessTokenService : ISpotifyAccessTokenService
    {
#nullable disable
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _apiUrl;
        private static readonly string _grantType = "client_credentials";
        private static Dictionary<string, (string AccessToken, DateTime Expiration)> _tokenCache = new Dictionary<string, (string AccessToken, DateTime Expiration)>();
        private readonly ILogger<SpotifyAccessTokenService> _logger;
        private readonly IConfiguration _spotifyConfig;
#nullable disable

        public SpotifyAccessTokenService(ILogger<SpotifyAccessTokenService> logger, IConfiguration config)
        {
            _logger = logger;
            _spotifyConfig = config.GetSection("Spotify");
            _apiUrl = _spotifyConfig["TokenApiUrl"];
            _clientId = _spotifyConfig["ClientId"];
            _clientSecret = _spotifyConfig["ClientSecret"];
        }

        public async Task<string> GetAccessToken()
        {
            _logger.LogInformation("Fetching Access Token");

            string cachedToken = GetCachedToken(_apiUrl);
            if (cachedToken != string.Empty)
            {
                _logger.LogInformation("Using cached access token");
            }
            else
            {
                _logger.LogInformation("Fetching a new access token...");
                string newToken = await GetNewAccessToken(_apiUrl);
                if (newToken == string.Empty) 
                {
                    _logger.LogError("Empty access token received");
                    throw new Exception("Empty access token received");
                }
                CacheToken(_apiUrl, newToken);
                _logger.LogInformation("New access token obtained");
            }

            return GetCachedToken(_apiUrl);
        }

        static string GetCachedToken(string _apiUrl)
        {
            if (_tokenCache.ContainsKey(_apiUrl) && _tokenCache[_apiUrl].Expiration > DateTime.UtcNow)
            {
                return _tokenCache[_apiUrl].AccessToken;
            }

            return string.Empty;
        }

        static void CacheToken(string _apiUrl, string accessToken)
        {
            DateTime expiration = DateTime.UtcNow.AddMinutes(50);
            _tokenCache[_apiUrl] = (accessToken, expiration);
        }

        private async Task<string> GetNewAccessToken(string _apiUrl)
        {
            string headerAccessToken = Base64Encode(new StringBuilder().Append(_clientId).Append(':').Append(_clientSecret).ToString());

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                { "grant_type", _grantType },
                // Add more key-value pairs as needed
            };

            using (HttpClient httpClient = new HttpClient())
            {
                // Add authorization header with bearer token
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", headerAccessToken);
                HttpContent content = new FormUrlEncodedContent(formData);

                try
                {
                    // Send GET request to the API endpoint
                    HttpResponseMessage response = await httpClient.PostAsync(_apiUrl, content);

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read and process the response content as needed
                        string responseBody = await response.Content.ReadAsStringAsync();

                        JsonSerializerOptions options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                        };

                        AccessTokenResponse? accessTokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(responseBody, options);

                        if (accessTokenResponse is null)
                        {
                            throw new Exception("Unable to deserialize access token");
                        }

                        if (accessTokenResponse.AccessToken is null)
                        {
                            throw new Exception("Received null access token");
                        }

                        return accessTokenResponse.AccessToken;
                    }
                    else
                    {
                        _logger.LogError("Request failed with status code: {statusCode}", response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("An error occurred: {errorMessage}", ex.Message);
                }
            }

            return string.Empty;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
