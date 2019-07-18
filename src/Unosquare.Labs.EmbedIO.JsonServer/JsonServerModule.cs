namespace EmbedIO.JsonServer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Unosquare.Swan.Formatters;

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
        
        /// <summary>
        /// Updates JSON file in disk.
        /// </summary>
        public void UpdateDataStore()
        {
            if (string.IsNullOrWhiteSpace(JsonPath))
                return;

            File.WriteAllText(JsonPath, Json.Serialize(Data, true));
        }
        
        private async Task<bool> AddRow(IHttpContext context, dynamic table, CancellationToken cancellationToken)
        {
            var array = (IList<object>)table;
            array.Add(await context.GetRequestDataAsync<object>(cancellationToken).ConfigureAwait(false));
            Task.Run(UpdateDataStore);

            return true;
        }

        private Task<bool> RemoveRow(dynamic table, dynamic row)
        {
            var array = (ICollection<object>)table;
            array.Remove(row);
            Task.Run(UpdateDataStore);

            return Task.FromResult(true);
        }

        private async Task<bool> UpdateRow(IHttpContext context, dynamic row, CancellationToken cancellationToken)
        {
            var update = await context.GetRequestDataAsync<Dictionary<string, object>>(cancellationToken).ConfigureAwait(false);

            foreach (var property in update)
            {
                row[property.Key] = property.Value;
            }

            Task.Run(UpdateDataStore);

            return true;
        }

        /// <inheritdoc />
        protected override Task OnRequestAsync(IHttpContext context, string path, CancellationToken cancellationToken)
        {
            var verb = context.Request.HttpVerb;

            if (path == BasePath)
                return context.SendDataAsync((object)Data, cancellationToken);

            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var table = Data[parts[0]];

            if (table == null)
                return Task.FromResult(false);

            switch (parts.Length)
            {
                case 1 when verb == HttpVerbs.Get:
                    return context.SendDataAsync((object)table, cancellationToken);
                case 1 when verb == HttpVerbs.Post:
                    return AddRow(context, table, cancellationToken);
                case 2:
                {
                    foreach (var row in table)
                    {
                        if (row["id"].ToString() != parts[1]) continue;

                        switch (verb)
                        {
                            case HttpVerbs.Get:
                                return context.SendDataAsync((object)row, cancellationToken);
                            case HttpVerbs.Put:
                                return UpdateRow(context, row, cancellationToken);
                            case HttpVerbs.Delete:
                                return RemoveRow(table, row);
                        }
                    }

                    break;
                }
            }

            return Task.FromResult(false);
        }
    }
}