using Microsoft.Extensions.Logging;
using TheArchiver.Services.Filtering;

namespace TheArchiver.Discord.Handlers;

/// <summary>
/// Handles incoming and updated Discord messages for filtering
/// </summary>
public class MessageHandler(KeywordMessageFilter filter, ILogger<MessageHandler> logger) {
    private readonly KeywordMessageFilter _filter = filter;
    private readonly ILogger<MessageHandler> _logger = logger;
    
    /// <summary>
    /// Filters new messages for bad words
    /// </summary>
    public async Task<Embed?> OnMessageReceivedAsync(SocketMessage rawMessage) {
        try {
            var embed = await _filter.ContainsKeywordAsync(rawMessage);
            if(embed != null) {
                // Send the filtered embed first, then attempt to delete the original message.
                // Deleting after sending avoids racing both operations and makes error handling clearer.
                await rawMessage.Channel.SendMessageAsync(embed: embed);
                try {
                    await rawMessage.DeleteAsync();
                } catch(HttpException ex) when (ex.DiscordCode == DiscordErrorCode.UnknownMessage) {
                    // Message was already deleted; not an error worth logging at Error level.
                    _logger.LogDebug("Message already deleted in channel {Channel} for user {User}", rawMessage.Channel.Name, rawMessage.Author.Username);
                } catch(HttpException ex) when (ex.DiscordCode == DiscordErrorCode.MissingPermissions) {
                    _logger.LogWarning("Missing permissions to delete messages in channel {Channel}", rawMessage.Channel.Name);
                }

                _logger.LogInformation("Filtered message sent for {User}", rawMessage.Author.Username);
            }
            return embed;
        } catch(HttpException ex) when(ex.DiscordCode == DiscordErrorCode.MissingPermissions) {
            _logger.LogWarning("Missing permissions in channel {Channel}", rawMessage.Channel.Name);
            return null;
        } catch(Exception ex) {
            _logger.LogError(ex, "Error handling message from {User}", rawMessage.Author.Username);
            return null;
        }
    }

    /// <summary>
    /// Filters edited messages for bad words
    /// </summary>
    public async Task<Embed?> OnMessageUpdatedAsync(Cacheable<IMessage, ulong> cachedMessage, SocketMessage updatedMessage, ISocketMessageChannel channel) {
        try {
            var embed = await _filter.ContainsKeywordAsync(updatedMessage);
            if(embed != null) {
                await updatedMessage.Channel.SendMessageAsync(embed: embed);
                try {
                    await updatedMessage.DeleteAsync();
                } catch(HttpException ex) when (ex.DiscordCode == DiscordErrorCode.UnknownMessage) {
                    _logger.LogDebug("Updated message already deleted in channel {Channel} for user {User}", updatedMessage.Channel.Name, updatedMessage.Author.Username);
                } catch(HttpException ex) when (ex.DiscordCode == DiscordErrorCode.MissingPermissions) {
                    _logger.LogWarning("Missing permissions to delete messages in channel {Channel}", updatedMessage.Channel.Name);
                }

                _logger.LogInformation("Filtered message sent for {User}", updatedMessage.Author.Username);
            }
            return embed;
        } catch(HttpException ex) when(ex.DiscordCode == DiscordErrorCode.MissingPermissions) {
            _logger.LogWarning("Missing permissions in channel {Channel}", updatedMessage.Channel.Name);
            return null;
        } catch(Exception ex) {
            _logger.LogError(ex, "Error handling updated message from {User}", updatedMessage.Author.Username);
            return null;
        }
    }

}