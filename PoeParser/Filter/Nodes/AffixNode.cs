using System.Collections.Generic;
using PoeParser.Common.Extensions;
using PoeParser.Entities;
using PoeParser.Parser;

namespace PoeParser.Filter.Nodes {
    public class AffixNode : INode {
        public List<INode> Nodes { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public bool IsDisplayOnly { get; set; }

        public IResultNode GetValue(Item item) {
            IAffixResultNode resultNode = new AffixResultNode();
            resultNode.Group = Group;
            resultNode.Name = Name;
            resultNode.DisplayOnly = IsDisplayOnly;

            if (!item.Affixes.ContainsKey(Name)) {
                resultNode.Value = 0;
            } else {
                resultNode.Value = Value == 0 ? 1 : item.Affixes[Name].Value/Value;
            }
            return resultNode;
        }

        public double FilteredValue { get; set; }

        public AffixNode(string m, string group) {
            Group = group;
            Name = m.ParseTo("", "([a-zA-z]+)");
            IsDisplayOnly = m.StartsWith("$");
            Value = m.ParseTo(0.0, "([\\d.]+)");
        }
    }
}