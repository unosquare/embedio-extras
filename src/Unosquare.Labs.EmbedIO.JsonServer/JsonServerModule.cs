namespace Unosquare.Labs.EmbedIO.JsonServer
{
    using Swan;
    using Constants;
    using Swan.Formatters;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
#if NET46
    using Net;
#else
    using System.Net;
#endif

    /// <summary>
    /// JsonServer Module
    /// </summary>
    public class JsonServerModule : WebModuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonServerModule"/> class.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="jsonPath">The json path.</param>
        public JsonServerModule(string basePath = "/api/", string jsonPath = null)
        {
            JsonPath = jsonPath;
            BasePath = basePath;

            if (string.IsNullOrWhiteSpace(jsonPath) == false)
            {
                if (File.Exists(jsonPath))
                {
                    var jsonData = File.ReadAllText(jsonPath);
                    Data = Json.Deserialize(jsonData);
                }
                else
                {
                    File.Create(jsonPath);
                }
            }

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, HandleRequest);
        }

        /// <summary>
        /// Dynamic database
        /// </summary>
        public dynamic Data { get; set; }

        /// <summary>
        /// Default JSON file path
        /// </summary>
        public string JsonPath { get; set; }

        /// <summary>
        /// Gets or sets the base path.
        /// </summary>
        /// <value>
        /// The base path.
        /// </value>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets the Module's name
        /// </summary>
        public override string Name => nameof(JsonServerModule).Humanize();

        /// <summary>
        /// Updates JSON file in disk
        /// </summary>
        /// <param name="state">The state.</param>
        public void UpdateDataStore(object state)
        {
            if (string.IsNullOrWhiteSpace(JsonPath))
                return;

            File.WriteAllText(JsonPath, Json.Serialize(Data, true));
        }

        /// <summary>
        /// Handles the request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        private Task<bool> HandleRequest(HttpListenerContext context, CancellationToken ct)
        {
            var path = context.RequestPath();
            var verb = context.RequestVerb();

            if (path.StartsWith(BasePath) == false)
                return Task.FromResult(false);

            if (path == BasePath)
            {
                context.JsonResponse((object) Data);
                return Task.FromResult(true);
            }

            var parts = path.Substring(BasePath.Length).Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            dynamic table = Data[parts[0]];
            if (table == null) return Task.FromResult(false);

            if (parts.Length == 1)
            {
                if (verb == HttpVerbs.Get)
                {
                    context.JsonResponse((object) table);
                    return Task.FromResult(true);
                }

                if (verb == HttpVerbs.Post)
                {
                    return AddRow(context, table);
                }
            }

            if (parts.Length == 2)
            {
                foreach (dynamic row in table)
                {
                    if (row["id"].ToString() != parts[1]) continue;

                    switch (verb)
                    {
                        case HttpVerbs.Get:
                            context.JsonResponse((object) row);
                            return Task.FromResult(true);
                        case HttpVerbs.Put:
                            return UpdateRow(context, row);
                        case HttpVerbs.Delete:
                            return RemoveRow(table, row);
                    }
                }
            }

            return Task.FromResult(false);
        }

        private Task<bool> AddRow(HttpListenerContext context, dynamic table)
        {
            var array = (IList<object>) table;
            array.Add(Json.Deserialize(context.RequestBody()));
            ThreadPool.QueueUserWorkItem(UpdateDataStore);

            return Task.FromResult(true);
        }

        private Task<bool> RemoveRow(dynamic table, dynamic row)
        {
            var array = (ICollection<object>) table;
            array.Remove(row);
            ThreadPool.QueueUserWorkItem(UpdateDataStore);

            return Task.FromResult(true);
        }

        private Task<bool> UpdateRow(HttpListenerContext context, dynamic row)
        {
            var update = Json.Deserialize<Dictionary<string, object>>(context.RequestBody());

            foreach (var property in update)
            {
                row[property.Key] = property.Value;
            }

            ThreadPool.QueueUserWorkItem(UpdateDataStore);

            return Task.FromResult(true);
        }
    }
}