using ManiaServerManager.Services;

namespace ManiaServerManager;

internal sealed class Startup : IHostedService
{
    private readonly IServerSetupService serverSetupService;
    private readonly IServerStartService serverStartService;
    private readonly ILogger<Startup> logger;

    public Startup(IServerSetupService serverSetupService, IServerStartService serverStartService, ILogger<Startup> logger)
    {
        this.serverSetupService = serverSetupService;
        this.serverStartService = serverStartService;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var setupResult = await serverSetupService.SetupAsync(cancellationToken);

        await serverStartService.RunServerAsync(setupResult, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
