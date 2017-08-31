using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PoeParser.Common.Extensions;
using PoeParser.Data;
using PoeParser.Entities.Affixes;

namespace PoeParser.PoeTradeIntegration {
    public class QueryGenerator {
        private readonly Dictionary<string, string> generalAffixes = new Dictionary<string, string> 
                                            { {"league",Config.LeagueName}, {"type",""}, {"base",""}, {"name",""}, {"dmg_min",""}, {"dmg_max",""},
                                            {"aps_min", ""}, {"aps_max",""}, {"crit_min",""}, {"crit_max",""}, {"dps_min",""},
                                            {"dps_max",""}, {"edps_min",""}, {"edps_max",""}, {"pdps_min",""}, {"pdps_max",""}, {"armour_min",""}, {"armour_max",""},
                                            {"evasion_min",""},{"evasion_max",""},{"shield_min",""},{"shield_max",""},{"block_min",""},{"block_max",""},{"sockets_min",""},
                                            {"sockets_max",""}, {"link_min",""}, {"link_max",""}, {"sockets_r",""}, {"sockets_g",""}, {"sockets_b",""}, {"sockets_w",""},
                                            {"linked_r",""},{"linked_g",""},{"linked_b",""},{"linked_w",""}, {"rlevel_min",""}, {"rlevel_max",""}, {"rstr_min",""}, {"rstr_max",""},
                                            {"rdex_min",""}, {"rdex_max",""},{"rint_min",""}, {"rint_max",""},
                                            {"q_min",""}, {"q_max",""}, {"level_min",""}, {"level_max",""}, {"mapq_min",""}, {"mapq_max",""}, {"rarity",""},
                                            {"seller",""}, {"thread",""}, {"time",""}, {"identified",""}, {"corrupted",""}, {"online","x"}, {"buyout","x"}, {"altart",""}, {"capquality","x"}, {"buyout_min",""},
                                            {"buyout_max",""}, {"buyout_currency",""}, {"crafted",""}, {"ilvl_min",""}, {"ilvl_max",""}, {"enchanted", ""} };

        private readonly List<string> affixes = new List<string>();

        public string GenerateQuery(List<IAffixValue> itemAffixes, Dictionary<string, string> generalParams) {
            if (generalParams.Any()) {
                generalParams.Each(pair => { if (generalAffixes.ContainsKey(pair.Key)) { generalAffixes[pair.Key] = pair.Value; } });
            }

            StringBuilder postBuilder = new StringBuilder();
            createQueryParams(itemAffixes.Where(a => !string.IsNullOrEmpty(a.RegexPattern)).ToList());

            foreach (var param in generalAffixes) {
                if (param.Key == "q_min") {
                    foreach (var rareMod in affixes) {
                        postBuilder.Append(UrlEncodeRareMod(rareMod)).Append("&");
                    }
                    string groupParams = "group_type=And&group_min=&group_max=&group_count=" + affixes.Count() + "&";
                    postBuilder.Append(groupParams);
                }
                postBuilder.Append(param.Key).Append("=").Append(param.Value).Append("&");
            }

            if (postBuilder[postBuilder.Length - 1] == '&')
                postBuilder.Remove(postBuilder.Length - 1, 1);
            return postBuilder.ToString();
        }

        private void createQueryParams(List<IAffixValue> itemAffixes) {
            itemAffixes.Each(a => {
                string prefix = "mod_name="; ;
                string expression;

                if (a is TotalAffixRecord) {
                    expression = a.RegexPattern;
                } else {
                    expression = a.RegexPattern.Replace("(\\d+)", "#").Replace("\\", "");
                }

                affixes.Add(prefix + expression + "&mod_min=" + string.Format(new System.Globalization.CultureInfo("en-US"), "{0:F2}", a.Value * Config.PoeTradeMod) + "&mod_max=");
            });
        }

        public static string UrlEncodeRareMod(string param) {
            if (param.Contains("mod_name=&") || param.Contains("group_type"))
                return param;
            int startIndex = param.IndexOf('=') + 1;
            int endIndex = param.IndexOf('&');
            string mod = param.Substring(startIndex, endIndex - startIndex);
            string encodedMod = System.Web.HttpUtility.UrlEncode(mod);
            encodedMod = encodedMod.Replace("(", "%28").Replace(")", "%29");
            return param.Replace(mod, encodedMod);
        }
    }
}
