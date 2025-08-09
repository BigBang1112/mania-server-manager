using ManiaServerManager.Enums;
using ManiaServerManager.Models;
using System.Text;

namespace ManiaServerManager;

internal interface IConfiguration
{
    ServerType Type { get; }
    string Version { get; }
    string? Identifier { get; } // null will use Type_Version as identifier
    ServerDownloadHost DownloadHost { get; }
    bool Reinstall { get; }
    string? Title { get; }
    bool IgnoreTitleDownload { get; }
    string TitleDownloadHost { get; }
    UnusedContent UnusedContent { get; }
    string DedicatedCfgFileName { get; }
    string? ValidatePath { get; } // Nullable because empty /validatepath still counts as validate request
    string? ParseGbx { get; } // Nullable because empty /parsegbx still counts as parse request

    string AccountLogin { get; }
    string AccountPassword { get; }
    string ServerName { get; }

    string ExampleMatchSettingsDirPath { get; }

    DedicatedCfg Cfg { get; }
    bool SkipDedicatedCfg { get; }
}

internal sealed class Configuration : IConfiguration
{
    public ServerType Type { get; }
    public string Version { get; }
    public string? Identifier { get; }
    public ServerDownloadHost DownloadHost { get; }
    public bool Reinstall { get; }
    public string? Title { get; }
    public bool IgnoreTitleDownload { get; }
    public string TitleDownloadHost { get; }
    public UnusedContent UnusedContent { get; }
    public string DedicatedCfgFileName { get; }
    public string? ValidatePath { get; }
    public string? ParseGbx { get; } // Nullable because empty /parsegbx still counts as parse request

    public string AccountLogin { get; }
    public string AccountPassword { get; }
    public string ServerName { get; }

    public string ExampleMatchSettingsDirPath { get; }

    public DedicatedCfg Cfg { get; }
    public bool SkipDedicatedCfg { get; }

    public Configuration()
    {
        var exceptions = new List<Exception>();

        if (Enum.TryParse<ServerType>(Environment.GetEnvironmentVariable("MSM_SERVER_TYPE"), ignoreCase: true, out var serverType) || serverType == ServerType.None)
        {
            Type = serverType;
        }
        else
        {
            exceptions.Add(new InvalidOperationException("MSM_SERVER_TYPE must be TM2020, ManiaPlanet, TMF, or TM."));
        }

        Version = Environment.GetEnvironmentVariable("MSM_SERVER_VERSION") ?? "Latest";

        Identifier = Environment.GetEnvironmentVariable("MSM_SERVER_IDENTIFIER") ?? $"{Type}_{Version}";

        DownloadHost = new();
        DownloadHost.All = Environment.GetEnvironmentVariable("MSM_SERVER_DOWNLOAD_HOST_ALL") ?? DownloadHost.All;
        DownloadHost.TM = Environment.GetEnvironmentVariable("MSM_SERVER_DOWNLOAD_HOST_TM") ?? DownloadHost.TM;
        DownloadHost.TMF = Environment.GetEnvironmentVariable("MSM_SERVER_DOWNLOAD_HOST_TMF") ?? DownloadHost.TMF;
        DownloadHost.ManiaPlanet = Environment.GetEnvironmentVariable("MSM_SERVER_DOWNLOAD_HOST_MANIAPLANET") ?? DownloadHost.ManiaPlanet;
        DownloadHost.TM2020 = Environment.GetEnvironmentVariable("MSM_SERVER_DOWNLOAD_HOST_TM2020") ?? DownloadHost.TM2020;

        Reinstall = bool.TryParse(Environment.GetEnvironmentVariable("MSM_REINSTALL"), out var reinstall) && reinstall;

        ValidatePath = Environment.GetEnvironmentVariable("MSM_VALIDATE_PATH");
        ParseGbx = Environment.GetEnvironmentVariable("MSM_PARSE_GBX");

        var isDediServer = ValidatePath is null && ParseGbx is null;

        if (serverType == ServerType.ManiaPlanet)
        {
            Title = Environment.GetEnvironmentVariable("MSM_TITLE");
            if (isDediServer && string.IsNullOrWhiteSpace(Title))
            {
                exceptions.Add(new InvalidOperationException("MSM_TITLE is not set."));
            }
        }

        IgnoreTitleDownload = bool.TryParse(Environment.GetEnvironmentVariable("MSM_IGNORE_TITLE_DOWNLOAD"), out var ignoreTitleDownload) && ignoreTitleDownload;
        TitleDownloadHost = Environment.GetEnvironmentVariable("MSM_TITLE_DOWNLOAD_HOST") ?? "https://maniaplanet.com/ingame/public/titles/download";
        UnusedContent = new();
        DedicatedCfgFileName = Environment.GetEnvironmentVariable("MSM_DEDICATED_CFG") ?? "dedicated_cfg.txt";

        AccountLogin = Environment.GetEnvironmentVariable("MSM_ACCOUNT_LOGIN") ?? string.Empty;
        if (isDediServer && string.IsNullOrWhiteSpace(AccountLogin))
        {
            exceptions.Add(new InvalidOperationException("MSM_ACCOUNT_LOGIN is not set."));
        }

        AccountPassword = Environment.GetEnvironmentVariable("MSM_ACCOUNT_PASSWORD") ?? string.Empty;
        if (isDediServer && string.IsNullOrWhiteSpace(AccountPassword))
        {
            exceptions.Add(new InvalidOperationException("MSM_ACCOUNT_PASSWORD is not set."));
        }

        ExampleMatchSettingsDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MatchSettings", serverType.ToString());

        if (serverType == ServerType.ManiaPlanet && Title is not null)
        {
            var titleFolder = Title;

            if (Title is "TMCanyon" or "TMCanyon@nadeo" or "TMStadium" or "TMStadium@nadeo" or "TMValley" or "TMValley@nadeo" or "TMLagoon" or "TMLagoon@nadeo")
            {
                titleFolder = "Trackmania";
            }

            ExampleMatchSettingsDirPath = Path.Combine(ExampleMatchSettingsDirPath, titleFolder);
        }

        if (isDediServer &&
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MSM_GAME_SETTINGS")) &&
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MSM_MATCH_SETTINGS")))
        {
            exceptions.Add(ConstructMatchSettingsException(serverType));
        }

        ServerName = Environment.GetEnvironmentVariable("MSM_SERVER_NAME") ?? "ManiaServerManager Server";

        SkipDedicatedCfg = bool.TryParse(Environment.GetEnvironmentVariable("MSM_SKIP_DEDICATED_CFG"), out var skipDedicatedCfg) && skipDedicatedCfg;

        Cfg = new();

        // Authorization settings
        Cfg.AuthorizationSuperAdminName = Environment.GetEnvironmentVariable("MSM_CFG_AUTHORIZATION_SUPERADMIN_NAME") ?? Cfg.AuthorizationSuperAdminName;
        Cfg.AuthorizationSuperAdminPassword = Environment.GetEnvironmentVariable("MSM_CFG_AUTHORIZATION_SUPERADMIN_PASSWORD") ?? Cfg.AuthorizationSuperAdminPassword;
        Cfg.AuthorizationAdminName = Environment.GetEnvironmentVariable("MSM_CFG_AUTHORIZATION_ADMIN_NAME") ?? Cfg.AuthorizationAdminName;
        Cfg.AuthorizationAdminPassword = Environment.GetEnvironmentVariable("MSM_CFG_AUTHORIZATION_ADMIN_PASSWORD") ?? Cfg.AuthorizationAdminPassword;
        Cfg.AuthorizationUserName = Environment.GetEnvironmentVariable("MSM_CFG_AUTHORIZATION_USER_NAME") ?? Cfg.AuthorizationUserName;
        Cfg.AuthorizationUserPassword = Environment.GetEnvironmentVariable("MSM_CFG_AUTHORIZATION_USER_PASSWORD") ?? Cfg.AuthorizationUserPassword;

        // Server settings
        Cfg.ServerComment = Environment.GetEnvironmentVariable("MSM_CFG_SERVER_COMMENT") ?? Cfg.ServerComment;
        if (Enum.TryParse<HideServer>(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_HIDE_SERVER"), ignoreCase: true, out var hideServer))
            Cfg.ServerHideServer = hideServer;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_MAX_PLAYERS"), out var maxPlayers))
            Cfg.ServerMaxPlayers = maxPlayers;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_MAX_SPECTATORS"), out var maxSpectators))
            Cfg.ServerMaxSpectators = maxSpectators;

        Cfg.ServerPasswordSpectator = Environment.GetEnvironmentVariable("MSM_CFG_SERVER_PASSWORD_SPECTATOR") ?? Cfg.ServerPasswordSpectator;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_KEEP_PLAYER_SLOTS"), out var keepPlayerSlots))
            Cfg.ServerKeepPlayerSlots = keepPlayerSlots;

        if (Enum.TryParse<LadderMode>(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_LADDER_MODE"), ignoreCase: true, out var ladderMode))
            Cfg.ServerLadderMode = ladderMode;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_ENABLE_P2P_UPLOAD"), out var enableP2pUpload))
            Cfg.ServerEnableP2pUpload = enableP2pUpload;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_ENABLE_P2P_DOWNLOAD"), out var enableP2pDownload))
            Cfg.ServerEnableP2pDownload = enableP2pDownload;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_CALLVOTE_TIMEOUT"), out var callVoteTimeout))
            Cfg.ServerCallVoteTimeout = callVoteTimeout;

        if (double.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_CALLVOTE_RATIO"), out var callVoteRatio))
            Cfg.ServerCallVoteRatio = callVoteRatio;

        // Note: ServerCallVoteRatios is more complex and would need custom parsing

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_ALLOW_MAP_DOWNLOAD"), out var allowMapDownload))
            Cfg.ServerAllowMapDownload = allowMapDownload;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_AUTOSAVE_REPLAYS"), out var autosaveReplays))
            Cfg.ServerAutosaveReplays = autosaveReplays;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_AUTOSAVE_VALIDATION_REPLAYS"), out var autosaveValidationReplays))
            Cfg.ServerAutosaveValidationReplays = autosaveValidationReplays;

        Cfg.ServerRefereePassword = Environment.GetEnvironmentVariable("MSM_CFG_SERVER_REFEREE_PASSWORD") ?? Cfg.ServerRefereePassword;

        if (Enum.TryParse<RefereeValidationMode>(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_REFEREE_VALIDATION_MODE"), ignoreCase: true, out var refereeValidationMode))
            Cfg.ServerRefereeValidationMode = refereeValidationMode;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_USE_CHANGING_VALIDATION_SEED"), out var useChangingValidationSeed))
            Cfg.ServerUseChangingValidationSeed = useChangingValidationSeed;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_DISABLE_HORNS"), out var disableHorns))
            Cfg.ServerDisableHorns = disableHorns;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_DISABLE_PROFILE_SKINS"), out var disableProfileSkins))
            Cfg.ServerDisableProfileSkins = disableProfileSkins;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_CLIENT_INPUTS_MAX_LATENCY"), out var clientInputsMaxLatency))
            Cfg.ServerClientInputsMaxLatency = clientInputsMaxLatency;

        // Connection config
        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_CONNECTION_UPLOAD_RATE"), out var connectionUploadRate))
            Cfg.ConfigConnectionUploadRate = connectionUploadRate;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_CONNECTION_DOWNLOAD_RATE"), out var connectionDownloadRate))
            Cfg.ConfigConnectionDownloadRate = connectionDownloadRate;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_WORKER_THREAD_COUNT"), out var workerThreadCount))
            Cfg.ConfigWorkerThreadCount = workerThreadCount;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_PACKETASSEMBLY_MULTITHREAD"), out var packetAssemblyMultithread))
            Cfg.ConfigPacketAssemblyMultithread = packetAssemblyMultithread;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_PACKETASSEMBLY_PACKETS_PER_FRAME"), out var packetsPerFrame))
            Cfg.ConfigPacketAssemblyPacketsPerFrame = packetsPerFrame;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_PACKETASSEMBLY_FULL_PACKETS_PER_FRAME"), out var fullPacketsPerFrame))
            Cfg.ConfigPacketAssemblyFullPacketsPerFrame = fullPacketsPerFrame;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_DELAYED_VISUALS_S2C_SENDING_RATE"), out var delayedVisualsS2cSendingRate))
            Cfg.ConfigDelayedVisualsS2cSendingRate = delayedVisualsS2cSendingRate;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_TRUST_CLIENT_SIMU_C2S_SENDING_RATE"), out var trustClientSimuC2sSendingRate))
            Cfg.ConfigTrustClientSimuC2sSendingRate = trustClientSimuC2sSendingRate;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_ALLOW_SPECTATOR_RELAYS"), out var allowSpectatorRelays))
            Cfg.ConfigAllowSpectatorRelays = allowSpectatorRelays;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_P2P_CACHE_SIZE"), out var p2pCacheSize))
            Cfg.ConfigP2pCacheSize = p2pCacheSize;

        //if (IPAddress.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_FORCE_IP_ADDRESS"), out var forceIpAddress))
        //    Cfg.ConfigForceIpAddress = forceIpAddress;

        if (ushort.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_SERVER_PORT"), out var serverPort))
            Cfg.ConfigServerPort = serverPort;

        if (ushort.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_SERVER_P2P_PORT"), out var serverP2pPort))
            Cfg.ConfigServerP2pPort = serverP2pPort;

        if (ushort.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_CLIENT_PORT"), out var clientPort))
            Cfg.ConfigClientPort = clientPort;

        //if (IPAddress.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_BIND_IP_ADDRESS"), out var bindIpAddress))
        //    Cfg.ConfigBindIpAddress = bindIpAddress;

        Cfg.ConfigUseNatUpnp = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_USE_NAT_UPNP") ?? Cfg.ConfigUseNatUpnp;
        Cfg.ConfigGspName = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_GSP_NAME") ?? Cfg.ConfigGspName;
        Cfg.ConfigGspUrl = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_GSP_URL") ?? Cfg.ConfigGspUrl;

        if (ushort.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_XMLRPC_PORT"), out var xmlRpcPort))
            Cfg.ConfigXmlRpcPort = xmlRpcPort;

        Cfg.ConfigXmlRpcAllowRemote = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_XMLRPC_ALLOW_REMOTE") ?? Cfg.ConfigXmlRpcAllowRemote;

        Cfg.ConfigBlacklistUrl = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_BLACKLIST_URL") ?? Cfg.ConfigBlacklistUrl;
        Cfg.ConfigGuestlistFileName = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_GUESTLIST_FILE_NAME") ?? Cfg.ConfigGuestlistFileName;
        Cfg.ConfigBlacklistFileName = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_BLACKLIST_FILE_NAME") ?? Cfg.ConfigBlacklistFileName;
        Cfg.ConfigMinimumClientBuild = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_MINIMUM_CLIENT_BUILD") ?? Cfg.ConfigMinimumClientBuild;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_DISABLE_COHERENCE_CHECKS"), out var disableCoherenceChecks))
            Cfg.ConfigDisableCoherenceChecks = disableCoherenceChecks;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_DISABLE_REPLAY_RECORDING"), out var disableReplayRecording))
            Cfg.ConfigDisableReplayRecording = disableReplayRecording;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_SAVE_ALL_INDIVIDUAL_RUNS"), out var saveAllIndividualRuns))
            Cfg.ConfigSaveAllIndividualRuns = saveAllIndividualRuns;

        if (bool.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_USE_PROXY"), out var useProxy))
            Cfg.ConfigUseProxy = useProxy;

        Cfg.ConfigProxyUrl = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_PROXY_URL") ?? Cfg.ConfigProxyUrl;

        // Additional settings
        Cfg.AccountValidationKey = Environment.GetEnvironmentVariable("MSM_CFG_ACCOUNT_VALIDATION_KEY") ?? Cfg.AccountValidationKey;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_PACKETASSEMBLY_THREAD_COUNT"), out var packetAssemblyThreadCount))
            Cfg.ConfigPacketAssemblyThreadCount = packetAssemblyThreadCount;

        if (Enum.TryParse<ScriptCloudSource>(Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_SCRIPTCLOUD_SOURCE"), ignoreCase: true, out var scriptCloudSource))
            Cfg.ConfigScriptCloudSource = scriptCloudSource;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_LADDER_SERVER_LIMIT_MIN"), out var ladderServerLimitMin))
            Cfg.ServerLadderServerLimitMin = ladderServerLimitMin;

        if (int.TryParse(Environment.GetEnvironmentVariable("MSM_CFG_SERVER_LADDER_SERVER_LIMIT_MAX"), out var ladderServerLimitMax))
            Cfg.ServerLadderServerLimitMax = ladderServerLimitMax;

        Cfg.ConfigPackMask = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_PACK_MASK") ?? Cfg.ConfigPackMask;
        Cfg.ConfigProxyLogin = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_PROXY_LOGIN") ?? Cfg.ConfigProxyLogin;
        Cfg.ConfigProxyPassword = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_PROXY_PASSWORD") ?? Cfg.ConfigProxyPassword;
        Cfg.AccountNation = Environment.GetEnvironmentVariable("MSM_CFG_ACCOUNT_NATION") ?? Cfg.AccountNation;
        Cfg.ConfigConnectionType = Environment.GetEnvironmentVariable("MSM_CFG_CONFIG_CONNECTION_TYPE") ?? Cfg.ConfigConnectionType;

        if (exceptions.Count > 0)
        {
            throw new AggregateException("Configuration errors occurred.", exceptions);
        }
    }

    private InvalidOperationException ConstructMatchSettingsException(ServerType serverType)
    {
        var availableMatchSettingsFileNames = Directory.Exists(ExampleMatchSettingsDirPath)
            ? Directory.GetFiles(ExampleMatchSettingsDirPath, "*.txt").Select(Path.GetFileName)
            : [];

        var sb = new StringBuilder("MSM_GAME_SETTINGS or MSM_MATCH_SETTINGS is not set.");

        if (availableMatchSettingsFileNames.Any())
        {
            sb.Append(" You can set MSM_MATCH_SETTINGS to one of these examples: ");
            sb.Append(string.Join(", ", availableMatchSettingsFileNames));
        }
        else
        {
            sb.Append(" No examples could be provied, so just try to se one of these variables to a MatchSettings file provided by the server");

            if (serverType == ServerType.ManiaPlanet)
            {
                sb.Append(" or the title pack.");

                if (string.IsNullOrWhiteSpace(Title))
                {
                    sb.Append(" Specifying the title pack could help giving some examples.");
                }
            }

            sb.Append('.');
        }

        if (serverType == ServerType.TM2020)
        {
            sb.Append(" TM2020 also includes the 'example.txt' by default, so you can use that.");
        }

        return new InvalidOperationException(sb.ToString());
    }
}
