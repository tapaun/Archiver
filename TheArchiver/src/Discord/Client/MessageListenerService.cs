using Microsoft.Extensions.Logging;
using TheArchiver.Discord.Handlers;

namespace TheArchiver.Discord.Client;

/// <summary>
/// Listens for Discord message events and routes them to handlers
/// </summary>
public class MessageListenerService(
    DiscordSocketClient client,
    ILogger<MessageListenerService> logger,
    MessageHandler handler)
    : DiscordClientService(client, logger) {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        Client.MessageReceived += handler.OnMessageReceivedAsync;
        Client.MessageUpdated += handler.OnMessageUpdatedAsync;
        await Task.CompletedTask;
    }
}