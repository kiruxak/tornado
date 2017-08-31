using System;
using System.Collections.Generic;
using System.Linq;
using PoeParser.Entities;
using PoeParser.Filter.Nodes;

namespace PoeParser.Filter {
    public class FilterTree {
        public INode RootNode { get; set; }
        public ItemType Type { get; set; }
        public string FilterPattern { get; set; }

        public FilterTree(ItemType type, INode rootNode, string pattern) {
            RootNode = rootNode;
            Type = type;
            FilterPattern = pattern;
        }

        public FilterResult Filter(Item item) {
            IResultNode filteredNode = RootNode.GetValue(item);
            ILogicResultNode node = filteredNode as ILogicResultNode;

            if (node == null) { throw new Exception("Фильтр не содержит группу"); }

            Dictionary<string, List<string>> groups = node.GetAffixNodes()
                                                          .GroupBy(n => n.Group)
                                                          .ToDictionary(n => n.Key, n => n.Select(af => af.Name)
                                                                                          .Distinct()
                                                                                          .ToList());
            return new FilterResult(groups, node, item);
        }
    }
}