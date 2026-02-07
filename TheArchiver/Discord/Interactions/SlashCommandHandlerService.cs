using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TheArchiver.Discord.Interactions;

/// <summary>
/// Registers and handles slash command interactions
/// </summary>
public class SlashCommandHandlerService(
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    ILogger<SlashCommandHandlerService> logger) : IHostedService {
    
    private readonly DiscordSocketClient _client = client;
    private readonly InteractionService _interactions = interactions;
    private readonly IServiceProvider _services = services;
    private readonly ILogger<SlashCommandHandlerService> _logger = logger;
    
    public Task StartAsync(CancellationToken cancellationToken) {
        _client.Ready += OnReadyAsync;
        _client.InteractionCreated += OnInteractionAsync;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        _client.Ready -= OnReadyAsync;
        _client.InteractionCreated -= OnInteractionAsync;
        return Task.CompletedTask;
    }

    private async Task OnReadyAsync() {
        try {
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _interactions.RegisterCommandsGloballyAsync();
            _logger.LogInformation("Slash commands registered");
        } catch(Exception ex) {
            _logger.LogError(ex, "Failed to register slash commands");
        }
    }

    
    private async Task OnInteractionAsync(SocketInteraction interaction) {
        try {
            var context = new SocketInteractionContext(_client, interaction);
            await _interactions.ExecuteCommandAsync(context, _services);
        } catch(Exception ex) {
            _logger.LogError(ex, "Error executing interaction");
        }
    }

}