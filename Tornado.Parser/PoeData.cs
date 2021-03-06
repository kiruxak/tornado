﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using Tornado.Common.Extensions;
using Tornado.Common.Utility;
using Tornado.Parser.Common.Extensions;
using Tornado.Parser.Data;
using Tornado.Parser.Data.Bases;
using Tornado.Parser.Entities.Affixes;
using Tornado.Parser.Filter;
using Tornado.Parser.Parser;
using Tornado.Parser.PoeTrade.Data;

namespace Tornado.Parser {
    public class PoeData {
        public static NiceDictionary<string, ArmourBase> Armors;
        public static NiceDictionary<string, ArmourBase> Boots;
        public static NiceDictionary<string, ArmourBase> Gloves;
        public static NiceDictionary<string, ArmourBase> Helmets;
        public static NiceDictionary<string, ArmourBase> Shields;

        public static NiceDictionary<string, WeaponBase> Weapons;
        public static NiceDictionary<string, DivCard> DivCards;
        public static NiceDictionary<string, SynthesisGroup> SynthesisImplicits;

        public static NiceDictionary<string, Affix> Affixes;
        public static NiceDictionary<string, TotalAffixRecord> TotalAffixes;

        public static FilterManager Manager;

        public static void Init() {
            Manager = FilterManager.GetManager();

            InitFromExcel();

            string config = FileReader.Read($"{Config.Folder_Root}\\config.ini");

            Config.PoeTradeMod = config.ParseTo(1.0, "PoeTradeMod=([\\d.]+)");
            Config.LeagueName = config.ParseTo("Standard", "LeagueName=(\\D+)\r\n");
            Config.Debug = config.ContainsPattern("Debug=True");
            Config.ShowNotification = config.ContainsPattern("ShowNotification=True");
            Config.ShowCraft = config.ContainsPattern("ShowCraft=True");
            Config.MinPrice = config.ParseTo(1.0, "MinPrice=([\\d.]+)");

            InitCurrencyValues();
            InitFossilValues();
            InitDivCardsValues();
        }

        private static async void InitCurrencyValues() {
            try {
                var client = new HttpClient();
                var response = await client.GetAsync("http://poe.ninja/api/Data/GetCurrencyOverview?league=" + Config.LeagueName);
                var stringData = await response.Content.ReadAsStringAsync();

                Currency.Data = JsonConvert.DeserializeObject<CurrencyData>(stringData);
            }
            catch (Exception e) {
                // ignore
            }
        }

        private static async void InitFossilValues() {
            try {
                var client = new HttpClient();
                var response = await client.GetAsync($"https://poe.ninja/api/data/itemoverview?league={Config.LeagueName}&type=Fossil&date={DateTime.Now:yyyy-MM-dd}");
                var stringData = await response.Content.ReadAsStringAsync();

                Currency.FossilData = JsonConvert.DeserializeObject<CurrencyData>(stringData);
            } catch (Exception e) {
                // ignore
            }
        }

        private static async void InitDivCardsValues() {
            try {
                var client = new HttpClient();
                var response = await client.GetAsync($"https://poe.ninja/api/data/itemoverview?league={Config.LeagueName}&type=DivinationCard&date={DateTime.Now:yyyy-MM-dd}");
                var stringData = await response.Content.ReadAsStringAsync();

                Currency.DivCardData = JsonConvert.DeserializeObject<CurrencyData>(stringData);
            } catch (Exception e) {
                // ignore
            }
        }

        public static void InitFromExcel() {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open($"{Config.Folder_Affixes}\\mods.xlsx", false)) {
                WorkbookPart wbPart = document.WorkbookPart;
                SharedStringTable sharedStringTable = document.WorkbookPart.SharedStringTablePart.SharedStringTable;
                List<Sheet> sheets = wbPart.Workbook.Sheets.Cast<Sheet>().ToList();
                NiceDictionary<string, SheetData> sheetsData = new NiceDictionary<string, SheetData>();

                foreach (var w in wbPart.WorksheetParts) {
                    string partRelationshipId = wbPart.GetIdOfPart(w);
                    var correspondingSheet = sheets.FirstOrDefault(s => s.Id.HasValue && s.Id.Value == partRelationshipId);

                    if (correspondingSheet != null) {
                        sheetsData.Add(correspondingSheet.Name.Value, w.Worksheet.Elements<SheetData>().First());
                    }
                }

                Affixes = sheetsData["mods"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new AffixRecord(r, sharedStringTable))
                                            .GroupBy(a => a.Name)
                                            .ToNiceDictionary(a => a.Key, a => new Affix {
                                                Name = a.Key,
                                                RegexPattern = a.First().RegexPattern,
                                                Tiers = a.Select(af => af.Tier).ToList(),
                                                Type = a.First().Type
                                            });

                TotalAffixes = sheetsData["affixes"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new TotalAffixRecord(r, sharedStringTable)).ToNiceDictionary(i => i.Name, i => i);

                Armors = sheetsData["armors"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new ArmourBase(r, sharedStringTable)).ToNiceDictionary(i => i.Name, i => i);
                Boots = sheetsData["boots"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new ArmourBase(r, sharedStringTable)).ToNiceDictionary(i => i.Name, i => i);
                Gloves = sheetsData["gloves"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new ArmourBase(r, sharedStringTable)).ToNiceDictionary(i => i.Name, i => i);
                Helmets = sheetsData["helmets"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new ArmourBase(r, sharedStringTable)).ToNiceDictionary(i => i.Name, i => i);
                Shields = sheetsData["shields"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new ArmourBase(r, sharedStringTable)).ToNiceDictionary(i => i.Name, i => i);
                Weapons = sheetsData["weapons"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new WeaponBase(r, sharedStringTable)).ToNiceDictionary(i => i.Name, i => i);
                DivCards = sheetsData["divcards"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new DivCard(r, sharedStringTable)).ToNiceDictionary(i => i.Name, i => i);
                SynthesisImplicits = sheetsData["synthesis2"].Elements<Row>().Skip(1).Where(x => x.Elements<Cell>().Any()).Select(r => new SynthesisImplicit(r, sharedStringTable)).GroupBy(x => x.ReqStat).ToNiceDictionary(i => i.Key, i => new SynthesisGroup(i.ToList()));
            }
        }
    }
}