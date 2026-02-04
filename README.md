# TheArchiver

A Discord bot for archiving messages and filtering inappropriate content.

## Features

- **Message Filtering**: Automatically filters and replaces inappropriate keywords in messages
- **Dynamic Filter Management**: Add and remove filter words at runtime via slash commands
- **Channel Archiving**: Automatically archive channels every 24 hours
- **Category Management**: Configure archive categories per server
- **Server Information**: Get detailed server and user information

## Setup

### Prerequisites
- .NET 10.0 SDK
- A Discord bot token from [Discord Developer Portal](https://discord.com/developers/applications)

### Installation

1. Clone the repository
```bash
git clone <repository-url>
cd TheArchiver
```

2. Set up your bot token using user secrets (secure):
```bash
cd TheArchiver
dotnet user-secrets set "BotToken" "YOUR_DISCORD_BOT_TOKEN_HERE"
```

3. (Optional) Customize filters by editing `TheArchiver/filters.json`:
```json
{
  "badword": "replacement",
  "anotherbadword": "substitute"
}
```

4. Build and run:
```bash
dotnet build
dotnet run
```

## Configuration

### Bot Token (Secure)
The bot token is stored in **user secrets** (not in files). Set it using:
```bash
dotnet user-secrets set "BotToken" "YOUR_TOKEN"
```

### Filter Words
Edit `TheArchiver/filters.json` to add/remove filter words:
```json
{
  "word1": "replacement1",
  "word2": "replacement2"
}
```

Filters can also be managed at runtime using slash commands (see below).

### Archiving Configuration
The bot automatically manages `archiving.json` to store server archiving configurations. No manual editing needed.

## Slash Commands

### Filter Management (Admin Only)
- `/add-filter` - Add a word to filter with its replacement
- `/remove-filter` - Remove a filtered word

### Archiving Management
- `/set-archive-category` - Configure the category where archived channels are moved
- `/set-archive-channel` - Set up a channel for automatic archiving

### Information
- `/get-server-info` - Get detailed information about the server
- `/get-user-info` - Get detailed information about a user
- `/purge` - Purge messages from a channel

## Project Structure

```
TheArchiver/
├── filters.json              # Word filters and replacements
├── archiving.json           # Server archiving configurations (auto-managed)
├── archive_state.json       # Channel archiving state (auto-managed)
├── src/
│   ├── Program.cs           # Application entry point
│   ├── Discord/             # Discord client and handlers
│   ├── Services/            # Business logic services
│   └── ...
└── TheArchiver.csproj
```

## Development

The bot uses:
- **Discord.Net** for Discord API interactions
- **Microsoft.Extensions.Hosting** for service management
- **User Secrets** for secure configuration
- **Structured logging** with ILogger

## Security Notes

- ✅ Bot token is stored in **user secrets** (not in files)
- ✅ `filters.json` is tracked in git (customize as needed)
- ✅ `archiving.json` and `archive_state.json` are auto-generated
- ⚠️ Never commit sensitive tokens or credentials to version control

## Documentation

- [CONFIGURATION.md](CONFIGURATION.md) - Detailed configuration guide
- [SETUP.md](SETUP.md) - Quick setup instructions

## License

[Add your license here]
