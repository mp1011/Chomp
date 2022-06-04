using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Chomp.Models
{
    public interface IPalette : IEnumerable<Color>
    {
        Color this[byte index] { get; }
        byte GetIndex(Color color);
    }

    public class SystemPalette : IEnumerable<Color>, IPalette
    {
        private readonly Color[] _colors;

        public Color this[byte b] => _colors[b % _colors.Length];

        public SystemPalette(IEnumerable<Color> colors)
        {

            _colors = new Color[] { new Color(0, 0, 0, 0) }
                        .Union(colors.Where(p => p.A == 255))
                        .ToArray();
        }

        public byte GetIndex(Color color)
        {
            return (byte)Array.IndexOf(_colors, color);
        }

        public byte? GetIndexOrDefault(Color color)
        {
            var index = Array.IndexOf(_colors, color);
            if (index == -1)
                return null;
            else
                return (byte)index;
        }

        public IEnumerator<Color> GetEnumerator()
        {
            return ((IEnumerable<Color>)_colors).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _colors.GetEnumerator();
        }
    }

    public class Palette<T> : IAddressable, IPalette
        where T:MemoryValue
    {
        private SystemPalette _systemPalette;
        public Array<T> Colors { get; }

        public int Address => Colors[0].Address;

        public Palette(SystemPalette systemPalette)
        {
            _systemPalette = systemPalette;
            throw new System.NotImplementedException();
        }

        public byte GetIndex(Color color)
        {
            byte i = 0;
            while (i < Colors.Length)
            {
                if (this[i] == color)
                    return i;

                i++;
            }

            return 0;
        }

        public void Set(SystemPalette colors)
        {
            //todo Set(colors.Select(c => _systemPalette.GetIndex(c)));
        }

        public void Set(IEnumerable<T> data)
        {
            int i = 0;
            foreach (var item in data)
                Colors[i++] = item;
        }

        private IEnumerable<Color> GetAllColors()
        {
            byte i = 0;
            while (i < Colors.Length)
                yield return this[i++];
        }

        public IEnumerator<Color> GetEnumerator() => GetAllColors().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetAllColors().GetEnumerator();

        //todo
        public Color this[byte index] => Color.Black;
           // (index < Colors.Length) ? _systemPalette[Colors[index]] : Color.Black;

    }


}
