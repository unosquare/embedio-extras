[![Analytics](https://ga-beacon.appspot.com/UA-8535255-2/unosquare/embedio-extras/)](https://github.com/igrigorik/ga-beacon)
[![Build Status](https://travis-ci.org/unosquare/embedio-extras.svg?branch=master)](https://travis-ci.org/unosquare/embedio-extras)
[![Build status](https://ci.appveyor.com/api/projects/status/70runy7vrgix31j5?svg=true)](https://ci.appveyor.com/project/geoperez/embedio-extras)
[![Coverage Status](https://coveralls.io/repos/unosquare/embedio-extras/badge.svg?branch=master)](https://coveralls.io/r/unosquare/embedio-extras?branch=master)

# EmbedIO Extras

Additional Modules showing how to extend EmbedIO. Feel free to use these modules in your projects.

## Bearer Token Module

Provides the ability to authenticate requests via a Bearer Token. This module creates a Token endpoint (at the predefined '/token' path) and all you need to do is provide a user validation delegate which authenticates the user. The module will create a **JsonWebToken** which can then be used by your client application in firther requests. The module can check all incoming requests or a predefined set of paths. The standard header in use is the **HTTP Authorization header**.

You can easily add Bearer Token to your EmbedIO application using a Basic Authorization Server Provider or writing your own:

```csharp
// Create basic authentication provider
var basicAuthProvider = new BasicAuthorizationServerProvider();
// You can set which routes to check; an empty routes array will secure entire server
var routes = new[] { "/secure.html" };

// Create Webserver with a simple console logger and attach the Bearer Token Module
var server = WebServer.CreateWithConsole("http://localhost:9696/");
server.RegisterModule(new BearerTokenModule(basicAuthProvider, routes));
```

### Nuget installation [![NuGet version](https://badge.fury.io/nu/EmbedIO.BearerToken.svg)](http://badge.fury.io/nu/EmbedIO.BearerToken)

```
PM> Install-Package EmbedIO.BearerToken
```

## OWIN

EmbedIO can use the OWIN platform in two different ways:

* You can use EmbedIO modules within an OWIN Server

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

* You can also use OWN modules (middleware) within EmbedIO.

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

### Nuget installation [![NuGet version](https://badge.fury.io/nu/EmbedIO.OWIN.svg)](http://badge.fury.io/nu/EmbedIO.OWIN)

```
PM> Install-Package EmbedIO.OWIN
```

## Markdown Static Module

The Markdown Static Module takes in a static markdown file and converts it into HTML before returning a response. 
It will accept markdown/html/htm extensions (This could become middleware later).

```csharp
// Create Webserver with console logger and attach Markdown Static Module
var server = WebServer.CreateWithConsole("http://localhost:9696/");
server.RegisterModule(new MarkdownStaticModule(@"c:\web"));
```

## JsonServer

Based on the [JsonServer's](https://github.com/typicode/json-server) project, with this module you are able to simply specify a JSON file as a database and use standard REST methods to create, update, retrieve and delete records from it. 

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
