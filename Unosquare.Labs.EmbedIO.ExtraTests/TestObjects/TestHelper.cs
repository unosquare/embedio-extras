namespace Unosquare.Labs.EmbedIO.ExtraTests
{
    using System.Collections.Generic;
    using System.IO;
    using Unosquare.Labs.EmbedIO.ExtraTests.Properties;

    public class TestHelper
    {
        public static string SetupStaticFolder()
        {
            var assemblyPath = Path.GetDirectoryName(typeof (TestHelper).Assembly.Location);
            var rootPath = Path.Combine(assemblyPath, "web");

            if (Directory.Exists(rootPath) == false)
                Directory.CreateDirectory(rootPath);

            var files = new Dictionary<string, string>()
            {
                {"index.markdown", Resources.index},
                {"secure.markdown", Resources.secure},
                {"database.json", Resources.database}
            };

            foreach (var file in files)
            {
                if (File.Exists(Path.Combine(rootPath, file.Key)) == false)
                    File.WriteAllText(Path.Combine(rootPath, file.Key), file.Value);
            }

            return rootPath;
        }
    }
}