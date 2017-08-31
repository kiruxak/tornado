using System;
using System.Collections.Generic;

namespace PoeTrade.Extensions {
    public static class EnumerableExtensions {
        public static void Each<T>(this IEnumerable<T> items, Action<T> action) {
            foreach (T obj in items)
                action(obj);
        }

        public static void ForEach<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dictionary, Action<TKey, TValue> action) {
            foreach (KeyValuePair<TKey, TValue> pair in dictionary) {
                action(pair.Key, pair.Value);
            }
        }
    }
}