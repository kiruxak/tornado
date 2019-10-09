using System.Collections.Generic;
using System.Linq;
using Tornado.Common.Utility;
using Tornado.Parser.Data;

namespace Tornado.Parser.Entities {
    public static class ItemTypes {
        public static string[] RareBases = new string[] {
            "Two-Toned Boots", "Cerulean Ring", "Vermillion Ring", "Convoking Wand", "Opal Ring", "Steel Ring", "Marble Amulet", "Blue Pearl Amulet",
            "Crystal Belt", "Stygian Vise", "Vanguard Belt", "Rustic Sash", "Spiked Gloves", "Fingerless Silk Gloves"
        };
        public static NiceDictionary<string, string> UniqueMaps = new NiceDictionary<string, string>() {
            { "Overgrown Shrine Map","Acton's Nightmare" },
            { "Underground River Map","Caer Blaidd, Wolfpack's Den" },
            { "Necropolis Map","Death and Taxes" },
            { "Maze Map","Doryani's Machinarium" },
            { "Promenade Map","Hall of Grandmasters" },
            { "Cemetery Map","Hallowed Ground" },
            { "Atoll Map","Maelström of Chaos" },
            { "Shore Map","Mao Kun" },
            { "Underground Sea Map","Oba's Cursed Trove" },
            { "Bone Crypt Map","Olmec's Sanctum" },
            { "Dunes Map","Pillars of Arun" },
            { "Temple Map","Poorjoy's Asylum" },
            { "Harbinger Map","The Beachhead" },
            { "Cursed Crypt Map","The Coward's Trial" },
            { "Chateau Map","The Perandus Manor" },
            { "Museum Map","The Putrid Cloister" },
            { "Moon Temple Map","The Twilight Temple" },
            { "Courtyard Map","The Vinktar Square" },
            { "Tropical Island Map","Untainted Paradise" },
            { "Vaal Pyramid Map","Vaults of Atziri" },
            { "Strand Map","Whakawairua Tuahu" }
        };

        public static bool IsWeapon(this ItemType type) {
            return type == ItemType.Bow ||
                   type == ItemType.Claw ||
                   type == ItemType.Axe ||
                   type == ItemType.Axe2H ||
                   type == ItemType.Mace ||
                   type == ItemType.Mace2H ||
                   type == ItemType.Sword ||
                   type == ItemType.Sword2H ||
                   type == ItemType.Sceptre ||
                   type == ItemType.Dagger ||
                   type == ItemType.Staff ||
                   type == ItemType.Wand;
        }

        public static bool Is1HWeapon(this ItemType type) {
            return type == ItemType.Claw ||
                   type == ItemType.Axe ||
                   type == ItemType.Mace ||
                   type == ItemType.Sword ||
                   type == ItemType.Sceptre ||
                   type == ItemType.Dagger ||
                   type == ItemType.Wand;
        }

        public static bool Is2HWeapon(this ItemType type) {
            return type == ItemType.Bow ||
                   type == ItemType.Axe2H ||
                   type == ItemType.Mace2H ||
                   type == ItemType.Sword2H ||
                   type == ItemType.Staff;
        }

        public static bool IsArmor(this ItemType type) {
            return type == ItemType.Boots ||
                   type == ItemType.Gloves ||
                   type == ItemType.BodyArmour ||
                   type == ItemType.Helmet ||
                   type == ItemType.Shield;
        }

        public static bool IsJewelery(this ItemType type) {
            return type == ItemType.Belt ||
                   type == ItemType.Amulet ||
                   type == ItemType.Ring;
        }

        public static bool IsSixLinkable(this ItemType itemType) {
            return itemType == ItemType.Bow ||
                   itemType == ItemType.Axe2H ||
                   itemType == ItemType.Mace2H ||
                   itemType == ItemType.Sword2H ||
                   itemType == ItemType.BodyArmour ||
                   itemType == ItemType.Staff;
        }

        private static Core GetItemType(string itemBase, Mark mark) {
            if (PoeData.Weapons.ContainsKey(itemBase)) {
                WeaponBase w = PoeData.Weapons[itemBase];
                return new Core(w.Type, mark, w);
            }

            if (Belts.Contains(itemBase))
                return new Core(ItemType.Belt, mark, Belts.First(x => x == itemBase));
            if (Amulets.Contains(itemBase))
                return new Core(ItemType.Amulet, mark, Amulets.First(x => x == itemBase));
            if (Rings.Contains(itemBase))
                return new Core(ItemType.Ring, mark, Rings.First(x => x == itemBase));
            if (PoeData.Boots.ContainsKey(itemBase))
                return new Core(ItemType.Boots, mark, PoeData.Boots[itemBase]);
            if (PoeData.Gloves.ContainsKey(itemBase))
                return new Core(ItemType.Gloves, mark, PoeData.Gloves[itemBase]);
            if (PoeData.Armors.ContainsKey(itemBase))
                return new Core(ItemType.BodyArmour, mark, PoeData.Armors[itemBase]);
            if (PoeData.Helmets.ContainsKey(itemBase))
                return new Core(ItemType.Helmet, mark, PoeData.Helmets[itemBase]);
            if (PoeData.Shields.ContainsKey(itemBase))
                return new Core(ItemType.Shield, mark, PoeData.Shields[itemBase]);
            if (Jewels.Contains(itemBase))
                return new Core(ItemType.Jewel, mark, Jewels.First(x => x == itemBase));
            if (AbyssalJewels.Contains(itemBase))
                return new Core(ItemType.AbyssalJewel, mark, AbyssalJewels.First(x => x == itemBase));
            if (itemBase == "Gem")
                return new Core(ItemType.Gem, mark, "Gem");
            if (Quivers.Contains(itemBase))
                return new Core(ItemType.Quiver, mark, Quivers.First(x => x == itemBase));
            if (itemBase.IndexOf("Map") > 0) {
                return new Core(ItemType.Map, mark, "Map");
            }
            if (itemBase.IndexOf("Flask") > 0)
                return new Core(ItemType.Flask, mark, "Flask");

            return new Core(ItemType.Unknown, mark, "Unknown");
        }

        private static Core GetMagicItemBase(string magicItemBase, Mark mark) {
            foreach (KeyValuePair<string, WeaponBase> pair in PoeData.Weapons) {
                if (magicItemBase.Contains(pair.Key)) {
                    return new Core(pair.Value.Type, mark, pair.Value);
                }
            }
            foreach (var body in PoeData.Armors.Values) {
                if (magicItemBase.Contains(body.Name)) {
                    return new Core(ItemType.BodyArmour, mark, PoeData.Armors[body.Name]);
                }
            }
            foreach (var boots in PoeData.Boots.Values) {
                if (magicItemBase.Contains(boots.Name)) {
                    return new Core(ItemType.Boots, mark, PoeData.Boots[boots.Name]);
                }
            }
            foreach (var gloves in PoeData.Gloves.Values) {
                if (magicItemBase.Contains(gloves.Name)) {
                    return new Core(ItemType.Gloves, mark, PoeData.Gloves[gloves.Name]);
                }
            }
            foreach (var helm in PoeData.Helmets.Values) {
                if (magicItemBase.Contains(helm.Name)) {
                    return new Core(ItemType.Helmet, mark, PoeData.Helmets[helm.Name]);
                }
            }
            foreach (var amulet in Amulets) {
                if (magicItemBase.Contains(amulet)) {
                    return new Core(ItemType.Amulet, mark, amulet);
                }
            }
            foreach (var ring in Rings) {
                if (magicItemBase.Contains(ring)) {
                    return new Core(ItemType.Ring, mark, ring);
                }
            }
            foreach (var quiver in Quivers) {
                if (magicItemBase.Contains(quiver)) {
                    return new Core(ItemType.Quiver, mark, quiver);
                }
            }
            foreach (var belt in Belts) {
                if (magicItemBase.Contains(belt)) {
                    return new Core(ItemType.Belt, mark, belt);
                }
            }
            foreach (var shield in PoeData.Shields.Values) {
                if (magicItemBase.Contains(shield.Name)) {
                    return new Core(ItemType.Shield, mark, PoeData.Shields[shield.Name]);
                }
            }
            foreach (var jewel in Jewels) {
                if (magicItemBase.Contains(jewel)) {
                    return new Core(ItemType.Jewel, mark, jewel);
                }
            }
            foreach (var jewel in AbyssalJewels) {
                if (magicItemBase.Contains(jewel)) {
                    return new Core(ItemType.AbyssalJewel, mark, jewel);
                }
            }
            if (magicItemBase.IndexOf("Flask") > 0) {
                return new Core(ItemType.Flask, mark, "Flask");
            }
            return new Core(ItemType.Unknown, mark, "Unknown");
        }

        public static Core GetItemBase(string[] itemParams) {
            Mark mark = Mark.Empty;
            if (itemParams.Any(x => x.Contains("Elder Item"))) mark = Mark.Elder;
            if (itemParams.Any(x => x.Contains("Shaper Item"))) mark = Mark.Shaper;
            if (itemParams.Any(x => x.Contains("Synthesised Item"))) mark = Mark.Synthesised;
            if (itemParams.Any(x => x.Contains("Fractured Item"))) mark = Mark.Fractured;
            
            if (itemParams[0].Contains("Rare") || itemParams[0].Contains("Unique"))
                return GetItemType(itemParams[2].Replace("<<set:MS>><<set:M>><<set:S>>", "").Replace("Synthesised ", ""), mark);
            if (itemParams[0].Contains("Magic") || itemParams[0].Contains("Normal")) {
                return GetMagicItemBase(itemParams[1].Replace("<<set:MS>><<set:M>><<set:S>>", "").Replace("Synthesised ", ""), mark);
            }
            if (itemParams[0].Contains("Gem")) {
                return new Core(ItemType.Gem, mark, "Gem");
            }
            return new Core(ItemType.Unknown, Mark.Empty, "Unknown");
        }

        #region All item bases categorized
        public static List<string> Maps = new List<string>() {
            "Pit of the Chimera",
            "Arcade",
            "Crystal Ore",
            "Desert",
            "Jungle Valley", //t1
            "Beach",
            "Factory",
            "Ghetto",
            "Oasis", //t2
            "Arid Lake",
            "Flooded Mine",
            "Cavern",
            "Channel",
            "Grotto",
            "Marshes",
            "Sewer",
            "Vaal Pyramid", //t3
            "Academy",
            "Acid Lakes",
            "Dungeon",
            "Graveyard",
            "Phantasmagoria",
            "Villa",
            "Waste Pool", //t4
            "Burial Chambers",
            "Bunes",
            "Mesa",
            "Peninsula",
            "Pit",
            "Spider Lair",
            "Tower", //t5
            "Canyon",
            "Quarry",
            "Racecourse",
            "Ramparts",
            "Spider Forest",
            "Strand",
            "Thicket",
            "Vaal City",
            "Wharf", //t6
            "Arachnid Tomb",
            "Armoury",
            "Ashen Wood",
            "Castle Ruins",
            "Catacombs",
            "Cells",
            "Mud Geyser", //t7
            "Arachnid Nest",
            "Primordial Pool",
            "Arena",
            "Atoll",
            "Barrows",
            "Bog",
            "Cemetery",
            "Pier",
            "Shore",
            "Tropical Island", //t8
            "Coves",
            "Crypt",
            "Museum",
            "Orchard",
            "Overgrown Shrine",
            "Promenade",
            "Reef",
            "Temple", //t9
            "Arsenal",
            "Ancient City",
            "Colonnade",
            "Leyline",
            "Courtyard",
            "Malformation",
            "Quay",
            "Terrace",
            "Underground River", //t10
            "Bazaar",
            "Chateau",
            "Excavation",
            "Precinct",
            "Torture Chamber",
            "Underground Sea",
            "Wasteland", //t11
            "Crematorium",
            "Estuary",
            "Ivory Temple",
            "Necropolis",
            "Plateau",
            "Residence",
            "Shipyard",
            "Vault", //t12
            "Beacon",
            "Gorge",
            "High Gardens",
            "Lair",
            "Plaza",
            "Scriptorium",
            "Defiled Cathedral",
            "Sulphur Wastes",
            "Waterways", //t13
            "Maze",
            "Mineral Pools",
            "Palace",
            "Shrine",
            "Springs",
            "Volcano", //t14
            "Abyss",
            "Carcass",
            "Colosseum",
            "Sunken City",
            "Courthouse",
            "Core",
            "Dark Forest",
            "Overgrown Ruin", //t15
            "Forge of the Phoenix",
            "Lair of the Hydra",
            "Maze of the Minotaur",
            "Vaal Temple", //t16
            "The Shaper's Realm", //t17 
            //3.5 new map
            "Fungal Hollow",
            "Acid Caverns",
            "Crater",
            "Glacier",
            "Primordial Blocks",
            //3.6
            "Caldera",
        };

        public static List<string> VaalFrarments = new List<string>() {
            "Sacrifice at Dawn",
            "Sacrifice at Midnight",
            "Sacrifice at Noon",
            "Sacrifice at Dusk",
            "Mortal Grief",
            "Mortal Rage",
            "Mortal Hope",
            "Mortal Ignorance"
        };

        public static List<string> GuardFrarments = new List<string>() {
            "Fragment of the Minotaur",
            "Fragment of the Chimera",
            "Fragment of the Hydra",
            "Fragment of the Phoenix"
        };

        public static List<string> ProphecyKeys = new List<string>() {
            "Eber's Key",
            "Inya's Key",
            "Yriel's Key",
            "Volkuur's Key"
        };

        public static List<string> IncursionVials = new List<string>() {
            "Vial of Awakening",
            "Vial of Consequence",
            "Vial of Dominance",
            "Vial of Fate",
            "Vial of Summoning",
            "Vial of the Ritual",
            "Vial of Transcendence",
            "Vial of the Ghost",
            "Vial of Sacrifice",
        };

        public static List<string> Quivers = new List<string>() {
            "Serrated Arrow Quiver",
            "Two-Point Arrow Quiver",
            "Sharktooth Arrow Quiver",
            "Blunt Arrow Quiver",
            "Fire Arrow Quiver",
            "Broadhead Arrow Quiver",
            "Penetrating Arrow Quiver",
            "Spike-Point Arrow Quiver"
        };

        public static List<string> Amulets = new List<string>() {
            "Jet Amulet",
            "Paua Amulet",
            "Coral Amulet",
            "Lapis Amulet",
            "Jade Amulet",
            "Amber Amulet",
            "Gold Amulet",
            "Turquoise Amulet",
            "Agate Amulet",
            "Citrine Amulet",
            "Onyx Amulet",
            "Marble Amulet",
            "Blue Pearl Amulet",
            "Prismatic Jewel"
        };

        public static List<string> Rings = new List<string>() {
            "Iron Ring",
            "Coral Ring",
            "Paua Ring",
            "Sapphire Ring",
            "Topaz Ring",
            "Ruby Ring",
            "Gold Ring",
            "Golden Hoop",
            "Jet Ring",
            "Two-Stone Ring",
            "Two-Stone Ring",
            "Two-Stone Ring",
            "Moonstone Ring",
            "Diamond Ring",
            "Prismatic Ring",
            "Amethyst Ring",
            "Unset Ring",
            "Breach Ring",
            "Steel Ring",
            "Opal Ring",
            "Cerulean Ring",
            "Vermillion Ring"
        };

        public static List<string> Belts = new List<string>() {
            "Chain Belt",
            "Rustic Sash",
            "Heavy Belt",
            "Leather Belt",
            "Golden Obi",
            "Cloth Belt",
            "Studded Belt",
            "Crystal Belt",
            "Stygian Vise",
            "Vanguard Belt"
        };

        private static readonly List<string> Jewels = new List<string>() {
            "Cobalt Jewel",
            "Crimson Jewel",
            "Viridian Jewel",
            };

        private static readonly List<string> AbyssalJewels = new List<string>() {
            "Ghastly Eye Jewel",
            "Murderous Eye Jewel",
            "Searching Eye Jewel",
            "Hypnotic Eye Jewel"
        };
        #endregion
    }
}