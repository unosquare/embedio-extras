namespace Unosquare.Labs.EmbedIO.Extra.Tests.TestObjects
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class TestHelper
    {
        public static string SetupStaticFolder()
        {
            var assemblyPath = Path.GetDirectoryName(typeof(TestHelper).GetTypeInfo().Assembly.Location);
            var rootPath = Path.Combine(assemblyPath, "web");

            if (Directory.Exists(rootPath) == false)
                Directory.CreateDirectory(rootPath);

            var files = new Dictionary<string, string>()
            {
                {"index.markdown", IndexMarkdown},
                {"secure.markdown", SecureMarkdown},
                {"database.json", DatabaseJson}
            };

            foreach (var file in files.Where(file => File.Exists(Path.Combine(rootPath, file.Key)) == false))
            {
                File.WriteAllText(Path.Combine(rootPath, file.Key), file.Value);
            }

            return rootPath;
        }

        public static string IndexMarkdown = @"# Hello EmbedIO

This is just a sample. [Go to help](/help.html).

* Because I want.
* Because bacon.

If you are using testing Bearer Tokens, try to access to [secure page](/secure.html).

[Visit EmbedIO](http://unosquare.github.io/embedio).

[Check JSON API](/api/)";

        public static string SecureMarkdown = @"# Secure

You are using bearer token correctly.";

        public static string DatabaseJson = @"{
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
    }
}