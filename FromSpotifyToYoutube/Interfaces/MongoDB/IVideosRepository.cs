using FromSpotifyToYoutube.Models.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Interfaces.MongoDB
{
    public interface IVideosRepository
    {
        Task<IEnumerable<Video>> GetAllVideos();

        Task<Video> GetVideoByName(string videoName);

        Task<Video> GetVideoById(string videoId);

        Task Create(Video video);

        Task<bool> Update(Video video);

        Task<bool> Delete(Video video);
    }
}
