using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Tornado.Common.Extensions {
    public static class OpenXmlExtensions {
        public static string GetValue(this Cell cell, SharedStringTable sh) {
            return cell.DataType != null && cell.DataType == CellValues.SharedString ? sh.ElementAt(int.Parse(cell.InnerText)).InnerText : cell.CellValue == null ? "" : cell.CellValue.InnerText;
        }
    }
}