using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PoeParser.Common;
using PoeParser.Filter.Tooltip;
using Tornado.Common.Utility;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Filter;
using Tornado.Parser.Filter.Tooltip;
using Tornado.Parser.PoeTrade.Response;

namespace Tornado.Parser.Entities {
    public class UniqueItem : Item {
        public static string[] HasFatedVatiant = new string[] {
            "Araku Tiki", "Asenath's Mark", "Atziri's Mirror",
            "Blackgleam", "Blackheart", "Bloodboil", "Bramblejack", "Briskwrap",
            "Cameria's Maul", "Chalice of Horrors ", "Craghead", "Crown of Thorns", "Eclipse Solaris",
            "Death's Harp", "Deidbell", "Doedre's Tenure", "Doomfletch", "Dreadarc",
            "Dusktoe", "Ezomyte Peak", "Fencoil", "Foxshade", "Geofri's Baptism",
            "Goredrill", "Heatshiver", "Hrimnor's Hymn","Hrimsorrow","Hyrri's Bite",
            "Icetomb","Kaltenhalt","Kaom's Sign","Karui Ward","Limbsplit","Malachai's Simula",
            "Quecholli","Queen's Decree","Realmshaper","Redbeak","Reverberation Rod",
            "Shavronne's Pace","Silverbranch","Springleaf","Storm Cloud",
            "Sundance","The Dancing Dervish","The Ignomon","The Magnate",
            "The Screaming Eagle","The Stormheart","Timeclasp","Windscream","Wondertrap"
        };

        public override string CacheName => $"{Name}|{(UniquePriceSets.ContainsKey(PriceName) ? string.Join(",", UniquePriceSets[PriceName].Select(x => x.Value)) : "")}";
        public string UniqueType { get; set; }
        public bool IsFated { get; set; }
        public bool IsWatcherEye { get; set; }
        public List<string> WatcherEyeAffixes { get; set; } = new List<string>();
        public NiceDictionary<string, List<TradeAffix>> UniquePriceSets = new NiceDictionary<string, List<TradeAffix>>();

        public UniqueItem(string source, Core core, string name, string type) : base(source, ItemRarity.Unique, core, null, name, true) {
            UniqueType = type;
            Color = ToolTipColor.UniqueGroup;
            IsFated = HasFatedVatiant.Any(x => name.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) >= 0);
            IsWatcherEye = Name == "Watcher's Eye";

            var affixes = core.GetAffixes(this);
            foreach (IAffixValue affix in affixes) {
                Affixes.Add(affix.Name, affix);
            }

            UniqueCorrelations(source);
        }

        private TradeAffix GetUniqueAffix(string tooltip, string pattern, string poeAffix = null, bool isTotal = false) {
            var fixedPattern = pattern.StartsWith("+") ? pattern.Substring(1) : pattern;
            if (pattern.Contains("+(\\d+)")) {
                fixedPattern = fixedPattern.Replace("+(\\d+)", "\\+(\\d+)");
            }

            return new TradeAffix (this) {
                Tooltip      = tooltip,
                PoeTradeName = poeAffix ?? pattern.Replace("(\\d+)", "#"),
                Value        = pattern.Contains("(\\d+) to (\\d+)") ? Source.ParseDualAverage(fixedPattern) : Source.ParseSum(fixedPattern),
                IsTotal      = isTotal
            };
        }

        private void UniqueCorrelations(string source) {
            if (IsWatcherEye) {
                foreach (string affix in AllWatcherEyeAffixes) {
                    var pattern = affix.Replace("#", "([\\d.]+)");
                    pattern = pattern.StartsWith("+") ? "\\" + pattern : pattern;
                    if (source.ContainsPattern(pattern)) {
                        WatcherEyeAffixes.Add(affix);
                    }
                }
            }

            if (Source.Contains("Has 2 Abyssal Sockets")) { UniquePriceSets.Add(PriceName, new List<TradeAffix>() { new TradeAffix { Tooltip = "!Has 2 Abyssal Sockets", PoeTradeName = "Has # Abyssal Sockets", Value = 2, IsTotal = false } }); }
            if (Name == "Goldwyrm") { GetUniqueAffix($"% quantity", "(\\d+)% increased Quantity of Items found").AddToPriceSet(PriceName); }
            if (Name == "Sadima's Touch") { GetUniqueAffix($"% quantity", "(\\d+)% increased Quantity of Items found").AddToPriceSet(PriceName); }
            if (Name == "Witchfire Brew") { GetUniqueAffix($"% dot damage", "(\\d+)% increased Damage Over Time during Flask Effect").AddToPriceSet(PriceName); }
            if (Name == "Lavianga's Spirit") { GetUniqueAffix($"% more recovered", "(\\d+)% increased Amount Recovered").AddToPriceSet(PriceName); }
            if (Name == "Lion's Roar") { GetUniqueAffix($"% phys damage", "(\\d+)% more Melee Physical Damage during effect").AddToPriceSet(PriceName); }
            if (Name == "Zerphi's Last Breath") { GetUniqueAffix($"% last breath", "Grants Last Breath when you Use a Skill during Flask Effect, for (\\d+)% of Mana Cost").AddToPriceSet(PriceName); }
            if (Name == "The Long Winter") { GetUniqueAffix("% cold damage", "(\\d+)% increased Cold Damage").AddToPriceSet(PriceName); }
            if (Name == "Frozen Trail") { GetUniqueAffix("% proj damage", "(\\d+)% increased Projectile Damage").AddToPriceSet(PriceName); }
            if (Name == "Facebreaker") { GetUniqueAffix("% dmg", "(\\d+)% more Physical Damage with Unarmed Attacks").AddToPriceSet(PriceName); }
            if (Name == "The Vigil") { GetUniqueAffix("% armour", "(\\d+)% increased Armour").AddToPriceSet(PriceName); }
            if (Name == "Combat Focus") { GetUniqueAffix("% ele hit damage", "Elemental Hit deals (\\d+)% increased Damage").AddToPriceSet(PriceName); }
            if (Name == "Brute Force Solution" || Name == "Fertile Mind") { GetUniqueAffix("+ int", "+(\\d+) to Intelligence").AddToPriceSet(PriceName); }
            if (Name == "Slivertongue") { GetUniqueAffix("% crit fork", "(\\d+)% increased Critical Strike Chance with arrows that Fork").AddToPriceSet(PriceName); }
            if (Name == "The Poet's Pen") { GetUniqueAffix($"% attack speed", "(\\d+)% increased Attack Speed", "(pseudo) (total) #% increased Attack Speed", true) .AddToPriceSet("AttSpeed"); }
            if (Name == "Primordial Harmony") { GetUniqueAffix($"% golem damage per type", "(\\d+)% increased Golem Damage for each Type of Golem you have Summoned").AddToPriceSet(PriceName); }
            if (Name == "The Wise Oak") { GetUniqueAffix($"% elem penetrate", "During Flask Effect, Damage Penetrates (\\d+)% Resistance of each Element for which your Uncapped Elemental Resistance is highest").AddToPriceSet(PriceName); }
            if (Name == "The Tempest" || Name == "Storm Cloud") { GetUniqueAffix($"% attack speed", "(\\d+)% increased Attack Speed", "(pseudo) (total) #% increased Attack Speed", true).AddToPriceSet(PriceName); }
            if (Name == "Arborix") { GetUniqueAffix($"% evasion rating", "(\\d+)% increased Evasion Rating").AddToPriceSet(PriceName); }
            if (Name == "Careful Planning") { GetUniqueAffix($" dex", "+(\\d+) to Dexterity").AddToPriceSet(PriceName); }
            if (Name == "Wildfire") { GetUniqueAffix($" fire dmg", "(\\d+)% increased Fire Damage").AddToPriceSet(PriceName); }
            if (Name == "Dead Reckoning") { GetUniqueAffix($"% minion all res", "Minions have +(\\d+)% to all Elemental Resistances").AddToPriceSet(PriceName); }
            if (Name == "Kaom's Heart") { GetUniqueAffix($"% fire damage", "(\\d+)% increased Fire Damage").AddToPriceSet(PriceName); }
            if (Name == "Shade of Solaris") { GetUniqueAffix($"% elem as extra chaos", "Gain (\\d+)% of Elemental Damage as Extra Chaos Damage").AddToPriceSet(PriceName); }
            if (Name == "Disintegrator") { GetUniqueAffix($"+ phys spell", "Adds (\\d+) to (\\d+) Physical Damage to Spells").AddToPriceSet("Spell"); }
            if (Name == "Breath of the Council") { GetUniqueAffix($"+ chaos dmg", "(\\d+)% increased Chaos Damage").AddToPriceSet("Spell"); }
            if (Name == "Shaper's Touch") { GetUniqueAffix($"% armor and es", "(\\d+)% increased Armour and Energy Shield").AddToPriceSet(PriceName); }
            if (Name == "Call of the Brotherhood") { GetUniqueAffix($"% lightning damage", "(\\d+)% increased Lightning Damage").AddToPriceSet(PriceName); }
            if (Name == "Dream Fragments") { GetUniqueAffix($"% cold res", "+(\\d+)% to Cold Resistance", "(pseudo) +#% total Resistance", true).AddToPriceSet(PriceName); }
            if (Name == "Andvarius") { GetUniqueAffix($"% rarity", "(\\d+)% increased Rarity of Items found", "(pseudo) (total) #% increased Rarity of Items found", true).AddToPriceSet(PriceName); }
            if (Name == "Singularity") { GetUniqueAffix($"% dmg against hindered", "(\\d+)% increased Damage with Hits and Ailments against Hindered Enemies").AddToPriceSet(PriceName); }
            if (Name == "Doedre's Damning") { GetUniqueAffix($" int", "+(\\d+) to Intelligence").AddToPriceSet(PriceName); }
            if (Name == "Spreading Rot") { GetUniqueAffix($" chaos dmg", "(\\d+)% increased Chaos Damage").AddToPriceSet(PriceName); }
            if (Name == "Cloak of Tawm'r Isley") { GetUniqueAffix($"% es", "(\\d+)% increased Energy Shield").AddToPriceSet(PriceName); }
            if (Name == "Kikazaru") { GetUniqueAffix($"% mana reg", "(\\d+)% increased Mana Regeneration Rate").AddToPriceSet(PriceName); }
            if (Name == "Coralito's Signature") { GetUniqueAffix($"% poison duration", "(\\d+)% increased Duration of Poisons you inflict during Flask effect").AddToPriceSet(PriceName); }
            if (Name == "Clayshaper") { GetUniqueAffix($"% minion hp", "Minions have (\\d+)% increased maximum Life").AddToPriceSet(PriceName); }

            if (Name == "Ventor's Gamble") {
                GetUniqueAffix($"% quantity", "(\\d+)% increased Quantity of Items found").AddToPriceSet(PriceName).AddToPriceSet("Quantity");
                GetUniqueAffix($"% quantity", "(\\d+)% reduced Quantity of Items found").AddToPriceSet(PriceName);

                GetUniqueAffix($" life", "+(\\d+) to maximum Life").AddToPriceSet(PriceName).AddToPriceSet("Life");
                var res = GetUniqueAffix($"% fire res", "(\\d+)% to Fire Resistance").Value + 
                          GetUniqueAffix($"% cold red", "(\\d+)% to Cold Resistance").Value +
                          GetUniqueAffix($"% lightning res", "(\\d+)% to Lightning Resistance").Value;
                var resAff = new TradeAffix { Tooltip = "+% allres", PoeTradeName = "(pseudo) +#% total Resistance", Value = res, IsTotal = true };
                UniquePriceSets[PriceName].Add(resAff);
                UniquePriceSets["Life"].Add(resAff);
            }
            if (Name == "Impresence") {
                if (Source.Contains("Vulnerability has 100% reduced Mana Reservation")) {
                    GetUniqueAffix(" Vulnerability", "Vulnerability has (\\d+)% reduced Mana Reservation").AddToPriceSet(PriceName);
                }
                if (Source.Contains("Despair has 100% reduced Mana Reservation")) {
                    GetUniqueAffix(" Despair", "Despair has (\\d+)% reduced Mana Reservation").AddToPriceSet(PriceName); 
                }
                if (Source.Contains("Frostbite has 100% reduced Mana Reservation")) {
                    GetUniqueAffix(" Frostbite", "Frostbite has (\\d+)% reduced Mana Reservation").AddToPriceSet(PriceName);
                }
                if (Source.Contains("Flammability has 100% reduced Mana Reservation")) {
                    GetUniqueAffix(" Flammability", "Flammability has (\\d+)% reduced Mana Reservation").AddToPriceSet(PriceName);
                }
                if (Source.Contains("Conductivity has 100% reduced Mana Reservation")) {
                    GetUniqueAffix(" Conductivity", "Conductivity has (\\d+)% reduced Mana Reservation").AddToPriceSet(PriceName);
                }
            }
            if (Name == "Thief's Torment") {
                UniquePriceSets.Add(PriceName, new List<TradeAffix>() { new TradeAffix { Tooltip = "% allres", PoeTradeName = "#% to all Elemental Resistances", Value = Source.ParseSum("(\\d+)% to all Elemental Resistances"), IsTotal = false } });
                GetUniqueAffix($"% quantity", "(\\d+)% increased Quantity of Items found").AddToPriceSet(PriceName);
            }
            if (Name == "Shroud of the Lightless") {
                GetUniqueAffix("% eva and es", "(\\d+)% increased Evasion and Energy Shield").AddToPriceSet(PriceName);
                GetUniqueAffix($"% life", "(\\d+)% increased maximum Life").AddToPriceSet(PriceName).AddToPriceSet("Life");
            }
            if (Name == "Cloak of Defiance") {
                GetUniqueAffix("% eva and es", "(\\d+)% increased Evasion and Energy Shield").AddToPriceSet(PriceName);
                GetUniqueAffix($"% mana", "+(\\d+) to maximum Mana").AddToPriceSet(PriceName).AddToPriceSet("Mana");
            }
            if (Name == "Cherrubim's Maleficence") {
                GetUniqueAffix("% chaos dmg", "(\\d+)% increased Chaos Damage").AddToPriceSet(PriceName).AddToPriceSet("ChaosDmg");
                GetUniqueAffix($"% life", "+(\\d+) to maximum Life").AddToPriceSet(PriceName).AddToPriceSet("Life");
            }
            if (Name == "Allelopathy") {
                GetUniqueAffix("% dot dmg", "(\\d+)% increased Damage over Time").AddToPriceSet(PriceName).AddToPriceSet("DotDmg");
                GetUniqueAffix($"% es", "(\\d+)% increased Energy Shield").AddToPriceSet(PriceName).AddToPriceSet("Es");
            }
            if (Name == "Carnage Heart") {
                UniquePriceSets.Add(PriceName, new List<TradeAffix>() { new TradeAffix { Tooltip = "+ all attr", PoeTradeName = "(pseudo) (total) +# to all Attributes", Value = Source.ParseSum("(\\d+) to all Attributes"), IsTotal = true } });
                UniquePriceSets.Add("Leech&Res&Attr", new List<TradeAffix>() { new TradeAffix { Tooltip = "+ all attr", PoeTradeName = "(pseudo) (total) +# to all Attributes", Value = Source.ParseSum("(\\d+) to all Attributes"), IsTotal = true } });
                GetUniqueAffix("% all res", "+(\\d+)% to all Elemental Resistances").AddToPriceSet(PriceName).AddToPriceSet("Leech&Res").AddToPriceSet("Leech&Res&Dmg").AddToPriceSet("Leech&Res&Attr");
                GetUniqueAffix($"% leech", "([\\d.]+)% of Physical Attack Damage Leeched as Life", "#% of Physical Attack Damage Leeched as Life").AddToPriceSet(PriceName).AddToPriceSet("Leech&Res").AddToPriceSet("Leech&Res&Dmg").AddToPriceSet("Leech&Res&Attr"); 
                GetUniqueAffix($"% dmg while leech", "(\\d+)% increased Damage while Leeching").AddToPriceSet(PriceName).AddToPriceSet("Leech&Res&Dmg");
            }
            if (Name == "Le Heup of All") {
                GetUniqueAffix(" all attr", "+(\\d+) to all Attributes").AddToPriceSet(PriceName).AddToPriceSet("Dmg&Res&Attr");
                GetUniqueAffix("% dmg", "(\\d+)% increased Damage").AddToPriceSet(PriceName).AddToPriceSet("Dmg&Res").AddToPriceSet("Dmg&Res&Rarity").AddToPriceSet("Dmg&Res&Attr");
                GetUniqueAffix("% all res", "+(\\d+)% to all Elemental Resistances").AddToPriceSet(PriceName).AddToPriceSet("Dmg&Res").AddToPriceSet("Dmg&Res&Rarity").AddToPriceSet("Dmg&Res&Attr");
                GetUniqueAffix("% rarity", "(\\d+)% increased Rarity of Items found").AddToPriceSet(PriceName).AddToPriceSet("Dmg&Res&Rarity");
            }
            if (Name == "Belt of the Deceiver") {
                GetUniqueAffix("% phys", "(\\d+)% increased Global Physical Damage").AddToPriceSet(PriceName).AddToPriceSet("Life&Phys");
                GetUniqueAffix($" life", "+(\\d+) to maximum Life").AddToPriceSet(PriceName).AddToPriceSet("Life&Phys").AddToPriceSet("Life&Res");
                GetUniqueAffix($" res", "+(\\d+)% to all Elemental Resistances").AddToPriceSet(PriceName).AddToPriceSet("Life&Res");
            }
            if (Name == "Saqawal's Flock") {
                GetUniqueAffix("% res", "+(\\d+)% to Lightning Resistance").AddToPriceSet(PriceName);
                GetUniqueAffix("% ev", "(\\d+)% increased Evasion Rating").AddToPriceSet(PriceName);
                GetUniqueAffix($" life", "+(\\d+) to maximum Life").AddToPriceSet(PriceName).AddToPriceSet("Life&Speed").AddToPriceSet("Life");
                GetUniqueAffix($" move speed", "(\\d+)% increased Movement Speed").AddToPriceSet(PriceName).AddToPriceSet("Life&Speed");
            }
            if (Name == "Soul Catcher") {
                GetUniqueAffix("% dmg", "Vaal Skills deal (\\d+)% increased Damage during effect").AddToPriceSet(PriceName);
                GetUniqueAffix($"% reduced soul gain", "Vaal Skills used during effect have (\\d+)% reduced Soul Gain Prevention Duration").AddToPriceSet(PriceName);
            }
            if (Name == "Aegis Aurora") {
                GetUniqueAffix("% ar and es", "(\\d+)% increased Armour and Energy Shield").AddToPriceSet(PriceName).AddToPriceSet("Es");
                GetUniqueAffix($"% wep", "(\\d+)% increased Elemental Damage with Attack Skills").AddToPriceSet(PriceName);
            }
            if (Name == "Shavronne's Wrappings") {
                GetUniqueAffix("% es", "(\\d+)% increased Energy Shield").AddToPriceSet(PriceName).AddToPriceSet("Es");
                GetUniqueAffix($"% light res", "+(\\d+)% to Lightning Resistance").AddToPriceSet(PriceName);
            }
            if (Name == "Vixen's Entrapment") {
                GetUniqueAffix(" es", "+(\\d+) to maximum Energy Shield").AddToPriceSet(PriceName).AddToPriceSet("Es");
                GetUniqueAffix($"% curse cast speed", "Curse Skills have (\\d+)% increased Cast Speed").AddToPriceSet(PriceName);
            }
            if (Name == "Doedre's Scorn") {
                GetUniqueAffix("% damage per curse", "(\\d+)% increased Damage with Hits and Ailments per Curse on Enemy").AddToPriceSet(PriceName).AddToPriceSet("Curse");
                GetUniqueAffix("% curse duration", "Curse Skills have (\\d+)% increased Skill Effect Duration").AddToPriceSet(PriceName).AddToPriceSet("Curse");
                GetUniqueAffix($" es", "+(\\d+) to maximum Energy Shield").AddToPriceSet(PriceName).AddToPriceSet("Es");
            }
            if (Name == "Demon Stitcher") {
                GetUniqueAffix("% cast speed", "(\\d+)% increased Cast Speed").AddToPriceSet(PriceName).AddToPriceSet("Life").AddToPriceSet("Es");
                GetUniqueAffix(" life", "+(\\d+) to maximum Life").AddToPriceSet(PriceName).AddToPriceSet("Life");
                GetUniqueAffix(" es", "+(\\d+) to maximum Energy Shield").AddToPriceSet(PriceName).AddToPriceSet("Es");
            }
            if (Name == "Xirgil's Crank") {
                GetUniqueAffix("% spell dmg", "(\\d+)% increased Spell Damage").AddToPriceSet(PriceName).AddToPriceSet("Spell");
                GetUniqueAffix(" es", "+(\\d+) to maximum Energy Shield").AddToPriceSet(PriceName).AddToPriceSet("Es");
            }
            if (Name == "Bisco's Collar") {
                GetUniqueAffix("% rarity", "(\\d+)% increased Rarity of Items Dropped by Slain Magic Enemies").AddToPriceSet(PriceName);
                GetUniqueAffix("% quantity", "(\\d+)% increased Quantity of Items Dropped by Slain Normal Enemies").AddToPriceSet(PriceName);
            }
            if (Name == "Belly of the Beast") {
                GetUniqueAffix($"% life", "(\\d+)% increased maximum Life").AddToPriceSet(PriceName);
                GetUniqueAffix($"% all res", "(\\d+)% to all Elemental Resistances").AddToPriceSet(PriceName);
            }
            if (Name == "Rumi's Concoction") {
                GetUniqueAffix($"% spell block", "+(\\d+)% Chance to Block Spell Damage during Flask effect").AddToPriceSet(PriceName);
                GetUniqueAffix($"% attack block", "+(\\d+)% Chance to Block Attack Damage during Flask effect").AddToPriceSet(PriceName);
            }
            if (Name == "Alpha's Howl") {
                GetUniqueAffix($"% ev", "(\\d+)% increased Evasion Rating").AddToPriceSet(PriceName);
                GetUniqueAffix($"% cold res", "+(\\d+)% to Cold Resistance").AddToPriceSet(PriceName);
            }
            if (Name == "Goldrim") {
                GetUniqueAffix($" ev", "+(\\d+) to Evasion Rating").AddToPriceSet(PriceName);
                GetUniqueAffix($"% all res", "+(\\d+)% to all Elemental Resistances").AddToPriceSet(PriceName).AddToPriceSet("Res");
            }
            if (Name == "Ming's Heart") {
                GetUniqueAffix($"% chaos res", "+(\\d+)% to Chaos Resistance", "(pseudo) +#% total Resistance", true).AddToPriceSet(PriceName).AddToPriceSet("Life").AddToPriceSet("Es");
                GetUniqueAffix($"% reduced life", "(\\d+)% reduced maximum Life").AddToPriceSet(PriceName).AddToPriceSet("Life");
                GetUniqueAffix($"% reduced es", "(\\d+)% reduced maximum Energy Shield").AddToPriceSet(PriceName).AddToPriceSet("Es");
            }
            if (Name == "Piscator's Vigil") {
                GetUniqueAffix($"% att speed", "(\\d+)% increased Attack Speed").AddToPriceSet(PriceName);
                GetUniqueAffix($"% wep", "Attacks with this Weapon have (\\d+)% increased Elemental Damage").AddToPriceSet(PriceName);
            }
            if (Name == "Lochtonial Caress") {
                GetUniqueAffix($"% att speed", "(\\d+)% increased Attack Speed").AddToPriceSet(PriceName).AddToPriceSet("LifeAtt");
                GetUniqueAffix($"% cast speed", "(\\d+)% increased Cast Speed").AddToPriceSet(PriceName).AddToPriceSet("LifeCast");
                GetUniqueAffix($" life", "+(\\d+) to maximum Life").AddToPriceSet(PriceName).AddToPriceSet("LifeAtt").AddToPriceSet("LifeCast");
                GetUniqueAffix($"% reduced mana", "(\\d+)% reduced maximum Mana").AddToPriceSet(PriceName);
            }
            if (Name == "Atziri's Promise") {
                GetUniqueAffix($"% phys as extra chaos", "Gain (\\d+)% of Physical Damage as Extra Chaos Damage during effect").AddToPriceSet(PriceName).AddToPriceSet("Phys");
                GetUniqueAffix($"% elem as extra chaos", "Gain (\\d+)% of Elemental Damage as Extra Chaos Damage during effect").AddToPriceSet(PriceName).AddToPriceSet("Elem");
            }
            if (Name == "Queen of the Forest") {
                UniquePriceSets.Add(PriceName, new List<TradeAffix>() {
                    new TradeAffix { Tooltip = "+ life", PoeTradeName    = "(pseudo) (total) +# to maximum Life", Value = Affixes.First(x => x.Key == "mLife").Value.Value, IsTotal  = true },
                    new TradeAffix { Tooltip = "+% allres", PoeTradeName = "(pseudo) +#% total Resistance", Value       = Affixes.First(x => x.Key == "allres").Value.Value, IsTotal = true }
                });
            }
            if (Name == "Starkonja's Head") {
                GetUniqueAffix($"% ev rating", "(\\d+)% increased Evasion Rating").AddToPriceSet(PriceName);
                GetUniqueAffix($" life", "+(\\d+) to maximum Life").AddToPriceSet(PriceName).AddToPriceSet("Life");
            }
            if (Name == "Shadows and Dust") {
                GetUniqueAffix($"% crit", "(\\d+)% increased Global Critical Strike Chance").AddToPriceSet(PriceName).AddToPriceSet("Dmg");
                GetUniqueAffix($"% crit dmg", "+(\\d+)% to Global Critical Strike Multiplier").AddToPriceSet(PriceName).AddToPriceSet("Dmg");
                GetUniqueAffix($" es", "(\\d+)% increased Evasion and Energy Shield").AddToPriceSet(PriceName).AddToPriceSet("ES");
            }
            if (Name == "Vulconus") {
                GetUniqueAffix($" fire dmg", "(\\d+)% increased Fire Damage with Hits and Ailments against Bleeding Enemies").AddToPriceSet(PriceName).AddToPriceSet("Fire");
                GetUniqueAffix($" phys dmg", "(\\d+)% increased Physical Damage with Hits and Ailments against Ignited Enemies").AddToPriceSet(PriceName).AddToPriceSet("Phys");
                GetUniqueAffix($" crit", "(\\d+)% increased Critical Strike Chance while you have Avatar of Fire").AddToPriceSet(PriceName).AddToPriceSet("Fire").AddToPriceSet("Phys");
            }
            if (Name == "Sin Trek") {
                GetUniqueAffix($" dex", "+(\\d+) to Dexterity").AddToPriceSet(PriceName);
                GetUniqueAffix($" int", "+(\\d+) to Intelligence").AddToPriceSet(PriceName);
                GetUniqueAffix($"% ev rating", "(\\d+)% increased Evasion Rating").AddToPriceSet(PriceName).AddToPriceSet("ES+EV").AddToPriceSet("EV");
                GetUniqueAffix($" es", "+(\\d+) to maximum Energy Shield").AddToPriceSet(PriceName).AddToPriceSet("ES+EV").AddToPriceSet("ES");
            }
            if (Name == "Rat's Nest") {
                GetUniqueAffix($"% crit", "(\\d+)% increased Global Critical Strike Chance").AddToPriceSet(PriceName).AddToPriceSet("Crit");
                GetUniqueAffix($"% rarity", "(\\d+)% increased Rarity of Items found").AddToPriceSet(PriceName);
            }
            if (Name == "Drillneck") {
                GetUniqueAffix($"% att speed", "(\\d+)% increased Attack Speed").AddToPriceSet(PriceName).AddToPriceSet("Damage");
                GetUniqueAffix($" phys damage", "Adds (\\d+) to (\\d+) Physical Damage to Bow Attacks").AddToPriceSet(PriceName).AddToPriceSet("Damage");
                GetUniqueAffix($" life", "+(\\d+) to maximum Life").AddToPriceSet(PriceName).AddToPriceSet("Life");
            }
            if (Name == "Perandus Signet") {
                GetUniqueAffix($" mana", "+(\\d+) to maximum Mana", "(pseudo) (total) +# to maximum Mana", true).AddToPriceSet(PriceName);
                GetUniqueAffix($"% mana reg", "(\\d+)% increased Mana Regeneration Rate").AddToPriceSet(PriceName);
            }
            if (Name == "Mark of the Shaper") {
                GetUniqueAffix($"% spell damage", "(\\d+)% increased Spell Damage if your other Ring is an Elder Item").AddToPriceSet(PriceName).AddToPriceSet("Spell").AddToPriceSet("ES+Spell").AddToPriceSet("Life+Spell");
                GetUniqueAffix($"% es", "(\\d+)% increased maximum Energy Shield").AddToPriceSet(PriceName).AddToPriceSet("ES+Spell");
                GetUniqueAffix($"% life", "(\\d+)% increased maximum Life").AddToPriceSet(PriceName).AddToPriceSet("Life+Spell");
            }
            if (Name == "Vessel of Vinktar") {
                GetUniqueAffix($" lightning attack", "Adds (\\d+) to (\\d+) Lightning Damage to Attacks during Flask effect").AddToPriceSet(PriceName);
                GetUniqueAffix($" lightning spell", "Adds (\\d+) to (\\d+) Lightning Damage to Spells during Flask effect").AddToPriceSet(PriceName);
                GetUniqueAffix($"% lightning penetrate", "Damage Penetrates (\\d+)% Lightning Resistance during Flask effect").AddToPriceSet(PriceName);
            }
            if (Name == "Carcass Jack") {
                GetUniqueAffix($"% area damage", "(\\d+)% increased Area Damage").AddToPriceSet(PriceName).AddToPriceSet("Area");
                GetUniqueAffix($"% area", "(\\d+)% increased Area of Effect").AddToPriceSet(PriceName).AddToPriceSet("Area");
                GetUniqueAffix($"% all res", "+(\\d+)% to all Elemental Resistances").AddToPriceSet(PriceName).AddToPriceSet("Life+Res");
                GetUniqueAffix($" life", "+(\\d+) to maximum Life").AddToPriceSet(PriceName).AddToPriceSet("Life+Res");
            }

            if (Name == "Marohi Erqi" || Name == "Mark of the Doubting Knight") { Affixes.Add("wepAttSpeed", new AffixValue(new AffixTier() { }, new Affix(), -10, false, false)); UpdateTotalAffixes(); }
            if (Name == "Gorebreaker") { Affixes.Add("wepAttSpeed", new AffixValue(new AffixTier() { }, new Affix(), -20, false, false)); UpdateTotalAffixes(); }
        }

        private string GetFixedName() {
            // if (Name.Contains("Piece") && UniqueType.Contains("Piece")) {
            //     return HttpUtility.UrlEncode(Name);
            // }
            return HttpUtility.UrlEncode(Name) + " " + UniqueType;
        }

        public override NiceDictionary<string, string> GetPoeTradeAffixes(FilterResult filter) {
            NiceDictionary<string, string> generalParams = Core.GetPoeTradeAffixes(filter);
            generalParams.Add("name", GetFixedName());
            generalParams.Add("rarity", "unique");
            generalParams.Add("corrupted", Corrupted ? "1" : "0");
            if (Type == ItemType.Flask) {
                generalParams.Add("q_min", Quality.ToString());
            }
            generalParams.Remove("type");
                
            if (WhiteSockets > 0) { generalParams.Add("sockets_w", WhiteSockets.ToString()); }
            if (Name == "Skin of the Loyal") {
                generalParams.Add("sockets_r", SocketLine.Count(c => c == 'R').ToString());
                generalParams.Add("sockets_g", SocketLine.Count(c => c == 'G').ToString());
                generalParams.Add("sockets_b", SocketLine.Count(c => c == 'B').ToString());
            }

            return generalParams;
        }

        public override List<ColoredLine> GetTooltip(FilterResult filter) {
            var baseResult = base.GetTooltip(filter);
            var size = 16;
            var color = ToolTipColor.Gem;
            if (UniquePriceSets.ContainsKey(PriceName)) {
                foreach (TradeAffix affix in UniquePriceSets[PriceName]) {
                    var textOnly = affix.Tooltip.StartsWith("!");
                    if (textOnly) {
                        var tooltip = affix.Tooltip.Replace("!", "");
                        baseResult.Add(new ColoredLine(tooltip, color, size));
                    } else {
                        var prefix = affix.Tooltip.Contains("%") ? "%" : "";
                        var starts = affix.Tooltip.StartsWith("+") ? "+" : "";
                        var tooltip = affix.Tooltip.Replace("%", "").Replace("+", "");
                        baseResult.Add(new ColoredLine(new List<ColoredText>()
                                                       .Add(starts + affix.Value.ToString("0.#") + prefix, ToolTipColor.GeneralGroup, size + 1)
                                                       .Add(' ' + tooltip, color, size)));
                    }
                }
            }
            if (IsFated) {
                baseResult.Add(new ColoredLine("Can be Fated", ToolTipColor.Gem, 18));
            }

            return baseResult;
        }

        public override string ToString() {
            return $"{EnumInfo<ItemType>.Current[Type].DisplayName}: {CacheName} - {Price}";
        }

        public List<string> AllWatcherEyeAffixes = new List<string> {
            "#% of Fire Damage Leeched as Life while affected by Anger",
            "Damage Penetrates #% Fire Resistance while affected by Anger",
            "+#% to Critical Strike Multiplier while affected by Anger",
            "#% increased Fire Damage while affected by Anger",
            "Gain #% of Physical Damage as Extra Fire Damage while affected by Anger",
            "#% of Physical Damage Converted to Fire Damage while affected by Anger",
            "#% of Damage taken from Mana before Life while affected by Clarity",
            "#% of Damage taken gained as Mana over 4 seconds when Hit while affected by Clarity",
            "Gain #% of Maximum Mana as Extra Maximum Energy Shield while affected by Clarity",
            "#% increased Mana Recovery Rate while affected by Clarity",
            "#% chance to Recover 10% of Maximum Mana when you use a Skill while affected by Clarity",
            "-# to Total Mana Cost of Skills while affected by Clarity",
            "+# to Armour while affected by Determination",
            "+#% Chance to Block Attack Damage while affected by Determination",
            "#% additional Physical Damage Reduction while affected by Determination",
            "You take #% reduced Extra Damage from Critical Strikes while affected by Determination",
            "#% reduced Reflected Physical Damage taken while affected by Determination",
            "Unaffected by Vulnerability while affected by Determination",
            "+#% Chance to Block Spell Damage while affected by Discipline",
            "+# Energy Shield gained for each Enemy Hit while affected by Discipline",
            "#% increased Energy Shield Recovery Rate while affected by Discipline",
            "#% of Maximum Energy Shield Regenerated per Second while affected by Discipline",
            "#% faster start of Energy Shield Recharge while affected by Discipline",
            "+#% chance to Evade Attacks while affected by Grace",
            "#% chance to Blind Enemies which Hit you while affected by Grace",
            "#% chance to Dodge Attack Hits while affected by Grace",
            "#% increased Movement Speed while affected by Grace",
            "Unaffected by Enfeeble while affected by Grace",
            "#% chance to Dodge Spell Hits while affected by Haste",
            "#% increased cooldown recovery speed of Movement Skills used while affected by Haste",
            "Debuffs on you expire #% faster while affected by Haste",
            "You gain Onslaught for 4 seconds on Kill while affected by Haste",
            "You have Phasing while affected by Haste",
            "Unaffected by Temporal Chains while affected by Haste",
            "Adds # to # Cold Damage while affected by Hatred",
            "+#% to Critical Strike Chance while affected by Hatred",
            "Damage Penetrates #% Cold Resistance while affected by Hatred",
            "#% increased Cold Damage while affected by Hatred",
            "#% of Physical Damage Converted to Cold Damage while affected by Hatred",
            "+#% to Chaos Resistance while affected by Purity of Elements",
            "#% reduced Reflected Elemental Damage taken while affected by Purity of Elements",
            "#% of Physical Damage from Hits taken as Cold Damage while affected by Purity of Elements",
            "#% of Physical Damage from Hits taken as Fire Damage while affected by Purity of Elements",
            "#% of Physical Damage from Hits taken as Lightning Damage while affected by Purity of Elements",
            "Unaffected by Elemental Weakness while affected by Purity of Elements",
            "Immune to Ignite while affected by Purity of Fire",
            "#% reduced Reflected Fire Damage taken while affected by Purity of Fire",
            "#% of Physical Damage from Hits taken as Fire Damage while affected by Purity of Fire",
            "Unaffected by Burning Ground while affected by Purity of Fire",
            "Unaffected by Flammability while affected by Purity of Fire",
            "Immune to Freeze while affected by Purity of Ice",
            "#% reduced Reflected Cold Damage taken while affected by Purity of Ice",
            "#% of Physical Damage from Hits taken as Cold Damage while affected by Purity of Ice",
            "Unaffected by Chilled Ground while affected by Purity of Ice",
            "Unaffected by Frostbite while affected by Purity of Ice",
            "Immune to Shock while affected by Purity of Lightning",
            "#% reduced Reflected Lightning Damage taken while affected by Purity of Lightning",
            "#% of Physical Damage from Hits taken as Lightning Damage while affected by Purity of Lightning",
            "Unaffected by Conductivity while affected by Purity of Lightning",
            "Unaffected by Shocked Ground while affected by Purity of Lightning",
            "#% of Damage leeched as Life while affected by Vitality",
            "# Life Regenerated per Second while affected by Vitality",
            "+# Life gained for each Enemy Hit while affected by Vitality",
            "#% increased Life Recovery from Flasks while affected by Vitality",
            "#% increased Life Recovery Rate while affected by Vitality",
            "#% increased Critical Strike Chance while affected by Wrath",
            "#% increased Lightning Damage while affected by Wrath",
            "#% of Lightning Damage is Leeched as Mana while affected by Wrath",
            "Damage Penetrates #% Lightning Resistance while affected by Wrath",
            "Gain #% of Physical Damage as Extra Lightning Damage while affected by Wrath",
            "#% of Physical Damage Converted to Lightning Damage while affected by Wrath",
            "#% increased Cast Speed while affected by Zealotry",
            "Effects of Consecrated Ground you create while affected by Zealotry Linger for 2 seconds",
            "Consecrated Ground you create while affected by Zealotry causes enemies to take #% increased Damage",
            "#% increased Critical Strike Chance against Enemies on Consecrated Ground while affected by Zealotry",
            "Critical Strikes Penetrate #% of Enemy Elemental Resistances while affected by Zealotry",
            "Gain Arcane Surge for 4 seconds when you create Consecrated Ground while affected by Zealotry",
            "#% increased Maximum total Recovery per second from Energy Shield Leech while affected by Zealotry",
            "+#% to Non-Ailment Chaos Damage over Time Multiplier while affected by Malevolence",
            "+#% to Cold Damage over Time Multiplier while affected by Malevolence",
            "#% increased Recovery rate of Life and Energy Shield while affected by Malevolence",
            "#% increased Skill Effect Duration while affected by Malevolence",
            "Unaffected by Bleeding while affected by Malevolence",
            "Unaffected by Poison while affected by Malevolence",
            "Damaging Ailments you inflict deal Damage #% faster while affected by Malevolence",
            "+#% to Critical Strike Multiplier while affected by Precision",
            "#% increased Attack Damage while affected by Precision",
            "Gain a Flask Charge when you deal a Critical Strike while affected by Precision",
            "#% increased Attack Speed while affected by Precision",
            "Cannot be Blinded while affected by Precision",
            "#% chance to deal Double Damage while using Pride",
            "Your Hits Intimidate Enemies for 4 seconds while you are using Pride",
            "#% increased Attack Physical Damage while using Pride",
            "#% chance to Impale Enemies on Hit with Attacks while using Pride",
            "Impales you inflict last # additional Hits while using Pride",
        };
    }
}
