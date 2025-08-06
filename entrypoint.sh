#!/bin/sh
set -e

# Runs the necessary setup of the server
./ManiaServerManager

cd data/versions/ManiaPlanet_Latest

original_args="$@"

# Reset positional parameters to prepare for argument construction
set --

if [ -n "$MSM_VALIDATE_PATH" ]; then
    echo "> Validate Path: $MSM_VALIDATE_PATH"
    set -- "$@" "/validatepath=${MSM_VALIDATE_PATH}"
else
    if [ -z "$MSM_DEDICATED_CFG" ]; then
        MSM_DEDICATED_CFG="dedicated_cfg.txt"
    fi
    echo "> Dedicated Cfg: $MSM_DEDICATED_CFG"
    set -- "$@" "/dedicated_cfg=${MSM_DEDICATED_CFG}"

    if [ -z "$MSM_MATCH_SETTINGS" ]; then
        game_settings="$MSM_GAME_SETTINGS"
        echo "> GameSettings: $game_settings"
    else
        game_settings="MatchSettings/$MSM_MATCH_SETTINGS"
        echo "> MatchSettings: $game_settings"
    fi
    set -- "$@" "/game_settings=${game_settings}"

    echo "> Login: $MSM_ACCOUNT_LOGIN"
    set -- "$@" "/login=${MSM_ACCOUNT_LOGIN}"

    # Masking password in output for security best practice; do not log sensitive credentials.
    echo "> Password: **PROTECTED**"
    set -- "$@" "/password=${MSM_ACCOUNT_PASSWORD}"

    if [ "$MSM_LAN" = "True" ] || [ "$MSM_LAN" = "1" ]; then
        echo "> Lan: $MSM_LAN"
        set -- "$@" "/lan"
    fi

    if [ "$MSM_SERVER_TYPE" = "TM" ]; then
        if [ "$MSM_LAN" != "True" ] && [ "$MSM_LAN" != "1" ]; then
            set -- "$@" "/internet"
        fi
        echo "> Game: $MSM_GAME"
        lower_game=$(echo "$MSM_GAME" | tr '[:upper:]' '[:lower:]')
        set -- "$@" "/game=${lower_game}"
    fi

    if [ -z "$MSM_SERVER_NAME" ]; then
        MSM_SERVER_NAME="ManiaServerManager Server"
    fi
    echo "> Server name: $MSM_SERVER_NAME"
    set -- "$@" "/servername=${MSM_SERVER_NAME}"

    if [ -n "$MSM_SERVER_PASSWORD" ]; then
        echo "> Server password: $MSM_SERVER_PASSWORD"
        set -- "$@" "/serverpassword=${MSM_SERVER_PASSWORD}"
    fi

    if [ -n "$MSM_JOIN" ]; then
        echo "> Join: $MSM_JOIN"
        set -- "$@" "/join=${MSM_JOIN}"
    fi

    if [ -n "$MSM_JOIN_PASSWORD" ]; then
        echo "> Join password: $MSM_JOIN_PASSWORD"
        set -- "$@" "/joinpassword=${MSM_JOIN_PASSWORD}"
    fi

    if [ "$MSM_LOAD_CACHE" = "True" ] || [ "$MSM_LOAD_CACHE" = "1" ]; then
        echo "> Load cache: $MSM_LOAD_CACHE"
        set -- "$@" "/loadcache"
    fi

    if [ -n "$MSM_FORCE_IP" ]; then
        echo "> Force IP: $MSM_FORCE_IP"
        set -- "$@" "/forceip=${MSM_FORCE_IP}"
    fi

    if [ -n "$MSM_BIND_IP" ]; then
        echo "> Bind IP: $MSM_BIND_IP"
        set -- "$@" "/bindip=${MSM_BIND_IP}"
    fi

    if [ "$MSM_VERBOSE_RPC_FULL" = "True" ] || [ "$MSM_VERBOSE_RPC_FULL" = "1" ]; then
        echo "> Verbose RPC Full: $MSM_VERBOSE_RPC_FULL"
        set -- "$@" "/verbose_rpc_full"
    fi

    if [ "$MSM_VERBOSE_RPC" = "True" ] || [ "$MSM_VERBOSE_RPC" = "1" ]; then
        echo "> Verbose RPC: $MSM_VERBOSE_RPC"
        set -- "$@" "/verbose_rpc"
    fi

    if [ -n "$MSM_TITLE" ]; then
        set -- "$@" "/title=${MSM_TITLE}"
    fi

    if [ -n "$MSM_PARSE_GBX" ]; then
        echo "> Parse Gbx: $MSM_PARSE_GBX"
        set -- "$@" "/parsegbx=${MSM_PARSE_GBX}"
    fi
fi

# Append args given to the entrypoint script
for arg in $original_args; do
    set -- "$@" "$arg"
done

# Print all constructed parameters for debugging purposes
# echo "> Parameters: $@"

# Forward all constructed parameters to ManiaPlanetServer
exec ./ManiaPlanetServer /nodaemon "$@"
