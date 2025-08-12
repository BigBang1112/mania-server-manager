#!/bin/sh
set -e

log() {
    if [ "$MSM_ONLY_SERVER_LOG" != "True" ] && [ "$MSM_ONLY_SERVER_LOG" != "1" ]; then
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

if [ -z "$MSM_SERVER_IDENTIFIER" ]; then
    if [ -z "$MSM_SERVER_VERSION" ]; then
        MSM_SERVER_VERSION="Latest"
    fi
    MSM_SERVER_IDENTIFIER="${MSM_SERVER_TYPE}_${MSM_SERVER_VERSION}"
fi

cd $MSM_SERVER_IDENTIFIER

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

# Forward all constructed parameters to server executable

if [ "$MSM_WINE" = "1" ]; then
    log "Running under Wine"

    if [ "$MSM_SERVER_TYPE" = "TMF" ] || [ "$MSM_SERVER_TYPE" = "TM" ]; then
        log "TMF and TM are currently not supported on Wine"
        exit 1
    fi

    if [ "$MSM_ONLY_STDOUT" = "True" ] || [ "$MSM_ONLY_STDOUT" = "1" ]; then
        # Print only stdout to the console, useful for /validatepath or /parsegbx
        if [ "$MSM_SERVER_TYPE" = "TM2020" ]; then
            exec wine TrackmaniaServer.exe "$@" 2>/dev/null
        elif [ "$MSM_SERVER_TYPE" = "ManiaPlanet" ]; then
            exec wine ManiaPlanetServer.exe "$@" 2>/dev/null
        elif [ "$MSM_SERVER_TYPE" = "TM" ]; then
            exec wine TrackManiaServer.exe "$@" 2>/dev/null
        else
            log "Unknown MSM_SERVER_TYPE: $MSM_SERVER_TYPE"
            exit 1
        fi
    else
        # Hook onto a log file for verbose logs
        if [ "$MSM_SERVER_TYPE" = "TM2020" ]; then
            exec wine TrackmaniaServer.exe "$@" 2>/dev/null &
        elif [ "$MSM_SERVER_TYPE" = "ManiaPlanet" ]; then
            exec wine ManiaPlanetServer.exe "$@" 2>/dev/null &
        elif [ "$MSM_SERVER_TYPE" = "TM" ]; then
            exec wine TrackManiaServer.exe "$@" 2>/dev/null &
        else
            log "Unknown MSM_SERVER_TYPE: $MSM_SERVER_TYPE"
            exit 1
        fi

        pid=$!
        log "Server started with PID: $pid"

        log_fd="/proc/$pid/fd/18"

        # Wait until log exists
        while [ ! -r "$log_fd" ]; do
            sleep 0.1
            #ls -l /proc/$pid/fd/
            kill -0 "$pid" 2>/dev/null || {
                log "Server exited before FD 18 was ready"
                exit 1
            }
        done

        # Monitor the server process and stop tailing if it exits
        tail -f "$log_fd" --pid "$pid" &
        wait "$pid" # Allows graceful shutdown
    fi
else
    # Regular Linux server start
    if [ "$MSM_SERVER_TYPE" = "TM2020" ]; then
        exec ./TrackmaniaServer /nodaemon "$@"
    elif [ "$MSM_SERVER_TYPE" = "ManiaPlanet" ]; then
        exec ./ManiaPlanetServer /nodaemon "$@"
    else
        if [ "$MSM_ONLY_STDOUT" = "True" ] || [ "$MSM_ONLY_STDOUT" = "1" ]; then
            # Print only stdout to the console, useful for /validatepath or /parsegbx
            if [ "$MSM_SERVER_TYPE" = "TMF" ]; then
                exec ./TrackmaniaServer /nodaemon "$@"
            elif [ "$MSM_SERVER_TYPE" = "TM" ]; then
                exec ./TrackManiaServer /nodaemon "$@"
            else
                log "Unknown MSM_SERVER_TYPE: $MSM_SERVER_TYPE"
                exit 1
            fi
        else
            # Hook onto a log file for verbose logs
            if [ "$MSM_SERVER_TYPE" = "TMF" ]; then
                exec ./TrackmaniaServer /nodaemon "$@" &
            elif [ "$MSM_SERVER_TYPE" = "TM" ]; then
                exec ./TrackManiaServer /nodaemon "$@" &
            else
                log "Unknown MSM_SERVER_TYPE: $MSM_SERVER_TYPE"
                exit 1
            fi

            # Listen to console on old TM dedicated servers
            if [ "$MSM_SERVER_TYPE" = "TM" ] || [ "$MSM_SERVER_TYPE" = "TMF" ]; then
                pid=$!
                fd_path="/proc/$pid/fd/3"

                # Wait until fd_path exists
                while [ ! -e "$fd_path" ]; do
                    sleep 0.1
                    kill -0 "$pid" 2>/dev/null || {
                        log "Server exited before FD 3 was ready"
                        exit 1
                    }
                done

                # Monitor the server process and stop tailing if it exits
                tail -f "$fd_path" --pid "$pid" &
                wait "$pid" # Allows graceful shutdown
            fi
        fi
    fi
fi
