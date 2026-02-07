# TheArchiver

A Discord bot for archiving messages and filtering inappropriate content.

## Features

- **Message Filtering**: Automatically filters and replaces inappropriate keywords in messages
- **Dynamic Filter Management**: Add and remove filter words at runtime via slash commands
- **Channel Archiving**: Automatically archive channels every 24 hours
- **Category Management**: Configure archive categories per server
- **Server Information**: Get detailed server and user information

## Quick Start

### Prerequisites
- .NET 10.0 SDK
- A Discord bot token from [Discord Developer Portal](https://discord.com/developers/applications)

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd TheArchiver/TheArchiver
```

2. Set up your bot token using user secrets:
```bash
dotnet user-secrets set "BotToken" "YOUR_DISCORD_BOT_TOKEN_HERE"
```

3. Run the bot:
```bash
dotnet run
```

That's it! The bot will automatically use the default configuration from `appsettings.json`.

## Configuration

See [CONFIGURATION.md](CONFIGURATION.md) for detailed configuration options.

### Bot Token (Required)
```bash
dotnet user-secrets set "BotToken" "YOUR_TOKEN"
```

### File Paths (Optional)
File paths are configured in `appsettings.json` with sensible defaults. You can override them via:
- `appsettings.json` (source-controlled defaults)
- User secrets (`dotnet user-secrets set "FilePaths:FiltersPath" "/custom/path"`)
- Environment variables (`FilePaths__FiltersPath=/custom/path`)

## Slash Commands

### Filter Management (Admin Only)
- `/add-filter` - Add a word to filter with its replacement
- `/remove-filter` - Remove a filtered word

### Archiving Management
- `/set-archive-category` - Configure the category where archived channels are moved
- `/set-archiving-channel` - Set up a channel for automatic 24-hour archiving

### Information
- `/get-server-info` - Get detailed information about the server
- `/get-user-info` - Get detailed information about a user
- `/purge` - Purge messages from a channel

## Project Structure

```
TheArchiver/
├── TheArchiver.csproj        # Project file with global usings
├── Program.cs                # Application entry point
├── appsettings.json          # Default configuration (file paths)
├── filters.json              # Word filters and replacements
├── archiving.json            # Server archiving configurations (auto-managed)
├── Configuration/
│   └── FilePathOptions.cs    # Configuration options class
├── Discord/
│   ├── Client/               # Discord client services
│   ├── Handlers/             # Message handlers
│   └── Interactions/         # Slash command modules
├── Services/
│   ├── Archiving/            # Channel/category archiving
│   ├── Embedding/            # Embed builders
│   ├── Filtering/            # Keyword filtering
│   └── Information/          # Server/user info services
└── Properties/
    └── launchSettings.json   # Development environment settings
```

## Development

The bot uses:
- **Discord.NET** - Discord API wrapper
- **Discord.Addons.Hosting** - Hosted service integration
- **Microsoft.Extensions.Hosting** - Generic host for DI and configuration
- **IOptions pattern** - Type-safe configuration via `IOptions<FilePathOptions>`

### Environment
The `launchSettings.json` automatically sets `DOTNET_ENVIRONMENT=Development` when running with `dotnet run`, enabling user secrets.

## License

MIT
