using System.Collections.Generic;
using Tornado.Common.Errors;

namespace Tornado.Common.Utility {
    public class NiceDictionary<TKey, TVal> : Dictionary<TKey, TVal> {
        public new TVal this[TKey key] {
            get {
                try {
                    return base[key];
                } catch (KeyNotFoundException knfe) {
                    throw new NiceKeyNotFoundException<TKey>(key, knfe.Message, knfe.InnerException);
                }
            }
            set {
                try {
                    base[key] = value;
                } catch (KeyNotFoundException knfe) {
                    throw new NiceKeyNotFoundException<TKey>(key, knfe.Message, knfe.InnerException);
                }
            }
        }
    }
}