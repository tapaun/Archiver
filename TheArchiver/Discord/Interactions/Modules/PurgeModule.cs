using Discord;
using Discord.Interactions.Builders;
using Discord.WebSocket;
using ContextType = Discord.Commands.ContextType;

namespace TheArchiver.Discord.Interactions.Modules;

/// <summary>
/// Slash commands for purging bot messages
/// </summary>
public class PurgeModule : InteractionModuleBase<SocketInteractionContext> {
    /// <summary>
    /// Deletes recent bot messages in the channel
    /// </summary>
    [SlashCommand("purge-bot", "Purges the bot's messages")]
    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    [global::Discord.Interactions.RequireContext(global::Discord.Interactions.ContextType.Guild)]
    public async Task PurgeBot(
        [global::Discord.Interactions.Summary("count", "Number of messages to check, 10 by default")]
        int count = 10) {
        if(count <= 0 || count >= 100) {
            await RespondAsync("Count must be greater than 0 and less than 100.", ephemeral: true);
            return;
        }

        if(Context.Channel is not ITextChannel channel) {
            await RespondAsync("This command can only be used in text channels.", ephemeral: true);
            return;
        }

        try {
            await DeferAsync(ephemeral: true);

            var messages = await channel
                .GetMessagesAsync(limit: count)
                .FlattenAsync();

            var botMessages = messages
                .Where(m =>
                    m.Author.Id == Context.Client.CurrentUser.Id &&
                    m.Timestamp >= DateTimeOffset.UtcNow.AddDays(-14))
                .ToList();

            await channel.DeleteMessagesAsync(botMessages);

            await FollowupAsync($"🧹 Deleted {botMessages.Count} bot messages.", ephemeral: true);
        } catch(HttpException ex) when(ex.DiscordCode == DiscordErrorCode.MissingPermissions) {
            await FollowupAsync("Missing permissions to delete messages.", ephemeral: true);
        } catch(Exception) {
            await FollowupAsync("An error occurred while purging messages.", ephemeral: true);
        }
    }

    /// <summary>
    /// Deletes recent messages from all users in the channel
    /// </summary>
    [SlashCommand("purge", "Purges messages from all users")]
    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    [global::Discord.Interactions.RequireContext(global::Discord.Interactions.ContextType.Guild)]
    public async Task Purge(
        [global::Discord.Interactions.Summary("count", "Number of messages to delete, 10 by default")]
        int count = 10) {
        if(count <= 0 || count >= 100) {
            await RespondAsync("Count must be greater than 0 and less than 100.", ephemeral: true);
            return;
        }

        if(Context.Channel is not ITextChannel channel) {
            await RespondAsync("This command can only be used in text channels.", ephemeral: true);
            return;
        }

        try {
            await DeferAsync(ephemeral: true);

            var messages = await channel
                .GetMessagesAsync(limit: count)
                .FlattenAsync();

            var recentMessages = messages
                .Where(m => m.Timestamp >= DateTimeOffset.UtcNow.AddDays(-14))
                .ToList();

            if (recentMessages.Count == 0) {
                await FollowupAsync("No messages found to delete (messages must be less than 14 days old).", ephemeral: true);
                return;
            }

            await channel.DeleteMessagesAsync(recentMessages);

            await FollowupAsync($"🧹 Deleted {recentMessages.Count} messages.", ephemeral: true);
        } catch(HttpException ex) when(ex.DiscordCode == DiscordErrorCode.MissingPermissions) {
            await FollowupAsync("Missing permissions to delete messages.", ephemeral: true);
        } catch(Exception) {
            await FollowupAsync("An error occurred while purging messages.", ephemeral: true);
        }
    }
}