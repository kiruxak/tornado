using Tornado.Common.Utility;
using Tornado.Parser.PoeTrade.Extensions;

namespace Tornado.Parser.PoeTrade.Data {
    public enum CurrencyType {
        fusing,
        alteration,
        chisel,
        gcp,
        alchemy,
        regret,
        jewellers,
        chance,
        chaos,
        exalted,
        vaal,
        regal,
        scouring,
        blessed,
        divine,
        chromatic,
        coin,
        mirror,
        bauble,
        silver
    }

    public class Currency {
        public static NiceDictionary<CurrencyType, double> CurrencyRate = new NiceDictionary<CurrencyType, double>() {
            {
                CurrencyType.coin, 0.01
            }, {
                CurrencyType.alteration, 0.09
            }, {
                CurrencyType.chromatic, 0.1
            }, {
                CurrencyType.silver, 0.22
            }, {
                CurrencyType.jewellers, 0.1
            }, {
                CurrencyType.bauble, 0.09
            }, {
                CurrencyType.chance, 0.1
            }, {
                CurrencyType.alchemy, 0.34
            }, {
                CurrencyType.chisel, 0.45
            }, {
                CurrencyType.blessed, 0.23
            }, {
                CurrencyType.fusing, 0.43
            }, {
                CurrencyType.scouring, 0.36
            }, {
                CurrencyType.regal, 0.8
            }, {
                CurrencyType.chaos, 1
            }, {
                CurrencyType.regret, 0.6
            }, {
                CurrencyType.vaal, 1
            }, {
                CurrencyType.gcp, 0.9
            }, {
                CurrencyType.divine, 12
            }, {
                CurrencyType.exalted, 75
            }, {
                CurrencyType.mirror, 13000
            }
        };

        public static NiceDictionary<CurrencyType, string> CurrencyAbbr = new NiceDictionary<CurrencyType, string>() {
            {
                CurrencyType.bauble, "bauble"
            }, {
                CurrencyType.alteration, "alt"
            }, {
                CurrencyType.chromatic, "chr"
            }, {
                CurrencyType.jewellers, "jew"
            }, {
                CurrencyType.chance, "cha"
            }, {
                CurrencyType.alchemy, "alc"
            }, {
                CurrencyType.blessed, "ble"
            }, {
                CurrencyType.chisel, "chi"
            }, {
                CurrencyType.scouring, "sco"
            }, {
                CurrencyType.fusing, "fus"
            }, {
                CurrencyType.regret, "regret"
            }, {
                CurrencyType.regal, "reg"
            }, {
                CurrencyType.chaos, "c"
            }, {
                CurrencyType.vaal, "vaal"
            }, {
                CurrencyType.gcp, "gcp"
            }, {
                CurrencyType.divine, "div"
            }, {
                CurrencyType.exalted, "exa"
            }, {
                CurrencyType.coin, "coin"
            }, {
                CurrencyType.mirror, "mirror"
            }, {
                CurrencyType.silver, "silver"
            }
        };

        public double Value { get; set; }
        public string Source { get; set; }
        public CurrencyType Type { get; set; }
        public double ValueInChaos { get; set; }

        public Currency(string source) {
            Source = source;

            Value = source.ParseTo(0.0, "([\\d.]+)");
            Type = source.ParseTo(CurrencyType.chaos, "([a-zA-Z]+)");
            ValueInChaos = Value * CurrencyRate[Type];
        }

        public override string ToString() {
            return $"{Value:0.#}{CurrencyAbbr[Type]}";
        }
    }
}