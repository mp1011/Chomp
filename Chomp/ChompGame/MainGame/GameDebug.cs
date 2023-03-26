using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChompGame.MainGame
{
    public class DebugWatch
    {
        public string Name { get; }
        public Func<int> GetValue;

        public DebugWatch(string name, Func<int> getValue)
        {
            Name = name;
            GetValue = getValue;
        }
    }

    public static class GameDebug
    {
        private static List<string> _log = new List<string>();

        public static DebugWatch Watch1 { get; set; }
        public static DebugWatch Watch2 { get; set; }
        public static DebugWatch Watch3 { get; set; }
        public static DebugWatch Watch4 { get; set; }

        public static void NoOp() { }

        public static IEnumerable<DebugWatch> Watches
        {
            get
            {
                if (Watch1 != null) yield return Watch1;
                if (Watch2 != null) yield return Watch2;
                if (Watch3 != null) yield return Watch3;
                if (Watch4 != null) yield return Watch4;
            }
        }

        public static void DebugLog(string message)
        {
#if DEBUG
            _log.Add(message);
            Debug.WriteLine("LOG: " + message);
#endif
        }
    }
}
