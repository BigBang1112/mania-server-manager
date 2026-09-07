using ManiaServerManager.Enums;

namespace ManiaServerManager.Tests;

public sealed class ConfigurationTests
{
    [Fact]
    public void Constructor_UsesExpectedDefaults()
    {
        using var environment = new TestEnvironment();

        var config = new Configuration();

        Assert.Equal(ServerType.TM2020, config.Type);
        Assert.Equal("Latest", config.Version);
        Assert.Equal("TM2020_Latest", config.Identifier);
        Assert.False(config.Reinstall);
        Assert.Equal("dedicated_cfg.txt", config.DedicatedCfgFileName);
        Assert.Empty(config.UserDataDownloadUrls);
        Assert.Empty(config.GameDataDownloadUrls);
    }

    [Fact]
    public void Constructor_ParsesDownloadUrlListsAndTrimsEntries()
    {
        using var environment = new TestEnvironment();
        environment.Set("MSM_USERDATA_DOWNLOAD_URLS", " https://example.com/a.zip, ; https://example.com/b.tar.gz ");
        environment.Set("MSM_GAMEDATA_DOWNLOAD_URLS", "https://example.com/c.tar;https://example.com/d.zip");

        var config = new Configuration();

        Assert.Equal(["https://example.com/a.zip", "https://example.com/b.tar.gz"], config.UserDataDownloadUrls);
        Assert.Equal(["https://example.com/c.tar", "https://example.com/d.zip"], config.GameDataDownloadUrls);
    }

    [Fact]
    public void Constructor_ParsesPreparedTitlesForManiaPlanet()
    {
        using var environment = new TestEnvironment();
        environment.Set("MSM_SERVER_TYPE", "ManiaPlanet");
        environment.Set("MSM_PREPARE_TITLES", "TitleA@user TitleB@user,TitleC@user;TitleD@user");

        var config = new Configuration();

        Assert.Equal(["TitleA@user", "TitleB@user", "TitleC@user", "TitleD@user"], config.PrepareTitles);
    }

    [Fact]
    public void Constructor_IgnoresPreparedTitlesForOtherServerTypes()
    {
        using var environment = new TestEnvironment();
        environment.Set("MSM_PREPARE_TITLES", "TitleA@user");

        var config = new Configuration();

        Assert.Empty(config.PrepareTitles);
    }

    [Fact]
    public void Constructor_ParsesCallVoteRatios()
    {
        using var environment = new TestEnvironment();
        environment.Set("MSM_CFG_SERVER_CALLVOTE_RATIOS", " Ban = -1 ; Kick = 1 ");

        var config = new Configuration();

        Assert.Collection(
            config.Cfg.ServerCallVoteRatios,
            ratio => { Assert.Equal("Ban", ratio.Command); Assert.Equal(-1, ratio.Ratio); },
            ratio => { Assert.Equal("Kick", ratio.Command); Assert.Equal(1, ratio.Ratio); });
    }

    [Fact]
    public void Constructor_ReportsInvalidCallVoteRatio()
    {
        using var environment = new TestEnvironment();
        environment.Set("MSM_CFG_SERVER_CALLVOTE_RATIOS", "Ban");

        var exception = Assert.Throws<AggregateException>(() => new Configuration());

        Assert.Contains(exception.InnerExceptions, error => error.Message.Contains("Invalid call vote ratio format", StringComparison.Ordinal));
    }

    [Fact]
    public void Constructor_ReportsMissingRequiredServerSettings()
    {
        using var environment = new TestEnvironment();
        environment.Set("MSM_ONLY_SETUP", null);

        var exception = Assert.Throws<AggregateException>(() => new Configuration());

        Assert.Contains(exception.InnerExceptions, error => error.Message.Contains("MSM_ACCOUNT_LOGIN", StringComparison.Ordinal));
        Assert.Contains(exception.InnerExceptions, error => error.Message.Contains("MSM_ACCOUNT_PASSWORD", StringComparison.Ordinal));
        Assert.Contains(exception.InnerExceptions, error => error.Message.Contains("MSM_GAME_SETTINGS", StringComparison.Ordinal));
    }
}
