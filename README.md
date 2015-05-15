[![Build Status](https://travis-ci.org/unosquare/embedio-extras.svg?branch=master)](https://travis-ci.org/unosquare/embedio-extras)
[![Build status](https://ci.appveyor.com/api/projects/status/70runy7vrgix31j5?svg=true)](https://ci.appveyor.com/project/geoperez/embedio-extras)

# EmbedIO Extras

Extras Modules to show how to extend EmbedIO

## Markdown

This Module takes a markdown file and convert it to HTML before to response. It will accept markdown/html/htm extensions.

This could be a middleware later.

## JsonServer

Based [JsonServer's](https://github.com/typicode/json-server) idea, you can set a JSON file as database and use any REST 
method to manipulate it.

## Bearer Token

Allow to authenticate with a Bearer Token. It uses a Token endpoint (at /token path) and with a defined validation delegate
create a **JsonWebToken**. The module can check all incoming requests or a paths collection to authorize with **HTTP Authorization
header**.