using ManiaServerManager.Enums;
using ManiaServerManager.Models;
using ManiaServerManager.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Formats.Tar;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Text;

namespace ManiaServerManager.Tests;

public sealed class ZipExtractServiceTests
{
    [Fact]
    public async Task ExtractContentAsync_ZipWithExpectedRootExtractsOnlyThatRoot()
    {
        using var directory = new TempDirectory();
        using var archive = CreateZip(
            ("UserData/Maps/map.gbx", "map"),
            ("side-content.txt", "ignored"));
        var service = CreateService();

        await service.ExtractContentAsync(archive, directory.Path, "UserData");

        Assert.Equal("map", await File.ReadAllTextAsync(Path.Combine(directory.Path, "UserData", "Maps", "map.gbx")));
        Assert.False(File.Exists(Path.Combine(directory.Path, "side-content.txt")));
    }

    [Fact]
    public async Task ExtractContentAsync_ZipWithoutExpectedRootExtractsAllContentIntoRoot()
    {
        using var directory = new TempDirectory();
        var existingPath = Path.Combine(directory.Path, "GameData", "Config", "settings.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(existingPath)!);
        await File.WriteAllTextAsync(existingPath, "old");
        using var archive = CreateZip(("Config/settings.txt", "new"), ("Scripts/mode.txt", "script"));
        var service = CreateService();

        await service.ExtractContentAsync(archive, directory.Path, "GameData");

        Assert.Equal("new", await File.ReadAllTextAsync(existingPath));
        Assert.Equal("script", await File.ReadAllTextAsync(Path.Combine(directory.Path, "GameData", "Scripts", "mode.txt")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExtractContentAsync_TarFormatsExtractExpectedRoot(bool gzip)
    {
        using var directory = new TempDirectory();
        using var archive = CreateTar(gzip,
            ("UserData/Maps/map.gbx", "map"),
            ("other.txt", "ignored"));
        var service = CreateService();

        await service.ExtractContentAsync(archive, directory.Path, "UserData");

        Assert.Equal("map", await File.ReadAllTextAsync(Path.Combine(directory.Path, "UserData", "Maps", "map.gbx")));
        Assert.False(File.Exists(Path.Combine(directory.Path, "other.txt")));
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("folder/../../outside.txt")]
    public async Task ExtractContentAsync_RejectsPathTraversal(string entryName)
    {
        using var directory = new TempDirectory();
        using var archive = CreateZip((entryName, "bad"));
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => service.ExtractContentAsync(archive, directory.Path, "UserData"));

        Assert.Contains("outside the server directory", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractServerAsync_StripsTmPrefixAndSkipsUnusedContent()
    {
        using var directory = new TempDirectory();
        using var archive = CreateZip(
            ("TmDedicatedServer/server.bin", "server"),
            ("CommandLine.html", "unused"),
            ("RemoteControlExamples/example.txt", "unused"));
        var service = CreateService();

        await service.ExtractServerAsync(ServerType.TM, archive, directory.Path);

        Assert.Equal("server", await File.ReadAllTextAsync(Path.Combine(directory.Path, "server.bin")));
        Assert.False(File.Exists(Path.Combine(directory.Path, "CommandLine.html")));
        Assert.False(Directory.Exists(Path.Combine(directory.Path, "RemoteControlExamples")));
    }

    [Fact]
    public async Task ExtractServerAsync_PreservesExistingDedicatedConfigAndWritesDefault()
    {
        using var directory = new TempDirectory();
        var configPath = Path.Combine(directory.Path, "dedicated.cfg");
        await File.WriteAllTextAsync(configPath, "custom");
        using var archive = CreateZip(("dedicated.cfg", "stock"));
        var service = CreateService();

        await service.ExtractServerAsync(ServerType.TM, archive, directory.Path);

        Assert.Equal("custom", await File.ReadAllTextAsync(configPath));
        Assert.Equal("stock", await File.ReadAllTextAsync(Path.Combine(directory.Path, "dedicated.default.cfg")));
    }

    [Theory]
    [InlineData("dedicated.cfg", "dedicated.default.cfg")]
    [InlineData("GameData/Config/dedicated_cfg.txt", "GameData/Config/dedicated_cfg.default.txt")]
    public void TryRenameEntry_KnownConfigReturnsDefaultName(string entryName, string expected)
    {
        Assert.True(ZipExtractService.TryRenameEntry(entryName, out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TryRenameEntry_OtherFileReturnsFalse()
    {
        Assert.False(ZipExtractService.TryRenameEntry("other.txt", out var newName));
        Assert.Null(newName);
    }

    private static ZipExtractService CreateService()
    {
        var config = Substitute.For<IConfiguration>();
        config.UnusedContent.Returns(new UnusedContent());
        return new ZipExtractService(config, new FileSystem(), Substitute.For<ILogger>());
    }

    private static MemoryStream CreateZip(params (string Name, string Content)[] entries)
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                var entry = archive.CreateEntry(name);
                using var writer = new StreamWriter(entry.Open(), Encoding.UTF8, leaveOpen: false);
                writer.Write(content);
            }
        }
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateTar(bool gzip, params (string Name, string Content)[] entries)
    {
        var stream = new MemoryStream();
        Stream archiveStream = gzip ? new GZipStream(stream, CompressionLevel.SmallestSize, leaveOpen: true) : stream;
        using (var writer = new TarWriter(archiveStream, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                writer.WriteEntry(new PaxTarEntry(TarEntryType.RegularFile, name)
                {
                    DataStream = new MemoryStream(Encoding.UTF8.GetBytes(content))
                });
            }
        }
        if (gzip)
        {
            archiveStream.Dispose();
        }
        stream.Position = 0;
        return stream;
    }
}
