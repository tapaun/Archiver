using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace TheArchiver.Discord.Interactions.Modules;

public class RemoveFilterModule(Services.Filtering.KeywordMessageFilter filter)
    : InteractionModuleBase<SocketInteractionContext> {
    [SlashCommand("remove-filter", "Remove a filtered word (Admin only)")]
    [global::Discord.Interactions.RequireUserPermission(global::Discord.GuildPermission.Administrator)]
    [global::Discord.Interactions.RequireContext(global::Discord.Interactions.ContextType.Guild)]
    public async Task RemoveFilter(
        [global::Discord.Interactions.Summary("word", "The word to remove from filter")]
        string word) {
        try {
            await filter.RemoveFilteredWordAsync(word);
            await RespondAsync($"✅ Removed filter: `{word}`", ephemeral: true);
        }
        catch (Exception ex) {
            await RespondAsync($"❌ Error removing filter: {ex.Message}", ephemeral: true);
        }
    }
}