#!/bin/sh
set -e

log() {
    if [ "$MSM_ONLY_SERVER_LOG" = "True" ] || [ "$MSM_ONLY_SERVER_LOG" = "1" ]; then
        echo "$1"
    fi
    # TODO: else write to a log file
}

# Runs the necessary setup of the server
if [ "$MSM_ONLY_SERVER_LOG" = "True" ] || [ "$MSM_ONLY_SERVER_LOG" = "1" ]; then
    ./ManiaServerManager > /dev/null 2>&1
else
    ./ManiaServerManager
fi

if [ "$MSM_ONLY_SETUP" = "True" ] || [ "$MSM_ONLY_SETUP" = "1" ]; then
    exit 0
fi

log "> Server Type: $MSM_SERVER_TYPE"

cd data/servers

if [ -z "$MSM_SERVER_VERSION" ]; then
    MSM_SERVER_VERSION="Latest"
fi

if [ "$MSM_SERVER_TYPE" = "TM" ]; then
    cd TM_$MSM_SERVER_VERSION
elif [ "$MSM_SERVER_TYPE" = "TMF" ]; then
    cd TMF_$MSM_SERVER_VERSION
elif [ "$MSM_SERVER_TYPE" = "TM2020" ]; then
    cd TM2020_$MSM_SERVER_VERSION
elif [ "$MSM_SERVER_TYPE" = "ManiaPlanet" ]; then
    cd ManiaPlanet_$MSM_SERVER_VERSION
else
    log "Unknown MSM_SERVER_TYPE: $MSM_SERVER_TYPE"
    exit 1
fi

original_args="$@"

# Reset positional parameters to prepare for argument construction
set --

if [ -n "$MSM_VALIDATE_PATH" ]; then
    log "> Validate Path: $MSM_VALIDATE_PATH"
    set -- "$@" "/validatepath=${MSM_VALIDATE_PATH}"
fi

if [ -n "$MSM_PARSE_GBX" ]; then
    log "> Parse Gbx: $MSM_PARSE_GBX"
    set -- "$@" "/parsegbx=${MSM_PARSE_GBX}"
fi

if [ -z "$MSM_DEDICATED_CFG" ]; then
    MSM_DEDICATED_CFG="dedicated_cfg.txt"
fi
log "> Dedicated Cfg: $MSM_DEDICATED_CFG"
set -- "$@" "/dedicated_cfg=${MSM_DEDICATED_CFG}"

if [ -z "$MSM_MATCH_SETTINGS" ]; then
    game_settings="$MSM_GAME_SETTINGS"
    log "> GameSettings: $game_settings"
else
    game_settings="MatchSettings/$MSM_MATCH_SETTINGS"
    log "> MatchSettings: $game_settings"
fi
set -- "$@" "/game_settings=${game_settings}"

log "> Login: $MSM_ACCOUNT_LOGIN"
set -- "$@" "/login=${MSM_ACCOUNT_LOGIN}"

# Masking password in output for security best practice; do not log sensitive credentials.
log "> Password: **PROTECTED**"
set -- "$@" "/password=${MSM_ACCOUNT_PASSWORD}"

if [ "$MSM_LAN" = "True" ] || [ "$MSM_LAN" = "1" ]; then
    log "> Lan: $MSM_LAN"
    set -- "$@" "/lan"
fi

if [ "$MSM_SERVER_TYPE" = "TM" ]; then
    if [ "$MSM_LAN" != "True" ] && [ "$MSM_LAN" != "1" ]; then
        set -- "$@" "/internet"
    fi
    log "> Game: $MSM_GAME"
    lower_game=$(log "$MSM_GAME" | tr '[:upper:]' '[:lower:]')
    set -- "$@" "/game=${lower_game}"
fi

if [ -z "$MSM_SERVER_NAME" ]; then
    MSM_SERVER_NAME="ManiaServerManager Server"
fi

log "> Server name: $MSM_SERVER_NAME"
set -- "$@" "/servername=${MSM_SERVER_NAME}"

if [ -n "$MSM_SERVER_PASSWORD" ]; then
    log "> Server password: $MSM_SERVER_PASSWORD"
    set -- "$@" "/serverpassword=${MSM_SERVER_PASSWORD}"
fi

if [ -n "$MSM_JOIN" ]; then
    log "> Join: $MSM_JOIN"
    set -- "$@" "/join=${MSM_JOIN}"
fi

if [ -n "$MSM_JOIN_PASSWORD" ]; then
    log "> Join password: $MSM_JOIN_PASSWORD"
    set -- "$@" "/joinpassword=${MSM_JOIN_PASSWORD}"
fi

if [ "$MSM_LOAD_CACHE" = "True" ] || [ "$MSM_LOAD_CACHE" = "1" ]; then
    log "> Load cache: $MSM_LOAD_CACHE"
    set -- "$@" "/loadcache"
fi

if [ -n "$MSM_FORCE_IP" ]; then
    log "> Force IP: $MSM_FORCE_IP"
    set -- "$@" "/forceip=${MSM_FORCE_IP}"
fi

if [ -n "$MSM_BIND_IP" ]; then
    log "> Bind IP: $MSM_BIND_IP"
    set -- "$@" "/bindip=${MSM_BIND_IP}"
fi

if [ "$MSM_VERBOSE_RPC_FULL" = "True" ] || [ "$MSM_VERBOSE_RPC_FULL" = "1" ]; then
    log "> Verbose RPC Full: $MSM_VERBOSE_RPC_FULL"
    set -- "$@" "/verbose_rpc_full"
fi

if [ "$MSM_VERBOSE_RPC" = "True" ] || [ "$MSM_VERBOSE_RPC" = "1" ]; then
    log "> Verbose RPC: $MSM_VERBOSE_RPC"
    set -- "$@" "/verbose_rpc"
fi

if [ -n "$MSM_TITLE" ]; then
    set -- "$@" "/title=${MSM_TITLE}"
fi

# Append args given to the entrypoint script
for arg in $original_args; do
    set -- "$@" "$arg"
done

# Print all constructed parameters for debugging purposes
# log "> Parameters: $@"

# Listen to console on old TM dedicated servers
if [ "$MSM_SERVER_TYPE" = "TM" ] || [ "$MSM_SERVER_TYPE" = "TMF" ]; then
    mkdir -p ./Logs && touch ./Logs/ConsoleLog.1.txt
    ln -sf /proc/self/fd/1 ./Logs/ConsoleLog.1.txt
fi

# Forward all constructed parameters to ManiaPlanetServer
if [ "$MSM_SERVER_TYPE" = "TMF" ] || [ "$MSM_SERVER_TYPE" = "TM2020" ]; then
    exec ./TrackmaniaServer /nodaemon "$@"
elif [ "$MSM_SERVER_TYPE" = "ManiaPlanet" ]; then
    exec ./ManiaPlanetServer /nodaemon "$@"
elif [ "$MSM_SERVER_TYPE" = "TM" ]; then
    exec ./TrackManiaServer /nodaemon "$@"
else
    log "Unknown MSM_SERVER_TYPE: $MSM_SERVER_TYPE"
    exit 1
fi
