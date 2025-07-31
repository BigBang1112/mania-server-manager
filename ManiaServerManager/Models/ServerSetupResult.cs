using ManiaServerManager.Enums;

namespace ManiaServerManager.Models;

/// <summary>
/// Result from <see cref="ServerSetupService"/>
/// </summary>
/// <param name="ServerType">Game category of the server.</param>
/// <param name="ServerVersion">Server version, always in lowercase.</param>
internal sealed record ServerSetupResult(ServerType ServerType, string ServerVersion)
{
    public string Identifier { get; } = $"{ServerType}_{(ServerVersion is Constants.Latest ? Constants.LatestUpper : ServerVersion)}";
}
