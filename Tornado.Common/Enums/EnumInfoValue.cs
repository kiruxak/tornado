namespace PoeParser.Common {
    internal class EnumValueInfo<TValue> : IEnumValueInfo<TValue> {
        public int Id { get; internal set; }
        public TValue Value { get; internal set; }
        public string Name { get; internal set; }
        public string DisplayName { get; internal set; }
    }
}