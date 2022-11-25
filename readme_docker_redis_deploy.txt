1. Download Docker Desktop & install

2. Pull Docker Redis image: 
	- basic way
		$ docker pull bitnami/redis
	- TL;DR
		$ docker run --name redis -e ALLOW_EMPTY_PASSWORD=yes bitnami/redis:latest
	- Docker Compose:
		$ curl -sSL https://raw.githubusercontent.com/bitnami/containers/main/bitnami/redis/docker-compose.yml > docker-compose.yml
		$ docker-compose up -d
	+ local through the yml file - minimal:
		- create "docker-compose.yml" file in solution folder :
			======= content =========
			version: '3.4'					-- docker compose syntax version

			services:						-- service with containers
			  redis:						-- redis container
				image: redis:alpine			-- alpine - minimal linux distributive
				ports:
				  - "6379:6379"				-- port forwarding from host to container
			======= content =========
		- open terminal in folder with created "docker-compose.yml" file 
			$ docker-compose up
			{see the messages and check container status in Docker Desktop application}
			
			
4. Add NuGet package to Server project : Microsoft.AspNetCore.SignalR.StackExchangeRedis 
	and setup Redis Bus - in Program.cs 
	+ add Redis connection string (appsettings.json e.g. { ...,"Redis":"localhost:6379", ...}) - get configuration from IConfiguration from dependency injection
		var redisConnectionString = configuration.GetValue("Redis","yourDefaultRedisConnectionStringIfNotFound");
	
	+ builder.Services.AddSignalR() -> builder.Services.AddSignalR().AddStackExchangeRedis(redisConnectionString, options => {
		options.Configuration.ChannelPrefix = "mychat";
	})
5. Modify EntryPoint Progam.cs for running different instances on configurable different ports (5001/5002) 
	- add default "ListenPort" key to appsettings.json { ...,"Redis":"localhost:6379", "ListenPort":5000, ...}
	// ############
	public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = GetConfiguration();

            var host = BuildWebHost(configuration, args);

            host.Run();
        }

        private static IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .CaptureStartupErrors(false)
                .ConfigureKestrel(options =>
                {
                    var port = configuration.GetValue<int>("ListenPort", 5000);
                    options.Listen(IPAddress.Any, port, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                })
                .UseStartup<Startup>()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(configuration)
                .Build();

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }
    }
	// ############
	add intput element to clients (type:number) and attach its value to init hubconnection port 

	