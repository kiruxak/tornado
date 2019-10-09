using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using Tornado.Common.Extensions;
using Tornado.Parser.Common.Extensions;

namespace Tornado.Parser.Data.Bases {
    public class DivCard {
        public string Name { get; private set; }
        public int Count { get; private set; }
        public string Description { get; private set; }
        public string Locations { get; private set; }
        public string Additional { get; private set; }

        public DivCard(Row row, SharedStringTable sh) {
            var cells = row.Elements<Cell>().ToList();
            Name        = cells[0].GetValue(sh);
            Count       = cells[1].GetValue(sh).ParseTo(0);
            Description = cells[2].GetValue(sh);
            Locations   = cells[3].GetValue(sh);
            Additional  = cells[4].GetValue(sh);
        }
    }
}
