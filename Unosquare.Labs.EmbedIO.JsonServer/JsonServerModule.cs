namespace Unosquare.Labs.EmbedIO.JsonServer
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

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

            if (String.IsNullOrWhiteSpace(jsonPath) == false)
            {
                if (File.Exists(jsonPath))
                {
                    var jsonData = File.ReadAllText(jsonPath);
                    Data = JsonConvert.DeserializeObject(jsonData);
                }
                else
                {
                    File.Create(jsonPath);
                }
            }

            var jsonFormatting = Formatting.None;
#if DEBUG
            jsonFormatting = Formatting.Indented;
#endif

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, (server, context) =>
            {
                var path = context.RequestPath();
                var verb = context.RequestVerb();

                if (path.StartsWith(basePath) == false) return false;

                if (path == basePath)
                {
                    context.JsonResponse((string) JsonConvert.SerializeObject(Data, jsonFormatting));
                    return true;
                }

                var parts = path.Substring(basePath.Length).Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

                dynamic table;

                if (parts.Length == 1)
                {
                    table = Data[parts[0]];
                    if (table == null) return false;

                    if (verb == HttpVerbs.Get)
                    {
                        context.JsonResponse((string) JsonConvert.SerializeObject(table, jsonFormatting));
                        return true;
                    }

                    if (verb == HttpVerbs.Post)
                    {
                        var array = (JArray) table;
                        array.Add(JsonConvert.DeserializeObject(context.RequestBody()));
                        ThreadPool.QueueUserWorkItem(UpdateDataStore);

                        return true;
                    }
                }

                if (parts.Length == 2)
                {
                    table = Data[parts[0]];
                    if (table == null) return false;

                    foreach (dynamic row in table)
                    {
                        if (row["id"] != parts[1]) continue;

                        if (verb == HttpVerbs.Get)
                        {
                            context.JsonResponse((string) JsonConvert.SerializeObject(row, jsonFormatting));
                            return true;
                        }

                        if (verb == HttpVerbs.Put)
                        {
                            var update = JsonConvert.DeserializeObject<JObject>(context.RequestBody());

                            foreach (KeyValuePair<string, JToken> property in update)
                            {
                                row[property.Key] = property.Value;
                            }

                            ThreadPool.QueueUserWorkItem(UpdateDataStore);

                            return true;
                        }

                        if (verb == HttpVerbs.Delete)
                        {
                            var array = (JArray)table;
                            array.Remove(row);
                            ThreadPool.QueueUserWorkItem(UpdateDataStore);

                            return true;
                        }
                    }
                }

                return false;
            });
        }

        /// <summary>
        /// Updates JSON file in disk
        /// </summary>
        /// <param name="state"></param>
        public void UpdateDataStore(object state)
        {
            if (String.IsNullOrWhiteSpace(JsonPath)) return;

            File.WriteAllText(JsonPath, JsonConvert.SerializeObject(Data, Formatting.Indented));
        }

        /// <summary>
        /// Gets the Module's name
        /// </summary>
        public override string Name
        {
            get { return "JSON Server Module"; }
        }
    }
}