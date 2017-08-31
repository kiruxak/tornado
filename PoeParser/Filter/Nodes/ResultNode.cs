using System.Collections.Generic;
using PoeParser.Common.Extensions;

namespace PoeParser.Filter.Nodes {
    public interface IResultNode {
        double Value { get; set; }
        bool DisplayOnly { get; set; }
    }

    public interface ILogicResultNode : IResultNode {
        List<IResultNode> Nodes { get; set; }
        List<IAffixResultNode> GetAffixNodes();
    }

    public interface IAffixResultNode : IResultNode {
        string Group { get; set; }
        string Name { get; set; }
    }

    public class ResultNode : ILogicResultNode {
        public double Value { get; set; }
        public bool DisplayOnly { get; set; }
        public List<IResultNode> Nodes { get; set; }

        public ResultNode() { Nodes = new List<IResultNode>(); }

        public List<IAffixResultNode> GetAffixNodes() {
            List<IAffixResultNode> result = new List<IAffixResultNode>();
            Nodes.Each(n => {
                IAffixResultNode node = n as IAffixResultNode;
                if (node != null) {
                    result.Add(node);
                } 
                ILogicResultNode logicNode = n as ILogicResultNode;
                if (logicNode != null) {
                    result.AddRange(logicNode.GetAffixNodes());
                }
            });

            return result;
        }
    }

    public class AffixResultNode : IAffixResultNode {
        public string Group { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public bool DisplayOnly { get; set; }
    }
}