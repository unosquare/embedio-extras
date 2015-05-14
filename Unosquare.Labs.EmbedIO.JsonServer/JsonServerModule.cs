using System.Linq;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Unosquare.Labs.EmbedIO.JsonServer
{
    public class JsonServerModule : WebModuleBase
    {
        public dynamic Data { get; set; }

        public JsonServerModule(string basePath = "/api/", string jsonPath = null)
        {
            if (String.IsNullOrWhiteSpace(jsonPath) == false && File.Exists(jsonPath))
            {
                var jsonData = File.ReadAllText(jsonPath);
                Data = JsonConvert.DeserializeObject(jsonData);
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

                var parts = path.Substring(basePath.Length).Trim().Split('/');
                if (String.IsNullOrWhiteSpace(parts[0]))
                {
                    context.JsonResponse((string)JsonConvert.SerializeObject(Data, jsonFormatting));
                    return true;
                }

                dynamic table;

                switch (parts.Length)
                {
                    case 1:
                        table = Data[parts[0]];
                        if (table == null) return false;
                        context.JsonResponse((string) JsonConvert.SerializeObject(table, jsonFormatting));
                        return true;
                    case 2:
                        table = Data[parts[0]];
                        if (table == null) return false;

                        foreach (dynamic row in table)
                        {
                            if (row["id"] == parts[1])
                            {
                                context.JsonResponse((string) JsonConvert.SerializeObject(row, jsonFormatting));
                                return true;
                            }
                        }
                        break;
                }

                return false;
            });
        }

        public override string Name
        {
            get { return "JSON Server Module"; }
        }
    }
}