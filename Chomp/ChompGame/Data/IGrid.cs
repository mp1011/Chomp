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

        T ValueFromChar(char s, int offset);
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

        public static Point Find<T>(this IGrid<T> grid, Point begin, int changeX, int changeY, Predicate<T> condition)
        {
            var position = begin;
            while(position.X >= 0 && 
                position.Y >= 0 &&
                position.X < grid.Width &&
                position.Y < grid.Height)
            {
                if (condition(grid[position.X, position.Y]))
                    return position;

                position = new Point(position.X + changeX, position.Y + changeY);
            }

            return new Point(255, 255);
        }

        public static void ForEach<T>(this IGrid<T> grid, Action<int, int, T> action)
        {
            grid.ForEach(Point.Zero, new Point(grid.Width, grid.Height), action);
        }

        public static void SetFromString<T>(this IGrid<T> grid, string block, Func<T, bool> shouldReplace = null)
        {
            grid.SetFromString(0, 0, 0, block, shouldReplace);
        }

        public static void SetFromString<T>(this IGrid<T> grid, int tileStart, string block, Func<T,bool> shouldReplace=null)
        {
            grid.SetFromString(0, 0, tileStart, block, shouldReplace);
        }

        public static void SetFromString<T>(this IGrid<T> grid, 
            int startX, 
            int startY, 
            int tileStart,
            string block,
            Func<T, bool> shouldReplace = null)
        {
            int maxLineLength = grid.Width - startX;
            if (maxLineLength <= 0)
                return;
            var lines = block.Split(Environment.NewLine)
                                .Select(p =>
                                {
                                    var line = p.Trim().Replace(" ", "");
                                    if (line.Length > maxLineLength)
                                        line = line.Substring(0, maxLineLength);
                                    return line;
                                })
                                .Where(p=>!string.IsNullOrEmpty(p))
                                .ToArray();

            for (int y = 0; y < lines.Length; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    if (shouldReplace == null || shouldReplace(grid[startX + x, startY + y]))
                    {
                        grid[startX + x, startY + y] = grid.ValueFromChar(lines[y][x], tileStart);
                    }
                }
            }
        }
    }
}
