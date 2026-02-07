using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheArchiver.Configuration;
using TheArchiver.Services.Embedding;

namespace TheArchiver.Services.Filtering;

/// <summary>
/// Filters messages for inappropriate keywords using regex pattern matching
/// </summary>
public class KeywordMessageFilter {
    private readonly UserEmbedBuilder _embedBuilder;
    private readonly ILogger<KeywordMessageFilter> _logger;
    private readonly string _filtersPath;
    private readonly Dictionary<Regex, string> _patterns = [];
    private readonly Dictionary<string, Regex> _wordToPattern = [];

    public KeywordMessageFilter(
        UserEmbedBuilder embedBuilder, 
        ILogger<KeywordMessageFilter> logger,
        IOptions<FilePathOptions> options) {
        _embedBuilder = embedBuilder;
        _logger = logger;
        _filtersPath = FilePathOptions.ResolvePath(options.Value.FiltersPath);
        _logger.LogInformation("Using filters path: {FiltersPath}", _filtersPath);
    }

    /// <summary>
    /// Loads filters from filters.json file
    /// </summary>
    public async Task LoadFiltersAsync() {
        if (!File.Exists(_filtersPath)) {
            _logger.LogWarning("Filters file not found at {FiltersPath}", _filtersPath);
            return;
        }

        var json = await File.ReadAllTextAsync(_filtersPath);
        var filters = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        
        if (filters == null) return;

        foreach (var kvp in filters) {
            AddFilterToRuntime(kvp.Key, kvp.Value);
        }
        
        _logger.LogInformation("Loaded {Count} filters from {FiltersPath}", filters.Count, _filtersPath);
    }

    private void AddFilterToRuntime(string word, string replacement) {
        var letters = word.Select(c => {
            string escaped = Regex.Escape(c.ToString());
            string sub = c switch {
                'i' => "[i1!]+",
                'a' => "[a@4]+",
                'e' => "[e3]+",
                'o' => "[o0]+",
                _ => escaped + "+"
            };
            return sub + "[^a-zA-Z0-9]*";
        });
        // Use \b at start but allow word to continue (catches plurals, suffixes like -ed, -ing, etc.)
        string pattern = @"\b" + string.Join("", letters);
        var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        _patterns[regex] = replacement;
        _wordToPattern[word.ToLower()] = regex;
    }

    /// <summary>
    /// Checks if a message contains filtered keywords and returns a replacement embed
    /// </summary>
    public Task<Embed?> ContainsKeywordAsync(SocketMessage message) {
        if(message.Author.IsBot)
            return Task.FromResult<Embed?>(null);
        try {
            var content = message.Content;
            var replaced = false;

            foreach(var kvp in _patterns.Where(kvp => kvp.Key.IsMatch(content))) {
                content = kvp.Key.Replace(content, kvp.Value);
                replaced = true;
            }

            if(!replaced)
                return Task.FromResult<Embed?>(null);

            return Task.FromResult(_embedBuilder.BuildUserMessageEmbed(message, content));
        } catch(Exception) {
            return Task.FromResult<Embed?>(null);
        }
    }

    /// <summary>
    /// Adds a new filter word and its replacement at runtime and saves to filters.json
    /// </summary>
    public async Task AddFilterWordAsync(string word, string replacement) {
        AddFilterToRuntime(word, replacement);
        await SaveFiltersAsync(word, replacement);
    }
    
    /// <summary>
    /// Removes a filter word from runtime and filters.json
    /// </summary>
    public async Task RemoveFilteredWordAsync(string word) {
        var lowerWord = word.ToLower();
        _logger.LogDebug("Attempting to remove: '{Word}'", lowerWord);
        _logger.LogDebug("Available words: {Words}", string.Join(", ", _wordToPattern.Keys));
    
        if (_wordToPattern.TryGetValue(lowerWord, out var regex)) {
            _patterns.Remove(regex);
            _wordToPattern.Remove(lowerWord);
            _logger.LogInformation("Removed '{Word}' from runtime filters", lowerWord);
            await RemoveFromFiltersAsync(word);
        } else {
            throw new KeyNotFoundException($"Word '{word}' not found in filter list.");
        }
    }

    /// <summary>
    /// Saves the new filter word to filters.json
    /// </summary>
    private async Task SaveFiltersAsync(string word, string replacement) {
        try {
            var json = await File.ReadAllTextAsync(_filtersPath);
            var filters = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            
            filters[word] = replacement;
            
            var updatedJson = JsonSerializer.Serialize(filters, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filtersPath, updatedJson);
            _logger.LogInformation("Saved filter '{Word}' to {FiltersPath}", word, _filtersPath);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error saving to filters.json");
            throw;
        }
    }
    
    /// <summary>
    /// Removes a filter word from filters.json
    /// </summary>
    private async Task RemoveFromFiltersAsync(string word) {
        try {
            var json = await File.ReadAllTextAsync(_filtersPath);
            var filters = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            var lowerWord = word.ToLower();

            var keyToRemove = filters.Keys.FirstOrDefault(k => k.Equals(lowerWord, StringComparison.OrdinalIgnoreCase));
            if (keyToRemove != null) {
                filters.Remove(keyToRemove);
            }

            var updatedJson = JsonSerializer.Serialize(filters, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filtersPath, updatedJson);
            _logger.LogInformation("Removed filter '{Word}' from {FiltersPath}", word, _filtersPath);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error removing from filters.json");
            throw;
        }
    }
}
