using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Tornado.Parser.PoeTrade.Extensions {
    public static class ParseExtensions {
        public static T ParseTo<T>(this string val, T defaultValue, string regexPattern = null) {
            if (!string.IsNullOrEmpty(regexPattern)) {
                Regex regex = new Regex(regexPattern);
                Match match = regex.Match(val);
                if (match.Success) {
                    val = match.Groups[1].Value;
                } else {
                    return defaultValue;
                }
            }
            return Parse(val, defaultValue);
        }

        public static bool ContainsPattern(this string val, string regexPattern) {
            if (!string.IsNullOrEmpty(regexPattern)) {
                Regex regex = new Regex(regexPattern);
                Match match = regex.Match(val);
                return match.Success;
            }
            return false;
        }

        public static List<string> GetAllMatches(this string val, string regexPattern) {
            Regex regex = new Regex(regexPattern);
            Match match = regex.Match(val);

            List<string> result = new List<string>();

            while (match.Success) {
                string m = match.Groups[1].Value;
                result.Add(string.IsNullOrEmpty(m) ? match.Groups[0].Value : m);
                match = match.NextMatch();
            }

            return result;
        }

        private static T Parse<T>(string val, T defaultValue) {
            if (!string.IsNullOrWhiteSpace(val))
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(null, new CultureInfo("en-US"), val);
            return defaultValue;
        }

        public static double ParseSum(this string val, string regexPattern, double defaultValue = 0) {
            Regex regex = new Regex(regexPattern);
            Match match = regex.Match(val);

            double result = 0;

            while (match.Success) {
                result += Parse(match.Groups[1].Value, defaultValue);
                match = match.NextMatch();
            }

            return result;
        }

        public static double ParseDualAverage(this string val, string regexPattern, double defaultValue = 0) {
            Regex regex = new Regex(regexPattern);
            Match match = regex.Match(val);

            double result = 0;

            while (match.Success) {
                result += (Parse(match.Groups[1].Value, defaultValue) + Parse(match.Groups[2].Value, defaultValue)) / 2;
                match = match.NextMatch();
            }

            return result;
        }
    }
}