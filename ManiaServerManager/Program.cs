using ManiaServerManager;
using ManiaServerManager.Services;
using System.IO.Abstractions;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddHttpClient();
builder.Services.AddTransient<IServerSetupService, ServerSetupService>();
builder.Services.AddTransient<IZipExtractService, ZipExtractService>();
builder.Services.AddTransient<IFileSystem, FileSystem>();
builder.Services.AddHostedService<Startup>();

var app = builder.Build();

app.Run();