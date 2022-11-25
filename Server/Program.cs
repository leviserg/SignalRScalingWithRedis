using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using Server.Providers;
using Server.Workers;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// ############# Services #############

string redisConnectionString = builder.Configuration.GetValue("Redis", "127.0.0.1:6379");
int listenHttpPort = builder.Configuration.GetValue("ListenHttpPort", 8000);
int listenHttpsPort = builder.Configuration.GetValue("ListenHttpsPort", 8001);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(listenHttpPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
    options.ListenAnyIP(listenHttpsPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps();
    });
});

builder.Services.AddSignalR().AddMessagePackProtocol().AddStackExchangeRedis(redisConnectionString, options => {
    options.Configuration.ChannelPrefix = "mychat";
});

builder.Services.AddSingleton<IUserIdProvider, CustomUserProvider>();
builder.Services.AddHostedService<SubscriptionWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", builder =>
    {
        builder.SetIsOriginAllowed(origin => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
    });
});

builder.Services.AddResponseCompression(
    options => options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" })
);

// ############# App #############

var app = builder.Build();

app.UseResponseCompression();

//app.UseHttpsRedirection(); // no need to use if ports are configurable from appsettings - will redirect to default 5000 port and exception will be throwed

app.UseRouting();
/*
app.UseCors(policy =>
{
    policy
        .SetIsOriginAllowed(origin => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
});
*/
app.UseCors("MyCorsPolicy");

app.MapHub<ChatHub>("/chat");

app.Run();
