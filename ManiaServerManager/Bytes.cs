namespace ManiaServerManager;

internal static class Bytes
{
    public static string Format(long bytes) => bytes switch
    {
        < 1_000 => $"{bytes} B",
        < 1_000_000 => $"{bytes / 1_000f:0.00} KB",
        < 1_000_000_000 => $"{bytes / 1_000_000f:0.00} MB",
        _ => $"{bytes / 1_000_000_000f:0.00} GB"
    };
}