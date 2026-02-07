namespace TheArchiver.Discord.Interactions.Modules;

public class GetServerInformationModule : InteractionModuleBase<SocketInteractionContext> {
    [SlashCommand("get-server-information", "Gets information about the current server.")]
    [global::Discord.Interactions.RequireContext(global::Discord.Interactions.ContextType.Guild)]
    public async Task GetServerInformationAsync() {
        try {
            var guild = Context.Guild;
            var embed = ServerEmbedBuilder(guild);
            await RespondAsync(embed: embed);
        }
        catch (Exception ex) {
            await RespondAsync($"An error occurred while fetching server information: {ex.Message}");
        }
    }
    /// <summary>
    /// Builds an embed containing server information.
    /// </summary>
    /// <param name="guild"></param>
    /// <returns>Embed</returns>
    private Embed ServerEmbedBuilder(IGuild guild) {
        var usersCount = guild.GetUsersAsync().Result.Count;
        var fields = new EmbedFieldBuilder()
            .WithName("Server Information")
            .WithValue($"ID: {guild.Id}\n" +
                   $"Owner ID: {guild.OwnerId}\n" +
                   $"Member Count: {usersCount}\n" +
                   $"Created At: {guild.CreatedAt.UtcDateTime}\n" +
                   $"Verification Level: {guild.VerificationLevel}\n" +
                   $"Boost Level: {guild.PremiumTier}\n" +
                   $"Boost Count: {guild.PremiumSubscriptionCount}");
        var embed = new EmbedBuilder()
            .WithTitle($"Server name: {guild.Name}")
            .WithFields(fields)
            .WithColor(Color.Magenta);
        return embed.Build();
    }
}