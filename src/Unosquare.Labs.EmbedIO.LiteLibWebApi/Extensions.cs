namespace Unosquare.Labs.EmbedIO.LiteLibWebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Swan;
    using Swan.Reflection;

    internal static class Extensions
    {
        /// <summary>
        /// Copies the properties from dictionary.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns></returns>
        internal static int CopyPropertiesFromDictionary(
            this IDictionary<string, object> source, 
            object target,
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
                                        .ToArray() ?? new string[] { };

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
