namespace EmbedIO.Markdown
{
    using System;
    using System.IO;
    using System.Linq;
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
        /// Initializes a new instance of the <see cref="MarkdownStaticModule" /> class.
        /// </summary>
        /// <param name="baseUrlPath">The base URL path.</param>
        /// <param name="fileSystemPath">The file system path.</param>
        /// <exception cref="ArgumentException">Path \'{fileSystemPath}\' does not exist.</exception>
        public MarkdownStaticModule(string baseUrlPath, string fileSystemPath)
            : base(baseUrlPath)
        {
            if (Directory.Exists(fileSystemPath) == false)
                throw new ArgumentException($"Path \'{fileSystemPath}\' does not exist.");

            FileSystemPath = fileSystemPath;
            DefaultDocument = DefaultDocumentName;
        }
        
        /// <inheritdoc />
        public override bool IsFinalHandler { get; } = true;

        /// <summary>
        /// Gets or sets the default document.
        /// Defaults to "index.html"
        /// Example: "root.xml".
        /// </summary>
        public string DefaultDocument { get; }

        /// <summary>
        /// Gets the file system path from which files are retrieved.
        /// </summary>
        public string FileSystemPath { get; protected set; }

        private string GetLocalPath(string path)
        {
            var urlPath = path.Replace('/', Path.DirectorySeparatorChar);

            // adjust the path to see if we've got a default document
            if (urlPath.Last() == Path.DirectorySeparatorChar)
                urlPath = urlPath + DefaultDocument;

            urlPath = urlPath.TrimStart(Path.DirectorySeparatorChar);

            if (Path.GetExtension(urlPath) == ".html") urlPath = urlPath.Replace(".html", ".markdown");
            if (Path.GetExtension(urlPath) == ".htm") urlPath = urlPath.Replace(".htm", ".markdown");

            return Path.Combine(FileSystemPath, urlPath);
        }

        /// <inheritdoc />
        protected override Task OnRequestAsync(IHttpContext context)
        {
            var localPath = GetLocalPath(context.RequestedPath);

            if (!File.Exists(localPath))
                throw HttpException.NotFound();

            using (var reader = File.OpenText(localPath))
            {
                using (var outputStream = context.OpenResponseStream())
                {
                    using (var writer = new StreamWriter(outputStream))
                    {
                        CommonMark.CommonMarkConverter.Convert(reader, writer);
                        context.Response.ContentType = "text/html";
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}