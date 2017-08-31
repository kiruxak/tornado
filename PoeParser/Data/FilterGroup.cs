using PoeParser.Common.Extensions;
using PoeParser.Parser;

namespace PoeParser.Data {
    public class FilterGroup {
        public string Color { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int Order { get; set; }

        public FilterGroup(string line) {
            Name = line.ParseTo("", "group (\\w+)");
            Color = line.ParseTo("", "color=(\\w+)");
            DisplayName = line.ParseTo("", "name=(\\w+)");
            Order = line.ParseTo(0, "order=(\\w+)");
        }    
    }
}