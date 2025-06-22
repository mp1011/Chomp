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

    public enum DebugLogFlags : byte
    {
        Misc = 1,
        SpriteSpawn = 2,
        WorldScroller = 4,
        LevelTransition = 8,
        All = 255
    }

    public static class GameDebug
    {
        public const bool BossTest = false;

        public const bool QuickReward = false;

        public const bool LevelSkipEnabled = false;

        public const bool InfiniteHp = true;

        public const bool OneHp = false;


        private const DebugLogFlags _debugLogFlags = DebugLogFlags.LevelTransition | DebugLogFlags.SpriteSpawn | DebugLogFlags.Misc;

        private static List<string> _log = new List<string>();

        public static bool EnableFly = true;
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

        public static void DebugLog(string message, DebugLogFlags flag)
        {
#if DEBUG
            if ((flag & _debugLogFlags) != 0)
            {
                _log.Add(message);
                Debug.WriteLine("LOG: " + message);
            }
#endif
        }
    }
}
