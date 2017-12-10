using System.Collections.Generic;
using Tornado.Parser.Data;

namespace Tornado.Parser.Entities {
    public static class ItemTypes {
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

        private static Core GetItemType(string itemBase) {
            if (PoeData.Weapons.ContainsKey(itemBase)) {
                WeaponBase w = PoeData.Weapons[itemBase];
                return new Core(w.Type, w);
            }

            if (Belts.Contains(itemBase))
                return new Core(ItemType.Belt);
            if (Amulets.Contains(itemBase))
                return new Core(ItemType.Amulet);
            if (Rings.Contains(itemBase))
                return new Core(ItemType.Ring);
            if (PoeData.Boots.ContainsKey(itemBase))
                return new Core(ItemType.Boots, PoeData.Boots[itemBase]);
            if (PoeData.Gloves.ContainsKey(itemBase))
                return new Core(ItemType.Gloves, PoeData.Gloves[itemBase]);
            if (PoeData.Armors.ContainsKey(itemBase))
                return new Core(ItemType.BodyArmour, PoeData.Armors[itemBase]);
            if (PoeData.Helmets.ContainsKey(itemBase))
                return new Core(ItemType.Helmet, PoeData.Helmets[itemBase]);
            if (PoeData.Shields.ContainsKey(itemBase))
                return new Core(ItemType.Shield, PoeData.Shields[itemBase]);
            if (Jewels.Contains(itemBase))
                return new Core(ItemType.Jewel);
            if (itemBase == "Gem")
                return new Core(ItemType.Gem);
            if (Quivers.Contains(itemBase))
                return new Core(ItemType.Quiver);
            if (itemBase.IndexOf("Map") > 0) {
                return new Core(ItemType.Map);
            }
            if (itemBase.IndexOf("Flask") > 0)
                return new Core(ItemType.Flask);

            return new Core(ItemType.Unknown);
        }

        private static Core GetMagicItemBase(string magicItemBase) {
            foreach (KeyValuePair<string, WeaponBase> pair in PoeData.Weapons) {
                if (magicItemBase.Contains(pair.Key)) {
                    return new Core(pair.Value.Type, pair.Value);
                }
            }
            foreach (var body in PoeData.Armors.Values) {
                if (magicItemBase.Contains(body.Name)) {
                    return new Core(ItemType.BodyArmour, PoeData.Armors[body.Name]);
                }
            }
            foreach (var boots in PoeData.Boots.Values) {
                if (magicItemBase.Contains(boots.Name)) {
                    return new Core(ItemType.Boots, PoeData.Boots[boots.Name]);
                }
            }
            foreach (var gloves in PoeData.Gloves.Values) {
                if (magicItemBase.Contains(gloves.Name)) {
                    return new Core(ItemType.Gloves, PoeData.Gloves[gloves.Name]);
                }
            }
            foreach (var helm in PoeData.Helmets.Values) {
                if (magicItemBase.Contains(helm.Name)) {
                    return new Core(ItemType.Helmet, PoeData.Helmets[helm.Name]);
                }
            }
            foreach (var amulet in Amulets) {
                if (magicItemBase.Contains(amulet)) {
                    return new Core(ItemType.Amulet);
                }
            }
            foreach (var ring in Rings) {
                if (magicItemBase.Contains(ring)) {
                    return new Core(ItemType.Ring);
                }
            }
            foreach (var belt in Belts) {
                if (magicItemBase.Contains(belt)) {
                    return new Core(ItemType.Belt);
                }
            }
            foreach (var shield in PoeData.Shields.Values) {
                if (magicItemBase.Contains(shield.Name)) {
                    return new Core(ItemType.Shield, PoeData.Shields[shield.Name]);
                }
            }
            foreach (var jewel in Jewels) {
                if (magicItemBase.Contains(jewel)) {
                    return new Core(ItemType.Jewel);
                }
            }
            if (magicItemBase.IndexOf("Flask") > 0) {
                return new Core(ItemType.Flask);
            }
            return new Core(ItemType.Unknown);
        }

        public static Core GetItemBase(string[] itemParams) {
            if (itemParams[0].Contains("Rare") || itemParams[0].Contains("Unique"))
                return GetItemType(itemParams[2]);
            if (itemParams[0].Contains("Magic") || itemParams[0].Contains("Normal")) {
                return GetMagicItemBase(itemParams[1]);
            }
            if (itemParams[0].Contains("Gem")) {
                return new Core(ItemType.Gem);
            }
            return new Core(ItemType.Unknown);
        }

        #region All item bases categorized
        public static List<string> Maps = new List<string>() {
            "Arcade",
            "Crystal Ore",
            "Desert",
            "Jungle Valley", //t1
            "Beach",
            "Factory",
            "Ghetto",
            "Oasis", //t2
            "Arid Lake",
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
            "Archnid Tomb",
            "Armoury",
            "Ashen Wood",
            "Castle Ruins",
            "Catacombs",
            "Cells",
            "Mud Geyser", //t7
            "Arachnid Nest",
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
            "Colonnade",
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
            "Sulphur Wastes",
            "Waterways", //t13
            "Maze",
            "Mineral Pools",
            "Palace",
            "Shrine",
            "Springs",
            "Volcano", //t14
            "Abyss",
            "Colosseum",
            "Core",
            "Dark Forest",
            "Overgrown Ruin", //t15
            "Forge of the Phoenix",
            "Lair of the Hydra",
            "Maze of the Minotaur",
            "Pit of the Chimera",
            "Vaal Temple", //t16
            "The Shaper's Realm" //t17 
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
            "Onyx Amulet"
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
            "Opal Ring"
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
            "Stygian Vise"
        };

        private static readonly List<string> Jewels = new List<string>() {
            "Cobalt Jewel",
            "Crimson Jewel",
            "Viridian Jewel"
        };
        #endregion
    }
}