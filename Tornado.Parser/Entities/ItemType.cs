using System.ComponentModel;

namespace Tornado.Parser.Entities {
    public enum ItemType {
        Unknown = 0,
        Wand = 8,
        Dagger = 7,
        Claw = 6, //1h
        Staff = 14,
        Bow = 13, //2h
        [Description("One Hand Axe")]
        Axe = 11,
        [Description("One Hand Mace")]
        Mace = 12,
        [Description("One Hand Sword")]
        Sword = 9,
        Sceptre = 32,
        [Description("Two Hand Axe")]
        Axe2H = 16,
        [Description("Two Hand Mace")]
        Mace2H = 17,
        [Description("Two Hand Sword")]
        Sword2H = 15,
        Belt = 21,
        Boots = 23,
        Helmet = 25,
        Gloves = 22,
        [Description("Body Armour")]
        BodyArmour = 24, // armour
        Shield = 26,
        Quiver = 20,
        Ring = 5,
        Amulet = 4, // jewelly
        Map = 100,
        Jewel = 41,
        [Description("Jewel")]
        AbyssalJewel = 50,
        Gem = 101,
        [Description("Divination Card")]
        DivinationCard = 102,
        [Description("Vaal Fragment")]
        VaalFragment = 103,
        Weapon1H = 104,
        Weapon2H = 105,
        Flask = 106,
        Normal = 107,
        Fossil = 108,
        Resonator = 109,
        Essence = 110,
        [Description("Rare Base")]
        RareBase = 111,
        Prophecy = 112,
        Breachstone = 113,
        Offering = 114,
        [Description("Divine Vessel")]
        DivineVessel = 115,
        [Description("Guard Fragment")]
        GuardFragment = 116,
        Vial = 117,
        Scarab = 118,
        Beast = 119,
    }

    public enum Mark {
        Empty,
        Shaper,
        Elder,
        Synthesised, 
        Fractured
    }
}