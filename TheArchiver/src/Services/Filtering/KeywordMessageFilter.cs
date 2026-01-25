using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TheArchiver.Common.Options;
using TheArchiver.Services.Embedding;

namespace TheArchiver.Services.Filtering;

/// <summary>
/// Filters messages for inappropriate keywords using regex pattern matching
/// </summary>
public class KeywordMessageFilter(IOptions<FilterOptions> options, UserEmbedBuilder embedBuilder) {
    private readonly UserEmbedBuilder _embedBuilder = embedBuilder;
    private readonly string _appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    private readonly Dictionary<Regex, string> _patterns = options.Value.SlurIndexPairs.ToDictionary(
        kvp => {
            var letters = kvp.Key.Select(c => {
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
            string pattern = @"\b" + string.Join("", letters) + @"\b";
            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        },
        kvp => kvp.Value
    );
    
    // Track original words to their regex patterns for removal
    private readonly Dictionary<string, Regex> _wordToPattern = options.Value.SlurIndexPairs.ToDictionary(
        kvp => kvp.Key.ToLower(),
        kvp => {
            var letters = kvp.Key.Select(c => {
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
            string pattern = @"\b" + string.Join("", letters) + @"\b";
            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
    );

    /// <summary>
    /// Checks if a message contains filtered keywords and returns a replacement embed
    /// </summary>
    public Task<Embed?> ContainsKeywordAsync(SocketMessage message) {
        if(message.Author.IsBot == true)
            return Task.FromResult<Embed?>(null);
        try {
            var content = message.Content;
            var replaced = false;

            foreach(var kvp in _patterns.Where(kvp => kvp.Key.IsMatch(content))) {
                content = kvp.Key.Replace(content, kvp.Value + ' ');
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
    /// Adds a new filter word and its replacement at runtime and saves to appsettings.json
    /// </summary>
    public void AddFilterWord(string word, string replacement) {
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
        string pattern = @"\b" + string.Join("", letters) + @"\b";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        _patterns[regex] = replacement;
        _wordToPattern[word.ToLower()] = regex;
        SaveToAppSettings(word, replacement);
    }
    
    public void RemoveFilteredWord(string word) {
        var lowerWord = word.ToLower();
        if (_wordToPattern.TryGetValue(lowerWord, out var regex)) {
            _patterns.Remove(regex);
            _wordToPattern.Remove(lowerWord);
            RemoveFromAppSettings(word);
        } else {
            throw new KeyNotFoundException($"Word '{word}' not found in filter list.");
        }
    }

    /// <summary>
    /// Saves the new filter word to appsettings.json
    /// </summary>
    private void SaveToAppSettings(string word, string replacement) {
        try {
            var json = File.ReadAllText(_appSettingsPath);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var options = new Dictionary<string, object?>();
            
            foreach (var property in root.EnumerateObject()) {
                if (property.Name == "FilterOptions") {
                    var filterOptions = new Dictionary<string, object?>();
                    foreach (var filterProp in property.Value.EnumerateObject()) {
                        if (filterProp.Name == "SlurIndexPairs") {
                            var slurPairs = new Dictionary<string, string>();
                            foreach (var slur in filterProp.Value.EnumerateObject()) {
                                slurPairs[slur.Name] = slur.Value.GetString() ?? "";
                            }
                            slurPairs[word] = replacement;
                            filterOptions["SlurIndexPairs"] = slurPairs;
                        } else {
                            filterOptions[filterProp.Name] = JsonSerializer.Deserialize<object>(filterProp.Value.GetRawText());
                        }
                    }
                    options["FilterOptions"] = filterOptions;
                } else {
                    options[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                }
            }

            var updatedJson = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_appSettingsPath, updatedJson);
        }
        catch (Exception ex) {
            Console.WriteLine($"Error saving to appsettings.json: {ex.Message}");
            throw;
        }
    }
    private void RemoveFromAppSettings(string word) {
        try {
            var json = File.ReadAllText(_appSettingsPath);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var options = new Dictionary<string, object?>();
            
            foreach (var property in root.EnumerateObject()) {
                if (property.Name == "FilterOptions") {
                    var filterOptions = new Dictionary<string, object?>();
                    foreach (var filterProp in property.Value.EnumerateObject()) {
                        if (filterProp.Name == "SlurIndexPairs") {
                            var slurPairs = new Dictionary<string, string>();
                            foreach (var slur in filterProp.Value.EnumerateObject()) {
                                if (slur.Name != word) {
                                    slurPairs[slur.Name] = slur.Value.GetString() ?? "";
                                }
                            }
                            filterOptions["SlurIndexPairs"] = slurPairs;
                        } else {
                            filterOptions[filterProp.Name] = JsonSerializer.Deserialize<object>(filterProp.Value.GetRawText());
                        }
                    }
                    options["FilterOptions"] = filterOptions;
                } else {
                    options[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                }
            }

            var updatedJson = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_appSettingsPath, updatedJson);
        }
        catch (Exception ex) {
            Console.WriteLine($"Error removing from appsettings.json: {ex.Message}");
            throw;
        }
    }
}
