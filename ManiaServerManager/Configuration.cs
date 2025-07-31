using ManiaServerManager.Enums;
using ManiaServerManager.Models;
using System.Net;

namespace ManiaServerManager;

internal interface IConfiguration
{
    ServerType Type { get; }
    string Version { get; }
    ServerDownloadHost DownloadHost { get; }
    bool Reinstall { get; }
    string? Title { get; }
    bool IgnoreTitleDownload { get; }
    string TitleDownloadHost { get; }
    UnusedContent UnusedContent { get; }
    string DedicatedCfgFileName { get; }

    string AccountLogin { get; }
    string AccountPassword { get; }
    string ServerName { get; }

    DedicatedCfg Cfg { get; }
}

internal sealed class Configuration : IConfiguration
{
    public ServerType Type { get; }
    public string Version { get; }
    public ServerDownloadHost DownloadHost { get; }
    public bool Reinstall { get; }
    public string? Title { get; }
    public bool IgnoreTitleDownload { get; }
    public string TitleDownloadHost { get; }
    public UnusedContent UnusedContent { get; }
    public string DedicatedCfgFileName { get; }

    public string AccountLogin { get; }
    public string AccountPassword { get; }
    public string ServerName { get; }

    public DedicatedCfg Cfg { get; }

    public Configuration()
    {
        Type = Enum.TryParse<ServerType>(Environment.GetEnvironmentVariable("MSM_SERVER_TYPE"), ignoreCase: true, out var serverType)
            ? serverType
            : throw new InvalidOperationException("MSM_SERVER_TYPE is not set or invalid.");
        Version = Environment.GetEnvironmentVariable("MSM_SERVER_VERSION") ?? throw new InvalidOperationException("MSM_SERVER_VERSION is not set.");

        DownloadHost = new();
        DownloadHost.All = Environment.GetEnvironmentVariable("MSM_SERVER_DOWNLOAD_HOST_ALL") ?? DownloadHost.All;
        DownloadHost.TM = Environment.GetEnvironmentVariable("MSM_SERVER_DOWNLOAD_HOST_TM") ?? DownloadHost.TM;
        DownloadHost.TMF = Environment.GetEnvironmentVariable("MSM_SERVER_DOWNLOAD_HOST_TMF") ?? DownloadHost.TMF;
        DownloadHost.ManiaPlanet = Environment.GetEnvironmentVariable("MSM_SERVER_DOWNLOAD_HOST_MANIAPLANET") ?? DownloadHost.ManiaPlanet;
        DownloadHost.TM2020 = Environment.GetEnvironmentVariable("MSM_SERVER_DOWNLOAD_HOST_TM2020") ?? DownloadHost.TM2020;

        Reinstall = bool.TryParse(Environment.GetEnvironmentVariable("MSM_REINSTALL"), out var reinstall) && reinstall;
        Title = Environment.GetEnvironmentVariable("MSM_TITLE") ?? "SMStorm@nadeo";
        IgnoreTitleDownload = bool.TryParse(Environment.GetEnvironmentVariable("MSM_IGNORE_TITLE_DOWNLOAD"), out var ignoreTitleDownload) && ignoreTitleDownload;
        TitleDownloadHost = Environment.GetEnvironmentVariable("MSM_TITLE_DOWNLOAD_HOST") ?? "https://maniaplanet.com/ingame/public/titles/download";
        UnusedContent = new();
        DedicatedCfgFileName = Environment.GetEnvironmentVariable("MSM_DEDICATEDCFG_FILENAME") ?? "dedicated_cfg.txt";

        AccountLogin = Environment.GetEnvironmentVariable("ACCOUNT_LOGIN") ?? throw new InvalidOperationException("ACCOUNT_LOGIN is not set.");
        AccountPassword = Environment.GetEnvironmentVariable("ACCOUNT_PASSWORD") ?? throw new InvalidOperationException("ACCOUNT_PASSWORD is not set.");
        ServerName = Environment.GetEnvironmentVariable("SERVER_NAME") ?? throw new InvalidOperationException("SERVER_NAME is not set.");

        Cfg = new();

        static string? GetCfgEnv(string name) => Environment.GetEnvironmentVariable($"CFG_{name}");

        // Authorization settings
        Cfg.AuthorizationSuperAdminName = GetCfgEnv("AUTHORIZATION_SUPERADMIN_NAME") ?? Cfg.AuthorizationSuperAdminName;
        Cfg.AuthorizationSuperAdminPassword = GetCfgEnv("AUTHORIZATION_SUPERADMIN_PASSWORD") ?? Cfg.AuthorizationSuperAdminPassword;
        Cfg.AuthorizationAdminName = GetCfgEnv("AUTHORIZATION_ADMIN_NAME") ?? Cfg.AuthorizationAdminName;
        Cfg.AuthorizationAdminPassword = GetCfgEnv("AUTHORIZATION_ADMIN_PASSWORD") ?? Cfg.AuthorizationAdminPassword;
        Cfg.AuthorizationUserName = GetCfgEnv("AUTHORIZATION_USER_NAME") ?? Cfg.AuthorizationUserName;
        Cfg.AuthorizationUserPassword = GetCfgEnv("AUTHORIZATION_USER_PASSWORD") ?? Cfg.AuthorizationUserPassword;

        // Server settings
        Cfg.ServerComment = GetCfgEnv("SERVER_COMMENT") ?? Cfg.ServerComment;
        if (Enum.TryParse<HideServer>(GetCfgEnv("SERVER_HIDE_SERVER"), ignoreCase: true, out var hideServer))
            Cfg.ServerHideServer = hideServer;

        if (int.TryParse(GetCfgEnv("SERVER_MAX_PLAYERS"), out var maxPlayers))
            Cfg.ServerMaxPlayers = maxPlayers;

        Cfg.ServerMaxSpectators = GetCfgEnv("SERVER_MAX_SPECTATORS") ?? Cfg.ServerMaxSpectators;
        Cfg.ServerPasswordSpectator = GetCfgEnv("SERVER_PASSWORD_SPECTATOR") ?? Cfg.ServerPasswordSpectator;

        if (bool.TryParse(GetCfgEnv("SERVER_KEEP_PLAYER_SLOTS"), out var keepPlayerSlots))
            Cfg.ServerKeepPlayerSlots = keepPlayerSlots;

        if (Enum.TryParse<LadderMode>(GetCfgEnv("SERVER_LADDER_MODE"), ignoreCase: true, out var ladderMode))
            Cfg.ServerLadderMode = ladderMode;

        if (bool.TryParse(GetCfgEnv("SERVER_ENABLE_P2P_UPLOAD"), out var enableP2pUpload))
            Cfg.ServerEnableP2pUpload = enableP2pUpload;

        if (bool.TryParse(GetCfgEnv("SERVER_ENABLE_P2P_DOWNLOAD"), out var enableP2pDownload))
            Cfg.ServerEnableP2pDownload = enableP2pDownload;

        if (int.TryParse(GetCfgEnv("SERVER_CALLVOTE_TIMEOUT"), out var callVoteTimeout))
            Cfg.ServerCallVoteTimeout = callVoteTimeout;

        if (double.TryParse(GetCfgEnv("SERVER_CALLVOTE_RATIO"), out var callVoteRatio))
            Cfg.ServerCallVoteRatio = callVoteRatio;

        // Note: ServerCallVoteRatios is more complex and would need custom parsing

        if (bool.TryParse(GetCfgEnv("SERVER_ALLOW_MAP_DOWNLOAD"), out var allowMapDownload))
            Cfg.ServerAllowMapDownload = allowMapDownload;

        if (bool.TryParse(GetCfgEnv("SERVER_AUTOSAVE_REPLAYS"), out var autosaveReplays))
            Cfg.ServerAutosaveReplays = autosaveReplays;

        if (bool.TryParse(GetCfgEnv("SERVER_AUTOSAVE_VALIDATION_REPLAYS"), out var autosaveValidationReplays))
            Cfg.ServerAutosaveValidationReplays = autosaveValidationReplays;

        Cfg.ServerRefereePassword = GetCfgEnv("SERVER_REFEREE_PASSWORD") ?? Cfg.ServerRefereePassword;

        if (Enum.TryParse<RefereeValidationMode>(GetCfgEnv("SERVER_REFEREE_VALIDATION_MODE"), ignoreCase: true, out var refereeValidationMode))
            Cfg.ServerRefereeValidationMode = refereeValidationMode;

        if (bool.TryParse(GetCfgEnv("SERVER_USE_CHANGING_VALIDATION_SEED"), out var useChangingValidationSeed))
            Cfg.ServerUseChangingValidationSeed = useChangingValidationSeed;

        if (bool.TryParse(GetCfgEnv("SERVER_DISABLE_HORNS"), out var disableHorns))
            Cfg.ServerDisableHorns = disableHorns;

        if (bool.TryParse(GetCfgEnv("SERVER_DISABLE_PROFILE_SKINS"), out var disableProfileSkins))
            Cfg.ServerDisableProfileSkins = disableProfileSkins;

        if (bool.TryParse(GetCfgEnv("SERVER_CLIENT_INPUTS_MAX_LATENCY"), out var clientInputsMaxLatency))
            Cfg.ServerClientInputsMaxLatency = clientInputsMaxLatency;

        // Connection config
        if (int.TryParse(GetCfgEnv("CONFIG_CONNECTION_UPLOAD_RATE"), out var connectionUploadRate))
            Cfg.ConfigConnectionUploadRate = connectionUploadRate;

        if (int.TryParse(GetCfgEnv("CONFIG_CONNECTION_DOWNLOAD_RATE"), out var connectionDownloadRate))
            Cfg.ConfigConnectionDownloadRate = connectionDownloadRate;

        if (int.TryParse(GetCfgEnv("CONFIG_WORKER_THREAD_COUNT"), out var workerThreadCount))
            Cfg.ConfigWorkerThreadCount = workerThreadCount;

        if (bool.TryParse(GetCfgEnv("CONFIG_PACKETASSEMBLY_MULTITHREAD"), out var packetAssemblyMultithread))
            Cfg.ConfigPacketAssemblyMultithread = packetAssemblyMultithread;

        if (int.TryParse(GetCfgEnv("CONFIG_PACKETASSEMBLY_PACKETS_PER_FRAME"), out var packetsPerFrame))
            Cfg.ConfigPacketAssemblyPacketsPerFrame = packetsPerFrame;

        if (int.TryParse(GetCfgEnv("CONFIG_PACKETASSEMBLY_FULL_PACKETS_PER_FRAME"), out var fullPacketsPerFrame))
            Cfg.ConfigPacketAssemblyFullPacketsPerFrame = fullPacketsPerFrame;

        if (int.TryParse(GetCfgEnv("CONFIG_DELAYED_VISUALS_S2C_SENDING_RATE"), out var delayedVisualsS2cSendingRate))
            Cfg.ConfigDelayedVisualsS2cSendingRate = delayedVisualsS2cSendingRate;

        if (int.TryParse(GetCfgEnv("CONFIG_TRUST_CLIENT_SIMU_C2S_SENDING_RATE"), out var trustClientSimuC2sSendingRate))
            Cfg.ConfigTrustClientSimuC2sSendingRate = trustClientSimuC2sSendingRate;

        if (bool.TryParse(GetCfgEnv("CONFIG_ALLOW_SPECTATOR_RELAYS"), out var allowSpectatorRelays))
            Cfg.ConfigAllowSpectatorRelays = allowSpectatorRelays;

        if (int.TryParse(GetCfgEnv("CONFIG_P2P_CACHE_SIZE"), out var p2pCacheSize))
            Cfg.ConfigP2pCacheSize = p2pCacheSize;

        if (IPAddress.TryParse(GetCfgEnv("CONFIG_FORCE_IP_ADDRESS"), out var forceIpAddress))
            Cfg.ConfigForceIpAddress = forceIpAddress;

        if (ushort.TryParse(GetCfgEnv("CONFIG_SERVER_PORT"), out var serverPort))
            Cfg.ConfigServerPort = serverPort;

        if (ushort.TryParse(GetCfgEnv("CONFIG_SERVER_P2P_PORT"), out var serverP2pPort))
            Cfg.ConfigServerP2pPort = serverP2pPort;

        if (ushort.TryParse(GetCfgEnv("CONFIG_CLIENT_PORT"), out var clientPort))
            Cfg.ConfigClientPort = clientPort;

        if (IPAddress.TryParse(GetCfgEnv("CONFIG_BIND_IP_ADDRESS"), out var bindIpAddress))
            Cfg.ConfigBindIpAddress = bindIpAddress;

        Cfg.ConfigUseNatUpnp = GetCfgEnv("CONFIG_USE_NAT_UPNP") ?? Cfg.ConfigUseNatUpnp;
        Cfg.ConfigGspName = GetCfgEnv("CONFIG_GSP_NAME") ?? Cfg.ConfigGspName;
        Cfg.ConfigGspUrl = GetCfgEnv("CONFIG_GSP_URL") ?? Cfg.ConfigGspUrl;

        if (ushort.TryParse(GetCfgEnv("CONFIG_XMLRPC_PORT"), out var xmlRpcPort))
            Cfg.ConfigXmlRpcPort = xmlRpcPort;

        if (bool.TryParse(GetCfgEnv("CONFIG_XMLRPC_ALLOW_REMOTE"), out var xmlRpcAllowRemote))
            Cfg.ConfigXmlRpcAllowRemote = xmlRpcAllowRemote;

        Cfg.ConfigBlacklistUrl = GetCfgEnv("CONFIG_BLACKLIST_URL") ?? Cfg.ConfigBlacklistUrl;
        Cfg.ConfigGuestlistFileName = GetCfgEnv("CONFIG_GUESTLIST_FILE_NAME") ?? Cfg.ConfigGuestlistFileName;
        Cfg.ConfigBlacklistFileName = GetCfgEnv("CONFIG_BLACKLIST_FILE_NAME") ?? Cfg.ConfigBlacklistFileName;
        Cfg.ConfigMinimumClientBuild = GetCfgEnv("CONFIG_MINIMUM_CLIENT_BUILD") ?? Cfg.ConfigMinimumClientBuild;

        if (bool.TryParse(GetCfgEnv("CONFIG_DISABLE_COHERENCE_CHECKS"), out var disableCoherenceChecks))
            Cfg.ConfigDisableCoherenceChecks = disableCoherenceChecks;

        if (bool.TryParse(GetCfgEnv("CONFIG_DISABLE_REPLAY_RECORDING"), out var disableReplayRecording))
            Cfg.ConfigDisableReplayRecording = disableReplayRecording;

        if (bool.TryParse(GetCfgEnv("CONFIG_SAVE_ALL_INDIVIDUAL_RUNS"), out var saveAllIndividualRuns))
            Cfg.ConfigSaveAllIndividualRuns = saveAllIndividualRuns;

        if (bool.TryParse(GetCfgEnv("CONFIG_USE_PROXY"), out var useProxy))
            Cfg.ConfigUseProxy = useProxy;

        Cfg.ConfigProxyUrl = GetCfgEnv("CONFIG_PROXY_URL") ?? Cfg.ConfigProxyUrl;

        // Additional settings
        Cfg.AccountValidationKey = GetCfgEnv("ACCOUNT_VALIDATION_KEY") ?? Cfg.AccountValidationKey;

        if (int.TryParse(GetCfgEnv("CONFIG_PACKETASSEMBLY_THREAD_COUNT"), out var packetAssemblyThreadCount))
            Cfg.ConfigPacketAssemblyThreadCount = packetAssemblyThreadCount;

        if (Enum.TryParse<ScriptCloudSource>(GetCfgEnv("CONFIG_SCRIPTCLOUD_SOURCE"), ignoreCase: true, out var scriptCloudSource))
            Cfg.ConfigScriptCloudSource = scriptCloudSource;

        if (int.TryParse(GetCfgEnv("SERVER_LADDER_SERVER_LIMIT_MIN"), out var ladderServerLimitMin))
            Cfg.ServerLadderServerLimitMin = ladderServerLimitMin;

        if (int.TryParse(GetCfgEnv("SERVER_LADDER_SERVER_LIMIT_MAX"), out var ladderServerLimitMax))
            Cfg.ServerLadderServerLimitMax = ladderServerLimitMax;

        Cfg.ConfigPackMask = GetCfgEnv("CONFIG_PACK_MASK") ?? Cfg.ConfigPackMask;
        Cfg.ConfigProxyLogin = GetCfgEnv("CONFIG_PROXY_LOGIN") ?? Cfg.ConfigProxyLogin;
        Cfg.ConfigProxyPassword = GetCfgEnv("CONFIG_PROXY_PASSWORD") ?? Cfg.ConfigProxyPassword;
        Cfg.AccountNation = GetCfgEnv("ACCOUNT_NATION") ?? Cfg.AccountNation;
        Cfg.ConfigConnectionType = GetCfgEnv("CONFIG_CONNECTION_TYPE") ?? Cfg.ConfigConnectionType;
    }
}
