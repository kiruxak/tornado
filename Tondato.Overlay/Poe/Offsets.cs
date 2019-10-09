namespace Tornado.Overlay.Poe {
    public class Offsets {
        public static Offsets Regular = new Offsets {
            IgsOffset = 0,
            IgsDelta = 0,
            ExeName = "PathOfExile"
        };
        public static Offsets Steam = new Offsets {
            IgsOffset = 24,
            IgsDelta = 0,
            ExeName = "PathOfExileSteam"
        };

        public static Offsets Notepad = new Offsets
        {
            IgsOffset = 24,
            IgsDelta  = 0,
            ExeName   = "Notepad"
        };

        public string ExeName { get; private set; }
        public int IgsDelta { get; private set; }
        public int IgsOffset { get; private set; }
    }
}