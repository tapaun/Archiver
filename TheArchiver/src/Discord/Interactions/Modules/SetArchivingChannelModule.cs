using TheArchiver.Services.Archiving;

namespace TheArchiver.Discord.Interactions.Modules;

/// <summary>
/// Interaction module for configuring channels to be automatically archived every 24 hours.
/// </summary>
public class SetArchivingChannelModule(
    ChannelArchivingService channelArchivingService,
    CategoryArchivingService categoryArchivingService)
    : InteractionModuleBase<SocketInteractionContext> {
    [SlashCommand("set-archiving-channel", "Sets the channel to be archived every 24 hours")]
    [DefaultMemberPermissions(GuildPermission.ManageChannels)]
    [global::Discord.Interactions.RequireContext(global::Discord.Interactions.ContextType.Guild)]
    public async Task SetArchivingChannelAsync(
        [global::Discord.Interactions.Summary("channel-name",
            "The name of the channel to archive every 24 hours")]
        string channelName) {
        try {
            await DeferAsync(ephemeral: true);

            var archivedCategoryId = categoryArchivingService.GetArchivedCategory(Context.Guild.Id);
            if (archivedCategoryId == null) {
                await FollowupAsync(
                    "❌ Please set up an archived category first using `/set-archive-category`",
                    ephemeral: true);
                return;
            }

            ulong originalCategoryId;
            ITextChannel newChannel;
            IReadOnlyCollection<Overwrite> originalPermissions;

            var existingChannel = Context.Guild.Channels
                .OfType<ITextChannel>()
                .FirstOrDefault(c => c.Name == channelName);

            if (existingChannel != null) {
                originalCategoryId = existingChannel.CategoryId ?? 0;
                originalPermissions = existingChannel.PermissionOverwrites.ToList();

                await existingChannel.ModifyAsync(props => {
                    props.CategoryId = archivedCategoryId.Value;
                    props.Name = $"archived-{channelName}-{DateTimeOffset.UtcNow:yyyy-MM-dd-HH-mm-ss}";
                });

                newChannel = await Context.Guild.CreateTextChannelAsync(channelName, props => {
                    if (originalCategoryId != 0) {
                        props.CategoryId = originalCategoryId;
                    }
                    props.Position = 0;
                    props.PermissionOverwrites = new Optional<IEnumerable<Overwrite>>(originalPermissions);
                });
            }
            else {
                var currentChannel = Context.Channel as ITextChannel;
                originalCategoryId = currentChannel?.CategoryId ?? 0;
                originalPermissions = currentChannel?.PermissionOverwrites.ToList() ?? new List<Overwrite>();

                newChannel = await Context.Guild.CreateTextChannelAsync(channelName, props => {
                    if (originalCategoryId != 0) {
                        props.CategoryId = originalCategoryId;
                    }
                    props.Position = 0;
                    props.PermissionOverwrites = new Optional<IEnumerable<Overwrite>>(originalPermissions);
                });
            }

            channelArchivingService.SetArchiveChannel(
                Context.Guild.Id,
                newChannel.Id,
                channelName,
                originalCategoryId,
                originalPermissions
            );

            await FollowupAsync(
                $"✅ Channel **{channelName}** {(existingChannel != null ? "archived and recreated" : "created")}.\n" +
                $"📁 Original category: {(originalCategoryId != 0 ? Context.Guild.GetCategoryChannel(originalCategoryId)?.Name ?? "None" : "None")}\n" +
                $"🔒 Permissions: Copied from original channel\n" +
                $"⏰ Next archive: <t:{((DateTimeOffset.UtcNow.AddHours(24)).ToUnixTimeSeconds())}:R>",
                ephemeral: true);
        }
        catch (Exception ex) {
            await FollowupAsync(
                $"❌ An error occurred while setting the archiving channel: {ex.Message}",
                ephemeral: true);
        }
    }
}