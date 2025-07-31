using ManiaServerManager.Enums;
using ManiaServerManager.Models;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Net;
using System.Text.Json;

namespace ManiaServerManager.Services;

internal interface IServerSetupService
{
    Task<ServerSetupResult> SetupAsync(CancellationToken cancellationToken);
}

internal sealed class ServerSetupService : IServerSetupService
{
    private readonly IZipExtractService zipExtractService;
    private readonly IDedicatedCfgService dedicatedCfgService;
    private readonly IConfiguration config;
    private readonly IFileSystem fileSystem;
    private readonly HttpClient http;
    private readonly ILogger logger;

    private readonly string baseWorkingPath = Constants.DataPath;

    public ServerSetupService(
        IZipExtractService zipExtractService,
        IDedicatedCfgService dedicatedCfgService,
        IConfiguration config,
        IFileSystem fileSystem,
        HttpClient http,
        ILogger logger)
    {
        this.zipExtractService = zipExtractService;
        this.dedicatedCfgService = dedicatedCfgService;
        this.config = config;
        this.fileSystem = fileSystem;
        this.http = http;
        this.logger = logger;
    }

    public async Task<ServerSetupResult> SetupAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation("Mania Server Manager!");
        logger.LogInformation("Setting up server...");

        // MSM_TYPE: TMF / ManiaPlanet / TM2020
        var serverType = config.Type;
        logger.LogInformation("> Type: {Type}", serverType);

        // VERSION: 0000-00-00 / Latest
        var serverVersion = config.Version;
        logger.LogInformation("> Version: {Version}", serverVersion);

        var isLatest = serverVersion.ToLowerInvariant() is Constants.Latest;

        var allServerDownloadHost = string.IsNullOrEmpty(config.DownloadHost.All) ? null : config.DownloadHost.All;

        var uri = config.Type switch
        {
            ServerType.TM => new Uri($"{allServerDownloadHost ?? config.DownloadHost.TM}/TmDedicatedServer_{(isLatest ? "2006-05-30" : serverVersion)}.zip"),
            ServerType.TMF => new Uri($"{allServerDownloadHost ?? config.DownloadHost.TMF}/{Constants.TrackmaniaServer}_{(isLatest ? Constants.LastTrackmaniaForeverServerVersion : serverVersion)}.zip"),
            ServerType.ManiaPlanet => new Uri($"{allServerDownloadHost ?? config.DownloadHost.ManiaPlanet}/ManiaplanetServer_{(isLatest ? Constants.LatestUpper : serverVersion)}.zip"),
            ServerType.TM2020 => new Uri($"{allServerDownloadHost ?? config.DownloadHost.TM2020}/{Constants.TrackmaniaServer}_{(isLatest ? Constants.LatestUpper : serverVersion)}.zip"),
            ServerType.None => throw new Exception("Server type not set"),
            _ => throw new Exception("Unknown server type"),
        };

        // should rather be a constant file located in /server/archives folder, which could be wiped out by cleaner commands or env variables
        // env var example: CLEANUP_ARCHIVES=true # deletes all archives
        // CLEANUP_DESTRUCTIVE=true # deletes all archives and servers other than the one being run
        // REINSTALL=true

        await using var serverArchiveResult = await DownloadArchiveAsync(uri, cancellationToken);

        var identifier = GetServerIdentifierFromZip(serverArchiveResult.Stream, serverType, isLatest);

        if (serverArchiveResult.NewlyDownloaded || config.Reinstall)
        {
            logger.LogInformation("Extracting archive...");

            await zipExtractService.ExtractServerAsync(serverType, serverArchiveResult.Stream, Path.Combine(Constants.ServerVersionsPath, identifier), cancellationToken);
        }

        if (serverType is ServerType.ManiaPlanet)
        {
            await LoadTitleAsync(identifier, cancellationToken);
        }

        // setup dedicated_cfg.txt
        switch (serverType)
        {
            case ServerType.TM2020:
                await dedicatedCfgService.CreateTM2020ConfigAsync(Path.Combine(baseWorkingPath, Constants.ServerVersionsPath, identifier, "UserData", "Config"), cancellationToken);
                break;
            case ServerType.ManiaPlanet:
                await dedicatedCfgService.CreateManiaPlanetConfigAsync(Path.Combine(baseWorkingPath, Constants.ServerVersionsPath, identifier, "UserData", "Config"), cancellationToken);
                break;
            case ServerType.TMF:
                await dedicatedCfgService.CreateTMFConfigAsync(Path.Combine(baseWorkingPath, Constants.ServerVersionsPath, identifier, "GameData", "Config"), cancellationToken);
                break;
            case ServerType.TM:
                await dedicatedCfgService.CreateTMConfigAsync(Path.Combine(baseWorkingPath, Constants.ServerVersionsPath, identifier), cancellationToken);
                break;
        }

        logger.LogInformation("Server setup complete!");

        return new ServerSetupResult(serverType, serverVersion);
    }

    private static string GetServerIdentifierFromZip(Stream serverZipStream, ServerType serverType, bool isLatest)
    {
        using var archive = new ZipArchive(serverZipStream, ZipArchiveMode.Read, leaveOpen: true);

        var executableName = serverType switch
        {
            ServerType.TM => $"{Constants.TmDedicatedServer}/TrackManiaServer",
            ServerType.TMF or ServerType.TM2020 => Constants.TrackmaniaServer,
            ServerType.ManiaPlanet => Constants.ManiaPlanetServer,
            _ => throw new Exception("Unknown server type")
        };

        var executableEntry = archive.GetEntry(executableName) ?? throw new Exception("Executable not found");

        if (isLatest)
        {
            return $"{serverType}_{Constants.LatestUpper}";
        }

        return $"{serverType}_{executableEntry.LastWriteTime:yyyy-MM-dd}";
    }

    private async Task LoadTitleAsync(string identifier, CancellationToken cancellationToken)
    {
        var title = config.Title;
        logger.LogInformation("> Title: {Title}", title);

        var ignoreTitleDownload = config.IgnoreTitleDownload;
        logger.LogInformation("> Ignore title download: {IgnoreTitleDownload}", ignoreTitleDownload);

        var titleDownloadHost = config.TitleDownloadHost;

        if (ignoreTitleDownload)
        {
            return;
        }

        var titleFileName = title + ".Title.Pack.gbx";
        var titleDownloadUri = new Uri($"{titleDownloadHost}/{titleFileName}");

        await using var titleArchiveResult = await DownloadArchiveAsync(titleDownloadUri, cancellationToken);

        // maybe could always run the copy?
        if (!titleArchiveResult.NewlyDownloaded && !config.Reinstall)
        {
            return;
        }

        var copiedTitlePath = Path.Combine(baseWorkingPath, Constants.ServerVersionsPath, identifier, "Packs", titleFileName);
        await using var titleStream = fileSystem.FileStream.NewWriteAsync(copiedTitlePath);
        await titleArchiveResult.Stream.CopyToAsync(titleStream, cancellationToken);
    }

    private async Task<DownloadResult> DownloadArchiveAsync(Uri uri, CancellationToken cancellationToken)
    {
        var serverArchivesPath = Path.Combine(baseWorkingPath, Constants.ServerArchivesPath);

        fileSystem.Directory.CreateDirectory(serverArchivesPath);

        var etagsFilePath = Path.Combine(serverArchivesPath, "etags.txt");

        var etags = new Dictionary<string, string>();

        if (fileSystem.File.Exists(etagsFilePath))
        {
            await using var etagsStream = fileSystem.FileStream.NewReadAsync(etagsFilePath);
            using var etagsReader = new StreamReader(etagsStream);

            string? line;
            while ((line = await etagsReader.ReadLineAsync(cancellationToken)) is not null)
            {
                var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    etags[parts[0]] = parts[1];
                }
            }
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);

        if (etags.TryGetValue(uri.AbsoluteUri, out var etag))
        {
            logger.LogInformation("Etag found: {Etag}", etag);
            request.Headers.IfNoneMatch.Add(new(etag));
        }

        logger.LogInformation("Requesting archive...");

        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        var archiveFileName = response.Content.Headers.ContentDisposition?.FileName ?? Path.GetFileName(uri.LocalPath);
        var archiveFilePath = Path.Combine(serverArchivesPath, archiveFileName);

        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            logger.LogInformation("Archive not modified");

            return new DownloadResult(fileSystem.FileStream.NewReadAsync(archiveFilePath), NewlyDownloaded: false, etag);
        }

        if (response.IsSuccessStatusCode)
        {
            if (response.Headers.ETag?.Tag == etag)
            {
                logger.LogInformation("Archive not modified but server returned OK (etag match)");
                return new DownloadResult(fileSystem.FileStream.NewReadAsync(archiveFilePath), NewlyDownloaded: false, etag);
            }

            if (response.Headers.ETag is { Tag: var newEtag })
            {
                logger.LogInformation("New Etag: {NewEtag}", newEtag);
                etags[uri.AbsoluteUri] = newEtag;
            }
        }

        response.EnsureSuccessStatusCode(); // TODO: replace with pipeline

        logger.LogInformation("Downloading archive...");

        await using (var fileStream = fileSystem.FileStream.NewWriteAsync(archiveFilePath))
        {
            await CopyToWithProgressAsync(fileStream, response.Content, cancellationToken: cancellationToken);
        }

        await using (var etagsStream = fileSystem.FileStream.NewWriteAsync(etagsFilePath))
        using (var etagsWriter = new StreamWriter(etagsStream))
        {
            foreach (var kvp in etags)
            {
                await etagsWriter.WriteLineAsync($"{kvp.Key} {kvp.Value}");
            }
        }

        return new DownloadResult(fileSystem.FileStream.NewReadAsync(archiveFilePath), NewlyDownloaded: true, etag);
    }

    private async Task CopyToWithProgressAsync(Stream toStream, HttpContent content, int percentageStep = 5, CancellationToken cancellationToken = default)
    {
        using var stream = await content.ReadAsStreamAsync(cancellationToken);

        var totalBytes = content.Headers.ContentLength;

        var buffer = new byte[ushort.MaxValue];
        int bytesRead;
        long bytesReceived = 0;

        var percentageBefore = 0;
        var percentage = 0;

        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await toStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

            bytesReceived += bytesRead;

            percentage = totalBytes.HasValue ? (int)(bytesReceived * 100 / totalBytes) : 0;

            if (percentageBefore == percentage)
            {
                continue;
            }

            if (percentage % percentageStep == 0)
            {
                if (totalBytes.HasValue)
                {
                    logger.LogInformation("Downloaded {BytesReceived} of {TotalBytes} ({Percentage}%)", Bytes.Format(bytesReceived), Bytes.Format(totalBytes.Value), percentage);
                }
                else
                {
                    logger.LogInformation("Downloaded {BytesReceived} ({Percentage})", Bytes.Format(bytesReceived), percentage);
                }
            }

            percentageBefore = percentage;
        }
    }
}