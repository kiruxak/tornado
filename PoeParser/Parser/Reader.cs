using System;
using System.Collections.Generic;
using System.IO;

namespace PoeParser.Parser {
    public static class FileReader {
        public static List<T> ParseTo<T>(string path, bool ignoreFirstLine = true) where T : IRecord {
            List<T> result = new List<T>();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                using (StreamReader sr = new StreamReader(fs)) {
                    if (ignoreFirstLine) {
                        sr.ReadLine();
                    }
                    while (!sr.EndOfStream) {
                        string line = sr.ReadLine();
                        T record = (T)Activator.CreateInstance(typeof(T));
                        record.Parse(line);
                        result.Add(record);
                    }
                }
            }
            return result;
        }
        public static string Read(string path) {
            string result = "";
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                using (StreamReader sr = new StreamReader(fs)) {
                    while (!sr.EndOfStream) {
                        result += sr.ReadLine() + Environment.NewLine;
                    }
                }
            }
            return result;
        }
        public static List<string> ReadToList(string path) {
            List<string> result = new List<string>();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                using (StreamReader sr = new StreamReader(fs)) {
                    while (!sr.EndOfStream) {
                        result.Add(sr.ReadLine());
                    }
                }
            }
            return result;
        }
    }
}