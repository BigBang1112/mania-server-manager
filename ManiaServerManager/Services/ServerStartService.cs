using CliWrap.EventStream;
using ManiaServerManager.Server;
using ManiaServerManager.Setup;

namespace ManiaServerManager.Services;

internal interface IServerStartService
{
    Task RunServerAsync(ServerSetupResult setupResult, CancellationToken cancellationToken);
}

internal sealed class ServerStartService : IServerStartService
{
    private readonly ICliService cliService;
    private readonly IWebHostEnvironment hostEnvironment;
    private readonly ILogger<ServerStartService> logger;
    private readonly ServerOptions serverOptions;

    public ServerStartService(ICliService cliService, IConfiguration config, IWebHostEnvironment hostEnvironment, ILogger<ServerStartService> logger)
    {
        this.cliService = cliService;
        this.hostEnvironment = hostEnvironment;
        this.logger = logger;

        serverOptions = new ServerOptions();
        config.GetSection("Server").Bind(serverOptions);
    }

    public async Task RunServerAsync(ServerSetupResult setupResult, CancellationToken cancellationToken)
    {
        var identifier = setupResult.Identifier;

        var executableName = setupResult.ServerType switch
        {
            ServerType.TM => "TrackManiaServer",
            ServerType.TMF or ServerType.TM2020 => Constants.TrackmaniaServer,
            ServerType.ManiaPlanet => Constants.ManiaPlanetServer,
            _ => throw new Exception("Unknown server type")
        };

        var workingDirectory = Path.Combine(hostEnvironment.ContentRootPath, "versions", identifier);
        var targetFilePath = Path.Combine(workingDirectory, OperatingSystem.IsWindows() ? executableName + ".exe" : executableName);
        var arguments = GetArguments(setupResult);

        logger.LogInformation("Starting the server...");
        var events = cliService.ListenAsync(targetFilePath, arguments, workingDirectory, cancellationToken);

        await foreach (var cmd in events)
        {
            switch (cmd)
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

    private IEnumerable<string> GetArguments(ServerSetupResult setupResult)
    {
        yield return "/nodaemon";

        if (!string.IsNullOrWhiteSpace(serverOptions.ValidatePath))
        {
            logger.LogInformation("> Validate Path: {ValidatePath}", serverOptions.ValidatePath);

            yield return $"/validatepath={serverOptions.ValidatePath}";
            yield break;
        }

        logger.LogInformation("> Dedicated Cfg: {cfg}", serverOptions.DedicatedCfg);
        yield return $"/dedicated_cfg={serverOptions.DedicatedCfg}";

        string gameSettings;
        if (string.IsNullOrWhiteSpace(serverOptions.MatchSettings))
        {
            gameSettings = serverOptions.GameSettings;
            logger.LogInformation("> GameSettings: {GameSettings}", gameSettings);
        }
        else
        {
            gameSettings = "MatchSettings/" + serverOptions.MatchSettings;
            logger.LogInformation("> MatchSettings: {GameSettings}", gameSettings);
        }
        yield return $"/game_settings={gameSettings}";

        logger.LogInformation("> Login: {Login}", serverOptions.Login);
        yield return $"/login={serverOptions.Login}";

        logger.LogInformation($"> Password: **PROTECTED**");
        yield return $"/password={serverOptions.Password}";

        logger.LogInformation("> Lan: {Lan}", serverOptions.Lan);
        if (serverOptions.Lan) yield return "/lan";

        if (setupResult.ServerType == ServerType.TM)
        {
            if (!serverOptions.Lan) yield return "/internet";

            logger.LogInformation("> Game: {Game}", serverOptions.Game);
            yield return $"/game={serverOptions.Game.ToString().ToLowerInvariant()}";
        }

        logger.LogInformation("> Server name: {ServerName}", serverOptions.ServerName);
        yield return $"/servername={serverOptions.ServerName}";

        if (!string.IsNullOrWhiteSpace(serverOptions.ServerPassword))
        {
            logger.LogInformation("> Server password: {ServerPassword}", serverOptions.ServerPassword);
            yield return $"/serverpassword={serverOptions.ServerPassword}";
        }

        if (!string.IsNullOrWhiteSpace(serverOptions.Join))
        {
            logger.LogInformation("> Join: {Join}", serverOptions.Join);
            yield return $"/join={serverOptions.Join}";
        }

        if (!string.IsNullOrWhiteSpace(serverOptions.JoinPassword))
        {
            logger.LogInformation("> Join password: {JoinPassword}", serverOptions.JoinPassword);
            yield return $"/joinpassword={serverOptions.JoinPassword}";
        }

        if (serverOptions.LoadCache)
        {
            logger.LogInformation("> Load cache: {LoadCache}", serverOptions.LoadCache);
            yield return "/loadcache";
        }

        if (!string.IsNullOrWhiteSpace(serverOptions.ForceIp))
        {
            logger.LogInformation("> Force IP: {ForceIp}", serverOptions.ForceIp);
            yield return $"/forceip={serverOptions.ForceIp}";
        }

        if (!string.IsNullOrWhiteSpace(serverOptions.BindIp))
        {
            logger.LogInformation("> Bind IP: {BindIp}", serverOptions.BindIp);
            yield return $"/bindip={serverOptions.BindIp}";
        }

        if (serverOptions.VerboseRpcFull)
        {
            logger.LogInformation("> Verbose RPC Full: {VerboseRpcFull}", serverOptions.VerboseRpcFull);
            yield return "/verbose_rpc_full";
        }

        if (serverOptions.VerboseRpc)
        {
            logger.LogInformation("> Verbose RPC: {VerboseRpc}", serverOptions.VerboseRpc);
            yield return "/verbose_rpc";
        }

        if (!string.IsNullOrWhiteSpace(setupResult.TitleId))
        {
            yield return $"/title={setupResult.TitleId}";
        }

        // TODO: Check if multiple /serverplugin are supported

        // TODO: Check if multiple /parsegbx are supported
        if (!string.IsNullOrWhiteSpace(serverOptions.ParseGbx))
        {
            logger.LogInformation("> Parse Gbx: {ParseGbx}", serverOptions.ParseGbx);
            yield return $"/parsegbx={serverOptions.ParseGbx}";
        }
    }
}
