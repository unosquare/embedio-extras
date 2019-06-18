namespace EmbedIO.Markdown
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The Markdown Static Module takes in a static markdown file and converts it into HTML before returning a response. 
    /// It will accept markdown/html/htm extensions.
    /// </summary>
    /// <seealso cref="WebModuleBase" />
    public class MarkdownStaticModule : WebModuleBase
    {
        /// <summary>
        /// Default document constant to "index.markdown".
        /// </summary>
        public const string DefaultDocumentName = "index.markdown";

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownStaticModule"/> class.
        /// </summary>
        /// <param name="fileSystemPath">The file system path.</param>
        /// <exception cref="System.ArgumentException">Path '" + fileSystemPath + "' does not exist.</exception>
        public MarkdownStaticModule(string fileSystemPath)
        {
            if (Directory.Exists(fileSystemPath) == false)
                throw new ArgumentException($"Path \'{fileSystemPath}\' does not exist.");

            FileSystemPath = fileSystemPath;
            DefaultDocument = DefaultDocumentName;

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Get, HandleGet);
        }

        /// <inheritdoc />
        public override string Name => nameof(MarkdownStaticModule);

        /// <summary>
        /// Gets or sets the default document.
        /// Defaults to "index.html"
        /// Example: "root.xml".
        /// </summary>
        public string DefaultDocument { get; set; }

        /// <summary>
        /// Gets the file system path from which files are retrieved.
        /// </summary>
        public string FileSystemPath { get; protected set; }

        private Task<bool> HandleGet(IHttpContext context, CancellationToken ct)
        {
            var localPath = GetLocalPath(context);

            if (!File.Exists(localPath))
                return Task.FromResult(false);

            using (var reader = File.OpenText(localPath))
            {
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    CommonMark.CommonMarkConverter.Convert(reader, writer);
                    context.Response.ContentType = "text/html";
                }
            }

            return Task.FromResult(true);
        }

        private string GetLocalPath(IHttpContext context)
        {
            var urlPath = context.Request.Url.LocalPath.Replace('/', Path.DirectorySeparatorChar);

            // adjust the path to see if we've got a default document
            if (urlPath.Last() == Path.DirectorySeparatorChar)
                urlPath = urlPath + DefaultDocument;

            urlPath = urlPath.TrimStart(Path.DirectorySeparatorChar);

            if (Path.GetExtension(urlPath) == ".html") urlPath = urlPath.Replace(".html", ".markdown");
            if (Path.GetExtension(urlPath) == ".htm") urlPath = urlPath.Replace(".htm", ".markdown");

            return Path.Combine(FileSystemPath, urlPath);
        }
    }
}