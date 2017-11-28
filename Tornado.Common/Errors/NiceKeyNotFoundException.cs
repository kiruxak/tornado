using System;
using System.Collections.Generic;

namespace Tornado.Common.Errors {
    public class NiceKeyNotFoundException<TKey> : KeyNotFoundException {
        public TKey Key { get; private set; }

        public NiceKeyNotFoundException(TKey key, string message) : base((key is string ? "Key: " + (key as string) + ". " : "") + message, null) {
            this.Key = key;
        }

        public NiceKeyNotFoundException(TKey key, string message, Exception innerException) : base((key is string ? "Key: " + (key as string) + ". " : "") + message, innerException) {
            this.Key = key;
        }
    }
}