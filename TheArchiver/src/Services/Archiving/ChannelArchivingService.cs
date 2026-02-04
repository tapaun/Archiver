using System.Collections.Concurrent;
using System.Text.Json;
using Discord;
using Microsoft.Extensions.Logging;

namespace TheArchiver.Services.Archiving;

/// <summary>
/// Service for managing channel archiving operations and tracking archived channels per guild.
/// </summary>
public class ChannelArchivingService {
    private readonly ConcurrentDictionary<ulong, ArchiveChannelInfo> _archiveChannels = new();
    private readonly ILogger<ChannelArchivingService> _logger;
    private readonly string _statePath;

    public ChannelArchivingService(ILogger<ChannelArchivingService> logger) {
        _logger = logger;
        
        var baseDir = AppContext.BaseDirectory;
        var projectPath = Path.Combine(baseDir, "..", "..", "..", "archive_state.json");
        
        if (File.Exists(Path.GetFullPath(projectPath)) || Directory.Exists(Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..")))) {
            _statePath = Path.GetFullPath(projectPath);
        } else {
            _statePath = Path.Combine(baseDir, "archive_state.json");
        }
        
        _logger.LogInformation("Using archive state path: {StatePath}", _statePath);
    }

    /// <summary>
    /// Registers a channel for automatic archiving every 24 hours.
    /// </summary>
    /// <param name="guildId">The Discord guild ID</param>
    /// <param name="channelId">The channel ID to archive</param>
    /// <param name="channelName">The name of the channel</param>
    /// <param name="originalCategoryId">The category where the channel should be recreated after archiving</param>
    /// <param name="permissions">The permission overwrites to apply to newly created channels</param>
    public void SetArchiveChannel(ulong guildId, ulong channelId, string channelName, ulong originalCategoryId, IReadOnlyCollection<Overwrite> permissions) {
        _archiveChannels[guildId] = new ArchiveChannelInfo {
            GuildId = guildId,
            ChannelId = channelId,
            ChannelName = channelName,
            OriginalCategoryId = originalCategoryId,
            OriginalPermissions = permissions,
            LastArchivedAt = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Retrieves the archive channel information for a specific guild.
    /// </summary>
    /// <param name="guildId">The Discord guild ID</param>
    /// <returns>The archive channel info if found, otherwise null</returns>
    public ArchiveChannelInfo? GetArchiveChannel(ulong guildId) {
        return _archiveChannels.GetValueOrDefault(guildId);
    }
    
    /// <summary>
    /// Updates the stored channel name for a guild's archive channel.
    /// </summary>
    /// <param name="guildId">The Discord guild ID</param>
    /// <param name="newName">The new channel name</param>
    public void UpdateChannelName(ulong guildId, string newName) {
        if (_archiveChannels.TryGetValue(guildId, out var info)) {
            info.ChannelName = newName;
        }
    }
    
    /// <summary>
    /// Updates the last archived timestamp for a guild's archive channel.
    /// </summary>
    /// <param name="guildId">The Discord guild ID</param>
    /// <param name="timestamp">The new timestamp</param>
    public void UpdateLastArchived(ulong guildId, DateTimeOffset timestamp) {
        if (_archiveChannels.TryGetValue(guildId, out var info)) {
            info.LastArchivedAt = timestamp;
        }
    }
    
    /// <summary>
    /// Gets all registered archive channels across all guilds.
    /// </summary>
    /// <returns>Collection of all archive channel information</returns>
    public IEnumerable<ArchiveChannelInfo> GetAllArchiveChannels() {
        return _archiveChannels.Values;
    }
    
    /// <summary>
    /// Saves the state and information of the archived channel
    /// </summary>
    public async Task SaveStateAsync() {
        var json = JsonSerializer.Serialize(_archiveChannels, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_statePath, json);
        _logger.LogInformation("Saved archive state to {StatePath}", _statePath);
    }
    
    /// <summary>
    /// Loads the state and information of the archived channel
    /// </summary>
    public async Task LoadStateAsync() {
        if (!File.Exists(_statePath)) {
            _logger.LogInformation("Archive state file not found at {StatePath}, will create on first save", _statePath);
            return;
        }
        
        var json = await File.ReadAllTextAsync(_statePath);
        var loaded = JsonSerializer.Deserialize<ConcurrentDictionary<ulong, ArchiveChannelInfo>>(json);
        if (loaded != null) {
            foreach (var kvp in loaded) {
                _archiveChannels[kvp.Key] = kvp.Value;
            }
            _logger.LogInformation("Loaded {Count} archive channels from {StatePath}", _archiveChannels.Count, _statePath);
        }
    }
    
}

/// <summary>
/// Contains information about a channel configured for automatic archiving.
/// </summary>
public class ArchiveChannelInfo {
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public ulong OriginalCategoryId { get; set; }
    public IReadOnlyCollection<Overwrite> OriginalPermissions { get; set; } = [];
    public DateTimeOffset LastArchivedAt { get; set; }
}
