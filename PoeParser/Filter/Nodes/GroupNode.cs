using System.Collections.Generic;
using System.Linq;
using PoeParser.Common.Extensions;
using PoeParser.Entities;
using PoeParser.Parser;

namespace PoeParser.Filter.Nodes {
    public class GroupNode : INode {
        public IResultNode GetValue(Item item) {
            ILogicResultNode resultNode = new ResultNode();
            var resultNodes = Nodes.Select(n => n.GetValue(item)).ToList();
            var resultValueNodes = resultNodes.Where(r => !r.DisplayOnly).ToList();

            resultNode.DisplayOnly = !resultValueNodes.Any();
            resultNode.Nodes.AddRange(resultNodes);
            resultNode.Value = resultNode.DisplayOnly ? resultNodes.Sum(n => n.Value) : resultValueNodes.Sum(n => n.Value) / resultValueNodes.Count;
            return resultNode;
        }

        public double FilteredValue { get; set; }
        public double Value { get; set; }
        public string Group { get; set; }
        public List<INode> Nodes { get; set; }

        public GroupNode(string match) {
            string affixes = match.ParseTo("", FilterTreeFactory.AFFIX_SEPARATOR_PATTERN);
            Nodes = new List<INode>();
            Group = match.ParseTo("", "([\\w]+)");
            Nodes.AddRange(FilterTreeFactory.BuildGroup(affixes.Substring(1, affixes.Length - 2), Group));
        }
    }
}