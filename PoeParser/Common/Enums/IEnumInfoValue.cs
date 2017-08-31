namespace PoeParser.Common {
    public interface IEnumValueInfo<out TValue> {
        int Id { get; }
        TValue Value { get; }
        string Name { get; }
        string DisplayName { get; }
    }
}