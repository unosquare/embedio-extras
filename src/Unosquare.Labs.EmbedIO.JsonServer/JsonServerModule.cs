namespace Unosquare.Labs.EmbedIO.JsonServer
{
    using Swan;
    using Swan.Formatters;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// JsonServer Module
    /// </summary>
    public class JsonServerModule : WebModuleBase
    {
        /// <summary>
        /// Dynamic database
        /// </summary>
        public dynamic Data { get; set; }

        /// <summary>
        /// Default JSON file path
        /// </summary>
        public string JsonPath { get; set; }

        /// <summary>
        /// Creates a JsonServer instance
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="jsonPath"></param>
        public JsonServerModule(string basePath = "/api/", string jsonPath = null)
        {
            JsonPath = jsonPath;

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

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, (context, ct) =>
            {
                var path = context.RequestPath();
                var verb = context.RequestVerb();

                if (path.StartsWith(basePath) == false)
                    return Task.FromResult(false);

                if (path == basePath)
                {
                    context.JsonResponse((object) Data);
                    return Task.FromResult(true);
                }

                var parts = path.Substring(basePath.Length).Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

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
                        var array = (IList<object>) table;
                        array.Add(Json.Deserialize(context.RequestBody()));
                        ThreadPool.QueueUserWorkItem(UpdateDataStore);

                        return Task.FromResult(true);
                    }
                }

                if (parts.Length == 2)
                {
                    foreach (dynamic row in table)
                    {
                        if (row["id"].ToString() != parts[1]) continue;

                        if (verb == HttpVerbs.Get)
                        {
                            context.JsonResponse((object) row);
                            return Task.FromResult(true);
                        }

                        if (verb == HttpVerbs.Put)
                        {
                            var update = Json.Deserialize<Dictionary<string, object>>(context.RequestBody());

                            foreach (var property in update)
                            {
                                row[property.Key] = property.Value;
                            }

                            ThreadPool.QueueUserWorkItem(UpdateDataStore);

                            return Task.FromResult(true);
                        }

                        if (verb == HttpVerbs.Delete)
                        {
                            var array = (ICollection<object>) table;
                            array.Remove(row);
                            ThreadPool.QueueUserWorkItem(UpdateDataStore);

                            return Task.FromResult(true);
                        }
                    }
                }

                return Task.FromResult(false);
            });
        }

        /// <summary>
        /// Updates JSON file in disk
        /// </summary>
        /// <param name="state"></param>
        public void UpdateDataStore(object state)
        {
            if (string.IsNullOrWhiteSpace(JsonPath))
                return;

            File.WriteAllText(JsonPath, Json.Serialize(Data, true));
        }

        /// <summary>
        /// Gets the Module's name
        /// </summary>
        public override string Name => nameof(JsonServerModule).Humanize();
    }
}
