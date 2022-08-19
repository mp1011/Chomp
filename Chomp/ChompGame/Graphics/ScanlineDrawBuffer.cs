using ChompGame.Data;
using ChompGame.GameSystem;

namespace ChompGame.Graphics
{
    public abstract class ScanlineDrawBuffer
    {
        protected readonly Specs _specs;

        public ScanlineDrawBuffer(Specs specs)
        {
            _specs = specs;
        }

        public abstract byte this[int index]
        {
            get;
            set;
        }
    }

    public class TwoBitPixelScanlineDrawBuffer : ScanlineDrawBuffer
    {
        private readonly TwoBitArray _pixels;

        public TwoBitPixelScanlineDrawBuffer(SystemMemoryBuilder memoryBuilder, Specs specs) : base(specs)
        {
            _pixels = memoryBuilder.AddTwoBitArray(specs.ScreenWidth);
        }

        public override byte this[int index] 
        {
            get => _pixels[index];
            set => _pixels[index] = value;
        }
    }
}
