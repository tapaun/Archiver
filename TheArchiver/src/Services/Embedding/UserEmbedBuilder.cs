namespace TheArchiver.Services.Embedding;

/// <summary>
/// Builds embeds for filtered messages
/// </summary>
public class UserEmbedBuilder {
    /// <summary>
    /// Creates an embed showing the filtered message with user info
    /// </summary>
    public Embed? BuildUserMessageEmbed(SocketMessage message, string replacedContent) {
        var embed = new EmbedBuilder()
            .WithAuthor(message.Author.Username, message.Author.GetAvatarUrl())
            .WithDescription(replacedContent)
            .WithFooter($"User ID: {message.Author.Id}")
            .WithColor(Color.DarkBlue)
            .Build();
        return embed;
    }
}