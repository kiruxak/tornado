﻿using System.ComponentModel;

namespace PoeParser.Entities {
    public enum ItemType {
        Unknown,
        Wand, Dagger, Claw, //1h
        Staff, Bow, //2h
        [Description("One Hand Axe")]Axe, [Description("One Hand Mace")]Mace, [Description("One Hand Sword")]Sword, Sceptre,
        [Description("Two Hand Axe")]Axe2H, [Description("Two Hand Mace")]Mace2H, [Description("Two Hand Sword")]Sword2H,
        Belt, Boots, Helmet, Gloves, [Description("Body Armour")]BodyArmour, // armour
        Shield,
        Quiver,
        Ring, Amulet, // jewelly
        Map,
        Jewel, Gem,
        [Description("Divination Card")]
        DivinationCard, VaalFragment,
        Weapon1H,
        Weapon2H,
        Flask,
        Normal
    }
}
