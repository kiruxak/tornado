using System.Collections.Generic;
using PoeParser.Entities;

namespace PoeParser.Filter.Nodes {
    public interface INode {
        IResultNode GetValue(Item item);
        List<INode> Nodes { get; set; }
    }
}