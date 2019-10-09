using System.Collections.Generic;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Entities;

namespace Tornado.Parser.Filter.Nodes {
    public class AffixNode : INode {
        public List<INode> Nodes { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public bool IsDisplayOnly { get; set; }
        public double DisplayValue { get; set; }

        public IResultNode GetValue(Item item) {
            IAffixResultNode resultNode = new AffixResultNode();
            resultNode.Group = Group;
            resultNode.Name = Name;
            resultNode.DisplayOnly = IsDisplayOnly;

            if (!item.Affixes.ContainsKey(Name)) {
                resultNode.Value = 0;
                resultNode.Logical = Value == DisplayValue ? -1 : 0;
            } else {
                var itemValue = item.Affixes[Name].Value;
                // if (itemValue > DisplayValue && DisplayValue > 0) {
                //     resultNode.DisplayOnly = false;
                // }

                // if (DisplayValue > 0 && Value > 0) {
                //     var min = itemValue - DisplayValue;
                //     var max = Value - DisplayValue;
                //     if (max > 0 && min > 0) {
                //         resultNode.Value = Value == 0 ? 1 : min / max;
                //     }
                //     else {
                //         resultNode.Value = Value == 0 ? 1 : itemValue / Value;
                //     }
                // }
                // else {
                    resultNode.Value = Value == 0 ? 1 : itemValue / Value;
                // }
                resultNode.Logical = Value == DisplayValue ? -1 : itemValue / DisplayValue;
            }

            return resultNode;
        }

        public double FilteredValue { get; set; }

        public AffixNode(string m, string group) {
            Group = group;
            Name = m.ParseTo("", "([a-zA-z]+)");
            IsDisplayOnly = m.StartsWith("$") && m.IndexOf(",") < 0;
            Value = m.IndexOf(",") > 0 ? m.ParseTo(0.0, ",([\\d.]+)") : m.ParseTo(0.0, "([\\d.]+)");
            DisplayValue = m.ParseTo(0.0, "([\\d.]+)");
        }
    }
}