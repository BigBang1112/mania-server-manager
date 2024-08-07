namespace ManiaServerManager.Server;

public sealed class ServerOptions
{
    public required ServerType Type { get; init; }
    public string Version { get; init; } = "Latest";
    public ServerDownloadHost DownloadHost { get; init; } = new();
}
