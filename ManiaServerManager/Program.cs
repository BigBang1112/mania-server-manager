using ManiaServerManager;
using ManiaServerManager.Configuration;
using ManiaServerManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO.Abstractions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog();

builder.Services.AddHttpClient<IServerSetupService, ServerSetupService>().AddStandardResilienceHandler();
builder.Services.AddTransient<IZipExtractService, ZipExtractService>();
builder.Services.AddTransient<IFileSystem, FileSystem>();
builder.Services.AddTransient<IServerStartService, ServerStartService>();
builder.Services.AddTransient<ICliService, CliService>();
builder.Services.AddTransient<IDedicatedCfgService, DedicatedCfgService>();
builder.Services.AddHostedService<Startup>();

builder.Services.AddTelemetryServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.Run();