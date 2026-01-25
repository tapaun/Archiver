# TheArchiver

A Discord bot for archiving messages and filtering inappropriate content.

## Features

- **Message Filtering**: Automatically filters and replaces inappropriate keywords in messages
- **Dynamic Filter Management**: Add and remove filter words at runtime via slash commands
- **Message Archiving**: Archive and retrieve Discord messages
- **User Statistics**: Track user activity and message counts

## Setup

1. Clone the repository
2. Copy `TheArchiver/appsettings.template.json` to `TheArchiver/appsettings.json`
3. Edit `appsettings.json` and add your Discord bot token and guild ID
4. Add your filter words and replacements to the `SlurIndexPairs` section
5. Build and run the project

```bash
dotnet build
dotnet run --project TheArchiver
```

## Configuration

Edit `appsettings.json` (not tracked in git):

```json
{
  "Startup": {
    "Token": "YOUR_DISCORD_BOT_TOKEN",
    "DevGuildId": YOUR_GUILD_ID
  },
  "FilterOptions": {
    "SlurIndexPairs": {
      "badword": "replacement"
    }
  }
}
```

## Slash Commands

- `/add-filter` - Add a word to filter (Admin only)
- `/remove-filter` - Remove a filtered word (Admin only)

## Security Note

**Never commit `appsettings.json` to version control.** It contains sensitive data like your Discord bot token and filter configurations.

