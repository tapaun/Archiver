using System.Collections.Concurrent;

namespace TheArchiver.Services.Archiving;

/// <summary>
/// Service for managing archived category configuration per guild.
/// </summary>
public class CategoryArchivingService {
    private readonly ConcurrentDictionary<ulong, ulong> _archivedCategories = new();
    
    /// <summary>
    /// Sets the archived category for a guild where old channels will be moved.
    /// </summary>
    /// <param name="guildId">The Discord guild ID</param>
    /// <param name="categoryId">The category ID to use for archived channels</param>
    public void SetArchivedCategory(ulong guildId, ulong categoryId) {
        _archivedCategories[guildId] = categoryId;
    }
    
    /// <summary>
    /// Gets the archived category ID for a specific guild.
    /// </summary>
    /// <param name="guildId">The Discord guild ID</param>
    /// <returns>The category ID if set, otherwise null</returns>
    public ulong? GetArchivedCategory(ulong guildId) {
        return _archivedCategories.TryGetValue(guildId, out var categoryId) ? categoryId : null;
    }
}