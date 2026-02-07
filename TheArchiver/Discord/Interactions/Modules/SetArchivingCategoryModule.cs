using TheArchiver.Services.Archiving;

namespace TheArchiver.Discord.Interactions.Modules;

/// <summary>
/// Interaction module for configuring the category where archived channels will be stored.
/// </summary>
public class SetArchivingCategoryModule(CategoryArchivingService categoryArchivingService)
    : InteractionModuleBase<SocketInteractionContext> {
    [SlashCommand("set-archive-category", "Sets the category where archived channels will be moved")]
    [DefaultMemberPermissions(GuildPermission.ManageChannels)]
    [global::Discord.Interactions.RequireContext(global::Discord.Interactions.ContextType.Guild)]
    public async Task SetArchivingCategory(
        [global::Discord.Interactions.Summary("channel",
            "The category to set as the archiving category, default Archived")]
        string categoryName,
        [global::Discord.Interactions.Summary("role", "Role which lets you see the channels in the category")]
        string roleName = "Archiver") {
        try {
            await DeferAsync(ephemeral: true);
        
            ICategoryChannel category;
            if (Context.Guild.CategoryChannels.Any(c => c.Name == categoryName)) {
                category = Context.Guild.CategoryChannels.First(c => c.Name == categoryName);
                await categoryArchivingService.SetArchivedCategoryAsync(Context.Guild.Id, category.Id);
            }
            else {
                category = await Context.Guild.CreateCategoryChannelAsync(categoryName);
                await categoryArchivingService.SetArchivedCategoryAsync(Context.Guild.Id, category.Id);
            }
        
            await SubscribeRoleToCategory(roleName, category);
        
            await FollowupAsync(
                $"✅ Set archived threads category to **{categoryName}** with role **{roleName}**.",
                ephemeral: true);
        }
        catch (Exception ex) {
            await FollowupAsync($"❌ Failed to set archiving category: {ex.Message}", ephemeral: true);
        }
    }

    private async Task SubscribeRoleToCategory(string roleName, ICategoryChannel category) {
        var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == roleName);
        if (role == null) {
            var createdRole = await Context.Guild.CreateRoleAsync(roleName);
            role = Context.Guild.Roles.First(r => r.Id == createdRole.Id);
        }
        await category.AddPermissionOverwriteAsync(role,
            new OverwritePermissions(viewChannel: PermValue.Allow));
    }
}