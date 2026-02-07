# Configuration Guide

## Bot Token Setup

The bot token is stored in **user secrets** for security:

```bash
cd TheArchiver
dotnet user-secrets set "BotToken" "your-discord-bot-token-here"
```

User secrets are automatically loaded in Development environment (set via `Properties/launchSettings.json`).

## File Path Configuration

File paths are configured via `appsettings.json` using the Options pattern:

```json
{
  "FilePaths": {
    "FiltersPath": "filters.json",
    "ArchivingPath": "archiving.json",
    "ArchiveStatePath": "archive_state.json"
  }
}
```

### Default Behavior
- Paths are relative to the application's base directory (output folder)
- Files are automatically copied to output during build
- Absolute paths are also supported

### Overriding Paths

You can override paths via (in order of precedence):

1. **Environment variables**:
   ```bash
   export FilePaths__FiltersPath="/custom/path/filters.json"
   ```

2. **User secrets**:
   ```bash
   dotnet user-secrets set "FilePaths:FiltersPath" "/custom/path/filters.json"
   ```

3. **appsettings.json** (defaults)

## Configuration Files

### 1. `filters.json`
Contains word filters and their replacements:
```json
{
  "badword": "replacement",
  "another": "substitute"
}
```

Manage at runtime via Discord commands:
- `/add-filter` - Add a new filter
- `/remove-filter` - Remove an existing filter

### 2. `archiving.json`
Stores server archiving configurations (auto-managed):
```json
{
  "Servers": {
    "123456789": {
      "ArchiveCategoryId": 987654321
    }
  }
}
```

Managed via `/set-archive-category` command.

### 3. `archive_state.json`
Tracks channel archiving state (auto-generated and managed by the bot).

## Architecture

The configuration uses the standard .NET Options pattern:

```csharp
// FilePathOptions.cs
public class FilePathOptions {
    public const string SectionName = "FilePaths";
    public string FiltersPath { get; set; } = "filters.json";
    public string ArchivingPath { get; set; } = "archiving.json";
    public string ArchiveStatePath { get; set; } = "archive_state.json";
}

// Program.cs
builder.Services.Configure<FilePathOptions>(
    builder.Configuration.GetSection(FilePathOptions.SectionName));

// Service injection
public MyService(IOptions<FilePathOptions> options) {
    var path = FilePathOptions.ResolvePath(options.Value.FiltersPath);
}
```

This provides:
- ✅ Type-safe configuration
- ✅ Default values in code as fallback
- ✅ Source-controlled defaults in `appsettings.json`
- ✅ Override capability via user secrets/environment variables
