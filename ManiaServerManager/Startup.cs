using ManiaServerManager.Services;
using System.Threading;

namespace ManiaServerManager;

internal sealed class Startup : BackgroundService
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var setupResult = await serverSetupService.SetupAsync(stoppingToken);

        await serverStartService.RunServerAsync(setupResult, stoppingToken);
    }
}
