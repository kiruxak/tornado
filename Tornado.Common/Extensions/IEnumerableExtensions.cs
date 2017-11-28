using System;
using System.Collections.Generic;
using Tornado.Common.Utility;

namespace Tornado.Common.Extensions {
    public static class IEnumerableExtensions {

        public static NiceDictionary<TKey, TValue> ToNiceDictionary<TKey, TValue, TSource>(this IEnumerable<TSource> source, Func<TSource, TKey> keySetter, Func<TSource, TValue> pairSetter) {
            var nd = new NiceDictionary<TKey, TValue>();

            foreach (TSource o in source) {
                nd.Add(keySetter(o), pairSetter(o));
            }

            return nd;
        }


        public static NiceDictionary<TKey, TSource> ToNiceDictionary<TKey, TSource>(this IEnumerable<TSource> source, Func<TSource, TKey> keySetter) {
            var nd = new NiceDictionary<TKey, TSource>();

            foreach (TSource o in source) {
                nd.Add(keySetter(o), o);
            }

            return nd;
        }
    }
}