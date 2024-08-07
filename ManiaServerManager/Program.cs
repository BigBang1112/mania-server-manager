using ManiaServerManager;
using ManiaServerManager.Server;
using ManiaServerManager.Services;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddOptions<ServerOptions>()
    .Bind(builder.Configuration.GetSection("Server"));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddTransient<IServerSetupService, ServerSetupService>();
builder.Services.AddHostedService<Startup>();

var app = builder.Build();

app.Run();