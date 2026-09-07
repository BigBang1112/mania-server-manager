using ManiaServerManager.Enums;
using ManiaServerManager.Models;

namespace ManiaServerManager.Tests;

public sealed class ModelDefaultsTests
{
    [Fact]
    public void DedicatedCfg_HasExpectedOperationalDefaults()
    {
        var cfg = new DedicatedCfg();

        Assert.Equal("SuperAdmin", cfg.AuthorizationSuperAdminName);
        Assert.Equal(32, cfg.ServerMaxPlayers);
        Assert.Equal(32, cfg.ServerMaxSpectators);
        Assert.Equal(LadderMode.Forced, cfg.ServerLadderMode);
        Assert.True(cfg.ServerEnableP2pUpload);
        Assert.False(cfg.ServerEnableP2pDownload);
        Assert.Equal(60000, cfg.ServerCallVoteTimeout);
        Assert.Equal(0.5, cfg.ServerCallVoteRatio);
        Assert.Collection(cfg.ServerCallVoteRatios, ratio => { Assert.Equal("Ban", ratio.Command); Assert.Equal(-1, ratio.Ratio); });
        Assert.Equal(2350, cfg.ConfigServerPort);
        Assert.Equal(3450, cfg.ConfigServerP2pPort);
        Assert.Equal(5000, cfg.ConfigXmlRpcPort);
        Assert.Equal("stadium", cfg.ConfigPackMask);
    }

    [Fact]
    public void UnusedContent_ContainsKnownServerDocumentationAndExamples()
    {
        var content = new UnusedContent();

        Assert.Contains("CommandLine.html", content.Files);
        Assert.Contains("RemoteControlExamples", content.Folders);
        Assert.Contains("TmDedicatedServer/RemoteControlExamples", content.Folders);
    }

    [Fact]
    public void VoteRatio_HasExpectedDefaults()
    {
        var ratio = new VoteRatio();

        Assert.Equal(string.Empty, ratio.Command);
        Assert.Equal(0.5, ratio.Ratio);
    }
}
