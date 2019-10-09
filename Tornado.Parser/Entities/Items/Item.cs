using System;
using System.Collections.Generic;
using System.Linq;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Data.Data;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Data;
using Tornado.Parser.Data.Bases;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;
using Tornado.Parser.PoeTrade.Response;

namespace Tornado.Parser.Entities {
    public abstract class Item : BaseItem {
        public Core Core { get; set; }
        public Tornado.Data.Data.BaseItem BaseItem { get; }
        public NiceDictionary<string, IAffixValue> Affixes { get; set; } = new NiceDictionary<string, IAffixValue>();
        public NiceDictionary<string, AffixMod> Mods { get; set; } = new NiceDictionary<string, AffixMod>();

        public string AffixSource { get; private set; }
        public string ImplicitSource { get; private set; }

        //todo fix this
        public bool CanCraft => Config.ShowCraft && (PrefixesCount + SuffixesCount < 6) && (PrefixesCount < 3 || SuffixesCount < 3) && !Corrupted && !HasCraft && Rarity != ItemRarity.Unique;
        public bool CanCraftSuffix => SuffixesCount < 3 && CanCraft;
        public bool CanCraftPrefix => PrefixesCount < 3 && CanCraft;
        public bool HasCraft { get; set; }
        public bool CanHaveCraft { get; set; } = true;
        public bool NoCraft { get; set; }
        public bool CraftAddedByFilter { get; set; } = false;
        public string SocketLine { get; set; }
        public int WhiteSockets { get; set; } = 0;
        //public List<string> ContainsGoodFracture { get; set; } = new List<string>();
        public List<SynthesisImplicit> SynthesisImplicits { get; set; } = new List<SynthesisImplicit>();
        public List<string> syncImplicits = new List<string>();
        public Dictionary<string, AffixValue> fracturedAffixes = new Dictionary<string, AffixValue>();
        public int FracturedAffixesInRecipient = 0;
        public int FracturedAffixesCount = 0;
        public string Enchant { get; set; } = null;

        public int PrefixesCount => Affixes.Values.Count(a => a.Type == AffixType.Prefix) + Affixes.Values.OfType<AffixValue>().Count(a => a.Type == AffixType.Prefix && a.AffixTier.Hybrid);
        public int SuffixesCount => Affixes.Values.Count(a => a.Type == AffixType.Suffix) + Affixes.Values.OfType<AffixValue>().Count(a => a.Type == AffixType.Suffix && a.AffixTier.Hybrid);

        protected Item(string source, ItemRarity rarity, Core core, Tornado.Data.Data.BaseItem baseItem, string name, bool noCraft) : base(source, rarity, core.Type, name) {
            Core = core;
            BaseItem = baseItem;
            NoCraft = noCraft;

            SearchParts(source, rarity, core);

            GetAffixes(AffixSource, PoeData.Affixes.Values.Where(x => x.Type == AffixType.Suffix || x.Type == AffixType.Prefix));
            GetAffixes(ImplicitSource, PoeData.Affixes.Values.Where(x => x.Type == AffixType.Implicit));
            GetUniqueAffixes();
            FixRarity();

            GetTotalAffixes();

            var index = Source.IndexOf("Sockets:", StringComparison.Ordinal);
            if (index > 0) {
                SocketLine   = Source.Substring(index, 20).Replace("Sockets: ", "");
                WhiteSockets = SocketLine.Count(c => c == 'W');
            }


            FindPossibleSynthesisImplicits();
        }

        private void FindPossibleSynthesisImplicits() {
            if (Core?.Mark == Mark.Fractured) {
                foreach (var mod in PoeData.SynthesisImplicits.Values.SelectMany(x => x.Mods)) {
                    if (mod.Types.Contains((Tornado.Data.Data.ItemType)Type)) {
                        var val = mod.ReqStat.Contains("([\\d.]+)%? to ([\\d.]+)%?") ? AffixSource.ParseDualAverage(mod.ReqStat+"(\r\n| \\(fractured\\))") : AffixSource.ParseTo(0.0, mod.ReqStat + "(\r\n| \\(fractured\\))");
                        if (val > 0) {
                            SynthesisImplicits.Add(mod);
                        }
                    }
                }

                foreach (var reqStatGroup in SynthesisImplicits.GroupBy(x => x.ReqStat)) {
                    var affix = reqStatGroup.First().ReqStat;
                    var contains = fracturedAffixes.Keys.FirstOrDefault(x => (affix.Contains("([\\d.]+)%? to ([\\d.]+)%?") ? x.ParseDualAverage(affix) : x.ParseTo(0.0, affix)) > 0);
                    var currentValue = -1.0;
                    if (contains != null) {
                        FracturedAffixesInRecipient++;
                        var affixFractured = fracturedAffixes[contains];
                        syncImplicits.Add($"{(affixFractured.CalculatedTier <= 2 ? "!" : "#")}[{affix.Replace("([\\d.]+)%?", "").Replace("\\+", "").Trim()}]:   Tier: {affixFractured.CalculatedTier}   Value: {affixFractured.Value}");
                        currentValue = affixFractured.Value;
                    } else {
                        syncImplicits.Add($"[{affix.Replace("([\\d.]+)%?", "").Replace("\\+", "").Trim()}]:  ");
                    }
                    var req = 1.0;
                    var affixIsMarked = false;
                    foreach (var modIdGroup in reqStatGroup.GroupBy(x => x.ReqValue).OrderBy(x => x.Key)) {
                        var syncImplicit = req == 1.0 ? $"Req - {req}:   " : $"Req - {req} (~{(req / 3.0):F1}):   ";
                        var syncImplicitLength = syncImplicit.Length;
                        if (!affixIsMarked && (currentValue >= (req/3) && currentValue <= (modIdGroup.First().ReqValue / 3.0)) ) {
                            syncImplicit = "!" + syncImplicit;
                            affixIsMarked = true;
                        }
                        req = modIdGroup.First().ReqValue + 1;
                        var texts = new List<string>();
                        var maxLength = 120;
                        foreach (var i in modIdGroup) {
                            if (!texts.Contains(i.Text)) {
                                syncImplicit += $"{i.Text}  |  ";
                                if (syncImplicit.Length > maxLength) {
                                    syncImplicit += "##" + new String(' ', (int)(syncImplicitLength*1.6));
                                    maxLength += 180;
                                }
                            }
                            texts.Add(i.Text);
                        }
                        syncImplicit = syncImplicit.Substring(0, syncImplicit.Length - 5);
                        foreach (var mod in syncImplicit.Split(new string[] {"##"}, StringSplitOptions.RemoveEmptyEntries)) {
                            syncImplicits.Add(mod);
                        }
                    }
                    syncImplicits.Add("---");
                }
                if (syncImplicits.Count > 0) {
                    syncImplicits.RemoveAt(syncImplicits.Count - 1);
                }
            }
        }

        private void AddUniqueAffix(string tooltip, string pattern, string poeAffix = null, bool isTotal = false) {
            var fixedPattern = pattern.StartsWith("+") ? pattern.Substring(1) : pattern;
            var t = new TradeAffix() {
                Tooltip      = tooltip,
                PoeTradeName = poeAffix ?? pattern.Replace("(\\d+)", "#"),
                Value        = pattern.Contains("(\\d+) to (\\d+)") ? AffixSource.ParseDualAverage(fixedPattern) : AffixSource.ParseTo(0.0, fixedPattern),
                IsTotal      = isTotal
            };

            if (t.Value > 0 || AffixSource.Contains(pattern)) {
                UniqueAffixes.Add(t);
            }
        }

        private void GetCorruptionAffix(string tooltip, string pattern) {
            var fixedPattern = $"{(pattern.StartsWith("+") ? "\\" : "")}{pattern}";

            var t = new TradeAffix() {
                Tooltip      = tooltip,
                PoeTradeName = "(implicit) " + pattern.Replace("(\\d+)", "#"),
                Value        = pattern.Contains("(\\d+) to (\\d+)") ? ImplicitSource.ParseDualAverage(fixedPattern) : ImplicitSource.ParseTo(0, fixedPattern),
                IsTotal      = false
            };

            if (t.Value > 0 || AffixSource.Contains(pattern)) {
                UniqueAffixes.Add(t);
            }
        }

        private void GetUniqueAffixes() {
            if (Corrupted) {
                GetCorruptionAffix("+ lvl gem", "+(\\d+) to Level of Socketed Gems");
                GetCorruptionAffix("!Additional Curse", "Enemies can have 1 additional Curse");
                GetCorruptionAffix("!Additional Arrow", "Bow Attacks fire an additional Arrow");
                GetCorruptionAffix("!Additional Projectile", "Skills fire an additional Projectile");
                GetCorruptionAffix("+% elem penetrate", "Damage Penetrates (\\d+)% Elemental Resistances");
                GetCorruptionAffix("+% spell crit", "Spells have +([\\d.]+)% to Critical Strike Chance");

                GetCorruptionAffix("+% max res", "+(\\d+)% to all maximum Resistances");
                GetCorruptionAffix("+ power charge", "+(\\d+) to Maximum Power Charges");
                GetCorruptionAffix("+ frenzy charge", "+(\\d+) to Maximum Frenzy Charges");
                GetCorruptionAffix("+ endurance charge", "+(\\d+) to Maximum Endurance Charges");
            }
            if (Type.Is1HWeapon() || Type.Is2HWeapon()) {
                AddUniqueAffix("+% phys as extra Chaos", "Gain (\\d+)% of Physical Damage as Extra Chaos Damage");
            }
            AddUniqueAffix("+% life", "\r\n(\\d+)% increased maximum Life\r\n", "#% increased maximum Life");
            if (Type != ItemType.Amulet) {
                AddUniqueAffix("+% es", "\r\n(\\d+)% increased maximum Energy Shield\r\n", "#% increased maximum Energy Shield");
            }
            if (Type == ItemType.Belt || Type == ItemType.Ring) {
                AddUniqueAffix("+% damage", "(\\d+)% increased Damage \\(crafted\\)", "(crafted) #% increased Damage");
            }
            AddUniqueAffix("+% mana", "\r\n(\\d+)% increased maximum Mana\r\n", "#% increased maximum Mana");
            AddUniqueAffix("- mana cost", "(\\d+) to Total Mana Cost of Skills");
            if (Type == ItemType.Gloves) {
                AddUniqueAffix("+% cast speed", "\r\n(\\d+)% increased Cast Speed\r\n", "#% increased Cast Speed");
            }

            AddUniqueAffix("+% phys hit as fire", "(\\d+)% of Physical Damage from Hits taken as Fire Damage");
            AddUniqueAffix("+% phys hit as cold", "(\\d+)% of Physical Damage from Hits taken as Cold Damage");
            AddUniqueAffix("+% phys hit as light", "(\\d+)% of Physical Damage from Hits taken as Lightning Damage");

            AddUniqueAffix("+% convert phys as fire", "(\\d+)% of Physical Damage Converted to Fire Damage");
            AddUniqueAffix("+% convert phys as cold", "(\\d+)% of Physical Damage Converted to Cold Damage");
            AddUniqueAffix("+% convert phys as light", "(\\d+)% of Physical Damage Converted to Lightning Damage");

            AddUniqueAffix("+% fire penetrate", "Damage Penetrates (\\d+)% Fire Resistances");
            AddUniqueAffix("+% cold penetrate", "Damage Penetrates (\\d+)% Cold Resistances");
            AddUniqueAffix("+% light penetrate", "Damage Penetrates (\\d+)% Lightning Resistances");

            AddUniqueAffix(" Spider Aspect", "Grants Level (\\d+) Aspect of the Spider Skill");
            AddUniqueAffix(" Crab Aspect", "Grants Level (\\d+) Aspect of the Crab Skill");
            AddUniqueAffix(" Cat Aspect", "Grants Level (\\d+) Aspect of the Cat Skill");
            AddUniqueAffix(" Avian Aspect", "Grants Level (\\d+) Aspect of the Avian Skill");

            //gems
            AddUniqueAffix("+ lvl aura gems", "+(\\d+) to Level of Socketed Aura Gems");
            AddUniqueAffix("+ lvl all support gems", "+(\\d+) to Level of Socketed Support Gems \\(crafted\\)", "(crafted) +# to Level of Socketed Support Gems");
            AddUniqueAffix("+ lvl all spell gems", "+(\\d+) to Level of all Spell Skill Gems");
            AddUniqueAffix("+ lvl all fire spell gems", "+(\\d+) to Level of all Fire Spell Skill Gems");
            AddUniqueAffix("+ lvl all cold spell gems", "+(\\d+) to Level of all Cold Spell Skill Gems");
            AddUniqueAffix("+ lvl all light spell gems", "+(\\d+) to Level of all Lightning Spell Skill Gems");
            AddUniqueAffix("+ lvl all chaos spell gems", "+(\\d+) to Level of all Chaos Spell Skill Gems");
            AddUniqueAffix("+ lvl all phys spell gems", "+(\\d+) to Level of all Physical Spell Skill Gems");
            AddUniqueAffix("+ lvl all spectre gems", "+(\\d+) to Level of all Raise Spectre Skill Gems");
            AddUniqueAffix("+ lvl all minion gems", "+(\\d+) to Level of all Minion Skill Gems");
            AddUniqueAffix("+ max spectre", "+(\\d+) to maximum number of Spectres");

            if (Type == ItemType.BodyArmour) {
                AddUniqueAffix("+% life reg", "([\\d.]+)% of Life Regenerated per second", "#% of Life Regenerated per second");
                AddUniqueAffix("!Level 1 Main", "Socketed Gems are Supported by Level 1 Maim", "Socketed Gems are Supported by Level # Maim");
                AddUniqueAffix("!-15 mana cost", "Socketed Attacks have -15 to Total Mana Cost", "Socketed Attacks have -# to Total Mana Cost");
            }

            if (Core.Mark == Mark.Empty) { return; }
            //helm
            if (Type == ItemType.Helmet) {
                if (Source.Contains("Place an additional Mine")) { AddUniqueAffix("!Place an additional Mine", "Place an additional Mine"); }
                AddUniqueAffix("+ cold spell", "Adds (\\d+) to (\\d+) Cold Damage to Spells");
                AddUniqueAffix("+ lightning spell", "Adds (\\d+) to (\\d+) Lightning Damage to Spells");
                AddUniqueAffix("+ fire spell", "Adds (\\d+) to (\\d+) Fire Damage to Spells");
                AddUniqueAffix("+ physical spell", "Adds (\\d+) to (\\d+) Physical Damage to Spells");
                AddUniqueAffix("+ chaos spell", "Adds (\\d+) to (\\d+) Chaos Damage to Spells");
            }
            if (Type == ItemType.BodyArmour) {
                AddUniqueAffix("+% flat spell crit", "Spells have +([\\d.]+)% to Critical Strike Chance");
                AddUniqueAffix("+% flat attack crit", "Attacks have +([\\d.]+)% to Critical Strike Chance");
            }
            if (Type == ItemType.Gloves) {
                AddUniqueAffix("+% attack and cast speed", "(\\d+)% increased Attack and Cast Speed", "#% increased Attack Speed");
            }
            if (Type == ItemType.Quiver) {
                if (Source.Contains("Bow Attacks fire an additional Arrow")) { AddUniqueAffix("!Additional Arrow", "Bow Attacks fire an additional Arrow"); }
                AddUniqueAffix("+% phys as extra cold", "Gain #% of Physical Damage as Extra Cold Damage");
            }
            if (Type == ItemType.Amulet) {
                AddUniqueAffix("+% damage per 15 str", "(\\d+)% increased Damage per 15 Strength", "#% increased Damage per # Strength");
                AddUniqueAffix("+% damage per 15 dex", "(\\d+)% increased Damage per 15 Dexterity", "#% increased Damage per # Dexterity");

                AddUniqueAffix(" Wrath", "Grants Level (\\d+) Wrath Skill");
                AddUniqueAffix(" Anger", "Grants Level (\\d+) Anger Skill");
                AddUniqueAffix(" Hatred", "Grants Level (\\d+) Hatred Skill");
                AddUniqueAffix(" Envy", "Grants Level (\\d+) Envy Skill");
                AddUniqueAffix(" Discipline", "Grants Level (\\d+) Discipline Skill");
                AddUniqueAffix(" Vitality", "Grants Level (\\d+) Vitality Skill");

                AddUniqueAffix("+% quantity", "(\\d+)% increased Quantity of Items found");
            }

            if (Type == ItemType.Ring || Type == ItemType.Amulet) {
                AddUniqueAffix(" Herald of Ice", "Grants Level (\\d+) Herald of Ice Skill");
                AddUniqueAffix(" Herald of Ash", "Grants Level (\\d+) Herald of Ash Skill");
            }

            if (Type == ItemType.Bow) {
                if (Source.Contains("Bow Attacks fire an additional Arrow")) { AddUniqueAffix("!Additional Arrow", "Bow Attacks fire an additional Arrow"); }
                AddUniqueAffix("+% cold damage per 10 dex", "Adds (\\d+) to (\\d+) Cold Damage to Attacks with this Weapon per 10 Dexterity", "Adds # to # Cold Damage to Attacks with this Weapon per # Dexterity");
            }
            if (Type == ItemType.Staff) {
                AddUniqueAffix("+% cold damage per 10 str", "Adds (\\d+) to (\\d+) Fire Damage to Attacks with this Weapon per 10 Strength", "Adds # to # Fire Damage to Attacks with this Weapon per # Strength");
                AddUniqueAffix("+% cold damage per 10 int", "Adds (\\d+) to (\\d+) Lightning Damage to Attacks with this Weapon per 10 Dexterity", "Adds # to # Lightning Damage to Attacks with this Weapon per # Intelligence");
            }
            if (Type == ItemType.Amulet || Type.Is1HWeapon() || Type.Is2HWeapon()) {
                AddUniqueAffix("+% elem penetrate", "Damage Penetrates (\\d+)% Elemental Resistances");
                AddUniqueAffix("+% phys as extra cold", "Gain (\\d+)% of Physical Damage as Extra Cold Damage");
                AddUniqueAffix("+% phys as extra Lightning", "Gain (\\d+)% of Physical Damage as Extra Lightning Damage");
                AddUniqueAffix("+% phys as extra Fire", "Gain (\\d+)% of Physical Damage as Extra Fire Damage");
                AddUniqueAffix("+% phys as extra Chaos", "Gain (\\d+)% of Physical Damage as Extra Chaos Damage");
                if (Rarity != ItemRarity.Unique) {
                    AddUniqueAffix("+% Non-Chaos as extra Chaos", "Gain (\\d+)% of Non-Chaos Damage as extra Chaos Damage");
                }
            }
        }

        private void SearchParts(string source, ItemRarity rarity, Core core) {
            var parts = source.Split(new[] {"--------"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (parts.Last().ContainsPattern("\r\nMirrored"))
                parts.RemoveAt(parts.Count - 1); // Mirrored
            if (parts.Last().ContainsPattern("Note:"))
                parts.RemoveAt(parts.Count - 1); // Note with b/o
            if (parts.Last().ContainsPattern("\r\nCorrupted"))
                parts.RemoveAt(parts.Count - 1); // Corrupted
            if (parts.Last().ContainsPattern("\r\nHas "))
                parts.RemoveAt(parts.Count - 1); // Effect
            if (parts.Last().ContainsPattern("\r\nElder Item") || parts.Last().ContainsPattern("\r\nShaper Item") ||
                parts.Last().ContainsPattern("\r\nSynthesised Item") || parts.Last().ContainsPattern("\r\nFractured Item"))
                parts.RemoveAt(parts.Count - 1); // Effect

            if (rarity == ItemRarity.Unique) {
                AffixSource = parts[parts.Count - 2];
                ImplicitSource = parts[parts.Count - 3];
                CheckLabEnchant(parts);
            }
            else {
                if (core.Type == ItemType.Jewel ||  core.Type == ItemType.AbyssalJewel) {
                    AffixSource = parts[parts.Count - 2];
                    ImplicitSource = "";
                }
                else {
                    AffixSource = parts.Last();
                    ImplicitSource = parts[parts.Count - 2];
                    CheckLabEnchant(parts);
                    if (Core?.Mark == Mark.Fractured && Rarity == ItemRarity.Magic) {
                        var affixes = AffixSource.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        foreach (string affix in affixes.ToList()) {
                            if (affix.Contains("(fractured)")) {
                                break;
                            }
                            affixes.Remove(affix);
                        }
                        AffixSource = String.Join("\r\n", affixes) + "\r\n";
                    }
                }
            }
        }

        public void CheckLabEnchant(List<string> parts) {
            if (Type == ItemType.Helmet) {
                var index = 0;
                foreach (string part in parts) {
                    if (part.Contains("Item Level")) {
                        index = parts.IndexOf(part);
                        break;
                    }
                }
                Enchant = parts[index + 1].Substring(2, parts[index + 1].Length - 4);
            }
        }

        public void UpdateTotalAffixes() {
            foreach (TotalAffixValue affix in Affixes.Values.OfType<TotalAffixValue>().ToList()) {
                TotalAffixValue affi = affix.TotalAffix.GetValue(this);

                if (Affixes.ContainsKey(affix.Name)) {
                    Affixes.Remove(affix.Name);
                }
                if (affi.Value > 0) {
                    Affixes.Add(affi.Name, affi);
                }
            }
            foreach (TotalAffixRecord affix in PoeData.TotalAffixes.Values.ToList()) {
                TotalAffixValue affi = affix.GetValue(this);

                if (Affixes.ContainsKey(affix.Name)) {
                    Affixes.Remove(affix.Name);
                }
                if (affi.Value > 0) {
                    Affixes.Add(affi.Name, affi);
                }
            }
        }

        private void FixRarity() {
            if (Affixes.ContainsKey("pRarity")) {
                if (Affixes["pRarity"].Value > Affixes["pRarity"].MaxValue) {
                    Affixes["sRarity"].Value = 0;
                }
                else {
                    if (PrefixesCount > 3 && SuffixesCount <= 3) {
                        Affixes.Remove("pRarity");
                    } else {
                        Affixes.Remove("sRarity");
                    }
                }
            }
        }

        private void GetTotalAffixes() {
            foreach (TotalAffixRecord affix in PoeData.TotalAffixes.Values) {
                TotalAffixValue affi = affix.GetValue(this);
                if (affi.Value > 0 || !affi.TotalAffix.Template.Contains("{0}")) {
                    Affixes.Add(affi.Name, affi);
                }
            }
        }

        private void GetAffixes(string val, IEnumerable<Affix> affixes) {
            var lastAffix = val.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            CanHaveCraft = !Source.Contains("(crafted)");

            void AddFracturedAffix(Affix affix1, double d, AffixTier affixTier, double affixValue1, bool b, int i) {
                FracturedAffixesCount++;
                var name = affix1.RegexPattern.Replace("([\\d.]+)", d.ToString()).Replace("(\\d+)", d.ToString());
                if (!fracturedAffixes.ContainsKey(name)) {
                    fracturedAffixes.Add(name, new AffixValue(affixTier, affix1, affixValue1, false, b, i - affixTier.Tier + 1));
                }
            }

            foreach (Affix affix in affixes) {
                bool fractured = false;
                double affixValue = 0;
                double craftValue = 0;
                double fracturedValue = 0;
                if (affix.RegexPattern.StartsWith("!") && val.Contains(affix.RegexPattern.Substring(1))) {
                    affixValue = 0;
                } else {
                    affixValue = affix.RegexPattern.Contains("(\\d+) to (\\d+)")
                            ? val.ParseDualAverage(affix.RegexPattern + "\r\n")
                            : val.ParseSum(affix.RegexPattern + "\r\n");

                    craftValue = affix.RegexPattern.Contains("(\\d+) to (\\d+)")
                                ? val.ParseDualAverage(affix.RegexPattern + @" \(crafted\)\r\n")
                                : val.ParseSum(affix.RegexPattern + @" \(crafted\)\r\n");

                    fracturedValue = (affix.RegexPattern.Contains("(\\d+) to (\\d+)")
                            ? val.ParseDualAverage(affix.RegexPattern + @" \(fractured\)\r\n")
                            : val.ParseSum(affix.RegexPattern + @" \(fractured\)\r\n"));
                    if (fracturedValue > 0) {
                        affixValue += fracturedValue;
                        fractured = true;
                        
                    }

                    if (craftValue > 0) {
                        affixValue += craftValue;
                    }
                    if (affixValue <= 0) {
                        continue;
                    }
                }

                AffixTier t = affix.GetTier(affixValue, Type);
                var maxTier = t == null ? affix.Tiers.Max(x => x.Tier) : affix.Tiers.Where(ct => ct.ForTypes.Contains(Type)).Max(x => x.Tier);

                if (t == null && craftValue > 0 && !NoCraft && !Corrupted) {
                    var craftTier = affix.Tiers.Where(ct => ct.Craft && ct.ForTypes.Contains(Type))
                                         .OrderByDescending(ct => ct.MinValue)
                                         .LastOrDefault(ct => affixValue >= ct.MinValue && affixValue <= ct.MaxValue);
                    if (craftTier != null) {
                        HasCraft = true;
                        Affixes.Add(affix.Name, new AffixValue(craftTier, affix, affixValue, true, fractured));
                    } 
                }

                if (t != null) {
                    bool canBeCraft = lastAffix.ContainsPattern(affix.RegexPattern) && affix.Type != AffixType.Implicit;
                    if (canBeCraft && !Corrupted) {
                        var craftTier = affix.GetCraftTier(affixValue, Type);
                        if (craftTier != null && !CanHaveCraft && !NoCraft) {
                            HasCraft = true;
                            Affixes.Add(affix.Name, new AffixValue(craftTier, affix, affixValue, true, fractured));
                        } else {
                            Affixes.Add(affix.Name, new AffixValue(t, affix, affixValue, false, fractured, maxTier - t.Tier + 1));
                            if (fractured) { AddFracturedAffix(affix, fracturedValue, t, affixValue, fractured, maxTier); }
                        }
                    } else {
                        Affixes.Add(affix.Name, new AffixValue(t, affix, affixValue, false, fractured, maxTier - t.Tier + 1));
                        if (fractured) { AddFracturedAffix(affix, fracturedValue, t, affixValue, fractured, maxTier); }
                    }
                }
            }
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            var lines = new List<ColoredLine>();
            var line = new ColoredLine();

            var uniqueType = $"{(Core.Type == ItemType.Map ? "Map:" : Core.Type == ItemType.Jewel ? "Jewel:" : Core.Type == ItemType.Flask ? "Flask" : "Unique:")} {Name}";

            line.Add(filter.Item.Rarity == ItemRarity.Unique ? uniqueType : $"{Type} P{Affixes.Count(a => a.Value.Type == AffixType.Prefix)} S{Affixes.Count(a => a.Value.Type == AffixType.Suffix)} -", Color, 16);

            if (filter.Item.Rarity != ItemRarity.Unique) {
                line.Add($" {(filter.Value * 100).ToString("F0")}%", filter.Color, 16);
                line.Add($" {(HasCraft ? "HasCraft" : "")}", ToolTipColor.Gem, 12, heightOffset: 2);
            }
            lines.Add(line);

            lines.AddEmptyLine()
                 .If(Core.Type == ItemType.Flask && filter.Item.Rarity == ItemRarity.Unique, l => { //if unique flask
                     l.Add($"Quality: {Quality}", ToolTipColor.GeneralGroup, 16);
                 })
                 .If(Links >= 5, l => { //if 5 or 6 links
                     l.Add($"Links: {Links}", ToolTipColor.NormalItem, 18);
                 });

            var groupMaps = filter.FilteredAffixes;
            var size = 16;
            foreach (KeyValuePair<FilterGroup, List<IAffixValue>> pair in groupMaps.OrderBy(g => g.Key.Order)) {
                string color = FilterManager.Groups[pair.Key.Name].Color;

                foreach (IAffixValue a in pair.Value.OrderBy(a => a.RegexPattern)) {
                    var total = a as TotalAffixValue;
                    if (total != null) {
                        lines.Add(total.GetTooltipLine(color, 16));
                    } else {
                        var prefix = a.RegexPattern.Contains("%") ? "%" : "";
                        var starts = a.RegexPattern.Contains("+(") ? "+" : "";
                        lines.Add(new ColoredLine(new List<ColoredText>()
                                                  .Add(starts + a.Value.ToString("0.#") + prefix, ToolTipColor.GeneralGroup, size + 1)
                                                  .Add(' '+a.Name, color, size)
                                                  .Add(a.Value >= a.MaxValue ? "" : $" [{a.MinValue:F1}-{a.MaxValue:F1}]", color, size - 4, heightOffset: 2)));
                    }
                }
            }

            DrawSynthesisAffixes(lines);

            foreach (TradeAffix affix in UniqueAffixes) {
                var textOnly = affix.Tooltip.StartsWith("!");
                if (textOnly) {
                    var tooltip = affix.Tooltip.Replace("!", "");
                    lines.Add(new ColoredLine(tooltip, ToolTipColor.Gem, size));
                } else {
                    var prefix = affix.Tooltip.Contains("%") ? "%" : "";
                    var starts = affix.Tooltip.StartsWith("+") ? "+" : "";
                    var tooltip = affix.Tooltip.Replace("%", "").Replace("+", "");
                    lines.Add(new ColoredLine(new List<ColoredText>()
                                                   .Add(starts + affix.Value.ToString("0.#") + prefix, ToolTipColor.GeneralGroup, size + 1)
                                                   .Add(' ' + tooltip, ToolTipColor.Gem, size)));
                }
            }

            lines.If(Corrupted, (list => list.Add("Corrupted", ToolTipColor.Corrupted, 18)));
            lines.If(WhiteSockets > 0, (list => list.Add($"Has {WhiteSockets} white sockets", ToolTipColor.NormalItem, 18)));

            if (Config.Debug) {
                lines.Add($"DEBUG", "FFFF454E", 18);

                foreach (var a in Affixes.Values.OfType<AffixValue>().OrderBy(a => a.Type)) {
                    string type = a.Type == AffixType.Implicit ? "I" : a.Type == AffixType.Prefix ? "P" : "S";
                    string color = a.Type == AffixType.Implicit ? "FFFFB735" : a.Type == AffixType.Prefix ? "FF458FFF" : "FF45FF5C";
                    lines.Add($"{(type)}{a.AffixTier.Tier}  {a.Name}{(a.IsCraft ? $"[C][{type}]" : "")}: {a.Value}/{a.MaxValue}  {a.AffixTier.MinValue:F1}/{a.AffixTier.MaxValue:F1}", a.IsCraft ? "FFA223EF" : color);
                }

                lines.AddEmptyLine()
                     .Add("Posible to craft", "FFFF454E", 18);

                foreach (var a in filter.PossiblyToCraft.OrderBy(a => a.Type)) {
                    string color = a.Type == AffixType.Implicit ? "FFFFB735" : a.Type == AffixType.Prefix ? "FF458FFF" : "FF45FF5C";
                    var tier = a.Tiers.Where(t => t.Craft && t.ForTypes.Contains(filter.Item.Type)).OrderByDescending(t => t.MinValue).FirstOrDefault();

                    if (tier != null)
                        lines.Add($"{a.Name}[{(a.Type == AffixType.Prefix ? "P" : "S")}]: {tier.MinValue:F1}/{tier.MaxValue:F1}", color);
                }
            }
            return lines;
        }

        protected void DrawSynthesisAffixes(List<ColoredLine> lines) {
            if (Core?.Mark == Mark.Fractured || Core?.Mark == Mark.Synthesised) {
                lines.AddEmptyLine();
                lines.Add(new ColoredLine(new List<ColoredText>().Add($"{Core?.Mark.ToString()}", ToolTipColor.Delve, 18)
                                                                 .Add($"{(Core?.Base != null ? $"  BaseTier: {Core.Base.BaseTier}" : "")}", ToolTipColor.Gem, 18)));
            }
            if (syncImplicits.Count > 0) {
                var freeFractured = FracturedAffixesCount - FracturedAffixesInRecipient;
                lines.If(freeFractured > 0, l => {
                    l.Add($"Free fractured affixes: {freeFractured}", ToolTipColor.Delve, 18);
                });
                foreach (var sync in syncImplicits) {
                    if (sync == "---") {
                        lines.AddEmptyLine();
                        continue;
                    }
                    var color = sync.StartsWith("!") ? ToolTipColor.Gem :
                            sync.StartsWith("#")     ? ToolTipColor.NormalItem :
                            sync.Contains("[")       ? ToolTipColor.Delve : ToolTipColor.Essence;
                    var sizes = sync.StartsWith("!") ? 18 : sync.StartsWith("#") ? 17 : sync.Contains("[") ? 16 : 15;

                    var text = sync;
                    if (sizes > 16) {
                        text = sync.Substring(1);
                    }

                    lines.Add(text, color, sizes);
                }
            }
        }
    }

    public class AffixMod {
        public double Value { get; set; }
        public Mod Mod { get; set; }
    }
}