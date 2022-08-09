** THIS REPO HAS BEEN ARCHIVED **

[![Build status](https://ci.appveyor.com/api/projects/status/70runy7vrgix31j5?svg=true)](https://ci.appveyor.com/project/geoperez/embedio-extras)
![Buils status](https://github.com/unosquare/embedio-extras/workflows/.NET%20Core%20CI/badge.svg)
 
# EmbedIO Extras

![EmbedIO](https://raw.githubusercontent.com/unosquare/embedio/master/images/embedio.png)

:star: *Please star this project if you find it useful!*

Additional Modules showing how to extend [EmbedIO](https://github.com/unosquare/embedio). Feel free to use these modules in your projects.

## Bearer Token Module

Provides the ability to authenticate requests via a Bearer Token. This module creates a Token endpoint (at the predefined '/token' path) and all you need to do is provide a user validation delegate which authenticates the user. The module will create a **JsonWebToken** which can then be used by your client application for further requests. The module can check all incoming requests or a predefined set of paths. The standard header in use is the **HTTP Authorization header**.

You can easily add Bearer Token to your EmbedIO application using the default Basic Authorization Server Provider or writing your own by implementing `IAuthorizationServerProvider` interface.

The following example will attach Bearer Token to the Web server in the "/api" base route and then a WebAPI Controller using the same base route.

```csharp
// Create Webserver and attach the Bearer Token Module
var server = new WebServer(url)
                .WithBearerToken("/api", "0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF")
                .WithWebApi("/api", o => o.WithController<SecureController>());
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
var server = new WebServer(url)
                .WithModule(new JsonServerModule(jsonPath: Path.Combine(WebRootPath, "database.json");               
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
var server = new WebServer(url)
                .WithModule(new LiteLibModule<TestDbContext>(new TestDbContext(), "/dbapi"));
                
 internal class TestDbContext : LiteDbContext
    {
        public TestDbContext()
            : base("dbase.db")
        {
            // Need to Define the tables Create  dyanmic types ?
        }

    }                
                
```

Supported methods: 

* `GET` collection (`http://yourhost/entity`) 
* `GET` single (`http://yourhost/entity/1` where 1 is the ID)
* `POST` (`http://yourhost/entity` with POST body the JSON object)
* `PUT` (`http://yourhost/entity/1` with POST body the JSON object)
* `DELETE` (`http://yourhost/entity/1` where 1 is the ID)



## Markdown Static Module

The Markdown Static Module takes in a static Markdown file and converts it into HTML before returning a response. 
It will accept markdown/html/htm extensions (This could become middleware later).

```csharp
// Create Webserver and attach Markdown Static Module
var server = new WebServer(url)
               .WithModule(new MarkdownStaticModule("/", WebRootPath));
```
