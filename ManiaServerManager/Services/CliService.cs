using CliWrap.EventStream;
using CliWrap;

namespace ManiaServerManager.Services;

internal interface ICliService
{
    Task ExecuteAsync(string targetFilePath, IEnumerable<string> arguments, string? workingDir = null, CancellationToken cancellationToken = default);
    IAsyncEnumerable<CommandEvent> ListenAsync(string targetFilePath, IEnumerable<string> arguments, string? workingDir = null, CancellationToken cancellationToken = default);
    void Observe(string targetFilePath, IEnumerable<string> arguments, string? workingDir = null, CancellationToken cancellationToken = default);
}

internal sealed class CliService : ICliService
{
    private readonly ILogger<CliService> logger;

    public CliService(ILogger<CliService> logger)
    {
        this.logger = logger;
    }

    public async Task ExecuteAsync(string targetFilePath, IEnumerable<string> arguments, string? workingDir = null, CancellationToken cancellationToken = default)
    {
        await Cli.Wrap(targetFilePath)
            .WithArguments(arguments)
            .WithWorkingDirectory(workingDir ?? Directory.GetCurrentDirectory())
            .ExecuteAsync(cancellationToken);
    }

    public IAsyncEnumerable<CommandEvent> ListenAsync(string targetFilePath, IEnumerable<string> arguments, string? workingDir = null, CancellationToken cancellationToken = default)
    {
        return Cli.Wrap(targetFilePath)
            .WithArguments(arguments)
            .WithWorkingDirectory(workingDir ?? Directory.GetCurrentDirectory())
            .WithStandardOutputPipe(PipeTarget.ToStream(Stream.Null))
            .WithValidation(CommandResultValidation.None)
            .ListenAsync(cancellationToken);
    }

    public void Observe(string targetFilePath, IEnumerable<string> arguments, string? workingDir = null, CancellationToken cancellationToken = default)
    {
        Cli.Wrap(targetFilePath)
            .WithArguments(arguments)
            .WithWorkingDirectory(workingDir ?? Directory.GetCurrentDirectory())
            .Observe(cancellationToken)
            .Subscribe(new Observer(logger));
    }

    private class Observer : IObserver<CommandEvent>
    {
        private readonly ILogger<CliService> logger;

        public Observer(ILogger<CliService> logger)
        {
            this.logger = logger;
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(CommandEvent value)
        {
            switch (value)
            {
                case StartedCommandEvent started:
                    logger.LogInformation("Server is about to start! (PID: {ProcessId})", started.ProcessId);
                    break;
                case StandardErrorCommandEvent errE:
                    logger.LogError("{Error}", errE.Text);
                    break;
                case ExitedCommandEvent exited:
                    logger.LogInformation("Server exited: {ExitCode}", exited.ExitCode);
                    break;
                case StandardOutputCommandEvent outE:
                    logger.LogInformation("{Output}", outE.Text);
                    break;
            }
        }
    }
}
