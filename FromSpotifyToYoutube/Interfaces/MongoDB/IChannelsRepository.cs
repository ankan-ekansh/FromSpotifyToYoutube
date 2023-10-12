using FromSpotifyToYoutube.Models.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FromSpotifyToYoutube.Interfaces.MongoDB
{
    public interface IChannelsRepository
    {
        Task<IEnumerable<Channel>> GetAllChannels();

        Task<Channel> GetChannelByName(string channelName);

        Task<Channel> GetChannelById(string channelId);

        Task Create(Channel channel);

        Task<bool> Update(Channel channel);

        Task<bool> Delete(Channel channel);
    }
}
