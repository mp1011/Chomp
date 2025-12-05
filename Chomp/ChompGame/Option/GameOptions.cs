using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ChompGame.Option
{
    public class GameOptions
    {
        private const string Path = "options.ini";

        public bool UseCRT { get; set; } = true;
        public bool FullScreen { get; set; } = false;

        public Dictionary<GameKey, Keys> KeyboardBindings { get; } = new Dictionary<GameKey, Keys>();
        public Dictionary<GameKey, Buttons> GamePadBindings { get; } = new Dictionary<GameKey, Buttons>();
        public bool HasBindings => KeyboardBindings.Any() || GamePadBindings.Any();

        public void Save()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"UseCRT = {UseCRT}");
            sb.AppendLine($"FullScreen = {FullScreen}");

            foreach(var key in KeyboardBindings.Keys)
                sb.AppendLine($"Keyboard.{key} = {KeyboardBindings[key]}");

            foreach (var key in GamePadBindings.Keys)
                sb.AppendLine($"GamePad.{key} = {GamePadBindings[key]}");

            File.WriteAllText(Path, sb.ToString());
        }

        public static GameOptions Load()
        {
            var options = new GameOptions();
            if (!File.Exists(Path))
                return options;

            var lines = File.ReadAllLines(Path);
            foreach(var line in lines)
            {
                var parts = line.Split('=').Select(p => p.Trim()).ToArray();
                if (parts.Length != 2)
                    continue;

                if (parts[0] == "UseCRT")
                    options.UseCRT = bool.Parse(parts[1]);
                else if (parts[0] == "FullScreen")
                    options.FullScreen = bool.Parse(parts[1]);
                else
                    options.ParseKeyBinding(parts[0].Split('.'), parts[1]);
            }

            return options;
        }

        private void ParseKeyBinding(string[] key, string value)
        {
            if (key[0] == "Keyboard")
            {
                KeyboardBindings[Enum.Parse<GameKey>(key[1])] = Enum.Parse<Keys>(value);
            }
            else if (key[0] == "GamePad")
            {
                GamePadBindings[Enum.Parse<GameKey>(key[1])] = Enum.Parse<Buttons>(value);
            }
        }
    }
}
