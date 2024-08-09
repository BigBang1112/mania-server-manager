using CliWrap.EventStream;
using CliWrap;

namespace ManiaServerManager.Services;

internal interface ICliService
{
    IAsyncEnumerable<CommandEvent> ListenAsync(string targetFilePath, IEnumerable<string> arguments, string workingDir, CancellationToken cancellationToken);
}

internal sealed class CliService : ICliService
{
    public IAsyncEnumerable<CommandEvent> ListenAsync(string targetFilePath, IEnumerable<string> arguments, string workingDir, CancellationToken cancellationToken)
    {
        return Cli.Wrap(targetFilePath)
            .WithArguments(arguments)
            .WithWorkingDirectory(workingDir)
            .ListenAsync(cancellationToken);
    }
}
