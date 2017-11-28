using System.Collections.Generic;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Entities;
using Tornado.Parser.Filter.Nodes;

namespace Tornado.Parser.Filter {
    public static class FilterTreeFactory {
        public const string GROUP_SEPARATOR_PATTERN = "((\\w*)\\((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!))\\))";
        public const string AFFIX_SEPARATOR_PATTERN = "(\\((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!))\\))";
        public const string AFFIX_PATTERN = "((\\w*)\\((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!))\\))|(([$\\d.]+\\w+))";

        public static INode Build(string s) {
            List<string> matches = s.GetAllMatches(GROUP_SEPARATOR_PATTERN);
            if (matches.Count == 1 && matches[0].StartsWith("any")) {
                return new AnyGroupNode(matches[0]);
            }
            return new AndGroupNode(matches);
        }

        public static FilterTree BuildTree(string s) {
            return new FilterTree(s.ParseTo(ItemType.Unknown, "(\\w+)"), Build(s), s);
        }

        public static List<INode> BuildGroup(string s, string key) {
            List<string> matches = s.GetAllMatches(AFFIX_PATTERN);

            List<INode> result = new List<INode>();

            foreach (string m in matches) {
                if (m.StartsWith("any")) {
                    result.Add(new AnyAffixNode(m, key));
                } else {
                    result.Add(new AffixNode(m, key));
                }
            }

            return result;
        }
    }
}