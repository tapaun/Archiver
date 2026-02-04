# Setup Instructions

## Quick Start

1. **Set your bot token** (required):
   ```bash
   cd /home/paun/Six/TheArchiver/TheArchiver
   dotnet user-secrets set "BotToken" "YOUR_DISCORD_BOT_TOKEN_HERE"
   ```

2. **Run the bot**:
   ```bash
   dotnet run
   ```

## What Changed

### Removed ✂️
- ❌ `appsettings.json` - No longer needed
- ❌ `appsettings.example.json` - No longer needed
- ❌ `Common/Options/` directory - All Options classes removed
- ❌ `INamedOptions` interface - No longer needed

### Added ✨
- ✅ `filters.json` - Contains word filters and replacements
- ✅ `archiving.json` - Stores server archiving configurations
- ✅ `CONFIGURATION.md` - Documentation for new configuration system

### Changed 🔄
- **Bot Token**: Now stored securely in user secrets instead of appsettings.json
- **KeywordMessageFilter**: Loads filters directly from `filters.json`
- **CategoryArchivingService**: Persists data to `archiving.json`
- **Program.cs**: Simplified - no more Options configuration, loads JSON files directly

## Configuration Files Location

Both JSON files are at the project root:
- `/home/paun/Six/TheArchiver/TheArchiver/filters.json`
- `/home/paun/Six/TheArchiver/TheArchiver/archiving.json`

They are automatically copied to the output directory during build.

## Benefits

✨ **Better Security**: Bot token is in user secrets, not committed to git
📁 **Simpler Structure**: Two focused JSON files instead of complex nested configuration
🔧 **More Maintainable**: Direct JSON manipulation without Options pattern overhead
🚀 **Runtime Editable**: Filters can be added/removed via Discord commands and persist immediately

