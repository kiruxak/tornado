using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using PoeParser.Common.Extensions;
using PoeParser.Entities;
using PoeParser.Entities.Affixes;
using PoeParser.Parser;

namespace PoeParser.Data {
    public class AffixRecord: IRecord {
        public string Name { get; set; }
        public string Description { get; set; }
        public string RegexPattern { get; set; }
        public string StringValue { get; set; }
        public string DisplayName { get; set; }
        public AffixType Type { get; set; }
        public AffixTier Tier { get; set; }

        public void Parse(string line) {
            string[] values = line.Split('\t');

            Name = values[0];
            Description = values[1];
            RegexPattern = values[2];

            StringValue = values[4];
            Type = values[10].ParseTo(AffixType.Unknown, "(\\w+)");
            DisplayName = values[7];

            Tier = new AffixTier();

            Tier.Tier = values[3].ParseTo(0, "(\\d+)");
            Tier.MinValue = values[5].ParseTo(0.0, "([\\d.]+)");
            Tier.MaxValue = values[6].ParseTo(0.0, "([\\d.]+)");
            Tier.LevelRequired = values[8].ParseTo(0, "(\\d+)");
            Tier.Craft = values[9].ContainsPattern("Yes");

            if (values[11].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Ring); }
            if (values[12].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Amulet); }
            if (values[13].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Belt); }
            if (values[14].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Helmet); }
            if (values[15].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Gloves); }
            if (values[16].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Boots); }
            if (values[17].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.BodyArmour); }
            if (values[18].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Shield); }
            if (values[19].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Quiver); }
            if (values[20].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Wand); }
            if (values[21].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Dagger); }
            if (values[22].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Claw); }
            if (values[23].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Sceptre); }
            if (values[24].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Staff); }
            if (values[25].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Bow); }
            if (values[26].ContainsPattern("Yes")) {
                Tier.ForTypes.Add(ItemType.Sword);
                Tier.ForTypes.Add(ItemType.Axe);
            }
            if (values[27].ContainsPattern("Yes")) {
                Tier.ForTypes.Add(ItemType.Sword2H);
                Tier.ForTypes.Add(ItemType.Axe2H);
            }
            if (values[28].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Mace); }
            if (values[29].ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Mace2H); }
        }

        public AffixRecord() {}
        public AffixRecord(Row row, SharedStringTable sh) {
            var cells = row.Elements<Cell>().ToList();
            Name = cells[0].GetValue(sh);

            Description = cells[1].GetValue(sh);
            RegexPattern = cells[2].GetValue(sh);

            StringValue = cells[4].GetValue(sh);
            Type = cells[10].GetValue(sh).ParseTo(AffixType.Unknown, "(\\w+)");
            DisplayName = cells[7].GetValue(sh);

            Tier = new AffixTier();

            Tier.Tier = cells[3].GetValue(sh).ParseTo(0, "(\\d+)");
            Tier.MinValue = cells[5].GetValue(sh).ParseTo(0.0, "([\\d.]+)");
            Tier.MaxValue = cells[6].GetValue(sh).ParseTo(0.0, "([\\d.]+)");
            Tier.LevelRequired = cells[8].GetValue(sh).ParseTo(0, "(\\d+)");
            Tier.Craft = cells[9].GetValue(sh).ContainsPattern("Yes");

            if (cells[11].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Ring); }
            if (cells[12].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Amulet); }
            if (cells[13].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Belt); }
            if (cells[14].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Helmet); }
            if (cells[15].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Gloves); }
            if (cells[16].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Boots); }
            if (cells[17].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.BodyArmour); }
            if (cells[18].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Shield); }
            if (cells[19].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Quiver); }
            if (cells[20].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Wand); }
            if (cells[21].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Dagger); }
            if (cells[22].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Claw); }
            if (cells[23].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Sceptre); }
            if (cells[24].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Staff); }
            if (cells[25].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Bow); }
            if (cells[26].GetValue(sh).ContainsPattern("Yes")) {
                Tier.ForTypes.Add(ItemType.Sword);
                Tier.ForTypes.Add(ItemType.Axe);
            }
            if (cells[27].GetValue(sh).ContainsPattern("Yes")) {
                Tier.ForTypes.Add(ItemType.Sword2H);
                Tier.ForTypes.Add(ItemType.Axe2H);
            }
            if (cells[28].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Mace); }
            if (cells[29].GetValue(sh).ContainsPattern("Yes")) { Tier.ForTypes.Add(ItemType.Mace2H); }
        }
    }
}