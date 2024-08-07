using ManiaServerManager.Server;
using ManiaServerManager.Setup;
using Microsoft.Extensions.Options;

namespace ManiaServerManager.Services;

internal interface IServerSetupService
{
    Task<ServerSetupResult> SetupAsync(CancellationToken cancellationToken);
}

internal sealed class ServerSetupService : IServerSetupService
{
    private readonly IOptions<ServerOptions> serverOptions;
    private readonly ILogger<ServerSetupService> logger;

    public ServerSetupService(IOptions<ServerOptions> serverOptions, ILogger<ServerSetupService> logger)
    {
        this.serverOptions = serverOptions;
        this.logger = logger;
    }

    public async Task<ServerSetupResult> SetupAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation("Mania Server Manager!");
        logger.LogInformation("Setting up server...");

        // SERVER_TYPE: TMF / ManiaPlanet / TM2020
        var serverType = serverOptions.Value.Type;
        logger.LogInformation("> Type: {Type}", serverType);

        // VERSION: 0000-00-00 / Latest
        var serverVersion = serverOptions.Value.Version;
        logger.LogInformation("> Version: {Version}", serverVersion);

        var isLatest = serverVersion.ToLowerInvariant() is Constants.Latest;

        var allServerDownloadHost = string.IsNullOrEmpty(serverOptions.Value.DownloadHost.All) ? null : serverOptions.Value.DownloadHost.All;

        var uri = serverOptions.Value.Type switch
        {
            ServerType.TM => new Uri($"{allServerDownloadHost ?? serverOptions.Value.DownloadHost.TM}/TmDedicatedServer_{(isLatest ? "2006-05-30" : serverVersion)}.zip"),
            ServerType.TMF => new Uri($"{allServerDownloadHost ?? serverOptions.Value.DownloadHost.TMF}/{Constants.TrackmaniaServer}_{(isLatest ? Constants.LastTrackmaniaForeverServerVersion : serverVersion)}.zip"),
            ServerType.ManiaPlanet => new Uri($"{allServerDownloadHost ?? serverOptions.Value.DownloadHost.ManiaPlanet}/ManiaplanetServer_{(isLatest ? Constants.LatestUpper : serverVersion)}.zip"),
            ServerType.TM2020 => new Uri($"{allServerDownloadHost ?? serverOptions.Value.DownloadHost.TM2020}/{Constants.TrackmaniaServer}_{(isLatest ? Constants.LatestUpper : serverVersion)}.zip"),
            _ => throw new Exception("Unknown server type"),
        };

        // should rather be a constant file located in /server/archives folder, which could be wiped out by cleaner commands or env variables
        // env var example: CLEANUP_ARCHIVES=true # deletes all archives
        // CLEANUP_DESTRUCTIVE=true # deletes all archives and servers other than the one being run
        // REINSTALL=true

        // DownloadAndExtractServerAsync
        // ...

        logger.LogInformation("Server setup complete!");

        return new ServerSetupResult(serverType, serverVersion, null);
    }
}