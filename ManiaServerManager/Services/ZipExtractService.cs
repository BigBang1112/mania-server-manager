using ManiaServerManager.Server;
using ManiaServerManager.Setup;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.IO.Compression;

namespace ManiaServerManager.Services;

internal interface IZipExtractService
{
    Task ExtractServerAsync(ServerType type, Stream stream, string outputDirectory, CancellationToken cancellationToken = default);
}

internal sealed class ZipExtractService : IZipExtractService
{
    private const string DedicatedCfgFileTM = "dedicated.cfg";
    private const string DedicatedCfgFileTMF = "GameData/Config/dedicated_cfg.txt";

    private readonly UnusedContentOptions unusedContentOptions;
    private readonly IFileSystem fileSystem;
    private readonly IWebHostEnvironment hostEnvironment;
    private readonly ILogger<ZipExtractService> logger;

    public ZipExtractService(
        IConfiguration config,
        IFileSystem fileSystem,
        IWebHostEnvironment hostEnvironment,
        ILogger<ZipExtractService> logger)
    {
        unusedContentOptions = new UnusedContentOptions();
        config.GetSection("UnusedContent").Bind(unusedContentOptions);

        this.fileSystem = fileSystem;
        this.hostEnvironment = hostEnvironment;
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

            if (unusedContentOptions.Files.Contains(entry.Name))
            {
                continue;
            }

            if (unusedContentOptions.Folders.Any(entry.FullName.StartsWith)) // be careful adding more folders
            {
                continue;
            }

            var entryPath = entry.FullName.StartsWith(Constants.TmDedicatedServer)
                ? Path.Combine(hostEnvironment.ContentRootPath, outputDirectory, entry.FullName[(Constants.TmDedicatedServer.Length + 1)..])
                : Path.Combine(hostEnvironment.ContentRootPath, outputDirectory, entry.FullName);

            var directoryPath = fileSystem.Path.GetDirectoryName(entryPath)!;
            fileSystem.Directory.CreateDirectory(directoryPath);

            const UnixFileMode OwnershipPermissions =
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute;

            var fileMode = (UnixFileMode)(entry.ExternalAttributes >> 16) & OwnershipPermissions;

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

            // Avoid overwriting files that users usually edit (usually dedicated_cfg.txt)
            if (TryRenameEntry(entry.FullName, out string? newFullName))
            {
                using var entryStreamInside = entry.Open();
                await using var fileStreamDefault = fileSystem.FileStream.New(Path.Combine(hostEnvironment.ContentRootPath, outputDirectory, newFullName), fileStreamOptions);
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
