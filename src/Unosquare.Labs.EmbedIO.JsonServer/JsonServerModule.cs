namespace Unosquare.Labs.EmbedIO.JsonServer
{
    using Constants;
    using Swan.Formatters;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// JsonServer Module.
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
        /// Dynamic database.
        /// </summary>
        public dynamic Data { get; set; }

        /// <summary>
        /// Default JSON file path.
        /// </summary>
        public string JsonPath { get; set; }

        /// <summary>
        /// Gets or sets the base path.
        /// </summary>
        /// <value>
        /// The base path.
        /// </value>
        public string BasePath { get; set; }

        /// <inheritdoc />
        public override string Name => nameof(JsonServerModule);

        /// <summary>
        /// Updates JSON file in disk.
        /// </summary>
        public void UpdateDataStore()
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
        /// <returns>A task representing the request action.</returns>
        private Task<bool> HandleRequest(IHttpContext context, CancellationToken ct)
        {
            var path = context.RequestPath();
            var verb = context.RequestVerb();

            if (path.StartsWith(BasePath) == false)
                return Task.FromResult(false);

            if (path == BasePath)
                return context.JsonResponseAsync((object) Data, ct);

            var parts = path.Substring(BasePath.Length)
                .Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            var table = Data[parts[0]];

            if (table == null) 
                return Task.FromResult(false);

            switch (parts.Length)
            {
                case 1 when verb == HttpVerbs.Get:
                    return context.JsonResponseAsync((object) table, ct);
                case 1 when verb == HttpVerbs.Post:
                    return AddRow(context, table);
                case 2:
                {
                    foreach (var row in table)
                    {
                        if (row["id"].ToString() != parts[1]) continue;

                        switch (verb)
                        {
                            case HttpVerbs.Get:
                                return context.JsonResponseAsync((object) row, ct);
                            case HttpVerbs.Put:
                                return UpdateRow(context, row);
                            case HttpVerbs.Delete:
                                return RemoveRow(table, row);
                        }
                    }

                    break;
                }
            }

            return Task.FromResult(false);
        }

        private async Task<bool> AddRow(IHttpContext context, dynamic table)
        {
            var array = (IList<object>) table;
            array.Add(Json.Deserialize(await context.RequestBodyAsync()));
            var _ = Task.Run(UpdateDataStore);
            
            return true;
        }

        private Task<bool> RemoveRow(dynamic table, dynamic row)
        {
            var array = (ICollection<object>) table;
            array.Remove(row);
            Task.Run(UpdateDataStore);
            
            return Task.FromResult(true);
        }

        private async Task<bool> UpdateRow(IHttpContext context, dynamic row)
        {
            var update = Json.Deserialize<Dictionary<string, object>>(await context.RequestBodyAsync());

            foreach (var property in update)
            {
                row[property.Key] = property.Value;
            }

            var _ = Task.Run(UpdateDataStore);
            
            return true;
        }
    }
}