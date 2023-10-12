using FromSpotifyToYoutube.Interfaces.MongoDB;
using FromSpotifyToYoutube.Models.MongoDB;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Repositories.MongoDB
{
    public class VideosRepository : IVideosRepository
    {
        private readonly IVideosContext _videosContext;

        public VideosRepository(IVideosContext videosContext)
        {
            _videosContext = videosContext;
        }
        public async Task Create(Video video)
        {
            await _videosContext.Videos.InsertOneAsync(video);
        }

        public Task<bool> Delete(Video video)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Video>> GetAllVideos()
        {
            return await _videosContext.Videos.Find(_ => true).ToListAsync();
        }

        public async Task<Video> GetVideoById(string videoId)
        {
            return await _videosContext.Videos.Find(x => x.VideoId == videoId).FirstOrDefaultAsync();
        }

        public async Task<Video> GetVideoByName(string videoName)
        {
            return await _videosContext.Videos.Find(x => x.VideoName == videoName).FirstOrDefaultAsync();
        }

        public Task<bool> Update(Video video)
        {
            throw new NotImplementedException();
        }
    }
}
