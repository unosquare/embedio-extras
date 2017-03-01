// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Unosquare.Labs.EmbedIO.OwinMiddleware.Collections
{
    /// <summary>
    /// Headers Dictionary Base
    /// </summary>
    internal abstract class HeadersDictionaryBase : IDictionary<string, string[]>
    {
        protected virtual WebHeaderCollection Headers { get; set; }

        public virtual ICollection<string> Keys => Headers.AllKeys;

        public virtual ICollection<string[]> Values
        {
            get { return this.Select(pair => pair.Value).ToList(); }
        }

        public int Count => Keys.Count();

        public bool IsReadOnly => false;

        public string[] this[string key]
        {
            get { return Get(key); }

            set { Set(key, value); }
        }

        public bool ContainsKey(string key)
        {
            return Keys.Contains(key, StringComparer.OrdinalIgnoreCase);
        }

        public virtual bool Remove(string key)
        {
            if (!ContainsKey(key)) return false;

            Headers.Remove(key);
            return true;
        }

        protected virtual void RemoveSilent(string header)
        {
            Headers.Remove(header);
        }

        protected virtual string[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            string[] values;
            if (!TryGetValue(key, out values))
            {
                throw new KeyNotFoundException(key);
            }

            return values;
        }

        protected void Set(string key, string[] value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null || value.Length == 0)
            {
                RemoveSilent(key);
            }
            else
            {
                Set(key, value[0]);
                for (var i = 1; i < value.Length; i++)
                {
                    Append(key, value[i]);
                }
            }
        }

        protected virtual void Set(string key, string value)
        {
            Headers.Set(key, value);
        }

        public void Add(string key, string[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (ContainsKey(key))
            {
                // IDictionary contract
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    "The key '{0}' is already present in the dictionary.",
                    key));
            }

            for (var i = 0; i < values.Length; i++)
            {
                Append(key, values[i]);
            }
        }

        protected virtual void Append(string key, string value)
        {
            Headers.Add(key, value);
        }

        public virtual bool TryGetValue(string key, out string[] value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = null;
                return false;
            }

            value = Headers.GetValues(key);
            return value != null;
        }

        public void Add(KeyValuePair<string, string[]> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Headers.Clear();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            string[] value;
            return TryGetValue(item.Key, out value) && ReferenceEquals(item.Value, value);
        }

        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex > Count - array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, string.Empty);
            }

            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            return Contains(item) && Remove(item.Key);
        }

        public virtual IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            for (var i = 0; i < Headers.Count; i++)
            {
                yield return new KeyValuePair<string, string[]>(Headers.GetKey(i), Headers.GetValues(i));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}