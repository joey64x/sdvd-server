# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

JunimoServer is a dedicated server for Stardew Valley multiplayer. It runs the game headlessly inside Docker, allowing 24/7 multiplayer farms. The primary language is **C# (.NET 6.0)** targeting the SMAPI modding framework, with supporting tools in TypeScript (Bun) and shell scripts.

## First-Time Setup

1. Copy `.env.example` to `.env` and fill in Steam credentials (`STEAM_USERNAME`, `STEAM_PASSWORD`)
2. Run `make setup` to authenticate with Steam (handles Steam Guard interactively) and download game files
3. Run `docker compose up -d` to start the server

The `steam-auth` sidecar handles game file downloads and Steam ticket fetching. The server container depends on it being healthy before starting.

## Docker Image Version

The `docker-compose.yml` is designed for the current source architecture where `steam-auth` handles game downloads and authentication via gRPC. The published `latest` tag on Docker Hub may lag behind — if you encounter startup issues (e.g. `STEAM_GUARD_CODE: unbound variable`, wrong game paths, Node.js ticket fetcher timeouts), set `IMAGE_VERSION=preview` in `.env`. The `preview` tag is built from the latest source on every commit.

## Build & Development Commands

All orchestration is through the Makefile. The `.env` file provides Steam credentials and configuration.

```bash
make install              # Install dev dependencies (commitlint, lefthook git hooks)
make build                # Build Docker image (requires Steam credentials in .env)
make up                   # Build and start server
make down                 # Stop server
make logs                 # Tail server logs
make cli                  # Attach to interactive tmux-based server console
make setup                # First-time Steam authentication
make docs                 # Extract OpenAPI spec from image, start VitePress dev server
make build-test-client    # Build containerized test client for E2E tests
```

### Running Tests

Tests are **end-to-end** using xUnit + Testcontainers. They spin up Docker containers with the actual game. Both the server image (`make build`) and test client image (`make build-test-client`) must be built before running tests.

```bash
make test                                    # Run all E2E tests
make test FILTER=PasswordProtection          # Run tests matching class name
make test FILTER="Login_WithCorrectPassword" # Run a single test by method name
```

Tests run sequentially (no parallel execution) with stop-on-fail enabled. Results go to `TestResults/`.

### Build Configuration

- `BUILD_CONFIGURATION=Debug` (default for local) or `Release` (for CI/production)
- `IMAGE_VERSION=local` (default) — set to change the Docker tag
- `GamePath` for .NET compilation is resolved from `.env` → `GAME_PATH` via `Directory.Build.props`

## Architecture

### Mod Structure (`mod/JunimoServer/`)

The mod is a SMAPI mod. Entry point is `ModEntry.cs` which:
1. Sets up a DI container (`Microsoft.Extensions.DependencyInjection`)
2. Auto-discovers all `IModService` implementations via reflection
3. Registers and starts them as singletons
4. Registers chat commands and console commands

New services implement `IModService` (or extend `ModService`) and are auto-registered. Services live in `Services/<ServiceName>/` subdirectories.

**Key service areas:**
- **GameLoader/GameCreator/GameManager** — Game lifecycle (load saves, create worlds, manage state)
- **AlwaysOnServer** — Keeps the server running when no players are connected
- **AuthService** — Steam ticket validation via gRPC to the `steam-auth` sidecar service
- **CabinManager** — Automatic farmhand cabin placement with configurable strategies
- **Api** — REST HTTP API with WebSocket support; OpenAPI spec auto-generated via NSwag
- **ChatCommands/Commands** — In-game `!command` system and SMAPI console commands
- **PasswordProtection** — Server password with login attempts and auth timeouts
- **Roles** — Player permission system (admin, ban, etc.)
- **ServerOptim** — Headless rendering optimizations (NullDisplayDevice)
- **Lobby/SteamGameServer** — Steam lobby and direct IP connection management
- **CropSaver** — Preserves crops during server downtime using Harmony patches
- **HostAutomation** — Automated host activities (festival handling, hide host, etc.)

### Harmony Patching

The mod uses HarmonyLib extensively to patch game methods at runtime. Patches are typically defined as static methods in `*Overrides.cs` files within each service directory.

### Multi-Container Setup

`docker-compose.yml` defines three services:
- **server** — Main game server (Stardew Valley + SMAPI + JunimoServer mod, with VNC and API)
- **steam-auth** — Steam authentication sidecar (gRPC, built from `tools/steam-service/`)
- **discord-bot** — Optional Discord integration (TypeScript/Bun, `tools/discord-bot/`)

### Docker Build

Multi-stage Dockerfile at `docker/Dockerfile`:
1. Builds steam-service
2. Downloads Stardew Valley + SMAPI from Steam (using build secrets for credentials)
3. Compiles the JunimoServer mod and tools
4. Assembles runtime image with game + mod + VNC server

### Tools (`tools/`)

- **steam-service** — C# gRPC service for Steam authentication and game downloads
- **discord-bot** — TypeScript/Bun Discord bot for status display and chat relay
- **test-client** — C# game client used by E2E tests
- **dll-patcher** — Patches game DLLs for server compatibility
- **openapi-generator** — Extracts OpenAPI spec from the compiled mod

### Documentation (`docs/`)

VitePress site with Vue components. Run with `make docs` (requires a built Docker image to extract the OpenAPI spec).

## Commit Conventions

Uses [Conventional Commits](https://www.conventionalcommits.org/) enforced by commitlint + lefthook:

```
feat|fix|perf|revert|docs|style|chore|refactor|test|build|ci(scope): description
```

Subject case is unrestricted (allows acronyms like `PR`, `CLI`). Release automation via `release-please`.

## Configuration

Server settings are in `server-settings.json` (path configured via `SETTINGS_PATH` env var, defaults to `/data/settings/server-settings.json` in Docker). Settings are **read once at startup** — changes require a server restart.

Two categories of settings:
- **Game creation settings** (FarmName, FarmType, ProfitMargin, StartingCabins, SpawnMonstersAtNight) — only used when creating a new world; changing these has no effect on an existing save
- **Runtime settings** (MaxPlayers, CabinStrategy, SeparateWallets, AllowIpConnections, LobbyMode, VerboseLogging, AdminSteamIds) — applied on every startup

Environment variables in `.env` handle infrastructure concerns (Steam credentials, ports, VNC password, API toggle, Discord bot token, server password).

## Server Restarts

There is no graceful save-on-shutdown. Stardew Valley saves at the end of each in-game day (when the host goes to bed). Wait for an end-of-day save before restarting, or verify no players are connected. Restart with `docker compose restart server`.

## Discord Bot

The discord-bot service is optional. It requires a valid `DISCORD_BOT_TOKEN` (bot token from Discord Developer Portal, **not** the application public key). If the token is missing or invalid, the bot will crash-loop. If you don't need it, either remove it from `docker-compose.yml` or don't start it (`docker compose up -d server steam-auth`).
