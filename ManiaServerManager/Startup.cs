using ManiaServerManager.Services;

namespace ManiaServerManager;

internal sealed class Startup : IHostedService
{
    private readonly IServerSetupService serverSetupService;
    private readonly ILogger<Startup> logger;

    public Startup(IServerSetupService serverSetupService, ILogger<Startup> logger)
    {
        this.serverSetupService = serverSetupService;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await serverSetupService.SetupAsync(cancellationToken);

        await StartServerAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task StartServerAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);

        logger.LogInformation("Server started!");
    }
}
