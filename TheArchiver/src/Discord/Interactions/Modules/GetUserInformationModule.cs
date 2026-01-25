using System.Text;
using TheArchiver.Services.Information;

namespace TheArchiver.Discord.Interactions.Modules;

public class GetUserInformationModule(DiscordInformationService informationService)
    : InteractionModuleBase<SocketInteractionContext> {
    [SlashCommand("get-user-info", "Gets information about a user")]
    [global::Discord.Interactions.RequireContext(global::Discord.Interactions.ContextType.Guild)]
    public async Task GetUserInfo(
        [global::Discord.Interactions.Summary("user", "The user to get information about")]
        IUser user,
        [global::Discord.Interactions.Summary("channel", "The channel to get message count from (optional)")]
        IChannel? channel = null){
        if(!Context.Guild.Users.Contains(user)) {
            await RespondAsync("User not found.", ephemeral: true);
            return;
        }
        try {
            StringBuilder userInfo = new StringBuilder();
            await DeferAsync(ephemeral: true);
            userInfo.Append($"User Info for {user.Username}#{user.Discriminator}:\n" +
                            $"- ID: {user.Id}\n" +
                            $"- Created At: {user.CreatedAt.UtcDateTime} UTC\n" +
                            $"- Is Bot: {user.IsBot}\n");
            if(channel != null) {
                var messageCount = await informationService.GetUserMessageCountAsync(user, channel);
                userInfo.Append($"- Message Count in #{channel.Name}: {messageCount}\n");
            }
            await FollowupAsync(userInfo.ToString(), ephemeral: true);
        }
        catch (Exception ex) {
            await FollowupAsync($"An error occurred while fetching user information: {ex.Message}", ephemeral: true);
        }
    }
}
