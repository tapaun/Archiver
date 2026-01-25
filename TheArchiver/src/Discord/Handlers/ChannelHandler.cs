using Microsoft.Extensions.Logging;

namespace TheArchiver.Discord.Handlers;

public class ChannelHandler(ILogger<ChannelHandler> logger) {
    public void OnChannelDeleted(SocketChannel channel) {
        logger.LogInformation("Channel deleted!");
        logger.LogInformation("Archived {ChannelId}!", channel.Id);
    }
}