using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using PoeParser;
using PoeParser.Parser;
using PoeParser.PoeTradeIntegration;
using PoeTrade;

namespace PoeTest {
    class Program {
        [STAThread]
        private static void Main(string[] args) {
            PoeData.Init();

            PoeData.InitFromExcel();

            var textClip = @"Rarity: Rare
Demon Scarab
Gold Amulet
--------
Requirements:
Level: 57
--------
Item Level: 81
--------
14% increased Rarity of Items found
--------
20% increased Spell Damage
+11% to all Elemental Resistances
+11% to Cold Resistance
+11% to Lightning Resistance
+36 to maximum Life
0.4% of Physical Attack Damage Leeched as Mana
--------6
Note: ~b/o 38 chaos";

            var item = ItemParser.Parse(textClip);
            if (item == null)
                return;


            PoeTradeClient poeTradeClient = new PoeTradeClient();

            item.OpenPoeTrade();
            var result = item.GetPoePrice();
            

            Console.WriteLine($"Value: {item.Value}");
            Console.ReadLine();

            //var x = PoeData.Manager.Filter(item);

            //var price = "";
            //if (itemChest1 != null) {
            //    Task.Run(() => {
            //        price = itemChest1.GetPoePrice(); 
            //    });

            //    itemChest1.OpenPoeTrade();
            //}

            //string filter = "Amulet S(60Allres 40FlatES $ModES) any1(D($SpellDmg $SpellDmgCrit $CritDmg $CastSpeed) D($10FlatPhys $CritDmg $Crit) D($30Wed $FlatElem $CritDmg $Crit)) U($ItemRarity)";

            //FilterTree f = FilterTreeFactory.BuildTree(filter);

            //var filterresult = f.Filter(item);
            //var tooltip = new ToolTipGenerator(filterresult).GetTooltip();

            //filterresult.OpenPoeTrade();

            //var matches = PoeData.Manager.Filter(item);
            //var generator = new ToolTipGenerator(matches);
            //var tooltip = generator.GetTooltip();

            //matches.OpenPoeTrade();
        }

        private static string GetText() {
            string clipBoardText = "";
            Thread staThread = new Thread(
                    delegate() {
                        try {
                            clipBoardText = Clipboard.GetText();
                        } catch (Exception) {
                            clipBoardText = "";
                        }
                    });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return clipBoardText;
        }
    }
}