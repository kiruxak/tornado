using System;
using System.Collections.Generic;
using System.Linq;
using Tornado.Common.Extensions;
using Tornado.Common.Utility;
using Tornado.Parser.Entities;
using Tornado.Parser.Filter.Nodes;

namespace Tornado.Parser.Filter {
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

            if (node == null) {
                throw new Exception("Фильтр не содержит группу");
            }

            NiceDictionary<string, List<string>> groups = node.GetAffixNodes()
                                                              .GroupBy(n => n.Group)
                                                              .ToNiceDictionary(n => n.Key, n => n.Select(af => af.Name)
                                                                                                  .Distinct()
                                                                                                  .ToList());
            return new FilterResult(groups, node, item);
        }
    }
}