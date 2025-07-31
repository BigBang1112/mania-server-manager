using Jab;
using ManiaServerManager;
using ManiaServerManager.Services;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

using var provider = new ServiceProvider();

var config = provider.GetService<IConfiguration>();

[ServiceProvider]
[Singleton<IConfiguration, Configuration>]
[Singleton<ILogger, Logger>]
[Transient<IDedicatedCfgService, DedicatedCfgService>]
[Transient<IZipExtractService, ZipExtractService>]
[Singleton<HttpClient>]
[Transient<IServerSetupService, ServerSetupService>]
[Transient<IFileSystem, FileSystem>]
internal partial class ServiceProvider;