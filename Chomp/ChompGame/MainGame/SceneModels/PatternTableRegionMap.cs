using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels
{
    /// <summary>
    /// Maps regions of the master pattern table to the VRAM pattern table
    /// </summary>
    public class PatternTableRegionMap
    {
        public const int ByteLength = 3;

        private NibbleRectangle _region;
        private NibblePoint _destination;

        public Point TileDestination => new Point(_destination.X, _destination.Y);

        public ByteRectangle TileRegion => _region;

        public PatternTableRegionMap(int address, SystemMemory systemMemory)
        {
            _region = new NibbleRectangle(address, systemMemory);
            _destination = new NibblePoint(address + 2, systemMemory);
        }
    }
}
