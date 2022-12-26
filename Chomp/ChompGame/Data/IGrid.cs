using ChompGame.Extensions;
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
                    action(x, y, grid[x.NMod(grid.Width), y.NMod(grid.Height)]);
                }
            }
        }

        public static void ForEach<T>(this IGrid<T> grid, Action<int, int, T> action)
        {
            grid.ForEach(Point.Zero, new Point(grid.Width, grid.Height), action);
        }

        public static void SetFromString<T>(this IGrid<T> grid, string block)
        {
            grid.SetFromString(0, 0, block);
        }

        public static void SetFromString<T>(this IGrid<T> grid, int startX, int startY, string block)
        {
            var lines = block.Split(Environment.NewLine)
                                .Select(p => p.Trim().Replace(" ",""))
                                .Where(p=>!string.IsNullOrEmpty(p))
                                .ToArray();

            for (int y = 0; y < lines.Length; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    grid[startX + x, startY + y] = grid.ValueFromChar(lines[y][x]);
                }
            }
        }
    }
}
