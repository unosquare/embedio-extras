namespace EmbedIO.JsonServer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Swan.Formatters;

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
        : base(basePath)
        {
            JsonPath = jsonPath;
            BasePath = basePath;

            if (string.IsNullOrWhiteSpace(jsonPath) || !File.Exists(jsonPath)) return;

            var jsonData = File.ReadAllText(jsonPath);
            Data = Json.Deserialize(jsonData);
        }
        
        /// <inheritdoc />
        public override bool IsFinalHandler { get; } = true;

        /// <summary>
        /// Dynamic database.
        /// </summary>
        public dynamic Data { get; }

        /// <summary>
        /// Default JSON file path.
        /// </summary>
        public string JsonPath { get; }

        /// <summary>
        /// Gets or sets the base path.
        /// </summary>
        /// <value>
        /// The base path.
        /// </value>
        public string BasePath { get; }

        /// <summary>
        /// Updates JSON file in disk.
        /// </summary>
        public void UpdateDataStore()
        {
            if (string.IsNullOrWhiteSpace(JsonPath))
                return;

            File.WriteAllText(JsonPath, Json.Serialize(Data, true));
        }

        /// <inheritdoc />
        protected override Task OnRequestAsync(IHttpContext context)
        {
            if (context.RequestedPath == "/")
                return context.SendDataAsync((object)Data);

            var parts = context.RequestedPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var table = Data[parts[0]];

            if (table == null)
                throw HttpException.NotFound();

            var verb = context.Request.HttpVerb;

            switch (parts.Length)
            {
                case 1 when verb == HttpVerbs.Get:
                    return context.SendDataAsync((object)table);
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
                                    return context.SendDataAsync((object)row);
                                case HttpVerbs.Put:
                                    return UpdateRow(context, row);
                                case HttpVerbs.Delete:
                                    RemoveRow(table, row);
                                    return Task.CompletedTask;
                            }
                        }

                        break;
                    }
            }

            throw HttpException.BadRequest();
        }

        private async Task AddRow(IHttpContext context, dynamic table)
        {
            var array = (IList<object>)table;
            array.Add(await context.GetRequestDataAsync<object>().ConfigureAwait(false));
            Task.Run(UpdateDataStore);
        }

        private void RemoveRow(dynamic table, dynamic row)
        {
            var array = (ICollection<object>)table;
            array.Remove(row);
            Task.Run(UpdateDataStore);
        }

        private async Task UpdateRow(IHttpContext context, dynamic row)
        {
            var update = await context.GetRequestDataAsync<Dictionary<string, object>>().ConfigureAwait(false);

            foreach (var property in update)
            {
                row[property.Key] = property.Value;
            }

            Task.Run(UpdateDataStore);
        }
    }
}