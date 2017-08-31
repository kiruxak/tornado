using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using PoeParser.Common.Extensions;
using PoeParser.Data;
using PoeParser.Entities.Affixes;
using PoeParser.Filter;
using PoeParser.Parser;

namespace PoeParser {
    public class PoeData {
        public static Dictionary<string, ArmourBase> Armors;
        public static Dictionary<string, ArmourBase> Boots;
        public static Dictionary<string, ArmourBase> Gloves;
        public static Dictionary<string, ArmourBase> Helmets;
        public static Dictionary<string, ArmourBase> Shields;

        public static Dictionary<string, WeaponBase> Weapons;

        public static Dictionary<string, Affix> Affixes;
        public static Dictionary<string, TotalAffixRecord> TotalAffixes;

        public static FilterManager Manager;

        public static void Init() {
            Manager = FilterManager.GetManager();

            //Armors = FileReader.ParseTo<ArmourBase>($"{Config.Folder_Bases}\\Armors.txt").ToDictionary(i => i.Name, i => i);
            //Boots = FileReader.ParseTo<ArmourBase>($"{Config.Folder_Bases}\\Boots.txt").ToDictionary(i => i.Name, i => i);
            //Gloves = FileReader.ParseTo<ArmourBase>($"{Config.Folder_Bases}\\Gloves.txt").ToDictionary(i => i.Name, i => i);
            //Helmets = FileReader.ParseTo<ArmourBase>($"{Config.Folder_Bases}\\Helmets.txt").ToDictionary(i => i.Name, i => i);
            //Shields = FileReader.ParseTo<ArmourBase>($"{Config.Folder_Bases}\\Shields.txt").ToDictionary(i => i.Name, i => i);

            //Weapons = FileReader.ParseTo<WeaponBase>($"{Config.Folder_Bases}\\weapons.txt").ToDictionary(i => i.Name, i => i);

            InitFromExcel();

            //Affixes = FileReader.ParseTo<AffixRecord>($"{Config.Folder_Affixes}\\mods.txt").GroupBy(a => a.Name)
            //    .ToDictionary(a => a.Key, a => new Affix {
            //        Name = a.Key,
            //        RegexPattern = a.First().RegexPattern,
            //        Tiers = a.Select(af => af.Tier).ToList(),
            //        Type = a.First().Type
            //    });

            //TotalAffixes = FileReader.ParseTo<TotalAffixRecord>($"{Config.Folder_Affixes}\\affixes.txt").ToDictionary(i => i.Name, i => i);

            string config = FileReader.Read($"{Config.Folder_Root}\\config.ini");

            Config.PoeTradeMod = config.ParseTo(1.0, "PoeTradeMod=([\\d.]+)");
            Config.LeagueName = config.ParseTo("Standard", "LeagueName=(\\D+)\r\n");
            Config.Debug = config.ContainsPattern("Debug=True");
            Config.ShowNotification = config.ContainsPattern("ShowNotification=True");
            Config.ShowCraft = config.ContainsPattern("ShowCraft=True");
            Config.MinPrice = config.ParseTo(1.0, "MinPrice=([\\d.]+)");
        }

        public static void InitFromExcel() {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open($"{Config.Folder_Affixes}\\mods.xlsx", false)) {
                WorkbookPart wbPart = document.WorkbookPart;
                SharedStringTable sharedStringTable = document.WorkbookPart.SharedStringTablePart.SharedStringTable;
                List<Sheet> sheets = wbPart.Workbook.Sheets.Cast<Sheet>().ToList();
                Dictionary<string, SheetData> sheetsData = new Dictionary<string, SheetData>();

                foreach (var w in wbPart.WorksheetParts) {
                    string partRelationshipId = wbPart.GetIdOfPart(w);
                    var correspondingSheet = sheets.FirstOrDefault(s => s.Id.HasValue && s.Id.Value == partRelationshipId);
                    
                    if (correspondingSheet != null)
                        sheetsData.Add(correspondingSheet.Name.Value, w.Worksheet.Elements<SheetData>().First());
                }

                Affixes = sheetsData["mods"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new AffixRecord(r, sharedStringTable))
                    .GroupBy(a => a.Name)
                    .ToDictionary(a => a.Key, a => new Affix {
                        Name = a.Key,
                        RegexPattern = a.First().RegexPattern,
                        Tiers = a.Select(af => af.Tier).ToList(),
                        Type = a.First().Type
                    });

                TotalAffixes = sheetsData["affixes"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new TotalAffixRecord(r, sharedStringTable)).ToDictionary(i => i.Name, i => i);

                Armors = sheetsData["armors"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new ArmourBase(r, sharedStringTable)).ToDictionary(i => i.Name, i => i);
                Boots = sheetsData["boots"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new ArmourBase(r, sharedStringTable)).ToDictionary(i => i.Name, i => i);
                Gloves = sheetsData["gloves"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new ArmourBase(r, sharedStringTable)).ToDictionary(i => i.Name, i => i);
                Helmets = sheetsData["helmets"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new ArmourBase(r, sharedStringTable)).ToDictionary(i => i.Name, i => i);
                Shields = sheetsData["shields"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new ArmourBase(r, sharedStringTable)).ToDictionary(i => i.Name, i => i);
                Weapons = sheetsData["weapons"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new WeaponBase(r, sharedStringTable)).ToDictionary(i => i.Name, i => i);
            }
        }
    }
}