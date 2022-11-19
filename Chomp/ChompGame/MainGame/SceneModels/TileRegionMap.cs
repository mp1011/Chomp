using ChompGame.Data;

namespace ChompGame.MainGame.SceneModels
{
    /// <summary>
    /// Maps regions of the level's tile master table to regions on the level map
    /// </summary>
    public class TileRegionMap
    {
        public const int ByteLength = 6;

        public NibbleRectangle Source { get; }
        public ByteRectangle Destination { get; }


        public TileRegionMap(int address, SystemMemory systemMemory)
        {
            Source = new NibbleRectangle(address, systemMemory);
            Destination = new ByteRectangle(address + 2, systemMemory);
        }
        public TileRegionMap(SystemMemoryBuilder memoryBuilder)
        {
            Source = new NibbleRectangle(memoryBuilder);
            Destination = new ByteRectangle(memoryBuilder);
        }

        public TileRegionMap SetSource(byte x, byte y, byte width, byte height)
        {
            Source.X = x;
            Source.Y = y;
            Source.Width = width;
            Source.Height = height;

            return this;
        }

        public TileRegionMap SetDestination(byte x, byte y, byte width, byte height)
        {
            Destination.X = x;
            Destination.Y = y;
            Destination.Width = width;
            Destination.Height = height;

            return this;
        }


    }
}
