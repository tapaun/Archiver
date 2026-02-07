namespace TheArchiver.Configuration;

/// <summary>
/// Configuration options for file paths used by the application.
/// Paths can be absolute or relative to the application's base directory.
/// </summary>
public class FilePathOptions {
    public const string SectionName = "FilePaths";
    
    /// <summary>
    /// Path to the filters.json file containing keyword filters.
    /// </summary>
    public string FiltersPath { get; set; } = "filters.json";
    
    /// <summary>
    /// Path to the archiving.json file containing category archiving configuration.
    /// </summary>
    public string ArchivingPath { get; set; } = "archiving.json";
    
    /// <summary>
    /// Path to the archive_state.json file containing channel archiving state.
    /// </summary>
    public string ArchiveStatePath { get; set; } = "archive_state.json";
    
    /// <summary>
    /// Resolves a path to an absolute path based on the application's base directory.
    /// </summary>
    public static string ResolvePath(string path) {
        if (Path.IsPathRooted(path)) {
            return path;
        }
        return Path.Combine(AppContext.BaseDirectory, path);
    }
}

