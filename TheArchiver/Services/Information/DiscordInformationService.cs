using System.Collections.Concurrent;
using Discord;

namespace TheArchiver.Services.Information;

/// <summary>
/// Service for managing Discord user and server information retrieval with caching.
/// </summary>
public class DiscordInformationService {
    private readonly ConcurrentDictionary<ulong, ChannelMessageCache> _channelMessageCache = new();
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);
    private const int MaxMessagesPerBatch = 100;
    private const int MaxBatches = 10; // Limits to 1000 messages max to avoid memory issues

    /// <summary>
    /// Gets the message count for a specific user in a channel.
    /// </summary>
    /// <param name="user">The user to count messages for</param>
    /// <param name="channel">The channel to search in</param>
    /// <returns>The number of messages by the user in the channel</returns>
    public async Task<int> GetUserMessageCountAsync(IUser user, IChannel channel) {
        if (channel is not IMessageChannel messageChannel) {
            return 0;
        }

        try {
            var messages = await GetChannelMessagesAsync(messageChannel);
            return messages.Count(m => m.Author.Id == user.Id);
        }
        catch {
            return 0;
        }
    }

    /// <summary>
    /// Gets all cached or fetches messages from a channel.
    /// </summary>
    /// <param name="channel">The message channel to fetch from</param>
    /// <returns>List of messages from the channel</returns>
    public async Task<List<IMessage>> GetChannelMessagesAsync(IMessageChannel channel) {
        // Check if we have valid cached data
        if (_channelMessageCache.TryGetValue(channel.Id, out var cache) && 
            DateTimeOffset.UtcNow - cache.CachedAt < _cacheExpiration) {
            return cache.Messages;
        }

        // Fetch fresh data
        var messages = new List<IMessage>();
        var fetchedBatches = 0;
        
        await foreach (var messageBatch in channel.GetMessagesAsync()) {
            messages.AddRange(messageBatch);
            fetchedBatches++;
            
            if (fetchedBatches >= MaxBatches) {
                break;
            }
        }

        // Update cache
        _channelMessageCache[channel.Id] = new ChannelMessageCache {
            Messages = messages,
            CachedAt = DateTimeOffset.UtcNow
        };

        return messages;
    }

    /// <summary>
    /// Gets statistics for a specific user across a channel.
    /// </summary>
    /// <param name="user">The user to get stats for</param>
    /// <param name="channel">The channel to analyze</param>
    /// <returns>User statistics including message count</returns>
    public async Task<UserChannelStats> GetUserChannelStatsAsync(IUser user, IChannel channel) {
        if (channel is not IMessageChannel messageChannel) {
            return new UserChannelStats { UserId = user.Id, ChannelId = channel.Id, MessageCount = 0 };
        }

        var messages = await GetChannelMessagesAsync(messageChannel);
        var userMessages = messages.Where(m => m.Author.Id == user.Id).ToList();

        return new UserChannelStats {
            UserId = user.Id,
            ChannelId = channel.Id,
            MessageCount = userMessages.Count,
            FirstMessageAt = userMessages.MinBy(m => m.Timestamp)?.Timestamp,
            LastMessageAt = userMessages.MaxBy(m => m.Timestamp)?.Timestamp
        };
    }

    /// <summary>
    /// Invalidates the cache for a specific channel.
    /// </summary>
    /// <param name="channelId">The channel ID to invalidate</param>
    public void InvalidateChannelCache(ulong channelId) {
        _channelMessageCache.TryRemove(channelId, out _);
    }

    /// <summary>
    /// Clears all expired cache entries.
    /// </summary>
    public void ClearExpiredCache() {
        var now = DateTimeOffset.UtcNow;
        var expiredKeys = _channelMessageCache
            .Where(kvp => now - kvp.Value.CachedAt >= _cacheExpiration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys) {
            _channelMessageCache.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Cache structure for storing channel messages and their retrieval time.
    /// </summary>
    private class ChannelMessageCache {
        public List<IMessage> Messages { get; set; } = [];
        public DateTimeOffset CachedAt { get; set; }
    }
}

/// <summary>
/// Statistics about a user's activity in a specific channel.
/// </summary>
public class UserChannelStats {
    public required ulong UserId { get; init; }
    public required ulong ChannelId { get; init; }
    public required int MessageCount { get; init; }
    public DateTimeOffset? FirstMessageAt { get; init; }
    public DateTimeOffset? LastMessageAt { get; init; }
}
