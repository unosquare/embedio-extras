[![Build status](https://ci.appveyor.com/api/projects/status/70runy7vrgix31j5?svg=true)](https://ci.appveyor.com/project/geoperez/embedio-extras)
[![Analytics](https://ga-beacon.appspot.com/UA-8535255-2/unosquare/embedio-extras/)](https://github.com/igrigorik/ga-beacon)
 ![Buils status](https://github.com/unosquare/embedio-extras/workflows/.NET%20Core%20CI/badge.svg)
 
# EmbedIO Extras

![EmbedIO](https://raw.githubusercontent.com/unosquare/embedio/master/images/embedio.png)

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
server.WithBearerToken("/", basicAuthProvider, routes)
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
server.WithModule(new JsonServerModule(jsonPath: Path.Combine(@"c:\web", "database.json")));
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
server.WithModule(new LiteLibModule<TestDbContext>(new TestDbContext(), "/dbapi/"));
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
server.WithModule(new MarkdownStaticModule("/", @"c:\web"));
```
