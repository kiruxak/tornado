using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace PoeParser.Common {
    public static class EnumUtils {
        private static readonly ConcurrentDictionary<CultureInfo, ConcurrentDictionary<Type, IEnumInfo<object>>> m_instances = new ConcurrentDictionary<CultureInfo, ConcurrentDictionary<Type, IEnumInfo<object>>>();

        internal static IReadOnlyList<EnumValueInfo<T>> GetEnumValues<T>(Type enumType) {
            if (!enumType.IsEnum) {
                throw new ArgumentOutOfRangeException("enumType");
            }
            return (from field in enumType.GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public)
                    let value = (T)field.GetValue(null)
                    let displayAttribute = field.GetCustomAttributes(true).OfType<DescriptionAttribute>().FirstOrDefault()
                    let displayName = displayAttribute == null ? Enum.GetName(enumType, value) : displayAttribute.Description
                    select new EnumValueInfo<T> {
                        Id = Convert.ToInt32(value),
                        Value = value,
                        Name = value.ToString(),
                        DisplayName = displayName,
                    })
                .ToArray();
        }

        public static IEnumInfo<T> GetEnumInfo<T>() {
            return EnumInfo<T>.Current;
        }

        public static IEnumInfo<object> GetEnumInfo(Type enumType) {
            ConcurrentDictionary<Type, IEnumInfo<object>> enumTypeMap = m_instances.GetOrAdd(Thread.CurrentThread.CurrentUICulture, c => new ConcurrentDictionary<Type, IEnumInfo<object>>());
            return enumTypeMap.GetOrAdd(enumType, et => new UntypedEnumInfo(et));
        }
    }
}