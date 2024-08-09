namespace ManiaServerManager.Setup;

internal sealed class UnusedContentOptions
{
    public HashSet<string> Folders { get; set; } = [];
    public HashSet<string> Files { get; set; } = [];
}