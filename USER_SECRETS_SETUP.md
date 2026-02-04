# User Secrets Setup

The bot token is stored in user-secrets for security. Follow these steps to configure it:

## Setting up User Secrets

1. Navigate to the project directory:
```bash
cd /home/paun/Six/TheArchiver/TheArchiver
```

2. Initialize user secrets (if not already done):
```bash
dotnet user-secrets init
```

3. Set your Discord bot token:
```bash
dotnet user-secrets set "Startup:Token" "YOUR_DISCORD_BOT_TOKEN_HERE"
```

Replace `YOUR_DISCORD_BOT_TOKEN_HERE` with your actual Discord bot token.

## Verifying Configuration

To list all user secrets:
```bash
dotnet user-secrets list
```

To remove a secret:
```bash
dotnet user-secrets remove "Startup:Token"
```

To clear all secrets:
```bash
dotnet user-secrets clear
```

## Why User Secrets?

- **Security**: Keeps sensitive data out of source control
- **appsettings.json** is still valuable for:
  - FilterOptions (word filter pairs that update at runtime)
  - Logging configuration
  - ConnectionStrings
  - DevGuildId (non-sensitive development server ID)

User secrets are only for the bot token and other sensitive credentials.

