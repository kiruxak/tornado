using System.Collections.Generic;
using Tornado.Parser.Entities;

namespace Tornado.Parser.Filter.Nodes {
    public interface INode {
        IResultNode GetValue(Item item);
        List<INode> Nodes { get; set; }
    }
}