using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TheArchiver.Services.Archiving;

/// <summary>
/// Service for managing archived category configuration per guild.
/// </summary>
public class CategoryArchivingService {
    private readonly ConcurrentDictionary<ulong, ulong> _archivedCategories = new();
    private readonly ILogger<CategoryArchivingService> _logger;
    private readonly string _archivingPath;

    public CategoryArchivingService(ILogger<CategoryArchivingService> logger) {
        _logger = logger;
        
        var baseDir = AppContext.BaseDirectory;
        var projectPath = Path.Combine(baseDir, "..", "..", "..", "archiving.json");
        
        if (File.Exists(Path.GetFullPath(projectPath))) {
            _archivingPath = Path.GetFullPath(projectPath);
        } else {
            _archivingPath = Path.Combine(baseDir, "archiving.json");
        }
        
        _logger.LogInformation("Using archiving path: {ArchivingPath}", _archivingPath);
    }
    
    /// <summary>
    /// Loads the archived categories from archiving.json
    /// </summary>
    public async Task LoadStateAsync() {
        if (!File.Exists(_archivingPath)) {
            _logger.LogInformation("Archiving file not found at {ArchivingPath}, will create on first save", _archivingPath);
            return;
        }
        
        var json = await File.ReadAllTextAsync(_archivingPath);
        var data = JsonSerializer.Deserialize<ArchivingData>(json);
        
        if (data?.Servers != null) {
            foreach (var kvp in data.Servers) {
                if (kvp.Value.ArchiveCategoryId.HasValue) {
                    _archivedCategories[kvp.Key] = kvp.Value.ArchiveCategoryId.Value;
                }
            }
            _logger.LogInformation("Loaded {Count} archived categories from {ArchivingPath}", _archivedCategories.Count, _archivingPath);
        }
    }
    
    /// <summary>
    /// Saves the archived categories to archiving.json
    /// </summary>
    public async Task SaveStateAsync() {
        var data = new ArchivingData { Servers = new Dictionary<ulong, ServerArchivingInfo>() };
        
        if (File.Exists(_archivingPath)) {
            var json = await File.ReadAllTextAsync(_archivingPath);
            data = JsonSerializer.Deserialize<ArchivingData>(json) ?? data;
        }
        
        foreach (var kvp in _archivedCategories) {
            if (!data.Servers.ContainsKey(kvp.Key)) {
                data.Servers[kvp.Key] = new ServerArchivingInfo();
            }
            data.Servers[kvp.Key].ArchiveCategoryId = kvp.Value;
        }
        
        var updatedJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_archivingPath, updatedJson);
        _logger.LogInformation("Saved archiving state to {ArchivingPath}", _archivingPath);
    }
    
    /// <summary>
    /// Sets the archived category for a guild where old channels will be moved.
    /// </summary>
    /// <param name="guildId">The Discord guild ID</param>
    /// <param name="categoryId">The category ID to use for archived channels</param>
    public async Task SetArchivedCategoryAsync(ulong guildId, ulong categoryId) {
        _archivedCategories[guildId] = categoryId;
        await SaveStateAsync();
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

/// <summary>
/// Root structure for archiving.json
/// </summary>
public class ArchivingData {
    public Dictionary<ulong, ServerArchivingInfo> Servers { get; set; } = new();
}

/// <summary>
/// Information about a server's archiving configuration
/// </summary>
public class ServerArchivingInfo {
    public ulong? ArchiveCategoryId { get; set; }
}
