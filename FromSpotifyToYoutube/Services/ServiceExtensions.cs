using FromSpotifyToYoutube.Interfaces.FromSpotifyToYoutube;
using FromSpotifyToYoutube.Interfaces.MongoDB;
using FromSpotifyToYoutube.Interfaces.Spotify;
using FromSpotifyToYoutube.Interfaces.Youtube;
using FromSpotifyToYoutube.Models.MongoDB;
using FromSpotifyToYoutube.Repositories.MongoDB;
using FromSpotifyToYoutube.Services.Spotify;
using FromSpotifyToYoutube.Services.Youtube;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Services
{
    public static class ServiceExtensions
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ISpotifyAccessTokenService, SpotifyAccessTokenService>();
            services.AddSingleton<IFromSpotifyToYoutube, FromSpotifyToYoutube>();
            services.AddSingleton<IGetSpotifyPlaylistService, GetSpotifyPlaylistService>();
            services.AddSingleton<IYoutubeClient, YoutubeClient>();
            services.AddSingleton<IChannelsContext, ChannelsContext>();
            services.AddSingleton<IChannelsRepository, ChannelsRepository>();
            services.AddSingleton<IVideosContext, VideosContext>();
            services.AddSingleton<IVideosRepository, VideosRepository>();
        }
    }
}
