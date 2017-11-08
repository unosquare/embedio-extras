namespace Unosquare.Labs.EmbedIO.LiteLibWebApi
{
    using Constants;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Reflection;
    using LiteLib;
    using Swan;
    using Swan.Formatters;
#if NET46
    using Net;
#else
    using System.Net;
#endif

    /// <summary>
    /// Represents a EmbedIO Module to create an automatic WebApi handler for each IDbSet from a LiteLib context.
    /// </summary>
    /// <typeparam name="T">The type of LiteDbContext</typeparam>
    /// <seealso cref="EmbedIO.WebModuleBase" />
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

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, (context, ct) =>
            {
                var path = context.RequestPath();
                var verb = context.RequestVerb();

                if (path.StartsWith(basePath) == false)
                    return Task.FromResult(false);

                var parts = path.Substring(basePath.Length).Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

                var setType =
                    _dbInstance.GetType()
                        .GetTypeInfo()
                        .Assembly.GetTypes()
                        .FirstOrDefault(x => x.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase));

                if (setType == null) return Task.FromResult(false);
                var table = _dbInstance.Set(setType);

                if (parts.Length == 1)
                {
                    switch (verb)
                    {
                        case HttpVerbs.Get:
                            var dataList = new List<object>();
                            var data = _dbInstance.Select<object>(table, "1=1");

                            foreach (var row in data)
                            {
                                var item = Activator.CreateInstance(setType);
                                ((IDictionary<string, object>) row).CopyPropertiesTo(item);
                                dataList.Add(item);
                            }
                            context.JsonResponse(dataList);
                            return Task.FromResult(true);
                        case HttpVerbs.Post:
                            return AddRow(context, setType);
                    }
                }

                if (parts.Length == 2)
                {
                    switch (verb)
                    {
                        case HttpVerbs.Get:
                        {
                            var data = _dbInstance.Select<object>(table, "[RowId] = @RowId", new {RowId = parts[1]});
                            var objTable = Activator.CreateInstance(setType);
                            ((IDictionary<string, object>) data.First()).CopyPropertiesTo(objTable);
                            context.JsonResponse(objTable);
                            return Task.FromResult(true);
                        }
                        case HttpVerbs.Put:
                        {
                            return UpdateRow(setType, table, parts[1], context);
                        }
                        case HttpVerbs.Delete:
                        {
                            return RemoveRow(table, parts[1], setType);
                        }
                    }
                }

                return Task.FromResult(false);
            });
        }

        /// <summary>
        /// Gets the name of this module.
        /// </summary>
        public override string Name => nameof(LiteLibModule<T>).Humanize();

        private Task<bool> AddRow(HttpListenerContext context, Type dbSetType)
        {
            var body = (IDictionary<string, object>) Json.Deserialize(context.RequestBody());
            var objTable = Activator.CreateInstance(dbSetType);
            body.CopyPropertiesTo(objTable, null);

            _dbInstance.Insert(objTable);

            return Task.FromResult(true);
        }

        private async Task<bool> UpdateRow(Type dbSetType, ILiteDbSet table, string rowId, HttpListenerContext context)
        {
            var objTable = Activator.CreateInstance(dbSetType);
            var data = _dbInstance.Select<object>(table, "[RowId] = @RowId", new {RowId = rowId});
            ((IDictionary<string, object>) data.First()).CopyPropertiesTo(objTable);
            var body = (IDictionary<string, object>) Json.Deserialize(context.RequestBody());
            body.CopyPropertiesTo(objTable, new[] {"RowId"});

            await _dbInstance.UpdateAsync(objTable);

            return true;
        }

        private async Task<bool> RemoveRow(ILiteDbSet table, string rowId, Type dbSetType)
        {
            var data = _dbInstance.Select<object>(table, "[RowId] = @RowId", new {RowId = rowId});
            var objTable = Activator.CreateInstance(dbSetType);
            ((IDictionary<string, object>) data.First()).CopyPropertiesTo(objTable);

            await _dbInstance.DeleteAsync(objTable);

            return true;
        }
    }
}