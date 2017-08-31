using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PoeParser.Common {
    internal class UntypedEnumInfo : IEnumInfo<object> {
        private readonly IReadOnlyList<IEnumValueInfo<object>> m_values;
        private readonly IReadOnlyDictionary<object, IEnumValueInfo<object>> m_map;

        public UntypedEnumInfo(Type enumType) {
            m_values = EnumUtils.GetEnumValues<object>(enumType);
            m_map = m_values.ToDictionary(v => v.Value);
        }

        public int Count {
            get { return m_values.Count; }
        }

        public IEnumValueInfo<object> this[int index] {
            get { return m_values[index]; }
        }

        public IEnumValueInfo<object> this[object value] {
            get { return m_map[value]; }
        }

        public IEnumerator<IEnumValueInfo<object>> GetEnumerator() {
            return m_values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}