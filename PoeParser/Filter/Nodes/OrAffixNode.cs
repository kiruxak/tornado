using System.Collections.Generic;
using System.Linq;
using PoeParser.Common.Extensions;
using PoeParser.Entities;
using PoeParser.Parser;

namespace PoeParser.Filter.Nodes {
    public class AnyAffixNode : INode {
        public List<INode> Nodes { get; set; }
        public int AtLeastCount { get; set; }
        public IResultNode GetValue(Item item) {
            ILogicResultNode resultNode = new ResultNode();
            var resultNodes = Nodes.Select(n => n.GetValue(item)).ToList();
            var resultValueNodes = resultNodes.Where(r => !r.DisplayOnly).OrderByDescending(n => n.Value).Take(AtLeastCount).ToList();

            resultNode.DisplayOnly = !resultValueNodes.Any();
            if (resultNode.DisplayOnly) {
                var displayResult = resultNodes.Where(n => n.Value >= 1).OrderByDescending(n => n.Value).Take(AtLeastCount).ToList();
                resultNode.Nodes.AddRange(displayResult);
                resultNode.Value = displayResult.Sum(n => n.Value) / AtLeastCount;
            } else {
                resultNode.Nodes.AddRange(resultValueNodes);
                resultNode.Value = resultValueNodes.Sum(n => n.Value) / AtLeastCount;
            }
            return resultNode;
        }

        public double FilteredValue { get; set; }
        public double Value { get; set; }

        public AnyAffixNode(string match, string key) {
            Nodes = new List<INode>();
            AtLeastCount = match.ParseTo(0, "any(\\d+)");
            Nodes.AddRange(FilterTreeFactory.BuildGroup(match.Substring(1, match.Length - 2), key));
        }
    }
}