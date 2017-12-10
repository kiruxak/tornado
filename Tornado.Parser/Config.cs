using System;

namespace Tornado.Parser {
    public class Config {
        public static string Folder_Root = AppDomain.CurrentDomain.BaseDirectory;
        public static string Folder_Affixes = $"{Folder_Root}";
        public static string PickitFileUrl = $"{Folder_Root}\\pickit.ini";

        public const string PoeTradeUrl = "http://poe.trade/search";
        public const string PoeTradeSortQuery = "sort=price_in_chaos&bare=true";

        public static double PoeTradeMod = 1;
        public static string LeagueName = "Standard";
        public static bool Debug = false;
        public static double MinPrice = 1;
        public static bool ShowNotification = true;
        public static bool ShowCraft = true;
    }
}