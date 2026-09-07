using ManiaServerManager.Enums;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Tar;
using System.IO.Abstractions;
using System.IO.Compression;

namespace ManiaServerManager.Services;

internal interface IZipExtractService
{
    Task ExtractServerAsync(ServerType type, Stream stream, string outputDirectory, CancellationToken cancellationToken = default);
    Task ExtractContentAsync(Stream stream, string outputDirectory, string rootFolderName, CancellationToken cancellationToken = default);
}

internal sealed class ZipExtractService : IZipExtractService
{
    private const string DedicatedCfgFileTM = "dedicated.cfg";
    private const string DedicatedCfgFileTMF = "GameData/Config/dedicated_cfg.txt";

    private readonly IConfiguration config;
    private readonly IFileSystem fileSystem;
    private readonly ILogger logger;

    public ZipExtractService(
        IConfiguration config,
        IFileSystem fileSystem,
        ILogger logger)
    {
        this.config = config;
        this.fileSystem = fileSystem;
        this.logger = logger;
    }

    public async Task ExtractServerAsync(ServerType type, Stream stream, string outputDirectory, CancellationToken cancellationToken = default)
    {
        using var archive = new ZipArchive(stream);

        foreach (var entry in archive.Entries)
        {
            if (entry.Name == "") // folder
            {
                continue;
            }

            if (config.UnusedContent.Files.Contains(entry.Name))
            {
                continue;
            }

            if (config.UnusedContent.Folders.Any(entry.FullName.StartsWith)) // be careful adding more folders
            {
                continue;
            }

            var entryPath = entry.FullName.StartsWith(Constants.TmDedicatedServer)
                ? Path.Combine(outputDirectory, entry.FullName[(Constants.TmDedicatedServer.Length + 1)..])
                : Path.Combine(outputDirectory, entry.FullName);

            var directoryPath = fileSystem.Path.GetDirectoryName(entryPath)!;
            fileSystem.Directory.CreateDirectory(directoryPath);

            const UnixFileMode OwnershipPermissions =
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute;

            var fileMode = (UnixFileMode)(entry.ExternalAttributes >> 16) & OwnershipPermissions;
            fileMode |= UnixFileMode.UserWrite | UnixFileMode.GroupWrite;

            var fileStreamOptions = new FileStreamOptions()
            {
                Access = FileAccess.Write,
                Mode = FileMode.Create,
                Share = FileShare.None,
                BufferSize = 4096,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            };

            if (fileMode != UnixFileMode.None && !OperatingSystem.IsWindows())
            {
                fileStreamOptions.UnixCreateMode = fileMode;
            }

            logger.LogInformation("Extracting {FileName} ({FileSize})...", entry.FullName, Bytes.Format(entry.Length));

            // Avoid overwriting files that users usually edit (often dedicated_cfg.txt)
            if (TryRenameEntry(entry.FullName, out string? newFullName))
            {
                await using var entryStreamInside = entry.Open();
                await using var fileStreamDefault = fileSystem.FileStream.New(Path.Combine(outputDirectory, newFullName), fileStreamOptions);
                await entryStreamInside.CopyToAsync(fileStreamDefault, cancellationToken);

                // Skips the code that would overwrite the file
                if (fileSystem.File.Exists(entryPath))
                {
                    continue;
                }
            }

            using var entryStream = entry.Open();
            await using var fileStream = fileSystem.FileStream.New(entryPath, fileStreamOptions);
            await entryStream.CopyToAsync(fileStream, cancellationToken);
        }
    }

    public async Task ExtractContentAsync(Stream stream, string outputDirectory, string rootFolderName, CancellationToken cancellationToken = default)
    {
        if (IsZip(stream))
        {
            await ExtractZipContentAsync(stream, outputDirectory, rootFolderName, cancellationToken);
            return;
        }

        await ExtractTarContentAsync(stream, outputDirectory, rootFolderName, IsGZip(stream), cancellationToken);
    }

    private async Task ExtractZipContentAsync(Stream stream, string outputDirectory, string rootFolderName, CancellationToken cancellationToken)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var rootPrefix = rootFolderName + "/";
        var containsRootFolder = archive.Entries.Any(entry => NormalizeEntryName(entry.FullName).StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase));

        foreach (var entry in archive.Entries)
        {
            var entryName = NormalizeEntryName(entry.FullName);
            if (entry.Name.Length == 0 || containsRootFolder && !entryName.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var relativePath = containsRootFolder ? entryName[rootPrefix.Length..] : entryName;
            await ExtractEntryAsync(entry.Open(), entry.Length, entry.FullName, Path.Combine(outputDirectory, rootFolderName), relativePath, cancellationToken);
        }
    }

    private async Task ExtractTarContentAsync(Stream stream, string outputDirectory, string rootFolderName, bool gzip, CancellationToken cancellationToken)
    {
        var rootPrefix = rootFolderName + "/";
        var containsRootFolder = false;

        using (var archiveStream = OpenTarStream(stream, gzip))
        using (var reader = new TarReader(archiveStream, leaveOpen: true))
        {
            while (reader.GetNextEntry() is { } entry)
            {
                if (NormalizeEntryName(entry.Name).StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    containsRootFolder = true;
                    break;
                }
            }
        }

        stream.Position = 0;
        using var extractionStream = OpenTarStream(stream, gzip);
        using var extractionReader = new TarReader(extractionStream, leaveOpen: true);

        while (extractionReader.GetNextEntry() is { } entry)
        {
            var entryName = NormalizeEntryName(entry.Name);
            if (entry.DataStream is null || containsRootFolder && !entryName.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var relativePath = containsRootFolder ? entryName[rootPrefix.Length..] : entryName;
            await ExtractEntryAsync(entry.DataStream, entry.Length, entry.Name, Path.Combine(outputDirectory, rootFolderName), relativePath, cancellationToken);
        }
    }

    private async Task ExtractEntryAsync(Stream entryStream, long length, string displayName, string outputDirectory, string relativePath, CancellationToken cancellationToken)
    {
        using (entryStream)
        {
            var outputRoot = fileSystem.Path.GetFullPath(outputDirectory);
            var entryPath = fileSystem.Path.GetFullPath(fileSystem.Path.Combine(outputRoot, relativePath));
            var relativeEntryPath = fileSystem.Path.GetRelativePath(outputRoot, entryPath);

            if (relativeEntryPath == ".." || relativeEntryPath.StartsWith($"..{fileSystem.Path.DirectorySeparatorChar}"))
            {
                throw new InvalidDataException($"Archive entry '{displayName}' points outside the server directory.");
            }

            var directoryPath = fileSystem.Path.GetDirectoryName(entryPath)!;
            fileSystem.Directory.CreateDirectory(directoryPath);
            logger.LogInformation("Extracting {FileName} ({FileSize})...", displayName, Bytes.Format(length));

            await using var fileStream = fileSystem.FileStream.New(entryPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            await entryStream.CopyToAsync(fileStream, cancellationToken);
        }
    }

    private static Stream OpenTarStream(Stream stream, bool gzip)
    {
        return gzip ? new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true) : new NonDisposingStream(stream);
    }

    private static bool IsZip(Stream stream)
    {
        Span<byte> signature = stackalloc byte[4];
        stream.Position = 0;
        var bytesRead = stream.Read(signature);
        stream.Position = 0;
        return bytesRead == signature.Length && signature[0] == 'P' && signature[1] == 'K';
    }

    private static bool IsGZip(Stream stream)
    {
        Span<byte> signature = stackalloc byte[2];
        stream.Position = 0;
        var bytesRead = stream.Read(signature);
        stream.Position = 0;
        return bytesRead == signature.Length && signature[0] == 0x1f && signature[1] == 0x8b;
    }

    private static string NormalizeEntryName(string entryName)
    {
        var normalizedName = entryName.Replace('\\', '/').TrimStart('/');
        return normalizedName.StartsWith("./") ? normalizedName[2..] : normalizedName;
    }

    private sealed class NonDisposingStream(Stream inner) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => inner.Length;
        public override long Position { get => inner.Position; set => inner.Position = value; }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    internal static bool TryRenameEntry(string entryFullName, [NotNullWhen(true)] out string? newName)
    {
        switch (entryFullName)
        {
            case DedicatedCfgFileTMF:
                newName = entryFullName.Replace(".txt", ".default.txt");
                return true;
            case DedicatedCfgFileTM:
                newName = entryFullName.Replace(".cfg", ".default.cfg");
                return true;
            default:
                newName = null;
                return false;
        }
    }
}
