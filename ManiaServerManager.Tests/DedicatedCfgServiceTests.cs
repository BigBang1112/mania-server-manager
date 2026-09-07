using ManiaServerManager.Models;
using ManiaServerManager.Services;
using NSubstitute;
using System.Xml.Linq;

namespace ManiaServerManager.Tests;

public sealed class DedicatedCfgServiceTests
{
    public static TheoryData<string, string> Variants => new()
    {
        { "TM2020", "workerthreadcount" },
        { "ManiaPlanet", "scriptcloud_source" },
        { "ManiaPlanet3", "proxy_login" },
        { "TMF", "packmask" },
        { "TM", "nation" }
    };

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task CreateConfigAsync_WritesValidVariant(string variant, string variantElement)
    {
        using var directory = new TempDirectory();
        var service = CreateService(new DedicatedCfg(), "custom.cfg");

        await CreateAsync(service, variant, directory.Path);

        var document = XDocument.Load(Path.Combine(directory.Path, "custom.cfg"));
        Assert.Equal("dedicated", document.Root?.Name.LocalName);
        Assert.NotNull(document.Descendants(variantElement).SingleOrDefault());
    }

    [Fact]
    public async Task CreateTM2020ConfigAsync_WritesConfiguredValuesAndRatios()
    {
        using var directory = new TempDirectory();
        var cfg = new DedicatedCfg
        {
            AuthorizationSuperAdminName = "Root",
            ServerMaxPlayers = 64,
            ConfigServerPort = 2355,
            ServerCallVoteRatios =
            [
                new VoteRatio { Command = "Ban", Ratio = -1 },
                new VoteRatio { Command = "Kick", Ratio = 1 }
            ]
        };
        var service = CreateService(cfg, "dedicated_cfg.txt");

        await service.CreateTM2020ConfigAsync(directory.Path, CancellationToken.None);

        var document = XDocument.Load(Path.Combine(directory.Path, "dedicated_cfg.txt"));
        Assert.Equal("Root", document.Descendants("authorization_levels").Descendants("name").First().Value);
        Assert.Equal("64", document.Descendants("max_players").Single().Value);
        Assert.Equal("2355", document.Descendants("server_port").Single().Value);
        Assert.Equal(["Ban", "Kick"], document.Descendants("voteratio").Select(element => element.Attribute("command")!.Value));
    }

    [Fact]
    public async Task CreateConfigAsync_ObservesPreCancelledToken()
    {
        using var directory = new TempDirectory();
        var service = CreateService(new DedicatedCfg(), "dedicated.cfg");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.CreateTM2020ConfigAsync(directory.Path, cancellation.Token));
    }

    private static DedicatedCfgService CreateService(DedicatedCfg cfg, string fileName)
    {
        var config = Substitute.For<IConfiguration>();
        config.Cfg.Returns(cfg);
        config.DedicatedCfgFileName.Returns(fileName);
        return new DedicatedCfgService(config);
    }

    private static Task CreateAsync(DedicatedCfgService service, string variant, string directory)
    {
        return variant switch
        {
            "TM2020" => service.CreateTM2020ConfigAsync(directory, CancellationToken.None),
            "ManiaPlanet" => service.CreateManiaPlanetConfigAsync(directory, CancellationToken.None),
            "ManiaPlanet3" => service.CreateManiaPlanet3ConfigAsync(directory, CancellationToken.None),
            "TMF" => service.CreateTMFConfigAsync(directory, CancellationToken.None),
            "TM" => service.CreateTMConfigAsync(directory, CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        };
    }
}
