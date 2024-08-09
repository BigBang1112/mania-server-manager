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
}
