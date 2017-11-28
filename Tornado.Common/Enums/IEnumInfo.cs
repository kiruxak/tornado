using System.Collections.Generic;

namespace PoeParser.Common {
    public interface IEnumInfo<TValue> : IReadOnlyList<IEnumValueInfo<TValue>> {
        new IEnumValueInfo<TValue> this[int index] { get; }
        IEnumValueInfo<TValue> this[TValue value] { get; }
    }
}