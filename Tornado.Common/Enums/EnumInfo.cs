using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace PoeParser.Common {
    public class EnumInfo<TValue> : IEnumInfo<TValue> {
        private static readonly ConcurrentDictionary<CultureInfo, EnumInfo<TValue>> m_instances = new ConcurrentDictionary<CultureInfo, EnumInfo<TValue>>();

        private readonly IReadOnlyList<IEnumValueInfo<TValue>> m_values;
        private readonly IReadOnlyDictionary<TValue, IEnumValueInfo<TValue>> m_map;

        public EnumInfo() {
            m_values = EnumUtils.GetEnumValues<TValue>(typeof(TValue));
            m_map = m_values.ToDictionary(v => v.Value);
        }

        public static IEnumInfo<TValue> Current {
            get { return m_instances.GetOrAdd(Thread.CurrentThread.CurrentUICulture, c => new EnumInfo<TValue>()); }
        }

        public int Count {
            get { return m_values.Count; }
        }

        public IEnumValueInfo<TValue> this[int index] {
            get { return m_values[index]; }
        }

        public IEnumValueInfo<TValue> this[TValue value] {
            get { return m_map[value]; }
        }

        public IEnumerator<IEnumValueInfo<TValue>> GetEnumerator() { return m_values.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}