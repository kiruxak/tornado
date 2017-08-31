using System.Collections.Generic;
using System.Globalization;
using PoeParser.Common.Extensions;

namespace PoeParser.PoeTradeIntegration {
    public enum CurrencyType {
        fusing, alteration, chisel, gcp, alchemy, regret, jewellers, chance, chaos, exalted, vaal, regal, scouring, blessed, divine, chromatic, coin, mirror, bauble, silver
    }

    public class Currency {
        public static Dictionary<CurrencyType, double> CurrencyRate = new Dictionary<CurrencyType, double>() {
            { CurrencyType.coin, 0.01 },
            { CurrencyType.alteration, 0.07 },
            { CurrencyType.chromatic, 0.08 },
            { CurrencyType.silver, 0.1 },
            { CurrencyType.jewellers, 0.13 },
            { CurrencyType.bauble, 0.15 },
            { CurrencyType.chance, 0.16 },
            { CurrencyType.alchemy, 0.30 },
            { CurrencyType.chisel, 0.30 },
            { CurrencyType.blessed, 0.43 },
            { CurrencyType.fusing, 0.5 },
            { CurrencyType.scouring, 0.6 },
            { CurrencyType.regal, 0.98 },
            { CurrencyType.chaos, 1 },
            { CurrencyType.regret, 1 },
            { CurrencyType.vaal, 1.1 },
            { CurrencyType.gcp, 1.5 },
            { CurrencyType.divine, 12 },
            { CurrencyType.exalted, 75 },
            { CurrencyType.mirror, 6000 }
        };

        public static Dictionary<CurrencyType, string> CurrencyAbbr = new Dictionary<CurrencyType, string>() {
            { CurrencyType.bauble, "bauble" },
            { CurrencyType.alteration, "alt" },
            { CurrencyType.chromatic, "chr" },
            { CurrencyType.jewellers, "jew" },
            { CurrencyType.chance, "cha" },
            { CurrencyType.alchemy, "alc" },
            { CurrencyType.blessed, "ble" },
            { CurrencyType.chisel, "chi" },
            { CurrencyType.scouring, "sco" },
            { CurrencyType.fusing, "fus" },
            { CurrencyType.regret, "regret" },
            { CurrencyType.regal, "reg" },
            { CurrencyType.chaos, "c" },
            { CurrencyType.vaal, "vaal" },
            { CurrencyType.gcp, "gcp" },
            { CurrencyType.divine, "div" },
            { CurrencyType.exalted, "exa" },
            { CurrencyType.coin, "coin" },
            { CurrencyType.mirror, "mirror" },
            { CurrencyType.silver, "silver" }
        };

        public double Value { get; set; }
        public string Source { get; set; }
        public CurrencyType Type { get; set; }
        public double ValueInChaos { get; set; }

        public Currency(string source) {
            Source = source;

            Value = source.ParseTo(0.0, "([\\d.]+)");
            Type = source.ParseTo(CurrencyType.chaos, "([a-zA-Z]+)");
            ValueInChaos = Value*CurrencyRate[Type];
        }

        public override string ToString() {
            return $"{Value.ToString("0.#", new CultureInfo("en-US"))}{CurrencyAbbr[Type]}";
        }
    }
}