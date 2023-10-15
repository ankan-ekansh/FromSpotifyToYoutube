using FromSpotifyToYoutube.Interfaces.Youtube;
using FromSpotifyToYoutube.Models.Spotify;
using FromSpotifyToYoutube.Models.Youtube;
using Google.Apis.Services;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using MongoDB.Driver;
using FromSpotifyToYoutube.Interfaces.MongoDB;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace FromSpotifyToYoutube.Services.Youtube
{
    public class YoutubeClient : IYoutubeClient
    {
#nullable disable
        private readonly string _applicationName;
        private readonly string _apiUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly ILogger<YoutubeClient> _logger;
        private readonly IChannelsRepository _channelsRepository;
        private readonly IVideosRepository _videosRepository;
#nullable disable

        // Create a mapping from Artist -> Tracks
        Dictionary<string, List<string>> mapping = new Dictionary<string, List<string>>();
        // Temporary cache for Successful Channel title to id mapping
        Dictionary<string, string> channelIds = new Dictionary<string, string>();
        // Temporary cache for unsuccessful channel title to id mapping, query these directly via search
        List<string> nonOfficialChannels = new List<string>();
        // Temporary cache for tracks not found in official artist upload playlist, query these directly via search
        List<string> remainingTracks = new List<string>();
        // Temporary cache for successfully retrieved youtube videos
        List<string> youtubeTracks = new List<string>();

        public YoutubeClient(ILogger<YoutubeClient> logger,
                            IConfiguration configuration,
                            IChannelsRepository channelsRepository,
                            IVideosRepository videosRepository)
        {
            _logger = logger;
            _channelsRepository = channelsRepository;
            _videosRepository = videosRepository;
            _applicationName = configuration["ApplicationName"];
            _apiUrl = configuration.GetSection("YoutubeOperationalApi")["ApiUrl"];
            _clientId = configuration.GetSection("Youtube")["ClientId"];
            _clientSecret = configuration.GetSection("Youtube")["ClientSecret"];
        }

        public async Task ConvertPlaylist(SpotifyPlaylist playlist)
        {
            int uniqueArtistCount = 0;

            for (int i = 0; i < playlist.Tracks.PlaylistTrackListItems.Length; i++)
            {
                SpotifyPlaylistTrack? spotifyPlaylistTrack = playlist.Tracks.PlaylistTrackListItems[i].Track;

                SpotifyArtist? spotifyArtist = playlist.Tracks.PlaylistTrackListItems[i].Track.Artists[0];
                if (mapping.ContainsKey(spotifyArtist.Name))
                {
                    mapping[spotifyArtist.Name].Add(spotifyPlaylistTrack.Name);
                }
                else
                {
                    uniqueArtistCount++;
                    mapping.Add(spotifyArtist.Name, new List<string> { spotifyPlaylistTrack.Name });
                }
            }


            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(10);

                List<Task<Tuple<string, string>>> tasks = new List<Task<Tuple<string, string>>>();

                int i = 0;
                foreach ((string s, List<string> ls) in mapping)
                {

                    try
                    {
                        // Add GET tasks
                        tasks.Add(ApiCall(httpClient, s));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("An error occurred: {errorMessage}", ex.Message);
                    }
                }

                await Task.WhenAll(tasks);

                foreach (var task in tasks)
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        if (task.Result.Item2 != string.Empty)
                        {
                            if (!channelIds.ContainsKey(task.Result.Item1))
                            {
                                channelIds.Add(task.Result.Item1, task.Result.Item2);
                            }
                        }
                        else
                        {
                            nonOfficialChannels.Add(task.Result.Item1);
                        }
                    }
                }

                List<Task> searchPlaylistTasks = new List<Task>();
                // For artists with official channel, get their upload playlist and search the track in it
                // For artists without official channel, search {title} by {artist} official video
                foreach ((string channelTitle, string channelId) in channelIds)
                {
                    // Add GET tasks
                    searchPlaylistTasks.Add(SearchUploadPlaylist(httpClient, channelId, channelTitle, mapping[channelTitle]));
                }

                await Task.WhenAll(searchPlaylistTasks);


                List<Task> remainingTasks = new List<Task>();

                foreach (var remainingTrack in remainingTracks)
                {
                    // Add GET tasks
                    remainingTasks.Add(SearchNonUploadPlaylist(httpClient, remainingTrack));
                }

                List<string> nonOfficialChannelTracks = new List<string>();
                foreach (var nonOfficialChannel in nonOfficialChannels)
                {
                    List<string> remTracks = mapping[nonOfficialChannel];
                    
                    for (int j = 0; j < remTracks.Count; j++)
                    {
                        remTracks[j] = remTracks[j] + " by " + nonOfficialChannel + " Official Video";
                        nonOfficialChannelTracks.Add(remTracks[j]);
                    }
                }

                foreach (var nonOfficialChannelTrack in nonOfficialChannelTracks)
                {
                    // Add GET tasks
                    remainingTasks.Add(SearchNonUploadPlaylist(httpClient, nonOfficialChannelTrack));
                }

                // Add GET tasks
                await Task.WhenAll(remainingTasks);

                // POST task
                await CreatePlaylistAndAddTracks(playlist.Name);
            }
        }

        private async Task<Tuple<string, string>> ApiCall(HttpClient httpClient, string s)
        {
            Models.MongoDB.Channel channel = await _channelsRepository.GetChannelByName(s);

            if (channel != null && channel.ChannelId != null && channel.ChannelName != null)
            {
                _logger.LogInformation("Channel found in db. Channel Id = {channelId} Channel Name = {channelName}", channel.ChannelId, channel.ChannelName);
                return new Tuple<string, string> ( channel.ChannelName, channel.ChannelId );
            }

            // Send GET request to the API endpoint
            HttpResponseMessage response = await httpClient.GetAsync(_apiUrl + "/search?part=snippet&maxResults=5&q=" + Uri.EscapeDataString(s) + "&type=channel");

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                // Read and process the response content as needed
                var responseBody = await response.Content.ReadAsStreamAsync();

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                YoutubeSearchResponse? youtubeSearchResponsev2 = await JsonSerializer.DeserializeAsync<YoutubeSearchResponse>(responseBody, options);

                if (youtubeSearchResponsev2 is null)
                {
                    throw new Exception("Unable to deserialize youtube search response v2");
                }

                try
                {
                    YoutubeSearchItemSnippet? youtubeSearchResponsev2ItemSnippet = null;
                    foreach (var item in youtubeSearchResponsev2.Items)
                    {
                        if (item.Snippet is not null && item.Snippet.ChannelTitle is not null && item.Snippet.ChannelApproval is not null && item.Snippet.ChannelId is not null)
                        {
                            if (item.Snippet.ChannelTitle.Contains(s) && item.Snippet.ChannelApproval.Equals("Official Artist Channel"))
                            {
                                youtubeSearchResponsev2ItemSnippet = item.Snippet;
                                break;
                            }
                        }
                    }

                    if (youtubeSearchResponsev2ItemSnippet is null)
                    {
                        return new Tuple<string, string>(s, string.Empty);
                    }

                    await _channelsRepository.Create(new Models.MongoDB.Channel { Id = Guid.NewGuid().ToString(), ChannelId = youtubeSearchResponsev2ItemSnippet.ChannelId, ChannelName = s });

                    return new Tuple<string, string>(s, youtubeSearchResponsev2ItemSnippet.ChannelId);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred for the search: {s}", s);
                    _logger.LogError("An error occurred: {errorMessage}", ex.Message);
                }
            }
            else
            {
                _logger.LogError("Request failed with status code: {statusCode} for: {s}", response.StatusCode, s);
            }

            return new Tuple<string, string>(s, string.Empty);
        }

        private async Task SearchUploadPlaylist(HttpClient httpClient, string channelId, string channelTitle, List<string> tracks)
        {
            string playlistId = channelId.Substring(0, 1) + "U" + channelId.Substring(2);
            string resourceUrl = _apiUrl + "/noKey/playlistItems?part=snippet&playlistId=" + playlistId + "&maxResults=50";
            string? nextPageToken = null;
            int pageCount = 1;
            List<Tuple<string, string>> allVideos = new List<Tuple<string, string>>();
            List<Tuple<string, string>> videos = new List<Tuple<string, string>>();

            do
            {
                // TODO: Add retry mechanism to HTTP requests with Polly or some other library
                HttpResponseMessage response = await httpClient.GetAsync(resourceUrl + (nextPageToken != null ? "&pageToken=" + nextPageToken : string.Empty));

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStreamAsync();

                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };

                    YoutubePlaylistItemSearchResponse? res = await JsonSerializer.DeserializeAsync<YoutubePlaylistItemSearchResponse>(responseBody, options);

                    foreach (var item in res.Items)
                    {
                        allVideos.Add(new Tuple<string, string>(item.Snippet.Title, item.Snippet.ResourceId.VideoId));
                    }

                    nextPageToken = res.NextPageToken;
                }
                else
                {
                    _logger.LogError("Request failed with status code: {statusCode} for: {channelId}", response.StatusCode, channelId);
                }

                pageCount++;

            } while (nextPageToken != null);

            // 1st step: Search the upload playlist of the channel
            foreach (var track in tracks)
            {
                string cleanedTrack = CleanTrackName(track);

                Models.MongoDB.Video video = await _videosRepository.GetVideoByName(cleanedTrack);

                if (video != null && video.VideoId != null && video.VideoName != null)
                {
                    youtubeTracks.Add(video.VideoId);
                    continue;
                }

                var a = allVideos.Find(x => x.Item1.Contains(cleanedTrack)
                                        && x.Item1.Contains(channelTitle)
                                        && x.Item1.Contains("Official")
                                        && x.Item1.Contains("Video")
                                        && !x.Item1.Contains("Lyric")
                                        && !x.Item1.Contains("Audio")
                                        && !x.Item1.Contains("Instrumental")
                                        );
                if (a == null)
                {
                    _logger.LogInformation("No track found for {track} (Cleaned track {cleanedTrack}) by {channelTitle} with label Official Video", track, cleanedTrack, channelTitle);
                    remainingTracks.Add(track + " by " + channelTitle + " Official Video");
                    continue;
                }

                youtubeTracks.Add(a.Item2);
                await _videosRepository.Create(new Models.MongoDB.Video { Id = Guid.NewGuid().ToString(), VideoId = a.Item2, VideoName = cleanedTrack });
            }

            // 2nd step: For anything not found in the upload playlist of the channel, search directly using {Track name} by {Channel Name} Official Video and pick the most relevant/most view count one
        }

        private async Task SearchNonUploadPlaylist(HttpClient httpClient, string track)
        {
            string resourceUrl = _apiUrl + "/noKey/search?part=snippet&maxResults=5&type=video&q=" + Uri.EscapeDataString(track);
            string trackName = track.Split(" by ")[0];
            string cleanTrackName = CleanTrackName(trackName);

            Models.MongoDB.Video video = await _videosRepository.GetVideoByName(cleanTrackName);

            if (video != null && video.VideoId != null && video.VideoName != null)
            {
                youtubeTracks.Add(video.VideoId);
                return;
            }

            HttpResponseMessage response = await httpClient.GetAsync(resourceUrl);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStreamAsync();

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                YoutubeSearchResponse? res = await JsonSerializer.DeserializeAsync<YoutubeSearchResponse>(responseBody, options);

                if (res is null)
                {
                    throw new Exception("Unable to deserialize youtube search response v2");
                }

                try
                {

                    string videoId = res.Items[0].Id.VideoId;
                    youtubeTracks.Add(videoId);
                    await _videosRepository.Create(new Models.MongoDB.Video { Id = Guid.NewGuid().ToString(), VideoId = videoId, VideoName = cleanTrackName });
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred for the search: {s}", track);
                    _logger.LogError("An error occurred: {errorMessage}", ex.Message);
                }
            }
            else
            {
                _logger.LogError("Request failed with status code: {statusCode} for: {s}", response.StatusCode, track);
            }
        }

        private Task SearchTrack()
        {
            throw new NotImplementedException();
        }

        private string CleanTrackName(string trackName)
        {
            int idx = trackName.IndexOf("feat.");
            return trackName.Substring(0, idx != -1 ? idx-1 : trackName.Length).Trim();
        }

        private async Task CreatePlaylistAndAddTracks(string playlistName)
        {
            UserCredential credential;

            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = _clientId,
                    ClientSecret = _clientSecret,
                },
                new[] { YouTubeService.Scope.Youtube, YouTubeService.Scope.YoutubeForceSsl, YouTubeService.Scope.Youtubepartner },
                "user",
                CancellationToken.None,
                null,
                new CodeReceiver()
                );

            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });

            // Playlist is named the same as the spotify playlist
            var playlistListRequest = service.Playlists.List("snippet");
            playlistListRequest.Mine = true;

            var playlistListResponse = await playlistListRequest.ExecuteAsync();

            List<Playlist> playlists = playlistListResponse.Items.ToList();

            Playlist playlist;
            playlist = playlists.FirstOrDefault(x => x.Snippet.Title.Equals(playlistName, StringComparison.OrdinalIgnoreCase), null);
            
            // If playlist is not there, create playlist
            if (playlist == null)
            {
                playlist = new Playlist();
                PlaylistSnippet snippet = new PlaylistSnippet();
                snippet.DefaultLanguage = "en";
                snippet.Description = "This is a sample playlist description.";
                string[] tags = { "sample playlist", "API call" };
                snippet.Tags = tags;
                snippet.Title = playlistName;
                playlist.Snippet = snippet;
                PlaylistStatus playlistStatus = new PlaylistStatus();
                playlistStatus.PrivacyStatus = "private";
                playlist.Status = playlistStatus;

                var createPlaylistReq = service.Playlists.Insert(playlist, "snippet,status");

                var createPlaylistResponse = await createPlaylistReq.ExecuteAsync();
                playlist = createPlaylistResponse;

                Console.WriteLine($"{createPlaylistResponse.Id}");
                Console.WriteLine($"{createPlaylistResponse.Snippet.Title}");
            }

            HashSet<string> playlistVideoIds = new HashSet<string>();

            // Get all PlaylistItems of this playlist, if the video id exists here, dont add again
            var playlistItemsListRequest = service.PlaylistItems.List("snippet");
            playlistItemsListRequest.PlaylistId = playlist.Id;
            playlistItemsListRequest.MaxResults = 50;
            PlaylistItemListResponse playlistItemsListResponse;
            do
            {
                playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                HashSet<string> x = new HashSet<string>(playlistItemsListResponse.Items.Select(obj => obj.Snippet.ResourceId.VideoId));

                playlistVideoIds.UnionWith(x);

                playlistItemsListRequest.PageToken = playlistItemsListResponse.NextPageToken;
            }
            while (playlistItemsListResponse.NextPageToken != null);

            int numberOfTracksAlreadyConverted = playlistVideoIds.Count();

            var insertTasks = new List<Task>();

            foreach (var youtubeTrack in youtubeTracks)
            {
                if (playlistVideoIds.Contains(youtubeTrack))
                {
                    continue;
                }

                insertTasks.Add(InsertToPlaylist(service, playlist.Id, youtubeTrack));
            }

            await Task.WhenAll(insertTasks);

            int numberOfTracksInserted = 0;

            foreach (var task in insertTasks)
            {
                if (task.IsCompletedSuccessfully)
                {
                    numberOfTracksInserted++;
                }
            }

            Console.WriteLine($"Number of tracks already in playlist: {numberOfTracksAlreadyConverted}");
            Console.WriteLine($"Number of tracks inserted in playlist: {numberOfTracksInserted}");
            Console.WriteLine($"Total number of tracks in playlist: {numberOfTracksAlreadyConverted + numberOfTracksInserted}");
        }

        private async Task InsertToPlaylist(YouTubeService service, string playlistId, string videoId)
        {
            try
            {
                // Get the Video Resource
                var videoRequest = service.Videos.List("snippet");
                videoRequest.Id = videoId;

                var videoResponse = await videoRequest.ExecuteAsync();
                Google.Apis.YouTube.v3.Data.Video video = videoResponse.Items.FirstOrDefault();

                ResourceId resourceId = new ResourceId()
                {
                    Kind = "youtube#video",
                    VideoId = video.Id,
                };
                PlaylistItemSnippet playlistItemSnippet = new PlaylistItemSnippet()
                {
                    PlaylistId = playlistId,
                    ResourceId = resourceId,
                };
                PlaylistItem playlistItem = new PlaylistItem()
                {
                    Snippet = playlistItemSnippet
                };

                var playlistItemsInsertRequest = service.PlaylistItems.Insert(playlistItem, "snippet");

                var playlistItemsInsertResponse = await playlistItemsInsertRequest.ExecuteAsync();

                Console.WriteLine($"Video with Title {video.Snippet.Title} added to playlist");
                _logger.LogInformation("Video with Title {title} added to playlist id {playlistId}", video.Snippet.Title, playlistItemsInsertResponse.Snippet.PlaylistId);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while inserting {videoId} to playlist {playlistId}", videoId, playlistId);
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}
