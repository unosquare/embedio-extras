[![Build Status](https://travis-ci.org/unosquare/embedio-extras.svg?branch=master)](https://travis-ci.org/unosquare/embedio-extras)
[![Build status](https://ci.appveyor.com/api/projects/status/70runy7vrgix31j5?svg=true)](https://ci.appveyor.com/project/geoperez/embedio-extras)

# EmbedIO Extras

Extras Modules to show how to extend EmbedIO

## Bearer Token

Allow to authenticate with a Bearer Token. It uses a Token endpoint (at /token path) and with a defined validation delegate
create a **JsonWebToken**. The module can check all incoming requests or a paths collection to authorize with **HTTP Authorization
header**.

You can easily add Bearer Token to your EmbedIO server using a Basic Authorization Server Provider or writing your own:

```csharp
// Create basic authentication provider
var basicAuthProvider = new BasicAuthorizationServerProvider();
// You can set which routes to check, empty param will secure entire server
var routes = new[] { "/secure.html" };

// Create Webserver with console logger and attach Bearer Token Module
var server = WebServer.CreateWithConsole("http://localhost:9696/");
server.RegisterModule(new BearerTokenModule(basicAuthProvider, routes));
```

## OWIN

EmbedIO can use the OWIN platform in two different approach:

* You can use EmbedIO as OWIN server and use all OWIN framework with EmbedIO modules.

```csharp
public class Program
{
    /// <summary>
    /// Entry point
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        try
        {
            OwinServerFactory.Log = new SimpleConsoleLog();

            var options = new StartOptions
            {
                ServerFactory = OwinServerFactory.ServerFactoryName,
                Port = 4578
            };

            using (WebApp.Start<Startup>(options))
            {
                OwinServerFactory.Log.DebugFormat("Running a http server on port {0}", options.Port);
                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            OwinServerFactory.Log.Error(ex);
            Console.ReadKey();
        }
    }
}

/// <summary>
/// Startup object
/// </summary>
public class Startup
{
    /// <summary>
    /// Configure the OwinApp
    /// </summary>
    /// <param name="app"></param>
    public void Configuration(IAppBuilder app)
    {
        app.UseErrorPage();
        app.UseDirectoryBrowser();
        app.UseRazor(InitRoutes);
        // Attach a EmbedIO WebAPI Controller directly to OwinApp Configuration
        app.UseWebApi(typeof (PeopleController).Assembly);
    }

    /// <summary>
    /// Initialize the Razor files
    /// </summary>
    /// <param name="table"></param>
    public static void InitRoutes(IRouteTable table)
    {
        table
            .AddFileRoute("/about/me", "Views/about.cshtml", new { Name = "EmbedIO Razor", Date = DateTime.UtcNow });
    }
}
```

* You can use OWIN Middleware into EmbedIO as a module.

```csharp
public class Program
{
    /// <summary>
    /// Entry point
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        try
        {
            // UseOwin() function returns Owin App for configuration
            using (var webServer = WebServer
                        .CreateWithConsole("http://localhost:4578")
                        .WithWebApi(typeof (PeopleController).Assembly)
                        .UseOwin((owinApp) => 
                            owinApp
                            .UseDirectoryBrowser()
                            .UseRazor(InitRoutes)))
                    {
                        webServer.RunAsync();
                        Console.ReadKey();
                    }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Initialize the Razor files
    /// </summary>
    /// <param name="table"></param>
    public static void InitRoutes(IRouteTable table)
    {
        table
            .AddFileRoute("/about/me", "Views/about.cshtml", new { Name = "EmbedIO Razor", Date = DateTime.UtcNow });
    }
}
```

## Markdown

Markdown Static Module takes a markdown file and convert it to HTML before to response. 
It will accept markdown/html/htm extensions (This could be a middleware later).

```csharp
// Create Webserver with console logger and attach Markdown Static Module
var server = WebServer.CreateWithConsole("http://localhost:9696/");
server.RegisterModule(new MarkdownStaticModule(@"c:\web"));
```

## JsonServer

Based [JsonServer's](https://github.com/typicode/json-server) idea, you can set a JSON file as database and use any REST 
method to manipulate it. 

```csharp
// Create Webserver with console logger and attach Json's Server
var server = WebServer.CreateWithConsole("http://localhost:9696/");
server.RegisterModule(new JsonServerModule(jsonPath: Path.Combine(@"c:\web", "database.json")));
```

Supported methods: 

* GET collection (//yourhost/entity) 
* GET single (//yourhost/entity/1 where 1 is the ID)
* POST (//yourhost/entity with POST body the JSON object)
* PUT (//yourhost/entity/1 with POST body the JSON object)
* DELETE (//yourhost/entity/1 where 1 is the ID)