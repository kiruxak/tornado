using System.Collections.Generic;
using System.Linq;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Entities;

namespace Tornado.Parser.Filter.Nodes {
    public class AnyGroupNode : INode {
        public List<INode> Nodes { get; set; }
        public int AtLeastCount { get; set; }

        public IResultNode GetValue(Item item) {
            ILogicResultNode resultNode = new ResultNode();
            var resultNodes = Nodes.Select(n => n.GetValue(item)).ToList();
            var resultValueNodes = resultNodes.Where(r => !r.DisplayOnly).OrderByDescending(n => n.Value).Take(AtLeastCount).ToList();

            resultNode.DisplayOnly = !resultValueNodes.Any();
            if (resultNode.DisplayOnly) {
                var displayResult = resultNodes.OrderByDescending(n => n.Value).Take(AtLeastCount).ToList();
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

        public AnyGroupNode(string match) {
            Nodes = new List<INode>();
            match = match.Substring(0, match.Length - 1);
            var groups = match.GetAllMatches(FilterTreeFactory.GROUP_SEPARATOR_PATTERN);
            AtLeastCount = match.ParseTo(0, "any(\\d+)");

            foreach (string g in groups) {
                if (g.StartsWith("any")) {
                    Nodes.Add(new AnyGroupNode(g));
                } else {
                    Nodes.Add(new GroupNode(g));
                }
            }
        }
    }
}