using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ChompGame.Data
{
    public interface IGrid<T>
    {
        int Width { get; }
        int Height { get; }
        int Bytes { get; }
        T this[int index] { get;set; }
        T this[int x, int y] { get;set; }

        T ValueFromChar(char s);
    }

    public static class IGridExtensions
    {
        public static void ForEach<T>(this IGrid<T> grid, Point topLeft, Point bottomRight, Action<int,int,T> action)
        {
            for(int y = topLeft.Y; y < bottomRight.Y; y++)
            {
                for(int x = topLeft.X; x < bottomRight.X; x++)
                {
                    action(x, y, grid[x, y]);
                }
            }
        }

        public static void SetFromString<T>(this IGrid<T> grid, string block)
        {
            var lines = block.Split(Environment.NewLine)
                                .Select(p => p.Trim().Replace(" ",""))
                                .Where(p=>!string.IsNullOrEmpty(p))
                                .ToArray();

            for (int y = 0; y < lines.Length; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    grid[x, y] = grid.ValueFromChar(lines[y][x]);
                }
            }
        }
    }
}
