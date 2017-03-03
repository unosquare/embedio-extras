using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unosquare.Labs.LiteLib;
using Unosquare.Swan;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Reflection;

namespace Unosquare.Labs.EmbedIO.LiteLibWebApi
{
    /// <summary>
    /// Represents a EmbedIO Module to create an automatic WebApi handler for each IDbSet from a LiteLib context.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Unosquare.Labs.EmbedIO.WebModuleBase" />
    public class LiteLibModule<T> : WebModuleBase
        where T : LiteDbContext
    {
        private readonly T _dbInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteLibModule{T}"/> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="basePath">The base path.</param>
        public LiteLibModule(T instance, string basePath = "/api/")
        {
            _dbInstance = instance;

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, (server, context) =>
            {
                var path = context.RequestPath();
                var verb = context.RequestVerb();

                if (path.StartsWith(basePath) == false)
                    return false;

                var parts = path.Substring(basePath.Length).Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

                var dbSetType =
                    _dbInstance.GetType()
                        .GetTypeInfo()
                        .Assembly.GetTypes()
                        .FirstOrDefault(x => x.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase));

                if (dbSetType == null) return false;
                var table = _dbInstance.Set(dbSetType);

                if (parts.Length == 1)
                {
                    switch (verb)
                    {
                        case HttpVerbs.Get:
                            var dataList = new List<object>();
                            var data = _dbInstance.Select<object>(table, "1=1");

                            foreach (var row in data)
                            {
                                var item = Activator.CreateInstance(dbSetType);
                                ((IDictionary<string, object>) row).CopyPropertiesFromDictionary(item, null);
                                dataList.Add(item);
                            }
                            context.JsonResponse(dataList);
                            return true;
                        case HttpVerbs.Post:
                            var body = (IDictionary<string, object>) Json.Deserialize(context.RequestBody());
                            var objTable = Activator.CreateInstance(dbSetType);
                            body.CopyPropertiesFromDictionary(objTable, null);

                            _dbInstance.Insert(objTable);

                            return true;
                    }
                }

                if (parts.Length == 2)
                {
                    switch (verb)
                    {
                        case HttpVerbs.Get:
                        {
                            var data = _dbInstance.Select<object>(table, "[RowId] = @RowId", new {RowId = parts[1]});
                            var objTable = Activator.CreateInstance(dbSetType);
                            ((IDictionary<string, object>) data.First()).CopyPropertiesFromDictionary(objTable, null);
                            context.JsonResponse(objTable);
                            return true;
                        }
                        case HttpVerbs.Put:
                        {
                            var objTable = Activator.CreateInstance(dbSetType);
                            var data = _dbInstance.Select<object>(table, "[RowId] = @RowId", new {RowId = parts[1]});
                            ((IDictionary<string, object>) data.First()).CopyPropertiesFromDictionary(objTable, null);
                            var body = (IDictionary<string, object>) Json.Deserialize(context.RequestBody());
                            body.CopyPropertiesFromDictionary(objTable, new string[] {"RowId"});

                            _dbInstance.Update(objTable);

                            return true;
                        }
                        case HttpVerbs.Delete:
                        {
                            var data = _dbInstance.Select<object>(table, "[RowId] = @RowId", new {RowId = parts[1]});
                            var objTable = Activator.CreateInstance(dbSetType);
                            ((IDictionary<string, object>) data.First()).CopyPropertiesFromDictionary(objTable, null);

                            _dbInstance.Delete(objTable);

                            return true;
                        }
                    }
                }

                return false;
            });
        }

        /// <summary>
        /// Gets the name of this module.
        /// </summary>
        public override string Name => nameof(LiteLibModule<T>).Humanize();
    }

    internal static class Extensions
    {
        internal static int CopyPropertiesFromDictionary(this IDictionary<string, object> source, object target,
            string[] ignoreProperties)
        {
            var copyPropertiesTargets = new Lazy<PropertyTypeCache>(() => new PropertyTypeCache());

            var copiedProperties = 0;

            var targetType = target.GetType();
            var targetProperties = copyPropertiesTargets.Value.Retrieve(targetType, () =>
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
                                        .ToArray() ?? new string[] {};

            foreach (var sourceKey in filteredSourceKeys)
            {
                var targetProperty =
                    targetProperties.SingleOrDefault(s => s.Name.ToLowerInvariant() == sourceKey.Key.ToLowerInvariant());
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

                    if (targetProperty.PropertyType == typeof(bool))
                        sourceStringValue = sourceStringValue == "1" ? "true" : "false";

                    object targetValue;
                    if (Definitions.BasicTypesInfo[targetProperty.PropertyType].TryParse(sourceStringValue,
                        out targetValue))
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