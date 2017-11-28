using System;

namespace Tornado.Overlay.Framework {
    public enum TimeActionType {
        DoWhileActive,
        Periodical
    }

    public class TimeResolver<T> {
        private TimeSpan range { get; }
        private DateTime lastActionTime { get; set; }
        private Func<T> action { get; }
        private T result { get; set; }
        private Func<T> resolver { get; }
        public T Value => resolver();

        public TimeResolver(TimeSpan range, Func<T> action, TimeActionType type) {
            this.range = range;
            this.action = action;

            switch(type) {
                case TimeActionType.DoWhileActive :
                    resolver = getDoWhileActive;
                    break;
                case TimeActionType.Periodical :
                    resolver = getPeriod;
                    break;
                default :
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            lastActionTime = DateTime.Now.Add(-range);
            result = action();
        }

        private T getPeriod() {
            if (DateTime.Now - lastActionTime > range) {
                result = action();
                lastActionTime = DateTime.Now;
            }
            return result;
        }

        private T getDoWhileActive() {
            if (DateTime.Now - lastActionTime < range)
                result = action();
            return result;
        }

        public void Reset() {
            lastActionTime = DateTime.Now;
        }
    }
}