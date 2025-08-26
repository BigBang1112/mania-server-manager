# ManiaServerManager

[![Docker pulls](https://img.shields.io/docker/pulls/bigbang1112/mania-server-manager?style=for-the-badge&logo=docker)](https://hub.docker.com/r/bigbang1112/mania-server-manager)
[![GitHub last commit (branch)](https://img.shields.io/github/last-commit/bigbang1112/mania-server-manager/main?style=for-the-badge&logo=github)](https://github.com/BigBang1112/mania-server-manager)

ManiaServerManager (MSM) is NOT a server controller! It is a stock server manager for all Trackmania and Shootmania versions.

You can easily open a communication between the server and a controller using this manager - examples are provided in the Samples folder. In fact, it forces you to separate your server controller from your stock server.

This tool is meant to be run with Docker, but it can also be integrated in other ways.

## How it works

The server is not executed immediately. Instead, a short-lived manager application is launched, which downloads all of the necessities and configures everything. The server is then launched afterwards, and the shell handles command-line argument propagation.

The management application is written in C# and was built with NativeAOT and trimmed, allowing fast startup time and a very small image. It turned out to be a convenient replacement for the Shell language.

## Differences from other Trackmania server images

- All Nadeo game server types handled within a single image
- Manage all of your servers within a single volume
- Checks for updates per restart, not per deployment - beneficial for ManiaPlanet title packs, also avoids waiting on image updates
- **Support for arm64 to run servers on Raspberry Pi**
- Specific versions can be picked, or the download sources can be modified (there's a use for it, soon^^)
- Alternatively, use the Windows executable with Wine (there's a use for it, soon^^)
- Many more possible options are configurable with environment variables
- Supports niche command line arguments like `/validatepath`
- Up-to-date base images, built every week

## Image variants

Various image variants are provided:

- Ubuntu Noble (default) + Plucky (experimental)
- Debian Bookworm (slim)
- Alpine (recommended for image size)
- Fedora

Multiple variants are available for `x86` (`amd64`) architectures only.

### Ubuntu Noble (24.04 LTS)

[![Docker Image Size (tag)](https://img.shields.io/docker/image-size/bigbang1112/mania-server-manager/noble?style=flat-square&logo=docker)](https://hub.docker.com/r/bigbang1112/mania-server-manager/tags)

This is the default for `latest`, specifically tagged as `noble`. It is a stable image that doesn't install anything additional to run Nadeo game servers. Ubuntu is known to be regularly updated with security patches.

### Debian Bookworm (slim)

[![Docker Image Size (tag)](https://img.shields.io/docker/image-size/bigbang1112/mania-server-manager/bookworm-slim?style=flat-square&logo=docker)](https://hub.docker.com/r/bigbang1112/mania-server-manager/tags)

This is the Debian variant, tagged as `bookworm-slim`, whose image is actually 1MB bigger than Noble for some reason. Use this only if you prefer Debian image bases in your orchestration or any other layer caching reasons.

### Alpine

[![Docker Image Size (tag)](https://img.shields.io/docker/image-size/bigbang1112/mania-server-manager/alpine?style=flat-square&logo=docker)](https://hub.docker.com/r/bigbang1112/mania-server-manager/tags)

Alpine is a recommended pick, tagged as `alpine`, crafted carefully to run Trackmania servers with just 24MB image size. There's a chance this variant is less stable, but no issues have been found yet.

It uses [frolvlad/alpine-glibc](https://github.com/Docker-Hub-frolvlad/docker-alpine-glibc) as a base so that breaking changes in future Alpine versions can still be handled by the server executable without much hassle.

### Other images

Fedora [`fedora`] and Ubuntu Plucky (25.04) [`plucky`] are experimental images with updated glibc (similarly to Alpine Linux) used for testing physics calculation differences.

## Experimental arm64 emulation support

[![Docker Image Size (tag)](https://img.shields.io/docker/image-size/bigbang1112/mania-server-manager/noble?arch=arm64&style=flat-square&logo=docker)](https://hub.docker.com/r/bigbang1112/mania-server-manager/tags)

There's an ongoing experiment with arm64 support to make it possible to run any Nadeo game server on a Raspberry Pi or other low-cost devices. It uses [box64](https://github.com/ptitSeb/box64) and the [apt repository by Ryan Fortner](https://github.com/ryanfortner/box64-debs).

Supported tags with arm64 are `latest`/`noble`, `plucky`, and `bookworm-slim`.

Tested on Raspberry Pi 5 8GB, there are some notes to take in count:

- TM2020 and ManiaPlanet work fine first try
- TMF needs to be (re)started at least 3 times to resolve directory issues (Cache is always broken though)
- TM (ESWC for example) might need way more (re)starts, if even possible to overcome it

Raspberry Pi Zero 2 W might work as well, but it hasn't been tested.

## Wine for running Windows

[![Docker Image Size (tag)](https://img.shields.io/docker/image-size/bigbang1112/mania-server-manager/noble-wine?style=flat-square&logo=docker)](https://hub.docker.com/r/bigbang1112/mania-server-manager/tags)

Because there seem to be minor build differences between Windows and Linux dedicated server builds, there are additional images, suffixed with `-wine` to emulate the Windows build on Linux. Currently only `noble-wine`.

## Environment variables reference

### Required variables

#### Server configuration

- **`MSM_SERVER_TYPE`** - Server type, must be one of: `TM2020`, `ManiaPlanet`, `TMF`, or `TM` (TMNESWC/TMSX)
- **`MSM_ACCOUNT_LOGIN`** - Server account login
- **`MSM_ACCOUNT_PASSWORD`** - Server account password
- **`MSM_MATCH_SETTINGS`** - MatchSettings file path relative to `UserData/Maps/MatchSettings` OR `GameData/Tracks/MatchSettings`
  - OR `MSM_GAME_SETTINGS` - MatchSettings file path relative to `UserData/Maps` OR `GameData/Tracks`
  - To **copy from base MatchSettings into your own one** (to avoid overwrites when adding more using controllers), **use `MSM_MATCH_SETTINGS_BASE`** with one of the examples below. By setting this, `MSM_MATCH_SETTINGS` file doesn't have to exist, it will be created.

Provided MatchSettings examples:

- For TM2020:
  - `MinimalTimeAttack.txt` - base for a TimeAttack server with custom maps, only has Training - 01
- For Trackmania Forever:
  - `MinimalTimeAttack.txt` - base for a TimeAttack server with custom maps, only has A01-Race
  - `MinimalCup.txt` - base for a Cup server with custom maps, only has A01-Race
  - `MinimalRounds.txt` - base for a Rounds server with custom maps, only has A01-Race
  - `MinimalLaps.txt` - base for a Laps server with custom maps, only has A08-Endurance
  - `MinimalTeams.txt` - base for a Teams server with custom maps, only has A01-Race
  - `MinimalStunts.txt` - base for a Stunts server with custom maps, only has StuntA1
  - `NadeoTimeAttack.txt` - TimeAttack server with all Nadeo maps from the server installation (no StarTrack)
  - `NadeoCup.txt` - Cup server with all Nadeo maps from the server installation (no StarTrack)
  - `NadeoRounds.txt` - Rounds server with all Nadeo maps from the server installation (no StarTrack)
  - `NadeoLaps.txt` - Laps server with multilap Nadeo maps from the server installation (no StarTrack)
  - `NadeoTeams.txt` - Teams server with all Nadeo maps from the server installation (no StarTrack)
  - `NadeoStunts.txt` - Stunts server with all Nadeo stunt maps from the server installation
  - `NadeoStadiumTimeAttack.txt` - TimeAttack server like `NadeoTimeAttack.txt` but with Stadium tracks only
  - `NadeoStadiumCup.txt` - Cup server like `NadeoCup.txt` but with Stadium tracks only
  - `NadeoStadiumRounds.txt` - Rounds server like `NadeoRounds.txt` but with Stadium tracks only
  - `NadeoStadiumLaps.txt` - Laps server wlike `NadeoLaps.txt` but with Stadium tracks only
  - `NadeoStadiumTeams.txt` - Teams server like `NadeoTeams.txt` but with Stadium tracks only
  - `NadeoStadiumStunts.txt` - Stunts server like `NadeoStunts.txt` but with Stadium tracks only
- For Trackmania 2 official title packs:
  - `MinimalTimeAttack.txt` - base for a TimeAttack server with custom maps, only has A01
  - `MinimalCup.txt` - base for a Cup server with custom maps, only has A01
  - `MinimalRounds.txt` - base for a Rounds server with custom maps, only has A01
  - `MinimalLaps.txt` - base for a Laps server with custom maps, only has A05
  - `NadeoTimeAttack.txt` - TimeAttack server with official maps in traditional format
  - `NadeoCup.txt` - Cup server with official maps in traditional format
  - `NadeoRounds.txt` - Rounds server with official maps in traditional format
  - `NadeoLaps.txt` - Laps server with official maps in traditional format
- For Trackmania Nations ESWC:
  - `MinimalTimeAttack.txt` - base for a TimeAttack server with custom maps, only has B-0
  - `MinimalRounds.txt` - base for a Rounds server with custom maps, only has B-0
  - `MinimalTeams.txt` - base for a Teams server with custom maps, only has B-0
  - `NadeoTimeAttack.txt` - TimeAttack server with all Nadeo maps from the server installation
  - `NadeoRounds.txt` - Teams server with all Nadeo maps from the server installation
  - `NadeoTeams.txt` - Teams server with all Nadeo maps from the server installation
- [Official](OFFICIAL_MATCHSETTINGS.md) - **only TMF/TM ones can be used as a base**

#### ManiaPlanet-specific

- **`MSM_TITLE`** - Title pack ID (required when `MSM_SERVER_TYPE=ManiaPlanet`, example: `TMStadium@nadeo`)

#### TMNESWC/TMSX-specific

- `MSM_GAME` - Must be one of `nations`, `sunrise`, or `original`

### Optional variables

#### Basic configuration

- `MSM_SERVER_IDENTIFIER` - Name of the server's folder (default: `{MSM_SERVER_TYPE}_{MSM_SERVER_VERSION}`)
- `MSM_SERVER_NAME` - Server name (default: `ManiaServerManager Server`)
- `MSM_SERVER_PASSWORD` - Server password (default: none)
- `MSM_DEDICATED_CFG` - Dedicated config filename (default: `dedicated_cfg.txt`)
- `MSM_LAN` - LAN mode (default: `False`)
- `MSM_REINSTALL` - Special variable to trigger server and title pack overwrite on the same version that's otherwise regularly triggered on server updates per restart (default: `False`)

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
  - `Inactive`
  - `Forced` (default)
  - `Normal` (old value)
- `MSM_CFG_SERVER_ENABLE_P2P_UPLOAD` (default: `True`)
- `MSM_CFG_SERVER_ENABLE_P2P_DOWNLOAD` (default: `False`)
- `MSM_CFG_SERVER_CALLVOTE_TIMEOUT` - vote timeout in milliseconds (default: `60000`)
- `MSM_CFG_SERVER_CALLVOTE_RATIO` - vote ratio to succeed (default: `0.5`)
- `MSM_CFG_SERVER_CALLVOTE_RATIOS` - individual ratios, formatted as `Command1=0.8;Command2=0.6`... supports `;` and `,` (default: `Ban=-1`)
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
- `MSM_CFG_CONFIG_PACKMASK` - **only TMF** (default: `stadium`)
- `MSM_CFG_CONFIG_PROXY_LOGIN` - **only TMF/TM** (default: none)
- `MSM_CFG_CONFIG_PROXY_PASSWORD` - **only TMF/TM** (default: none)
- `MSM_CFG_CONFIG_CONNECTION_TYPE` - **only TM** (default: `DSL_16384_4096`)

Provided via command line arguments when starting the server:

- `MSM_FORCE_IP` (default: none)
- `MSM_BIND_IP` (default: none)
- `MSM_JOIN` (default: none)
- `MSM_JOIN_PASSWORD` (default: none)
- `MSM_LOAD_CACHE` (default: `False`) - loads the "checksum.txt" instead of recomputing it, to speed up first launch time if P2P is enabled. DO NOT USE if you run several servers in the same directory!

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

#### Specialized settings

- `MSM_SERVER_VERSION` (default: `Latest`)
- `MSM_VALIDATE_PATH` - specify validation path, works only for ManiaPlanet and TM2020 (this invalidates previous server variable requirements) (default: none)
- `MSM_PARSE_GBX` - specify Gbx file path relative from work directory (this invalidates previous server variable requirements) (default: none)
- `MSM_ONLY_SETUP` - only run the setup without running the server (default: `False`)
- `MSM_ONLY_SERVER_LOG` - avoid logging anything than server's log content and stdout (default: `False`)
- `MSM_ONLY_STDOUT` - avoid logging server's log content, only print stdout, useful for `MSM_VALIDATE_PATH` or `MSM_PARSE_GBX` combined with `MSM_ONLY_SERVER_LOG` (default: `False`)
- `MSM_SKIP_DEDICATED_CFG` - skips dedicated_cfg creation/overwrite (default: `False`)

## Docker setup

Images use the `app` user like `app:app` (in UID form: `1654:1654`). Root-less approach was chosen for security reasons and recommended practices.

For volumes, make sure the `app` user has access to the places where you declare the volumes. You might also need to precreate the host folders and set the running user of the container (`--user` in Docker Run, `user:` in Docker Compose), so that Docker doesn't create them with its own ownership rules, which then the `app` cannot access.

You also cannot create a volume further than the `versions` folder. I'm investigating how to improve this so that servers don't have to see each other.

## Example Docker Run

For TM2020:

```bash
docker run -d \
  -e MSM_SERVER_TYPE=TM2020 \
  -e MSM_SERVER_IDENTIFIER=MyServer \
  -e MSM_ACCOUNT_LOGIN=your_login \
  -e MSM_ACCOUNT_PASSWORD=your_password \
  -e MSM_MATCH_SETTINGS=MapList.txt \
  -e MSM_MATCH_SETTINGS_BASE=MinimalTimeAttack.txt \
  -e MSM_SERVER_NAME="My ManiaServerManager Server" \
  -e MSM_CFG_SERVER_MAX_PLAYERS=255 \
  -p 2350:2350/tcp \
  -p 2350:2350/udp \
  -p 3450:3450/tcp \
  -p 3450:3450/udp \
  -v msm_archives:/app/data/archives \
  -v ./servers:/app/data/servers \
  bigbang1112/mania-server-manager:alpine
```

For ManiaPlanet:

```bash
docker run -d \
  -e MSM_SERVER_TYPE=ManiaPlanet \
  -e MSM_SERVER_IDENTIFIER=MyServer \
  -e MSM_ACCOUNT_LOGIN=your_login \
  -e MSM_ACCOUNT_PASSWORD=your_password \
  -e MSM_TITLE=TMStadium@nadeo \
  -e MSM_MATCH_SETTINGS=MapList.txt \
  -e MSM_MATCH_SETTINGS_BASE=NadeoTimeAttack.txt \
  -e MSM_SERVER_NAME="My ManiaServerManager Server" \
  -e MSM_CFG_SERVER_MAX_PLAYERS=255 \
  -p 2350:2350/tcp \
  -p 2350:2350/udp \
  -p 3450:3450/tcp \
  -p 3450:3450/udp \
  -v msm_archives:/app/data/archives \
  -v ./servers:/app/data/servers \
  bigbang1112/mania-server-manager:alpine
```

For TMF (for United maps, don't forget to set `MSM_CFG_CONFIG_PACKMASK` to `united`):

```bash
docker run -d \
  -e MSM_SERVER_TYPE=TMF \
  -e MSM_SERVER_IDENTIFIER=MyServer \
  -e MSM_ACCOUNT_LOGIN=your_login \
  -e MSM_ACCOUNT_PASSWORD=your_password \
  -e MSM_MATCH_SETTINGS=tracklist.txt \
  -e MSM_MATCH_SETTINGS_BASE=NadeoStadiumTimeAttack.txt \
  -e MSM_SERVER_NAME="My ManiaServerManager Server" \
  -e MSM_CFG_SERVER_MAX_PLAYERS=255 \
  -p 2350:2350/tcp \
  -p 2350:2350/udp \
  -p 3450:3450/tcp \
  -p 3450:3450/udp \
  -v msm_archives:/app/data/archives \
  -v ./servers:/app/data/servers \
  bigbang1112/mania-server-manager:alpine
```

For TMNESWC:

```bash
docker run -d \
  -e MSM_SERVER_TYPE=TM \
  -e MSM_SERVER_IDENTIFIER=MyServer \
  -e MSM_ACCOUNT_LOGIN=your_login \
  -e MSM_ACCOUNT_PASSWORD=your_password \
  -e MSM_CFG_ACCOUNT_NATION=CZE \
  -e MSM_MATCH_SETTINGS=tracklist.txt \
  -e MSM_MATCH_SETTINGS_BASE=NadeoTimeAttack.txt \
  -e MSM_SERVER_NAME="My ManiaServerManager Server" \
  -e MSM_CFG_SERVER_MAX_PLAYERS=255 \
  -p 2350:2350/tcp \
  -p 2350:2350/udp \
  -p 3450:3450/tcp \
  -p 3450:3450/udp \
  -v msm_archives:/app/data/archives \
  -v ./servers:/app/data/servers \
  bigbang1112/mania-server-manager:alpine
```

Different ports need to be also configured with variables (due to the way master server handles servers):

```bash
docker run -d \
  -e MSM_SERVER_TYPE=TM2020 \
  -e MSM_SERVER_IDENTIFIER=MyServer \
  -e MSM_ACCOUNT_LOGIN=your_login \
  -e MSM_ACCOUNT_PASSWORD=your_password \
  -e MSM_MATCH_SETTINGS=MapList.txt \
  -e MSM_MATCH_SETTINGS_BASE=example.txt \
  -e MSM_SERVER_NAME="My ManiaServerManager Server" \
  -e MSM_CFG_SERVER_MAX_PLAYERS=255 \
  -e MSM_CFG_CONFIG_SERVER_PORT=2355 \
  -e MSM_CFG_CONFIG_SERVER_P2P_PORT=3455 \
  -p 2355:2355/tcp \
  -p 2355:2355/udp \
  -p 3455:3455/tcp \
  -p 3455:3455/udp \
  -v msm_archives:/app/data/archives \
  -v ./servers:/app/data/servers \
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
      MSM_SERVER_IDENTIFIER: MyServer
      MSM_ACCOUNT_LOGIN: your_login
      MSM_ACCOUNT_PASSWORD: your_password
      MSM_MATCH_SETTINGS: MapList.txt
      MSM_MATCH_SETTINGS_BASE: example.txt
      MSM_SERVER_NAME: My ManiaServerManager Server
      MSM_CFG_SERVER_MAX_PLAYERS: 255
    ports:
      - "2350:2350/tcp"
      - "2350:2350/udp"
      - "3450:3450/tcp"
      - "3450:3450/udp"
    volumes:
      - msm_archives:/app/data/archives
      - ./servers:/app/data/servers
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
      MSM_SERVER_IDENTIFIER: MyServer
      MSM_ACCOUNT_LOGIN: your_login
      MSM_ACCOUNT_PASSWORD: your_password
      MSM_TITLE: TMStadium@nadeo
      MSM_MATCH_SETTINGS: MapList.txt
      MSM_MATCH_SETTINGS_BASE: NadeoTimeAttack.txt
      MSM_SERVER_NAME: My ManiaServerManager Server
      MSM_CFG_SERVER_MAX_PLAYERS: 255
    ports:
      - "2350:2350/tcp"
      - "2350:2350/udp"
      - "3450:3450/tcp"
      - "3450:3450/udp"
    volumes:
      - msm_archives:/app/data/archives
      - ./servers:/app/data/servers
volumes:
  msm_archives:
```

For TMF (for United maps, don't forget to set `MSM_CFG_CONFIG_PACKMASK` to `united`):

```yml
services:
  server:
    image: bigbang1112/mania-server-manager:alpine
    restart: unless-stopped
    environment:
      MSM_SERVER_TYPE: TMF
      MSM_SERVER_IDENTIFIER: MyServer
      MSM_ACCOUNT_LOGIN: your_login
      MSM_ACCOUNT_PASSWORD: your_password
      MSM_MATCH_SETTINGS: tracklist.txt
      MSM_MATCH_SETTINGS_BASE: NadeoStadiumTimeAttack.txt
      MSM_SERVER_NAME: My ManiaServerManager Server
      MSM_CFG_SERVER_MAX_PLAYERS: 255
    ports:
      - "2352:2350/tcp"
      - "2352:2350/udp"
      - "3452:3450/tcp"
      - "3452:3450/udp"
    volumes:
      - msm_archives:/app/data/archives
      - ./servers:/app/data/servers
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
      MSM_SERVER_IDENTIFIER: MyServer
      MSM_ACCOUNT_LOGIN: your_login
      MSM_ACCOUNT_PASSWORD: your_password
      MSM_CFG_ACCOUNT_NATION: CZE
      MSM_MATCH_SETTINGS: tracklist.txt
      MSM_MATCH_SETTINGS_BASE: NadeoTimeAttack.txt
      MSM_SERVER_NAME: My ManiaServerManager Server
      MSM_CFG_SERVER_MAX_PLAYERS: 255
    ports:
      - "2353:2350/tcp"
      - "2353:2350/udp"
      - "3453:3450/tcp"
      - "3453:3450/udp"
    volumes:
      - msm_archives:/app/data/archives
      - ./servers:/app/data/servers
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
      MSM_SERVER_IDENTIFIER: MyServer
      MSM_ACCOUNT_LOGIN: your_login
      MSM_ACCOUNT_PASSWORD: your_password
      MSM_MATCH_SETTINGS: MapList.txt
      MSM_MATCH_SETTINGS_BASE: example.txt
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
      - ./servers:/app/data/servers
volumes:
  msm_archives:
```

## Quirks to be aware of on host network

If you decide to use host for simplicity/performance, make sure that:

- You don't forget to change the XML-RPC port for each new server (if you want separate communication)
- That your XML-RPC ports are behind a firewall if you set `MSM_CFG_CONFIG_XMLRPC_ALLOW_REMOTE=True`
- If you need to communicate XML-RPC remotely from any address (there are minimal reasons), absolutely make sure to change `MSM_CFG_AUTHORIZATION_SUPERADMIN_PASSWORD` and `MSM_CFG_AUTHORIZATION_ADMIN_PASSWORD`

## Special thanks

You helped me throughout the struggles:

- [Mystixor](https://github.com/Mystixor)
- [Auris](https://github.com/AurisTFG)
- [Vennstone](https://interfacinglinux.com/2024/10/04/trackberry-raspberry-pi-trackmania-server/)
