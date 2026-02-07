using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TheArchiver.Configuration;
using TheArchiver.Discord.Client;
using TheArchiver.Discord.Handlers;
using TheArchiver.Discord.Interactions;
using TheArchiver.Services.Archiving;
using TheArchiver.Services.Embedding;
using TheArchiver.Services.Filtering;
using TheArchiver.Services.Information;
using RunMode = Discord.Interactions.RunMode;

namespace TheArchiver;

public class Program {
    public static async Task Main(string[] args) {
        var builder = Host.CreateApplicationBuilder(args);
        
        // Add JSON configuration
        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        
        // Add user secrets for development
        if (builder.Environment.IsDevelopment()) {
            builder.Configuration.AddUserSecrets<Program>();
        }

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Configure file path options from configuration
        builder.Services.Configure<FilePathOptions>(
            builder.Configuration.GetSection(FilePathOptions.SectionName));

        // Register services
        builder.Services.AddSingleton<UserEmbedBuilder>();
        builder.Services.AddSingleton<KeywordMessageFilter>();
        builder.Services.AddSingleton<MessageHandler>();
        
        // Register archiving services
        builder.Services.AddSingleton<DiscordInformationService>();
        builder.Services.AddSingleton<ChannelArchivingService>();
        builder.Services.AddSingleton<CategoryArchivingService>();
        
        // Register hosted services    
        builder.Services.AddHostedService<MessageListenerService>();
        builder.Services.AddHostedService<SlashCommandHandlerService>();
        builder.Services.AddHostedService<ArchiveTimerService>();
        
        // Configure Discord client
        builder.Services.AddDiscordHost((config, _) => {
            var token = builder.Configuration["BotToken"];
            if (string.IsNullOrWhiteSpace(token)) {
                throw new InvalidOperationException("BotToken must be set in user secrets. Run: dotnet user-secrets set \"BotToken\" \"your-token-here\"");
            }
            
            config.Token = token;
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

        // Build the host
        var host = builder.Build();
        
        // Initialize services that need to load data
        var filterService = host.Services.GetRequiredService<KeywordMessageFilter>();
        await filterService.LoadFiltersAsync();
        
        var categoryArchivingService = host.Services.GetRequiredService<CategoryArchivingService>();
        await categoryArchivingService.LoadStateAsync();
        
        var channelArchivingService = host.Services.GetRequiredService<ChannelArchivingService>();
        await channelArchivingService.LoadStateAsync();
        
        // Run the host
        await host.RunAsync();
    }
}
