﻿using System.Threading;

namespace EmbedIO.Extra.Tests.TestObjects
{
    internal static class Resources
    {
        private const string ServerAddress = "http://localhost:{0}/";
        private static int _counter = 9699;

        public static string GetServerAddress()
        {
            Interlocked.Increment(ref _counter);
            return string.Format(ServerAddress, _counter);
        }

        public static string IndexHtml = @"<h1>Hello EmbedIO</h1>
<p>This is just a sample. <a href=""/help.html"">Go to help</a>.</p>
<ul>
<li>Because I want.</li>
<li>Because bacon.</li>
</ul>
<p>If you are using testing Bearer Tokens, try to access to <a href=""/secure.html"">secure page</a>.</p>
<p><a href=""http://unosquare.github.io/embedio"">Visit EmbedIO</a>.</p><p><a href=""/api/"">Check JSON API</a></p>";
    }
}
