using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO.LiteLibWebApi;
using Unosquare.Labs.LiteLib;
using System.Reflection;
using Unosquare.Swan.Formatters;
using Unosquare.Swan;

namespace Unosquare.Labs.EmbedIO.LiteLibWebApi
{
    public class LiteLibModule<T> : WebModuleBase
        where T : LiteDbContext
    {
        internal class GenericLiteModel : ILiteModel
        {
            public long RowId { get; set; }
        }

        private readonly T _dbInstance;
        
        public LiteLibModule(T instance, string basePath = "/api/")
        {
            _dbInstance = instance;
            
            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, (server, context) =>
            {
                var path = context.RequestPath();
                var verb = context.RequestVerb();

                if (path.StartsWith(basePath) == false)
                    return false;
                
                var parts = path.Substring(basePath.Length).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                var dbSetType = _dbInstance.GetType().GetTypeInfo().Assembly.GetTypes().FirstOrDefault(x => x.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase));

                if (dbSetType == null) return false;
                var table = _dbInstance.Set(dbSetType);

                if (parts.Length == 1)
                {
                    if (verb == HttpVerbs.Get)
                    {
                        var data = _dbInstance.Select<object>(table, "1=1");
                        context.JsonResponse(data);
                        return true;
                    }

                    //if (verb == HttpVerbs.Post)
                    //{
                    //    var array = (IList<object>)table;
                    //    array.Add(Json.Deserialize(context.RequestBody()));
                    //    ThreadPool.QueueUserWorkItem(UpdateDataStore);

                    //    return true;
                    //}
                }

                if (parts.Length == 2)
                {
                    if (verb == HttpVerbs.Get)
                    {
                        context.JsonResponse(_dbInstance.Select<object>(table, "[RowId] = @RowId", new { RowId = parts[1] }));
                        return true;
                    }

                    //foreach (dynamic row in table)
                    //{
                    //    if (row["id"].ToString() != parts[1]) continue;

                    //    if (verb == HttpVerbs.Get)
                    //    {
                    //        context.JsonResponse((object)row);
                    //        return true;
                    //    }

                        //if (verb == HttpVerbs.Put)
                        //{
                        //    var update = Json.Deserialize<Dictionary<string, object>>(context.RequestBody());

                        //    foreach (var property in update)
                        //    {
                        //        row[property.Key] = property.Value;
                        //    }

                        //    ThreadPool.QueueUserWorkItem(UpdateDataStore);

                        //    return true;
                        //}

                        //if (verb == HttpVerbs.Delete)
                        //{
                        //    var array = (IList<object>)table;
                        //    array.Remove(row);
                        //    ThreadPool.QueueUserWorkItem(UpdateDataStore);

                        //    return true;
                        //}
                    //}
                }

                return false;
            });
        }

        public override string Name => nameof(LiteLibModule<T>).Humanize();
    }

    public static class Extensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static dynamic ToDynamicObject(this IDictionary<string, object> source)
        {
            ICollection<KeyValuePair<string, object>> someObject = new ExpandoObject();
            someObject.AddRange(source);
            return someObject;
        }
    }
}
