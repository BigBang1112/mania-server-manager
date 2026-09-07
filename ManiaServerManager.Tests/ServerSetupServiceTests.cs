using ManiaServerManager.Enums;
using ManiaServerManager.Models;
using ManiaServerManager.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace ManiaServerManager.Tests;

public sealed class ServerSetupServiceTests
{
    [Fact]
    public async Task SetupAsync_DownloadsCustomContentAndReusesCachedArchives()
    {
        using var directory = new TempDirectory();
        using var workingDirectory = new WorkingDirectory(directory.Path);
        var serverUri = new Uri("https://example.test/server/TrackmaniaServer_2025.zip");
        var userDataUri = new Uri("https://example.test/content/user.zip");
        var gameDataUri = new Uri("https://example.test/content/game.zip");
        var responses = new Dictionary<Uri, byte[]>
        {
            [serverUri] = CreateZip(("TrackmaniaServer", "server")),
            [userDataUri] = CreateZip(("UserData/Maps/custom.Map.Gbx", "map"), ("side.txt", "ignored")),
            [gameDataUri] = CreateZip(("Scripts/custom.Script.txt", "script"))
        };
        var handler = new ArchiveHttpHandler(responses);
        using var http = new HttpClient(handler);
        var config = CreateConfig(userDataUri, gameDataUri);
        var logger = Substitute.For<ILogger>();
        var fileSystem = new FileSystem();
        var extraction = new ZipExtractService(config, fileSystem, logger);
        var service = new ServerSetupService(
            extraction,
            Substitute.For<IDedicatedCfgService>(),
            config,
            fileSystem,
            http,
            logger);

        var firstResult = await service.SetupAsync(CancellationToken.None);
        var secondResult = await service.SetupAsync(CancellationToken.None);

        Assert.Equal(ServerType.TM2020, firstResult.ServerType);
        Assert.Equal("2025", secondResult.ServerVersion);
        var serverPath = Path.Combine(directory.Path, "data", "servers", "TestServer");
        Assert.Equal("server", await File.ReadAllTextAsync(Path.Combine(serverPath, "TrackmaniaServer")));
        Assert.Equal("map", await File.ReadAllTextAsync(Path.Combine(serverPath, "UserData", "Maps", "custom.Map.Gbx")));
        Assert.Equal("script", await File.ReadAllTextAsync(Path.Combine(serverPath, "GameData", "Scripts", "custom.Script.txt")));
        Assert.False(File.Exists(Path.Combine(serverPath, "side.txt")));
        Assert.Equal(6, handler.Requests.Count);
        Assert.All(handler.Requests.Skip(3), request => Assert.True(request.HadIfNoneMatch));
    }

    [Fact]
    public async Task SetupAsync_InvalidCustomContentUrlThrowsClearError()
    {
        using var directory = new TempDirectory();
        using var workingDirectory = new WorkingDirectory(directory.Path);
        var serverUri = new Uri("https://example.test/server/TrackmaniaServer_2025.zip");
        var handler = new ArchiveHttpHandler(new Dictionary<Uri, byte[]>
        {
            [serverUri] = CreateZip(("TrackmaniaServer", "server"))
        });
        using var http = new HttpClient(handler);
        var config = CreateConfig(null, null);
        config.UserDataDownloadUrls.Returns(["not a URL"]);
        var logger = Substitute.For<ILogger>();
        var fileSystem = new FileSystem();
        var service = new ServerSetupService(
            new ZipExtractService(config, fileSystem, logger),
            Substitute.For<IDedicatedCfgService>(),
            config,
            fileSystem,
            http,
            logger);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SetupAsync(CancellationToken.None));

        Assert.Contains("Invalid UserData download URL", exception.Message, StringComparison.Ordinal);
    }

    private static IConfiguration CreateConfig(Uri? userDataUri, Uri? gameDataUri)
    {
        var config = Substitute.For<IConfiguration>();
        config.Type.Returns(ServerType.TM2020);
        config.Version.Returns("2025");
        config.Identifier.Returns("TestServer");
        config.DownloadHost.Returns(new ServerDownloadHost { All = "https://example.test/server" });
        config.UnusedContent.Returns(new UnusedContent());
        config.UserDataDownloadUrls.Returns(userDataUri is null ? [] : [userDataUri.AbsoluteUri]);
        config.GameDataDownloadUrls.Returns(gameDataUri is null ? [] : [gameDataUri.AbsoluteUri]);
        config.PrepareTitles.Returns([]);
        config.SkipDedicatedCfg.Returns(true);
        return config;
    }

    private static byte[] CreateZip(params (string Name, string Content)[] entries)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                var entry = archive.CreateEntry(name);
                using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
                writer.Write(content);
            }
        }
        return stream.ToArray();
    }

    private sealed class ArchiveHttpHandler(IReadOnlyDictionary<Uri, byte[]> responses) : HttpMessageHandler
    {
        public List<(Uri Uri, bool HadIfNoneMatch)> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri!;
            var hadIfNoneMatch = request.Headers.IfNoneMatch.Count > 0;
            Requests.Add((uri, hadIfNoneMatch));

            if (hadIfNoneMatch)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified));
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(responses[uri])
            };
            response.Headers.ETag = new EntityTagHeaderValue("\"v1\"");
            return Task.FromResult(response);
        }
    }

    private sealed class WorkingDirectory : IDisposable
    {
        private readonly string originalDirectory = Environment.CurrentDirectory;

        public WorkingDirectory(string path) => Environment.CurrentDirectory = path;

        public void Dispose() => Environment.CurrentDirectory = originalDirectory;
    }
}
