using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Tornado.Data.Data {
    public class Root {
        public static Root Data { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("mods")]
        public Dictionary<string, Mod> Mods { get; set; }

        [JsonProperty("stats")]
        public Dictionary<string, StatValue> Stats { get; set; }

        [JsonProperty("statDescriptions")]
        public Dictionary<string, StatDescription> StatDescriptions { get; set; }
//
//        [JsonProperty("essences")]
//        public Essence[] Essences { get; set; }

        [JsonProperty("enchants")]
        public Dictionary<string, long[]> Enchants { get; set; }

//        [JsonProperty("craft")]
//        public Craft[] Craft { get; set; }

        [JsonProperty("itemClasses")]
        public ItemClass[] ItemClasses { get; set; }

        //        [JsonProperty("currency")]
        //        public Dictionary<string, Currency> Currency { get; set; }
        //
        //        [JsonProperty("buffs")]
        //        public Buffs Buffs { get; set; }
        //
        //        [JsonProperty("grantedEffects")]
        //        public GrantedEffects GrantedEffects { get; set; }

        //        [JsonProperty("tags")]
        //        public Dictionary<string, long> Tags { get; set; }

        public BaseItem GetBase(string[] lines) {
            if (lines[0].Contains("Rare") || lines[0].Contains("Unique"))
                return ItemClasses.SelectMany(x => x.BaseItems).First(s => s.Name == lines[2]);
            if (lines[0].Contains("Magic") || lines[0].Contains("Normal")) {
                return ItemClasses.SelectMany(x => x.BaseItems).First(s => s.Name.IndexOf(lines[1], StringComparison.InvariantCultureIgnoreCase) > 0);
            }

            return null;
        }
    }

    public class StatValue
    {
        [JsonProperty("index")]
        public long Index { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("local")]
        public bool Local { get; set; }

        [JsonProperty("weaponLocal")]
        public bool WeaponLocal { get; set; }

        [JsonProperty("mainhandAlias")]
        public long? MainhandAlias { get; set; }

        [JsonProperty("offhandAlias")]
        public long? OffhandAlias { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        private StatDescription _statDescription = null;

        public StatDescription StatDescription => _statDescription ?? (_statDescription = Root.Data.StatDescriptions.ContainsKey(Id) ? Root.Data.StatDescriptions[Id] : null);
    }

    public class StatDescription {
        [JsonProperty("def")]
        public Def Def { get; set; }
    }

    public class Def {
        [JsonProperty("ids")]
        public string[] Ids { get; set; }

        [JsonProperty("$id")]
        public long Id { get; set; }

        [JsonProperty("key")]
        public Key Key { get; set; }
    }

    public class Key {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("condCount")]
        public long CondCount { get; set; }

        [JsonProperty("conditions")]
        public Condition[] Conditions { get; set; }
    }

    public class Condition {
        [JsonProperty("param")]
        public string[] Param { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("suffixes")]
        public string[] Suffixes { get; set; }
    }
}