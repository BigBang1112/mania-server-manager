# ManiaServerManager

[![Docker pulls](https://img.shields.io/docker/pulls/bigbang1112/mania-server-manager?style=for-the-badge&logo=docker)](https://hub.docker.com/r/bigbang1112/mania-server-manager)
[![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/bigbang1112/mania-server-manager?include_prereleases&style=for-the-badge&logo=github)](https://github.com/bigbang1112/mania-server-manager/releases)
[![GitHub last commit (branch)](https://img.shields.io/github/last-commit/bigbang1112/mania-server-manager/master?style=for-the-badge&logo=github)](#)

ManiaServerManager (MSM) is NOT a server controller! It is a stock server manager for all Trackmania and Shootmania versions.

You can easily open a communication between your server controller using this manager. In fact, it forces you to separate your server controller from your stock server.

This tool is meant to be run with Docker, but can also be integrated in other ways.

## How it works

The server is not executed immediately. Instead, a short-lived manager application is launched, which downloads all of the necessities and configures everything. The server is then launched afterwards, and the shell handles command-line argument propagation.

The management application is written in C# and was built with NativeAOT and trimmed, allowing fast startup time and a very small image. It turned out to be a convenient replacement for the Shell language.

## Advantaged over `docker-trackmania/forever` or `pyplanet/maniaplanet-docker` images

- All Nadeo game servers within a single image
- Checks for updates per restart, not per deployment - beneficial for ManiaPlanet title packs
- Specific versions can be picked, or the download sources can be modified (there's a use for it, soon^^)
- Alternatively, use the Windows executable with Wine (there's a use for it, soon^^)
- Many more possible options are configurable with environment variables
- Supports niche command line arguments like `/validatepath`
- Up-to-date base images

## Image variants

Various image variants are provided:

- Ubuntu Noble (default)
- Debian Bookworm (slim)
- Alpine (recommended for image size)

Multiple variants are available for `x86` (`amd64`) architectures only.

### Ubuntu Noble

This is the default for `latest`, specifically tagged as `noble`. It is a stable image that doesn't install anything additional to run Nadeo game servers. Ubuntu is known to be regularly updated with security patches.

### Debian Bookworm (slim)

This is the Debian variant, tagged as `bookworm-slim`, whose image is actually 1MB bigger than Noble for some reason. Use this only if you prefer Debian image bases in your orchestration or any other layer caching reasons.

### Alpine

Alpine is a recommended pick, tagged as `alpine`, crafted carefully to run Trackmania servers with just 24MB image size. There's a chance this variant is less stable, but no issues have been found yet.

It uses [frolvlad/alpine-glibc](https://github.com/Docker-Hub-frolvlad/docker-alpine-glibc) as a base so that breaking changes in future Alpine versions can still be handled by the server executable without much hassle.

## Experimental ARM64 emulation support

There's an ongoing experiment with ARM64 support to make it possible to run any Nadeo game server on a Raspberry PI or other low-cost devices. ARM64 is a future-proof variant of ARM, so it is supported over ARM32.

Alpine is not supported here for obvious complexity reasons.

## Wine for running Windows

Because there seem to be minor build differences between Windows and Linux dedicated server builds, there are additional images, suffixed with `-wine` to emulate the Windows build on Linux.

## Environment variables reference

### Required variables

#### Server configuration

- **`MSM_SERVER_TYPE`** - Server type, must be one of: `TM2020`, `ManiaPlanet`, `TMF`, or `TM` (TMNESWC/TMSX)
- **`MSM_ACCOUNT_LOGIN`** - Server account login
- **`MSM_ACCOUNT_PASSWORD`** - Server account password
- **`MSM_MATCH_SETTINGS`** - MatchSettings file path relative to `UserData/Maps/MatchSettings` OR `GameData/Tracks/MatchSettings`
  - OR `MSM_GAME_SETTINGS` - MatchSettings file path relative to `UserData/Maps` OR `GameData/Tracks`

Provided MatchSettings examples:

- [Official](OFFICIAL_MATCHSETTINGS.md)
- For Trackmania 2 official title packs:
  - `NadeoTimeAttack.txt`

#### ManiaPlanet-specific

- **`MSM_TITLE`** - Title pack ID (required when `MSM_SERVER_TYPE=ManiaPlanet`, example: `TMStadium@nadeo`)

#### TMNESWC/TMSX-specific

- `MSM_GAME` - Must be one of `nations`, `sunrise`, or `original`

### Optional variables

#### Basic configuration

- `MSM_SERVER_NAME` - Server name (default: `ManiaServerManager Server`)
- `MSM_SERVER_PASSWORD` - Server password (default: none)
- `MSM_DEDICATED_CFG` - Dedicated config filename (default: `dedicated_cfg.txt`)
- `MSM_REINSTALL` - Special variable to trigger server and title pack overwrite on the same version that's otherwise regularly triggered on server updates per restart (default: `False`)
- `MSM_LAN` - LAN mode (default: `False`)

#### Download hosts

- `MSM_SERVER_DOWNLOAD_HOST_ALL` - Override download host for all server types, if you host all types of servers on some URL (default: none)
- `MSM_SERVER_DOWNLOAD_HOST_TM2020` - Override download host for TM2020 servers (default: https://nadeo-download.cdn.ubi.com/trackmania)
- `MSM_SERVER_DOWNLOAD_HOST_MANIAPLANET` - Override download host for ManiaPlanet servers (default: http://files.v04.maniaplanet.com/server)
- `MSM_SERVER_DOWNLOAD_HOST_TMF` - Override download host for TMF servers (default: http://files2.trackmaniaforever.com)
- `MSM_SERVER_DOWNLOAD_HOST_TM` - Override download host for TMNESWC/TMSX servers (default: http://slig.free.fr/TM/dedicated)

#### Title pack configuration (ManiaPlanet)

- `MSM_IGNORE_TITLE_DOWNLOAD` - Skip title pack download (default: `False`)
- `MSM_TITLE_DOWNLOAD_HOST` - Title pack download host (default: https://maniaplanet.com/ingame/public/titles/download)

#### Server settings

- `MSM_CFG_SERVER_COMMENT` - Server description (default: none)
- `MSM_CFG_SERVER_HIDE_SERVER` - How to show/hide the server from the browser, values:
  - `AlwaysShown` (default)
  - `AlwaysHidden`
  - `HiddenFromNations`
- `MSM_CFG_SERVER_MAX_PLAYERS` (default: `32`)
- `MSM_CFG_SERVER_MAX_SPECTATORS` (default: `32`)
- `MSM_CFG_SERVER_PASSWORD_SPECTATOR` (default: none)
- `MSM_CFG_SERVER_KEEP_PLAYER_SLOTS` (default: `False`)
- `MSM_CFG_SERVER_LADDER_MODE` - values:
  - `Inactive` (default)
  - `Forced`
  - `Normal` (old value)
- `MSM_CFG_SERVER_ENABLE_P2P_UPLOAD` (default: `True`)
- `MSM_CFG_SERVER_ENABLE_P2P_DOWNLOAD` (default: `False`)
- `MSM_CFG_SERVER_CALLVOTE_TIMEOUT` - vote timeout in milliseconds (default: `60000`)
- `MSM_CFG_SERVER_CALLVOTE_RATIO` - vote ratio to succeed (default: `0.5`)
- `MSM_CFG_SERVER_ALLOW_MAP_DOWNLOAD` (default: `True`)
- `MSM_CFG_SERVER_AUTOSAVE_REPLAYS` (default: `False`)
- `MSM_CFG_SERVER_AUTOSAVE_VALIDATION_REPLAYS` (default: `False`)
- `MSM_CFG_SERVER_REFEREE_PASSWORD` (default: none)
- `MSM_CFG_SERVER_REFEREE_VALIDATION_MODE` - values:
  - `OnlyTop3` (default)
  - `AllPlayers`   
- `MSM_CFG_SERVER_USE_CHANGING_VALIDATION_SEED` (default: `False`)
- `MSM_CFG_SERVER_DISABLE_HORNS` (default: `False`)
- `MSM_CFG_SERVER_DISABLE_PROFILE_SKINS` (default: `False`)
- `MSM_CFG_SERVER_CLIENT_INPUTS_MAX_LATENCY` (default: `False`)
- `MSM_CFG_SERVER_LADDER_SERVER_LIMIT_MIN` - **only TMF** (default: `0`)
- `MSM_CFG_SERVER_LADDER_SERVER_LIMIT_MAX` - **only TMF** (default: `50000`)

#### Connection settings

- `MSM_CFG_CONFIG_CONNECTION_UPLOAD_RATE` (default: `102400`)
- `MSM_CFG_CONFIG_CONNECTION_DOWNLOAD_RATE` (default: `102400`)
- `MSM_CFG_CONFIG_WORKER_THREAD_COUNT` - **only TM2020** (default: `2`)
- `MSM_CFG_CONFIG_PACKETASSEMBLY_MULTITHREAD` - **only TM2020** (default: `True`)
- `MSM_CFG_CONFIG_PACKETASSEMBLY_PACKETS_PER_FRAME` - **only TM2020** (default: `60`)
- `MSM_CFG_CONFIG_PACKETASSEMBLY_FULL_PACKETS_PER_FRAME` - **only TM2020** (default: `30`)
- `MSM_CFG_CONFIG_DELAYED_VISUALS_S2C_SENDING_RATE` - **only TM2020** (default: `32`)
- `MSM_CFG_CONFIG_TRUST_CLIENT_SIMU_C2S_SENDING_RATE` - **only TM2020** (default: `64`)
- `MSM_CFG_CONFIG_ALLOW_SPECTATOR_RELAYS` (default: `False`)
- `MSM_CFG_CONFIG_P2P_CACHE_SIZE` (default: `600`)
- `MSM_CFG_CONFIG_SERVER_PORT` (default: `2350`)
- `MSM_CFG_CONFIG_SERVER_P2P_PORT` (default: `3450`)
- `MSM_CFG_CONFIG_CLIENT_PORT` (default: `0`)
- `MSM_CFG_CONFIG_USE_NAT_UPNP` (default: none)
- `MSM_CFG_CONFIG_GSP_NAME` (default: none)
- `MSM_CFG_CONFIG_GSP_URL` (default: none)
- `MSM_CFG_CONFIG_XMLRPC_PORT` (default: `5000`)
- `MSM_CFG_CONFIG_XMLRPC_ALLOW_REMOTE` - to enable XML-RPC communication. **Prefer specifying a concrete address** instead of just `True` (default: `False`)
- `MSM_CFG_CONFIG_BLACKLIST_URL` (default: none)
- `MSM_CFG_CONFIG_GUESTLIST_FILE_NAME` (default: none)
- `MSM_CFG_CONFIG_BLACKLIST_FILE_NAME` (default: none)
- `MSM_CFG_CONFIG_MINIMUM_CLIENT_BUILD` - **only TM2020/ManiaPlanet** (default: none)
- `MSM_CFG_CONFIG_DISABLE_COHERENCE_CHECKS` (default: `False`)
- `MSM_CFG_CONFIG_DISABLE_REPLAY_RECORDING` (default: `False`)
- `MSM_CFG_CONFIG_SAVE_ALL_INDIVIDUAL_RUNS` (default: `False`)
- `MSM_CFG_CONFIG_USE_PROXY` (default: `False`)
- `MSM_CFG_CONFIG_PROXY_URL` - **only TM2020/ManiaPlanet** (default: none)
- `MSM_CFG_CONFIG_PACKETASSEMBLY_THREAD_COUNT` - **only TM2020/ManiaPlanet** (default: `1`)
- `MSM_CFG_CONFIG_SCRIPTCLOUD_SOURCE` - **only ManiaPlanet**, values:
  - `NadeoCloud` (default)
  - `LocalDebug`
  - `XmlRpc`
- `MSM_CFG_CONFIG_PACK_MASK` - **only TMF** (default: `stadium`)
- `MSM_CFG_CONFIG_PROXY_LOGIN` - **only TMF/TM** (default: none)
- `MSM_CFG_CONFIG_PROXY_PASSWORD` - **only TMF/TM** (default: none)
- `MSM_CFG_CONFIG_CONNECTION_TYPE` - **only TM** (default: `DSL_16384_4096`)

Provided via command line arguments when starting the server:

- `MSM_FORCE_IP` (default: none)
- `MSM_BIND_IP` (default: none)

#### Authorization settings

These don't need to be changed if port 5000 is not publically accessible.

- `MSM_CFG_AUTHORIZATION_SUPERADMIN_NAME` (default: `SuperAdmin`)
- `MSM_CFG_AUTHORIZATION_SUPERADMIN_PASSWORD` - if you use `MSM_CFG_CONFIG_XMLRPC_ALLOW_REMOTE=True`, make sure to change it to something more secure! (default: `SuperAdmin`)
- `MSM_CFG_AUTHORIZATION_ADMIN_NAME` (default: `Admin`)
- `MSM_CFG_AUTHORIZATION_ADMIN_PASSWORD` - if you use `MSM_CFG_CONFIG_XMLRPC_ALLOW_REMOTE=True`, make sure to change it to something more secure! (default: `Admin`)
- `MSM_CFG_AUTHORIZATION_USER_NAME` (default: `User`)
- `MSM_CFG_AUTHORIZATION_USER_PASSWORD` (default: `User`)

#### Account settings

- `MSM_CFG_ACCOUNT_VALIDATION_KEY` - **only ManiaPlanet/TMF** (default: none)
- `MSM_CFG_ACCOUNT_NATION` - **only TM** (default: none)

#### Debug settings

- `MSM_VERBOSE_RPC` (default: `False`)
- `MSM_VERBOSE_RPC_FULL` (default: `False`)

## Example Docker Run

For TM2020:

```bash
docker run -d \
  -e MSM_SERVER_TYPE=TM2020 \
  -e MSM_ACCOUNT_LOGIN=your_login \
  -e MSM_ACCOUNT_PASSWORD=your_password \
  -e MSM_MATCH_SETTINGS=example.txt \
  -e MSM_SERVER_NAME="My ManiaServerManager Server" \
  -e MSM_CFG_SERVER_MAX_PLAYERS=255 \
  -p 2350:2350/tcp \
  -p 2350:2350/udp \
  -p 3450:3450/tcp \
  -p 3450:3450/udp \
  -v msm_archives:/app/data/archives \
  -v ./GameData:/app/data/versions/TM2020_Latest/GameData \
  bigbang1112/mania-server-manager:alpine
```

For ManiaPlanet:

```bash
docker run -d \
  -e MSM_SERVER_TYPE=ManiaPlanet \
  -e MSM_ACCOUNT_LOGIN=your_login \
  -e MSM_ACCOUNT_PASSWORD=your_password \
  -e MSM_TITLE=TMStadium@nadeo \
  -e MSM_MATCH_SETTINGS=NadeoTimeAttack.txt \
  -e MSM_SERVER_NAME="My ManiaServerManager Server" \
  -e MSM_CFG_SERVER_MAX_PLAYERS=255 \
  -p 2350:2350/tcp \
  -p 2350:2350/udp \
  -p 3450:3450/tcp \
  -p 3450:3450/udp \
  -v msm_archives:/app/data/archives \
  -v ./GameData:/app/data/versions/ManiaPlanet_Latest/GameData \
  bigbang1112/mania-server-manager:alpine
```

For TMF:

```bash
docker run -d \
  -e MSM_SERVER_TYPE=TMF \
  -e MSM_ACCOUNT_LOGIN=your_login \
  -e MSM_ACCOUNT_PASSWORD=your_password \
  -e MSM_MATCH_SETTINGS=Nations/NationsWhite.txt \
  -e MSM_SERVER_NAME="My ManiaServerManager Server" \
  -e MSM_CFG_SERVER_MAX_PLAYERS=255 \
  -p 2350:2350/tcp \
  -p 2350:2350/udp \
  -p 3450:3450/tcp \
  -p 3450:3450/udp \
  -v msm_archives:/app/data/archives \
  -v ./GameData:/app/data/versions/TMF_Latest/GameData \
  bigbang1112/mania-server-manager:alpine
```

For TMNESWC:

```bash
docker run -d \
  -e MSM_SERVER_TYPE=TM \
  -e MSM_ACCOUNT_LOGIN=your_login \
  -e MSM_ACCOUNT_PASSWORD=your_password \
  -e MSM_MATCH_SETTINGS=Internet/ProRace.txt \
  -e MSM_SERVER_NAME="My ManiaServerManager Server" \
  -e MSM_CFG_SERVER_MAX_PLAYERS=255 \
  -p 2350:2350/tcp \
  -p 2350:2350/udp \
  -p 3450:3450/tcp \
  -p 3450:3450/udp \
  -v msm_archives:/app/data/archives \
  -v ./GameData:/app/data/versions/TM_Latest/GameData \
  bigbang1112/mania-server-manager:alpine
```

Different ports need to be also configured with variables (due to the way master server handles servers):

```bash
docker run -d \
  -e MSM_SERVER_TYPE=TM2020 \
  -e MSM_ACCOUNT_LOGIN=your_login \
  -e MSM_ACCOUNT_PASSWORD=your_password \
  -e MSM_MATCH_SETTINGS=example.txt \
  -e MSM_SERVER_NAME="My ManiaServerManager Server" \
  -e MSM_CFG_SERVER_MAX_PLAYERS=255 \
  -e MSM_CFG_CONFIG_SERVER_PORT=2355 \
  -e MSM_CFG_CONFIG_SERVER_P2P_PORT=3455 \
  -p 2355:2355/tcp \
  -p 2355:2355/udp \
  -p 3455:3455/tcp \
  -p 3455:3455/udp \
  -v msm_archives:/app/data/archives \
  -v ./GameData:/app/data/versions/TM2020_Latest/GameData \
  bigbang1112/mania-server-manager:alpine
```

## Example Docker Compose

For TM2020:

```yml
services:
  server:
    image: bigbang1112/mania-server-manager:alpine
    restart: unless-stopped
    environment:
      MSM_SERVER_TYPE: TM2020
      MSM_ACCOUNT_LOGIN: your_login
      MSM_ACCOUNT_PASSWORD: your_password
      MSM_MATCH_SETTINGS: example.txt
      MSM_SERVER_NAME: My ManiaServerManager Server
      MSM_CFG_SERVER_MAX_PLAYERS: 255
    ports:
      - "2350:2350/tcp"
      - "2350:2350/udp"
      - "3450:3450/tcp"
      - "3450:3450/udp"
    volumes:
      - msm_archives:/app/data/archives
      - ./GameData:/app/data/versions/TM2020_Latest/GameData
volumes:
  msm_archives:
```

For ManiaPlanet:

```yml
services:
  server:
    image: bigbang1112/mania-server-manager:alpine
    restart: unless-stopped
    environment:
      MSM_SERVER_TYPE: ManiaPlanet
      MSM_ACCOUNT_LOGIN: your_login
      MSM_ACCOUNT_PASSWORD: your_password
      MSM_TITLE: TMStadium@nadeo
      MSM_MATCH_SETTINGS: NadeoTimeAttack.txt
      MSM_SERVER_NAME: My ManiaServerManager Server
      MSM_CFG_SERVER_MAX_PLAYERS: 255
    ports:
      - "2350:2350/tcp"
      - "2350:2350/udp"
      - "3450:3450/tcp"
      - "3450:3450/udp"
    volumes:
      - msm_archives:/app/data/archives
      - ./GameData:/app/data/versions/ManiaPlanet_Latest/GameData
volumes:
  msm_archives:
```

For TMF:

```yml
services:
  server:
    image: bigbang1112/mania-server-manager:alpine
    restart: unless-stopped
    environment:
      MSM_SERVER_TYPE: TMF
      MSM_ACCOUNT_LOGIN: your_login
      MSM_ACCOUNT_PASSWORD: your_password
      MSM_MATCH_SETTINGS: Nations/NationsWhite.txt
      MSM_SERVER_NAME: My ManiaServerManager Server
      MSM_CFG_SERVER_MAX_PLAYERS: 255
    ports:
      - "2352:2350/tcp"
      - "2352:2350/udp"
      - "3452:3450/tcp"
      - "3452:3450/udp"
    volumes:
      - msm_archives:/app/data/archives
      - ./GameData:/app/data/versions/TMF_Latest/GameData
volumes:
  msm_archives:
```

For TMNESWC:

```yml
services:
  server:
    image: bigbang1112/mania-server-manager:alpine
    restart: unless-stopped
    environment:
      MSM_SERVER_TYPE: TM
      MSM_ACCOUNT_LOGIN: your_login
      MSM_ACCOUNT_PASSWORD: your_password
      MSM_MATCH_SETTINGS: Internet/ProRace.txt
      MSM_SERVER_NAME: My ManiaServerManager Server
      MSM_CFG_SERVER_MAX_PLAYERS: 255
    ports:
      - "2353:2350/tcp"
      - "2353:2350/udp"
      - "3453:3450/tcp"
      - "3453:3450/udp"
    volumes:
      - msm_archives:/app/data/archives
      - ./GameData:/app/data/versions/TM_Latest/GameData
volumes:
  msm_archives:
```

Different ports need to be also configured with variables (due to the way master server handles servers):

```yml
services:
  server:
    image: bigbang1112/mania-server-manager:alpine
    restart: unless-stopped
    environment:
      MSM_SERVER_TYPE: TM2020
      MSM_ACCOUNT_LOGIN: your_login
      MSM_ACCOUNT_PASSWORD: your_password
      MSM_MATCH_SETTINGS: example.txt
      MSM_SERVER_NAME: My ManiaServerManager Server
      MSM_CFG_SERVER_MAX_PLAYERS: 255
      MSM_CFG_CONFIG_SERVER_PORT: 2355
      MSM_CFG_CONFIG_SERVER_P2P_PORT: 3455
    ports:
      - "2355:2355/tcp"
      - "2355:2355/udp"
      - "3455:3455/tcp"
      - "3455:3455/udp"
    volumes:
      - msm_archives:/app/data/archives
      - ./GameData:/app/data/versions/TM2020_Latest/GameData
volumes:
  msm_archives:
```

## Quirks to be aware of on host network

If you decide to use host for simplicity/performance, make sure that:

- You don't forget to change the XML-RPC port for each new server (if you want separate communication)
- That your XML-RPC ports are behind a firewall if you set `MSM_CFG_CONFIG_XMLRPC_ALLOW_REMOTE=True`
- If you need to communicate XML-RPC remotely from any address (there are minimal reasons), absolutely make sure to change `MSM_CFG_AUTHORIZATION_SUPERADMIN_PASSWORD` and `MSM_CFG_AUTHORIZATION_ADMIN_PASSWORD`
