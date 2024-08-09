namespace ManiaServerManager.Server;

internal sealed class ServerOptions
{
    public ServerType Type { get; set; }
    public string Version { get; set; } = Constants.LatestUpper;
    public string Title { get; set; } = "SMStorm@nadeo";
    public bool IgnoreTitleDownload { get; set; }
    public string TitleDownloadHost { get; set; } = "https://maniaplanet.com/ingame/public/titles/download";
    public bool Reinstall { get; set; }
    public ServerDownloadHost DownloadHost { get; set; } = new();
    public string ValidatePath { get; set; } = "";
    public string DedicatedCfg { get; set; } = "dedicated_cfg";
    public string MatchSettings { get; set; } = "";
    public string GameSettings { get; set; } = "";
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public GameType Game { get; set; }
    public bool Lan { get; set; }
    public string ServerName { get; set; } = "";
    public string ServerPassword { get; set; } = "";
    public string Join { get; set; } = "";
    public string JoinPassword { get; set; } = "";
    public bool LoadCache { get; set; }
    public string ForceIp { get; set; } = "";
    public string BindIp { get; set; } = "";
    public bool VerboseRpcFull { get; set; }
    public bool VerboseRpc { get; set; }
    public string ParseGbx { get; set; } = "";
    public string SuperAdminPassword { get; set; } = "SuperAdmin";
    public string AdminPassword { get; set; } = "Admin";
    public string UserPassword { get; set; } = "User";
    public string ValidationKey { get; set; } = "";
    public string Comment { get; set; } = "";
    public HideServer HideServer { get; set; }
    public int MaxPlayers { get; set; } = 255;
    public int MaxSpectators { get; set; } = 255;
    public string SpectatorPassword { get; set; } = "";
    public bool KeepPlayerSlots { get; set; }
    public LadderMode LadderMode { get; set; } = LadderMode.Forced;
    public int ServerPort { get; set; } = 2350;
    public int ServerP2pPort { get; set; } = 3450;
    public int XmlRpcPort { get; set; } = 5000;
}
