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
    public class ChannelsRepository : IChannelsRepository
    {
        private readonly IChannelsContext _channelsContext;

        public ChannelsRepository(IChannelsContext channelsContext)
        {
            _channelsContext = channelsContext;
        }

        public async Task Create(Channel newChannel)
        {
            await _channelsContext.Channels.InsertOneAsync(newChannel);
        }

        public async Task<IEnumerable<Channel>> GetAllChannels()
        {
            return await _channelsContext.Channels.Find(_ => true).ToListAsync();
        }

        public async Task<Channel> GetChannelByName(string channelName)
        {
            return await _channelsContext.Channels.Find(x => x.ChannelName == channelName).FirstOrDefaultAsync();
        }

        public async Task<Channel> GetChannelById(string channelId)
        {
            return await _channelsContext.Channels.Find(x => x.ChannelId == channelId).FirstOrDefaultAsync();
        }

        public Task<bool> Update(Channel channel)
        {
            throw new NotImplementedException();
        }
        public Task<bool> Delete(Channel channel)
        {
            throw new NotImplementedException();
        }
    }
}
