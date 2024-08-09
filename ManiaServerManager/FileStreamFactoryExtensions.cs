using System.IO.Abstractions;

namespace ManiaServerManager;

internal static class FileStreamFactoryExtensions
{
    public static FileSystemStream NewReadAsync(this IFileStreamFactory fs, string path, int bufferSize = 4096)
    {
        return fs.New(path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize,
            useAsync: true);
    }

    public static FileSystemStream NewWriteAsync(this IFileStreamFactory fs, string path, int bufferSize = 4096)
    {
        return fs.New(path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Write,
            bufferSize,
            useAsync: true);
    }
}
