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
    /// <seealso cref="WebModuleBase" />
    public class LiteLibModule<T> : WebModuleBase, IDisposable
        where T : LiteDbContext
    {
        private const string RowSelector = "[RowId] = @RowId";

        private readonly T _dbInstance;

        private bool _disposedValue; // To detect redundant calls

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

                if (!path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
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

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the module.
        /// </summary>
        /// <param name="disposing"><c>true</c> if the object is disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                _dbInstance.Dispose();
            }

            _disposedValue = true;
        }

        private static object SetValues(object objTable, object data)
        {
            ((IDictionary<string, object>)data)?.CopyKeyValuePairTo(objTable);
            return objTable;
        }

        private async Task<bool> AddRow(IHttpContext context, Type setType)
        {
            var body = (IDictionary<string, object>)Json.Deserialize(await context.RequestBodyAsync().ConfigureAwait(false));
            var objTable = Activator.CreateInstance(setType);
            body.CopyKeyValuePairTo(objTable);

            await _dbInstance.InsertAsync(objTable).ConfigureAwait(false);

            return true;
        }

        private async Task<bool> UpdateRow(Type setType, ILiteDbSet table, string rowId, IHttpContext context)
        {
            var objTable = Activator.CreateInstance(setType);
            var data = _dbInstance.Select<object>(table, RowSelector, new { RowId = rowId });
            ((IDictionary<string, object>)data.First()).CopyKeyValuePairTo(objTable);
            var body = (IDictionary<string, object>)Json.Deserialize(await context.RequestBodyAsync().ConfigureAwait(false));
            body.CopyKeyValuePairTo(objTable, "RowId");

            await _dbInstance.UpdateAsync(objTable).ConfigureAwait(false);

            return true;
        }

        private async Task<bool> RemoveRow(ILiteDbSet table, string rowId, Type setType)
        {
            var data = _dbInstance.Select<object>(table, RowSelector, new { RowId = rowId });
            var objTable = SetValues(Activator.CreateInstance(setType), data.First());

            await _dbInstance.DeleteAsync(objTable).ConfigureAwait(false);

            return true;
        }
    }
}