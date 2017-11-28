using System.Collections.Generic;
using System.Linq;
using Tornado.Parser.Entities;

namespace Tornado.Parser.Filter.Nodes {
    public class AndGroupNode : INode {
        public List<INode> Nodes { get; set; }

        public IResultNode GetValue(Item item) {
            ILogicResultNode resultNode = new ResultNode();
            var resultNodes = Nodes.Select(n => n.GetValue(item)).ToList();
            var resultValueNodes = resultNodes.Where(r => !r.DisplayOnly).ToList();

            resultNode.DisplayOnly = !resultValueNodes.Any();
            resultNode.Nodes.AddRange(resultNodes);
            resultNode.Value = resultNode.DisplayOnly ? resultNodes.Sum(n => n.Value) / resultNodes.Count : resultValueNodes.Sum(n => n.Value) / resultValueNodes.Count;
            return resultNode;
        }

        public double FilteredValue { get; set; }
        public double Value { get; set; }

        public AndGroupNode(List<string> matches) {
            Nodes = new List<INode>();

            foreach (string m in matches) {
                Nodes.Add(m.StartsWith("any") ? FilterTreeFactory.Build(m) : new GroupNode(m));
            }
        }
    }
}