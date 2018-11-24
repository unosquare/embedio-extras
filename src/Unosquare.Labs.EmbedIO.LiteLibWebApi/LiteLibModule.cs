namespace Unosquare.Labs.EmbedIO.LiteLibWebApi
{
    using Constants;
    using LiteLib;
    using Swan;
    using Swan.Formatters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a EmbedIO Module to create an automatic WebApi handler for each IDbSet from a LiteLib context.
    /// </summary>
    /// <typeparam name="T">The type of LiteDbContext.</typeparam>
    /// <seealso cref="EmbedIO.WebModuleBase" />
    public class LiteLibModule<T> : WebModuleBase
        where T : LiteDbContext
    {
        private const string RowSelector = "[RowId] = @RowId";

        private readonly T _dbInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteLibModule{T}"/> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="basePath">The base path.</param>
        public LiteLibModule(T instance, string basePath = "/api/")
        {
            _dbInstance = instance;
            var types = _dbInstance.GetType()
                .GetTypeInfo()
                .Assembly
                .GetTypes()
                .ToList();

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, async (context, ct) =>
            {
                var path = context.RequestPath();
                var verb = context.RequestVerb();

                if (path.StartsWith(basePath) == false)
                    return false;

                var parts = path.Substring(basePath.Length)
                    .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                var setType = types
                        .FirstOrDefault(x => x.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase));

                if (setType == null) 
                    return false;
                
                var table = _dbInstance.Set(setType);

                switch (parts.Length)
                {
                    case 1 when verb == HttpVerbs.Get:
                        var current = await _dbInstance.SelectAsync<object>(table, "1=1");

                        return await context.JsonResponseAsync(current.Select(row => SetValues(Activator.CreateInstance(setType), row)).ToList());
                    case 1 when verb == HttpVerbs.Post:
                        return await AddRow(context, setType);
                    case 2 when verb == HttpVerbs.Get:
                        var data = _dbInstance.Select<object>(table, RowSelector, new { RowId = parts[1] });
                        var objTable = SetValues(Activator.CreateInstance(setType), data.First());

                        return await context.JsonResponseAsync(objTable);
                    case 2 when verb == HttpVerbs.Put:
                        return await UpdateRow(setType, table, parts[1], context);
                    case 2 when verb == HttpVerbs.Delete:
                        return await RemoveRow(table, parts[1], setType);
                }

                return false;
            });
        }

        /// <inheritdoc />
        public override string Name => nameof(LiteLibModule<T>).Humanize();

        private async Task<bool> AddRow(IHttpContext context, Type setType)
        {
            var body = (IDictionary<string, object>)Json.Deserialize(context.RequestBody());
            var objTable = Activator.CreateInstance(setType);
            body.CopyKeyValuePairTo(objTable);

            await _dbInstance.InsertAsync(objTable);

            return true;
        }

        private async Task<bool> UpdateRow(Type setType, ILiteDbSet table, string rowId, IHttpContext context)
        {
            var objTable = Activator.CreateInstance(setType);
            var data = _dbInstance.Select<object>(table, RowSelector, new { RowId = rowId });
            ((IDictionary<string, object>)data.First()).CopyKeyValuePairTo(objTable);
            var body = (IDictionary<string, object>)Json.Deserialize(context.RequestBody());
            body.CopyKeyValuePairTo(objTable, new[] { "RowId" });

            await _dbInstance.UpdateAsync(objTable);

            return true;
        }

        private async Task<bool> RemoveRow(ILiteDbSet table, string rowId, Type setType)
        {
            var data = _dbInstance.Select<object>(table, RowSelector, new { RowId = rowId });
            var objTable = SetValues(Activator.CreateInstance(setType), data.First());

            await _dbInstance.DeleteAsync(objTable);

            return true;
        }

        private static object SetValues(object objTable, object data)
        {
            ((IDictionary<string, object>)data)?.CopyKeyValuePairTo(objTable);
            return objTable;
        }
    }
}