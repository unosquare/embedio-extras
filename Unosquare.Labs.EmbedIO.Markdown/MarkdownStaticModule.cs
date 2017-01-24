using System;
using System.IO;
using System.Linq;
using Unosquare.Net;

namespace Unosquare.Labs.EmbedIO.Markdown
{
    public class MarkdownStaticModule : WebModuleBase
    {
        /// <summary>
        /// Default document constant to "index.markdown"
        /// </summary>
        public const string DefaultDocumentName = "index.markdown";

        /// <summary>
        /// Gets or sets the default document.
        /// Defaults to "index.html"
        /// Example: "root.xml"
        /// </summary>
        /// <value>
        /// The default document.
        /// </value>
        public string DefaultDocument { get; set; }

        /// <summary>
        /// Gets the file system path from which files are retrieved.
        /// </summary>
        /// <value>
        /// The file system path.
        /// </value>
        public string FileSystemPath { get; protected set; }

        public MarkdownStaticModule(string fileSystemPath)
        {
            if (Directory.Exists(fileSystemPath) == false)
                throw new ArgumentException("Path '" + fileSystemPath + "' does not exist.");

            FileSystemPath = fileSystemPath;
            DefaultDocument = DefaultDocumentName;

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Get, (server, context) => HandleGet(context, server));
        }

        private bool HandleGet(HttpListenerContext context, WebServer server)
        {
            var urlPath = context.Request.Url.LocalPath.Replace('/', Path.DirectorySeparatorChar);

            // adjust the path to see if we've got a default document
            if (urlPath.Last() == Path.DirectorySeparatorChar)
                urlPath = urlPath + DefaultDocument;

            urlPath = urlPath.TrimStart(new[] {Path.DirectorySeparatorChar});

            if (Path.GetExtension(urlPath) == ".html") urlPath = urlPath.Replace(".html", ".markdown");
            if (Path.GetExtension(urlPath) == ".htm") urlPath = urlPath.Replace(".htm", ".markdown");

            var localPath = Path.Combine(FileSystemPath, urlPath);

            if (!File.Exists(localPath))
                return false;

            using (var reader = new StreamReader(localPath))
            {
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    CommonMark.CommonMarkConverter.Convert(reader, writer);
                    context.Response.ContentType = "text/html";
                }
            }

            return true;
        }

        public override string Name => "Markdown Static Module";
    }
}