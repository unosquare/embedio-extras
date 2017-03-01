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
using System.Collections;
using Unosquare.Swan.Reflection;

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
                        List<object> dataList = new List<object>();
                        var data = _dbInstance.Select<object>(table, "1=1");
                        foreach (var row in data)
                        {
                            var objTable = Activator.CreateInstance(dbSetType);
                            ((IDictionary<string, object>)row).CopyPropertiesFromDictionary(objTable, null);
                            dataList.Add(objTable);
                        }
                        context.JsonResponse(dataList);
                        return true;
                    }

                    if (verb == HttpVerbs.Post)
                    {
                        var body = (IDictionary<string, object>)Json.Deserialize(context.RequestBody());
                        var objTable = Activator.CreateInstance(dbSetType);
                        body.CopyPropertiesFromDictionary(objTable, null);

                        _dbInstance.Insert(objTable);

                        return true;
                    }
                }

                if (parts.Length == 2)
                {
                    if (verb == HttpVerbs.Get)
                    {
                        var data = _dbInstance.Select<object>(table, "[RowId] = @RowId", new { RowId = parts[1] });
                        var objTable = Activator.CreateInstance(dbSetType);
                        ((IDictionary<string, object>)data.First()).CopyPropertiesFromDictionary(objTable, null);
                        context.JsonResponse(objTable);
                        return true;
                    }

                    if (verb == HttpVerbs.Put)
                    {
                        var objTable = Activator.CreateInstance(dbSetType);
                        var data = _dbInstance.Select<object>(table, "[RowId] = @RowId", new { RowId = parts[1] });
                        ((IDictionary<string, object>)data.First()).CopyPropertiesFromDictionary(objTable, null);
                        var body = (IDictionary<string, object>)Json.Deserialize(context.RequestBody());
                        body.CopyPropertiesFromDictionary(objTable, new string[] { "RowId" });

                        _dbInstance.Update(objTable);

                        return true;
                    }

                    if (verb == HttpVerbs.Delete)
                    {
                        var data = _dbInstance.Select<object>(table, "[RowId] = @RowId", new { RowId = parts[1] });
                        var objTable = Activator.CreateInstance(dbSetType);
                        ((IDictionary<string, object>)data.First()).CopyPropertiesFromDictionary(objTable, null);

                        _dbInstance.Delete(objTable);

                        return true;
                    }
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

        public static int CopyPropertiesFromDictionary(this IDictionary<string, object> source, object target, string[] ignoreProperties)
        {
            Lazy<PropertyTypeCache> CopyPropertiesTargets = new Lazy<PropertyTypeCache>(() => new PropertyTypeCache());

            var copiedProperties = 0;

            var targetType = target.GetType();
            var targetProperties = CopyPropertiesTargets.Value.Retrieve(targetType, () =>
            {
                return targetType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanWrite && Definitions.AllBasicTypes.Contains(x.PropertyType));
            });

            var targetPropertyNames = targetProperties.Select(t => t.Name.ToLowerInvariant());
            var filteredSourceKeys = source
                .Where(s => targetPropertyNames.Contains(s.Key.ToLowerInvariant()) && s.Value != null)
                .ToArray();

            var ignoredProperties = ignoreProperties?.Where(p => string.IsNullOrWhiteSpace(p) == false)
                                        .Select(p => p.ToLowerInvariant())
                                        .ToArray() ?? new string[] { };

            foreach (var sourceKey in filteredSourceKeys)
            {
                var targetProperty = targetProperties.SingleOrDefault(s => s.Name.ToLowerInvariant() == sourceKey.Key.ToLowerInvariant());
                if (targetProperty == null) continue;

                if (ignoredProperties.Contains(targetProperty.Name.ToLowerInvariant()))
                    continue;

                try
                {
                    if (targetProperty.PropertyType == sourceKey.Value.GetType())
                    {
                        targetProperty.SetValue(target, sourceKey.Value);
                        copiedProperties++;
                        continue;
                    }

                    var sourceStringValue = sourceKey.Value.ToStringInvariant();

                    if (targetProperty.PropertyType == typeof(Boolean))
                        sourceStringValue = sourceStringValue == "1" ? "true" : "false";

                    object targetValue;
                    if (Definitions.BasicTypesInfo[targetProperty.PropertyType].TryParse(sourceStringValue, out targetValue))
                    {
                        targetProperty.SetValue(target, targetValue);
                        copiedProperties++;
                    }
                }
                catch
                {
                    // swallow
                }
            }

            return copiedProperties;
        }
    }
}
