using Jab;
using ManiaServerManager;
using ManiaServerManager.Services;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Runtime.InteropServices;

using var cts = new CancellationTokenSource();

using var registration = PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
{
    Console.WriteLine("Received SIGTERM, shutting down early...");
    cts.Cancel();
});

using var provider = new ServiceProvider();

var config = provider.GetService<IConfiguration>();
var setup = provider.GetService<IServerSetupService>();

await setup.SetupAsync(cts.Token);

[ServiceProvider]
[Singleton<IConfiguration, Configuration>]
[Singleton<ILogger, Logger>]
[Transient<IDedicatedCfgService, DedicatedCfgService>]
[Transient<IZipExtractService, ZipExtractService>]
[Singleton<HttpClient>]
[Transient<IServerSetupService, ServerSetupService>]
[Transient<IFileSystem, FileSystem>]
internal partial class ServiceProvider;