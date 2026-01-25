using System.ComponentModel.DataAnnotations;

namespace TheArchiver.Common.Options;

public class FilterOptions : INamedOptions {
    public static string GetSectionName() => "FilterOptions";
    
    /// <summary>
    /// Pairs each slur with its replacement, made by KMC
    /// </summary>
    [Required]
    public Dictionary<string, string> SlurIndexPairs { get; init; } = null!;
}