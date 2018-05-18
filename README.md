[![Analytics](https://ga-beacon.appspot.com/UA-8535255-2/unosquare/embedio-extras/)](https://github.com/igrigorik/ga-beacon)
[![Build Status](https://travis-ci.org/unosquare/embedio-extras.svg?branch=master)](https://travis-ci.org/unosquare/embedio-extras)
[![Build status](https://ci.appveyor.com/api/projects/status/70runy7vrgix31j5?svg=true)](https://ci.appveyor.com/project/geoperez/embedio-extras)
[![Coverage Status](https://coveralls.io/repos/unosquare/embedio-extras/badge.svg?branch=master)](https://coveralls.io/r/unosquare/embedio-extras?branch=master)

# EmbedIO Extras

:star: *Please star this project if you find it useful!*

Additional Modules showing how to extend [EmbedIO](https://github.com/unosquare/embedio). Feel free to use these modules in your projects.

## Bearer Token Module

Provides the ability to authenticate requests via a Bearer Token. This module creates a Token endpoint (at the predefined '/token' path) and all you need to do is provide a user validation delegate which authenticates the user. The module will create a **JsonWebToken** which can then be used by your client application for further requests. The module can check all incoming requests or a predefined set of paths. The standard header in use is the **HTTP Authorization header**.

You can easily add Bearer Token to your EmbedIO application using a Basic Authorization Server Provider or writing your own:

```csharp
// Create basic authentication provider
var basicAuthProvider = new BasicAuthorizationServerProvider();
// You can set which routes to check; an empty routes array will secure entire server
var routes = new[] { "/secure.html" };

// Create Webserver and attach the Bearer Token Module
var server = WebServer.Create("http://localhost:9696/");
server.RegisterModule(new BearerTokenModule(basicAuthProvider, routes));
```

### Nuget installation [![NuGet version](https://badge.fury.io/nu/EmbedIO.BearerToken.svg)](http://badge.fury.io/nu/EmbedIO.BearerToken)

```
PM> Install-Package EmbedIO.BearerToken
```

## Json Server Module

Based on the [JsonServer's](https://github.com/typicode/json-server) project, with this module, you are able to simply specify a 
JSON file as a database and use standard REST methods to create, update, retrieve and delete records from it. 

```csharp
// Create Webserver and attach JsonServerModule
var server = WebServer.Create("http://localhost:9696/");
server.RegisterModule(new JsonServerModule(jsonPath: Path.Combine(@"c:\web", "database.json")));
```

Supported methods: 

* `GET` collection (`http://yourhost/entity`) 
* `GET` single (`http://yourhost/entity/1` where 1 is the ID)
* `POST` (`http://yourhost/entity` with POST body the JSON object)
* `PUT` (`http://yourhost/entity/1` with POST body the JSON object)
* `DELETE` (`http://yourhost/entity/1` where 1 is the ID)

## LiteLib WebAPI

Similar to Json Server Module, but you can serve an SQLite file with all HTTP verbs using [LiteLib](https://github.com/unosquare/litelib) library.

```csharp
// Create Webserver and attach LiteLibModule with a LiteLib DbContext
var server = WebServer.Create("http://localhost:9696/");
server.RegisterModule(new LiteLibModule<TestDbContext>(new TestDbContext(), "/dbapi/"));
```

Supported methods: 

* `GET` collection (`http://yourhost/entity`) 
* `GET` single (`http://yourhost/entity/1` where 1 is the ID)
* `POST` (`http://yourhost/entity` with POST body the JSON object)
* `PUT` (`http://yourhost/entity/1` with POST body the JSON object)
* `DELETE` (`http://yourhost/entity/1` where 1 is the ID)


### Nuget installation [![NuGet version](https://badge.fury.io/nu/EmbedIO.LiteLibWebApi.svg)](https://badge.fury.io/nu/EmbedIO.LiteLibWebApi)

```
PM> Install-Package EmbedIO.LiteLibWebApi
```

## Markdown Static Module

The Markdown Static Module takes in a static Markdown file and converts it into HTML before returning a response. 
It will accept markdown/html/htm extensions (This could become middleware later).

```csharp
// Create Webserver and attach Markdown Static Module
var server = WebServer.Create("http://localhost:9696/");
server.RegisterModule(new MarkdownStaticModule(@"c:\web"));
```

## OWIN Integration

*Note:* The support to OWIN is not under development, and it may not work correctly. 
EmbedIO can use the OWIN platform in two different ways:

* You can use EmbedIO modules within an OWIN Server. In other words, host your application with an OWIN server and make use of EmbedIO modules.

```csharp
public class Program
{
    /// <summary>
    /// Entry point
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
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

* You can also use OWN middleware within EmbedIO. In other words, you can do stuff like serving Razor views from EmbedIO.

```csharp
public class Program
{
    /// <summary>
    /// Entry point
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        // UseOwin() function returns Owin App for configuration
        using (var webServer = WebServer
                    .Create("http://localhost:4578")
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
