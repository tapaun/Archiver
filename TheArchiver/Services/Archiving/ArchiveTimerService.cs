using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TheArchiver.Services.Archiving;

/// <summary>
/// Background service that periodically checks and archives channels every 24 hours.
/// </summary>
public class ArchiveTimerService(
    DiscordSocketClient client,
    ChannelArchivingService channelArchivingService,
    CategoryArchivingService categoryArchivingService,
    ILogger<ArchiveTimerService> logger)
    : BackgroundService {
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Executes the background archiving service loop.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested) {
            try {
                await CheckAndArchiveChannels();
            }
            catch (Exception ex) {
                logger.LogError(ex, "Error during archive check");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndArchiveChannels() {
        var now = DateTimeOffset.UtcNow;
        
        foreach (var archiveInfo in channelArchivingService.GetAllArchiveChannels()) {
            var timeSinceLastArchive = now - archiveInfo.LastArchivedAt;
            
            if (timeSinceLastArchive >= TimeSpan.FromHours(24)) {
                await ArchiveChannel(archiveInfo);
            }
        }
    }

    private async Task ArchiveChannel(ArchiveChannelInfo archiveInfo) {
        try {
            var guild = client.GetGuild(archiveInfo.GuildId);
            if (guild == null) {
                logger.LogWarning("Guild {GuildId} not found", archiveInfo.GuildId);
                return;
            }

            var currentChannel = guild.GetTextChannel(archiveInfo.ChannelId);
            if (currentChannel == null) {
                logger.LogWarning("Channel {ChannelId} not found in guild {GuildId}", 
                    archiveInfo.ChannelId, archiveInfo.GuildId);
                return;
            }

            var archivedCategoryId = categoryArchivingService.GetArchivedCategory(archiveInfo.GuildId);
            if (archivedCategoryId == null) {
                logger.LogWarning("No archived category set for guild {GuildId}", archiveInfo.GuildId);
                return;
            }

            // Get current channel name (in case it was changed)
            var currentChannelName = currentChannel.Name;
            channelArchivingService.UpdateChannelName(archiveInfo.GuildId, currentChannelName);

            // Move current channel to archived category
            await currentChannel.ModifyAsync(props => {
                props.CategoryId = archivedCategoryId.Value;
                props.Name = $"archived-{currentChannelName}-{DateTimeOffset.UtcNow:yyyy-MM-dd-HH-mm-ss}";
            });

            logger.LogInformation("Archived channel {ChannelName} in guild {GuildId}", 
                currentChannelName, archiveInfo.GuildId);

            var newChannel = await guild.CreateTextChannelAsync(currentChannelName, props => {
                props.CategoryId = archiveInfo.OriginalCategoryId != 0 ? archiveInfo.OriginalCategoryId : null;
                props.Position = 0;
                props.PermissionOverwrites = new Optional<IEnumerable<Overwrite>>(archiveInfo.OriginalPermissions);
            });

            channelArchivingService.SetArchiveChannel(
                archiveInfo.GuildId,
                newChannel.Id,
                currentChannelName,
                archiveInfo.OriginalCategoryId,
                archiveInfo.OriginalPermissions
            );

            logger.LogInformation("Created new archive channel {ChannelName} in guild {GuildId}", 
                currentChannelName, archiveInfo.GuildId);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error archiving channel for guild {GuildId}", archiveInfo.GuildId);
        }
    }
}
