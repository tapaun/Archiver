using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheArchiver.Common.Options;
using TheArchiver.Discord.Client;
using TheArchiver.Discord.Handlers;
using TheArchiver.Discord.Interactions;
using TheArchiver.Discord.Interactions.Modules;
using TheArchiver.Services.Archiving;
using TheArchiver.Services.Embedding;
using TheArchiver.Services.Filtering;
using TheArchiver.Services.Information;
using RunMode = Discord.Interactions.RunMode;

namespace TheArchiver;

public class Program {
    public static async Task Main(string[] args) {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure options
        builder.Services
            .AddOptions<StartupOptions>()
            .Bind(builder.Configuration.GetSection(StartupOptions.GetSectionName()))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Token), "Startup:Token must be set")
            .ValidateOnStart();

        // Configure filter options
        builder.Services
            .AddOptions<FilterOptions>()
            .Bind(builder.Configuration.GetSection(FilterOptions.GetSectionName()))
            .ValidateOnStart();
        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Register services
        builder.Services.AddSingleton<UserEmbedBuilder>();
        builder.Services.AddSingleton<KeywordMessageFilter>();
        builder.Services.AddSingleton<MessageHandler>();
        
        // Register archiving services and interaction modules
        builder.Services.AddSingleton<DiscordInformationService>();
        builder.Services.AddSingleton<ChannelArchivingService>();
        builder.Services.AddSingleton<CategoryArchivingService>();
        builder.Services.AddSingleton<SetArchivingCategoryModule>();
        builder.Services.AddSingleton<SetArchivingChannelModule>();
        builder.Services.AddSingleton<GetServerInformationModule>();
        builder.Services.AddSingleton<GetUserInformationModule>();
        builder.Services.AddSingleton<PurgeModule>();
        builder.Services.AddSingleton<AddFilterModule>();
        builder.Services.AddSingleton<RemoveFilterModule>();
        
        // Register  hosted services    
        builder.Services.AddHostedService<MessageListenerService>();
        builder.Services.AddHostedService<SlashCommandHandlerService>();
        builder.Services.AddHostedService<ArchiveTimerService>();
        
        // Configure Discord client
        builder.Services.AddDiscordHost((config, services) => {
            var startupOptions = services.GetRequiredService<IOptions<StartupOptions>>().Value;
            config.Token = startupOptions.Token;
            config.SocketConfig = new DiscordSocketConfig {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.Guilds |
                                 GatewayIntents.GuildMessages |
                                 GatewayIntents.MessageContent |
                                 GatewayIntents.GuildMembers |
                                 GatewayIntents.AllUnprivileged
            };
        });
        
        // Configure interaction service
        builder.Services.AddInteractionService((config, _) => {
            config.LogLevel = LogSeverity.Info;
            config.DefaultRunMode = RunMode.Async;
        });

        // Build and run the host
        var host = builder.Build();
        await host.RunAsync();
    }
}
