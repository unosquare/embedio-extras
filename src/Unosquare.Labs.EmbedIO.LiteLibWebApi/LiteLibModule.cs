namespace EmbedIO.LiteLibWebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Unosquare.Labs.LiteLib;
    using Swan;
    using Swan.Formatters;

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
        private readonly Type[] _dbTypes;

        private bool _disposedValue; // To detect redundant calls

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteLibModule{T}" /> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="baseUrlPath">The base URL path.</param>
        public LiteLibModule(T instance, string baseUrlPath = "/api/") 
            : base(baseUrlPath)
        {
            _dbInstance = instance;
            _dbTypes = _dbInstance.GetType()
                .GetTypeInfo()
                .Assembly
                .GetTypes()
                .ToArray();
        }
        
        /// <inheritdoc />
        public override bool IsFinalHandler { get; } = true;

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

        private async Task AddRow(IHttpContext context, Type setType)
        {
            var body = (IDictionary<string, object>)Json.Deserialize(await context.GetRequestBodyAsStringAsync().ConfigureAwait(false));
            var objTable = Activator.CreateInstance(setType);
            body.CopyKeyValuePairTo(objTable);

            await _dbInstance.InsertAsync(objTable).ConfigureAwait(false);
        }

        private async Task UpdateRow(Type setType, ILiteDbSet table, string rowId, IHttpContext context)
        {
            var objTable = Activator.CreateInstance(setType);
            var data = _dbInstance.Select<object>(table, RowSelector, new { RowId = rowId });
            ((IDictionary<string, object>)data.First()).CopyKeyValuePairTo(objTable);
            var body = (IDictionary<string, object>)Json.Deserialize(await context.GetRequestBodyAsStringAsync().ConfigureAwait(false));
            body.CopyKeyValuePairTo(objTable, "RowId");

            await _dbInstance.UpdateAsync(objTable).ConfigureAwait(false);
        }

        private async Task RemoveRow(ILiteDbSet table, string rowId, Type setType)
        {
            var data = _dbInstance.Select<object>(table, RowSelector, new { RowId = rowId });
            var objTable = SetValues(Activator.CreateInstance(setType), data.First());

            await _dbInstance.DeleteAsync(objTable).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override async Task OnRequestAsync(IHttpContext context)
        {
            var verb = context.Request.HttpVerb;
                
            var parts = context.RequestedPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var setType = _dbTypes
                .FirstOrDefault(x => x.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase));

            if (setType == null)
                throw HttpException.NotFound();
                
            var table = _dbInstance.Set(setType);

            switch (parts.Length)
            {
                case 1 when verb == HttpVerbs.Get:
                    var current = await _dbInstance.SelectAsync<object>(table, "1=1");

                    await context.SendDataAsync(current.Select(row => SetValues(Activator.CreateInstance(setType), row)).ToList());
                    return;
                case 1 when verb == HttpVerbs.Post:
                    await AddRow(context, setType);
                    return;
                case 2 when verb == HttpVerbs.Get:
                    var data = _dbInstance.Select<object>(table, RowSelector, new { RowId = parts[1] });
                    var objTable = SetValues(Activator.CreateInstance(setType), data.First());

                    await context.SendDataAsync(objTable);
                    return;
                case 2 when verb == HttpVerbs.Put:
                    await UpdateRow(setType, table, parts[1], context);
                    return;
                case 2 when verb == HttpVerbs.Delete:
                    await RemoveRow(table, parts[1], setType);
                    return;
            }

            throw HttpException.BadRequest();
        }
    }
}