using System.IO.Abstractions;

namespace ManiaServerManager.Setup;

internal sealed record DownloadResult(FileSystemStream Stream, bool NewlyDownloaded, string? ETag)
    : IDisposable, IAsyncDisposable
{
    public void Dispose()
    {
        Stream.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return Stream.DisposeAsync();
    }
}
