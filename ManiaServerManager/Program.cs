using ManiaServerManager;
using ManiaServerManager.Extensions;
using ManiaServerManager.Services;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using System.IO.Abstractions;

var builder = WebApplication.CreateSlimBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.OpenTelemetry(options =>
    {
        // TODO
    })
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddHttpClient();
builder.Services.AddTransient<IServerSetupService, ServerSetupService>();
builder.Services.AddTransient<IZipExtractService, ZipExtractService>();
builder.Services.AddTransient<IFileSystem, FileSystem>();
builder.Services.AddTransient<IServerStartService, ServerStartService>();
builder.Services.AddTransient<ICliService, CliService>();
builder.Services.AddTransient<IDedicatedCfgService, DedicatedCfgService>();
builder.Services.AddHostedService<Startup>();

builder.Services.AddOpenTelemetry()
    .WithMetrics(x =>
    {
        x.AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
            .AddMeter("Microsoft.AspNetCore.Http.Connections")
            .AddMeter("Microsoft.AspNetCore.Routing")
            .AddMeter("Microsoft.AspNetCore.Diagnostics")
            .AddMeter("Microsoft.AspNetCore.RateLimiting")
            .AddMeter("System.Net.Http")
            .AddOtlpExporter();
    })
    .WithTracing(x =>
    {
        if (builder.Environment.IsDevelopment())
        {
            x.SetSampler<AlwaysOnSampler>();
        }

        x.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    });

var app = builder.Build();

app.MapOpenApi();

app.MapScalarUi();

app.Run();