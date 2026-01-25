using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace TheArchiver.Discord.Interactions.Modules;

public class AddFilterModule(Services.Filtering.KeywordMessageFilter filter)
    : InteractionModuleBase<SocketInteractionContext> {
    [SlashCommand("add-filter", "Add a word to filter and its replacement (Admin only)")]
    [global::Discord.Interactions.RequireUserPermission(global::Discord.GuildPermission.Administrator)]
    [global::Discord.Interactions.RequireContext(global::Discord.Interactions.ContextType.Guild)]
    public async Task AddFilter(
        [global::Discord.Interactions.Summary("word", "The word to filter")]
        string word,
        [global::Discord.Interactions.Summary("replacement", "What to replace it with")]
        string replacement) {
        
        try {
            filter.AddFilterWord(word, replacement);
            await RespondAsync($"✅ Added filter: `{word}` → `{replacement}`", ephemeral: true);
        }
        catch (Exception ex) {
            await RespondAsync($"❌ Error adding filter: {ex.Message}", ephemeral: true);
        }
    }
}
