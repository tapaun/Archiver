# Configuration Guide

## Bot Token Setup

The bot token is now stored in **user secrets** for security. To set it up:

```bash
dotnet user-secrets set "BotToken" "your-discord-bot-token-here"
```

Run this command from the `TheArchiver` project directory.

## Configuration Files

The application uses two JSON configuration files:

### 1. `filters.json`
Located at: `TheArchiver/filters.json`

This file contains word filters and their replacements. Format:
```json
{
  "badword": "replacement",
  "another": "substitute"
}
```

You can add/remove filters at runtime using the Discord slash commands:
- `/add-filter` - Add a new filter (Admin only)
- `/remove-filter` - Remove an existing filter (Admin only)

### 2. `archiving.json`
Located at: `TheArchiver/archiving.json`

This file stores server archiving configurations. It's automatically managed by the bot when you use archiving commands:
- `/set-archive-category` - Configure the category for archived channels

Format:
```json
{
  "servers": {
    "123456789": {
      "ArchiveCategoryId": 987654321
    }
  }
}
```

### 3. `archive_state.json` (auto-generated)
This file is automatically created and managed by the bot to track channel archiving state.

## Migration from appsettings.json

The old `appsettings.json` file has been removed. Configuration is now split into:
- **Bot Token**: User secrets (secure)
- **Filters**: `filters.json` (persistent, editable)
- **Archiving**: `archiving.json` (auto-managed)

This separation improves security and makes the configuration more modular and maintainable.

