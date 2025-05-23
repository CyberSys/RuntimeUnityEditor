﻿using RuntimeUnityEditor.Core.Utils;

namespace RuntimeUnityEditor.Core.Inspector.Entries
{
    /// <summary>
    /// Represents a read-only cache entry in the inspector for list items.
    /// </summary>
    public class ReadonlyListCacheEntry : ReadonlyCacheEntry
    {
        /// <inheritdoc />
        public ReadonlyListCacheEntry(object o, int index) : base(GetListItemName(index), o)
        {
        }

        internal static string GetListItemName(int index)
        {
            return "Index: " + index;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var isNull = Object.IsNullOrDestroyedStr();
            if (isNull != null) return "[" + isNull + "]";

            return Object.ToString();
        }
    }
}
