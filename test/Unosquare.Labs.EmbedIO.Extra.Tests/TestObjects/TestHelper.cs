using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EmbedIO.Extra.Tests.TestObjects
{
    internal class TestHelper
    {
        private const string IndexMarkdown = @"# Hello EmbedIO

This is just a sample. [Go to help](/help.html).

* Because I want.
* Because bacon.

If you are using testing Bearer Tokens, try to access to [secure page](/secure.html).

[Visit EmbedIO](http://unosquare.github.io/embedio).

[Check JSON API](/api/)";

        private const string SecureMarkdown = @"# Secure

You are using bearer token correctly.";

        private const string DatabaseJson = @"{
  ""posts"": [
    { ""id"": 1, ""title"": ""json-server"", ""author"": ""typicode"" },
    { ""id"": 2, ""title"": ""embedio"", ""author"": ""unosquare"" },
    { ""id"": 3, ""title"": ""tubular"", ""author"": ""unosquare"" }
  ],
  ""comments"": [
    { ""id"": 1, ""body"": ""some comment"", ""postId"": 1 },
    { ""id"": 2, ""body"": ""some comment"", ""postId"": 2 }
  ]
}";

        public static string SetupStaticFolder()
        {
            var assemblyPath = Path.GetDirectoryName(typeof(TestHelper).GetTypeInfo().Assembly.Location);
            var rootPath = Path.Combine(assemblyPath, "web");

            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);

            var files = new Dictionary<string, string>
            {
                {"index.markdown", IndexMarkdown},
                {"secure.markdown", SecureMarkdown},
                {"database.json", DatabaseJson}
            };

            foreach (var (fileName, file) in files.Where(x => !File.Exists(Path.Combine(rootPath, x.Key))))
            {
                File.WriteAllText(Path.Combine(rootPath, fileName), file);
            }

            return rootPath;
        }
    }
}