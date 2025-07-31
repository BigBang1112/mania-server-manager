using ManiaServerManager.Enums;
using System.Net;

namespace ManiaServerManager.Models;

internal sealed class DedicatedCfg
{
    public string AuthorizationSuperAdminName { get; set; } = "SuperAdmin";
    public string AuthorizationSuperAdminPassword { get; set; } = "SuperAdmin";
    public string AuthorizationAdminName { get; set; } = "Admin";
    public string AuthorizationAdminPassword { get; set; } = "Admin";
    public string AuthorizationUserName { get; set; } = "User";
    public string AuthorizationUserPassword { get; set; } = "User";
    //public string AccountLogin { get; set; } = string.Empty;
    //public string AccountPassword { get; set; } = string.Empty;
    //public string ServerName { get; set; } = string.Empty;
    public string ServerComment { get; set; } = string.Empty;
    public HideServer ServerHideServer { get; set; }
    public int ServerMaxPlayers { get; set; } = 32;
    public int ServerMaxSpectators { get; set; } = 32;
    public string ServerPasswordSpectator { get; set; } = string.Empty;
    public bool ServerKeepPlayerSlots { get; set; }
    public LadderMode ServerLadderMode { get; set; }
    public bool ServerEnableP2pUpload { get; set; } = true;
    public bool ServerEnableP2pDownload { get; set; }
    public int ServerCallVoteTimeout { get; set; } = 60000;
    public double ServerCallVoteRatio { get; set; } = 0.5;
    public VoteRatio[] ServerCallVoteRatios { get; set; } = [new() { Command = "Ban", Ratio = -1 }];
    public bool ServerAllowMapDownload { get; set; } = true;
    public bool ServerAutosaveReplays { get; set; }
    public bool ServerAutosaveValidationReplays { get; set; }
    public string ServerRefereePassword { get; set; } = string.Empty;
    public RefereeValidationMode ServerRefereeValidationMode { get; set; }
    public bool ServerUseChangingValidationSeed { get; set; }
    public bool ServerDisableHorns { get; set; }
    public bool ServerDisableProfileSkins { get; set; }
    public bool ServerClientInputsMaxLatency { get; set; }
    public int ConfigConnectionUploadRate { get; set; } = 102400;
    public int ConfigConnectionDownloadRate { get; set; } = 102400;
    public int ConfigWorkerThreadCount { get; set; } = 2;
    public bool ConfigPacketAssemblyMultithread { get; set; } = true;
    public int ConfigPacketAssemblyPacketsPerFrame { get; set; } = 60;
    public int ConfigPacketAssemblyFullPacketsPerFrame { get; set; } = 30;
    public int ConfigDelayedVisualsS2cSendingRate { get; set; } = 32;
    public int ConfigTrustClientSimuC2sSendingRate { get; set; } = 64;
    public bool ConfigAllowSpectatorRelays { get; set; }
    public int ConfigP2pCacheSize { get; set; } = 600;
    public IPAddress? ConfigForceIpAddress { get; set; }
    public ushort ConfigServerPort { get; set; } = 2350;
    public ushort ConfigServerP2pPort { get; set; } = 3450;
    public ushort ConfigClientPort { get; set; } = 0;
    public IPAddress? ConfigBindIpAddress { get; set; }
    public string ConfigUseNatUpnp { get; set; } = string.Empty;
    public string ConfigGspName { get; set; } = string.Empty;
    public string ConfigGspUrl { get; set; } = string.Empty;
    public ushort ConfigXmlRpcPort { get; set; } = 5000;
    public bool ConfigXmlRpcAllowRemote { get; set; }
    public string ConfigBlacklistUrl { get; set; } = string.Empty;
    public string ConfigGuestlistFileName { get; set; } = string.Empty;
    public string ConfigBlacklistFileName { get; set; } = string.Empty;
    public string ConfigMinimumClientBuild { get; set; } = string.Empty;
    public bool ConfigDisableCoherenceChecks { get; set; }
    public bool ConfigDisableReplayRecording { get; set; }
    public bool ConfigSaveAllIndividualRuns { get; set; }
    public bool ConfigUseProxy { get; set; }
    public string ConfigProxyUrl { get; set; } = string.Empty;

    public string AccountValidationKey { get; set; } = string.Empty;
    public int ConfigPacketAssemblyThreadCount { get; set; } = 1;
    public ScriptCloudSource ConfigScriptCloudSource { get; set; }
    public int ServerLadderServerLimitMin { get; set; }
    public int ServerLadderServerLimitMax { get; set; } = 50000;
    public string ConfigPackMask { get; set; } = "stadium";
    public string ConfigProxyLogin { get; set; } = string.Empty;
    public string ConfigProxyPassword { get; set; } = string.Empty;
    public string AccountNation { get; set; } = string.Empty;
    public string ConfigConnectionType { get; set; } = "DSL_16384_4096";
}
