using Chomp.SystemModels;
using System;

namespace Chomp.Models
{
    public abstract class Grid<T>
    {
        public int Width { get; }
        public int Height { get; }
        public int Cells => Width * Height;

        public abstract T this[int index] { get; }
        public T this[int x, int y] => this[(y * Width) + x];

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Grid(TableSpecs tableSpecs) : this(tableSpecs.Width,tableSpecs.Height)
        {
        }

        public void ForEach(Action<int,int,T> action)
        {
            for(int y =0; y < Height;y++)
            {
                for(int x=0; x < Width;x++)
                {
                    action(x, y, this[x, y]);
                }
            }
        }
    }

    public class MemoryGrid<T> : Grid<T>
        where T : IAddressable
    {
        private readonly T _firstCell;

        public MemoryGrid(TableSpecs tableSpecs, IAddressable firstCell) : base(tableSpecs)
        {
        }

        public override T this[int index] => throw new NotImplementedException();
    }
}
