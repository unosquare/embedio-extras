namespace Unosquare.Labs.EmbedIO.Markdown
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Swan;
#if NET46
    using System.Net;
#else
    using Unosquare.Net;
#endif

    /// <summary>
    /// The Markdown Static Module takes in a static markdown file and converts it into HTML before returning a response. 
    /// It will accept markdown/html/htm extensions
    /// </summary>
    /// <seealso cref="Unosquare.Labs.EmbedIO.WebModuleBase" />
    public class MarkdownStaticModule : WebModuleBase
    {
        /// <summary>
        /// Default document constant to "index.markdown"
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
                throw new ArgumentException("Path '" + fileSystemPath + "' does not exist.");

            FileSystemPath = fileSystemPath;
            DefaultDocument = DefaultDocumentName;

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Get, HandleGet);
        }
        
        /// <summary>
        /// Gets the name of this module.
        /// </summary>
        public override string Name => nameof(MarkdownStaticModule).Humanize();

        /// <summary>
        /// Gets or sets the default document.
        /// Defaults to "index.html"
        /// Example: "root.xml"
        /// </summary>
        public string DefaultDocument { get; set; }

        /// <summary>
        /// Gets the file system path from which files are retrieved.
        /// </summary>
        public string FileSystemPath { get; protected set; }

        private Task<bool> HandleGet(HttpListenerContext context, CancellationToken ct)
        {
            var urlPath = context.Request.Url.LocalPath.Replace('/', Path.DirectorySeparatorChar);

            // adjust the path to see if we've got a default document
            if (urlPath.Last() == Path.DirectorySeparatorChar)
                urlPath = urlPath + DefaultDocument;

            urlPath = urlPath.TrimStart(Path.DirectorySeparatorChar);

            if (Path.GetExtension(urlPath) == ".html") urlPath = urlPath.Replace(".html", ".markdown");
            if (Path.GetExtension(urlPath) == ".htm") urlPath = urlPath.Replace(".htm", ".markdown");

            var localPath = Path.Combine(FileSystemPath, urlPath);

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
    }
}
