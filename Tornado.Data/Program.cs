using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Tornado.Data.Data;

namespace Tornado.Data
{
    class Program
    {
        static void Main(string[] args) {
            var result = "";
            using (FileStream fs = new FileStream("C:\\Git\\Tornado.Build\\json.txt", FileMode.Open, FileAccess.Read)) {

                using (StreamReader sr = new StreamReader(fs)) {
                    while (!sr.EndOfStream) {
                        result += sr.ReadLine() + Environment.NewLine;
                    }
                }

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                Root.Data = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(result);

                foreach (var stat in Root.Data.StatDescriptions) {
                    foreach (Condition condition in stat.Value.Def.Key.Conditions) {
                        condition.Text = condition.Text.Replace("%1$+d%%", "\\+(\\d+)%").Replace("%1$+d", "\\+(\\d+)").Replace("%1%%%", "(\\d+)%").Replace("%1%", "(\\d+)").Replace("%2%", "(\\d+)").Replace("%%", "%");
                    }
                }

                //                var domains = data2.ItemClasses.SelectMany(x => x.Special).GroupBy(x => x.Type);
                //                var gtypes = data2.Mods.Values.GroupBy(x => x.GenerationType);
                //                var xs = data2.Mods.Values.Select(x => $"{x.CorrectGroup} = {x.ModTypeKey}").Distinct();
                //                var modtypekey = string.Join(", ", xs.ToArray());

                //                var item = Root.Data.ItemClasses[4].BaseItems[0];
                //                var mods = Root.Data.Mods.Values.Where(x => x.Weight != null && x.Weight.Any(w => w.Key != Tags.@default && item.Tags.Contains(w.Key) ));
                //                var emods = Root.Data.Mods.Values.Where(x => x.Weight != null && x.Weight.Any(w => w.Key == Root.Data.ItemClasses[4].ElderTag));
                //
                //                var p = mods.Where(x => x.GenerationType == GenType.Prefix);


                using (var wb = new XLWorkbook()) {
                    //                    foreach (var c in Root.Data.ItemClasses) {
                    //                        var ws = wb.AddWorksheet(c.Name);
                    //
                    //                        ws.Cell(1, 1).InsertData(new object[] {"Index", "Id", "Name", "ElderTag", "ShaperTag"}, true);
                    //                        ws.Cell(2, 1).InsertData(new object[] {c.Index, c.Id, c.Name, c.ElderTag.ToString(), c.ShaperTag.ToString()}, true);
                    //
                    //
                    //                        ws.Cell(4, 1).InsertData(new object[] {
                    //                                "Index", "Name", "DropLevel", "Image", "Implicits", "Tags", "",
                    //                                "Defense.Ar", "Defense.Es", "Defense.Ev", "Defense.Block", "",
                    //                                "Req.Str", "Req.Dex", "Req.Int", "",
                    //                                "Weapon.AttackSpeed", "Weapon.Crit", "Weapon.DMax", "Weapon.DMin", "Weapon.Range", "",
                    //                                "Flask.ChargesPerUse", "Flask.Duration", "Flask.Life", "Flask.Mana", "Flask.MaxCharges"
                    //                        }, true);
                    //
                    //                        var ri = 5;
                    //                        foreach (var b in c.BaseItems) {
                    //                            ws.Cell(ri, 1).InsertData(new object[] {
                    //                                    b.Index, b.Name, b.DropLevel, b.Image, string.Join(",", b.Implicits), string.Join(",", b.Tags.Where(x => x != Tags.@default)), "",
                    //                                    b.Stats.Defense?.Ar.ToString() ?? "", b.Stats.Defense?.Es.ToString() ?? "", b.Stats.Defense?.Ev.ToString() ?? "", b.Stats.Defense?.Block.ToString() ?? "", "",
                    //                                    b.Stats.Req?.Str.ToString() ?? "", b.Stats.Req?.Dex.ToString() ?? "", b.Stats.Req?.Int.ToString() ?? "", "",
                    //                                    b.Stats.Weapon?.AttackSpeed.ToString() ?? "", b.Stats.Weapon?.Crit.ToString() ?? "", b.Stats.Weapon?.DMax.ToString() ?? "", b.Stats.Weapon?.DMin.ToString() ?? "", b.Stats.Weapon?.Range.ToString() ?? "", "",
                    //                                    b.Stats.Flask?.ChargesPerUse.ToString() ?? "", b.Stats.Flask?.Duration.ToString() ?? "", b.Stats.Flask?.Life.ToString() ?? "", b.Stats.Flask?.Mana.ToString() ?? "", b.Stats.Flask?.MaxCharges.ToString() ?? ""
                    //                            }, true);
                    //                            ri++;
                    //                        }
                    //
                    //
                    //                    }

                    //mods
                    //                    foreach (var mod in Root.Data.Mods.GroupBy(x => x.Value.Domain)) {
                    //                        if (mod.Key == Domain.Item) {
                    //                            foreach (var imod in mod.GroupBy(x => x.Value.GenerationType)) {
                    //                                var ws = wb.AddWorksheet($"{mod.Key}-{imod.Key}");
                    //                                ws.Cell(1, 1).InsertData(new object[] {"CorrectGroup", "Index", "Id", "Domain", "GenerationType", "Weight" }, true);
                    //                                var mi = 2;
                    //
                    //                                foreach (var modValue in imod.ToList()) {
                    //                                    var v = modValue.Value;
                    //                                    ws.Cell(mi, 1).InsertData(
                    //                                            new object[] {
                    //                                                    v.CorrectGroup, v.Index, v.Id, v.Domain.ToString(), v.GenerationType.ToString(),
                    //                                                    v.Weight != null ? string.Join(",", v.Weight.Where(x => x.Key != Tags.@default).Select(x => x.Key.ToString())) : "", string.Join(", ", v.Stats.Select(x => $"{x.StatValue.StatDescription?.Def.Key.Conditions.Length}{x.StatValue.StatDescription?.Def.Key.Conditions[0].Text ?? "Empty"}:{x.Min}-{x.Max}")),
                    //                                            }, true);
                    //                                    mi++;
                    //                                }
                    //                            }
                    //                        }
                    //                        else {
                    //                            var ws = wb.AddWorksheet(mod.Key.ToString());
                    //                            ws.Cell(1, 1).InsertData(new object[] {"Key", "CorrectGroup", "Index", "Id", "Domain", "GenerationType", "Weight"}, true);
                    //                            var mi = 2;
                    //
                    //                            foreach (var modValue in mod.ToList()) {
                    //                                var v = modValue.Value;
                    //                                ws.Cell(mi, 1).InsertData(
                    //                                        new object[] {
                    //                                                v.CorrectGroup, v.Index, v.Id, v.Domain.ToString(), v.GenerationType.ToString(),
                    //                                                v.Weight != null ? string.Join(",", v.Weight.Where(x => x.Key != Tags.@default).Select(x => x.Key.ToString())) : "", string.Join(", ", v.Stats.Select(x => $"{x.Key}:{x.Min}-{x.Max}"))
                    //                                        }, true);
                    //                                mi++;
                    //                            }
                    //                        }
                    //                    }

                    //mask
//                    foreach (var mod in Root.Data.Mods.GroupBy(x => x.Value.Domain)) {
//                        if (mod.Key == Domain.Item) {
//                            foreach (var imod in mod.GroupBy(x => x.Value.GenerationType)) {
//                                var ws = wb.AddWorksheet($"{mod.Key}-{imod.Key}");
//                                ws.Cell(1, 1).InsertData(new object[] {"CorrectGroup", "Index", "Id", "Domain", "GenerationType", "Weight"}, true);
//                                var mi = 2;
//
//                                foreach (var modValue in imod.ToList().GroupBy(x => x.Value.CorrectGroup)) {
//                                    var v = modValue.FirstOrDefault().Value;
//                                    ws.Cell(mi, 1).InsertData(
//                                            new object[] { v.CorrectGroup, string.Join(", ", v.Stats.Select(x => $"{(x.StatValue.StatDescription == null ? "empty" : string.Join(", ", x.StatValue.StatDescription.Def.Key.Conditions.Select(c => c.Text)))}")), }, true);
//                                    mi++;
//                                }
//                            }
//                        }
//                        else {
//                            var ws = wb.AddWorksheet(mod.Key.ToString());
//                            ws.Cell(1, 1).InsertData(new object[] {"Key", "CorrectGroup", "Index", "Id", "Domain", "GenerationType", "Weight"}, true);
//                            var mi = 2;
//
//                            foreach (var modValue in mod.ToList().GroupBy(x => x.Value.CorrectGroup))
//                            {
//                                var v = modValue.FirstOrDefault().Value;
//                                ws.Cell(mi, 1).InsertData(
//                                        new object[] { v.CorrectGroup, string.Join(", ", v.Stats.Select(x => $"{(x.StatValue.StatDescription == null ? "empty" : string.Join(", ", x.StatValue.StatDescription.Def.Key.Conditions.Select(c => c.Text)))}")), }, true);
//                                mi++;
//                            }
//                        }
//                    }
//
//                    wb.SaveAs("C:\\Git\\Tornado.Build\\map.xlsx");
                }

                var xx = 1;
            }
        }
    }
}
