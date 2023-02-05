using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.Graphics
{
    public abstract class ScanlineDrawBuffer
    {
        protected readonly Specs _specs;

        public abstract int Address { get; }

        public abstract int BitsPerPixel { get; }

        public ScanlineDrawBuffer(Specs specs)
        {
            _specs = specs;
        }

        public abstract byte this[int index]
        {
            get;
            set;
        }

        public abstract void Clear(SystemMemory memory);
    }

    public class TwoBitPixelScanlineDrawBuffer : ScanlineDrawBuffer
    {
        private readonly Specs _specs;
        private readonly TwoBitArray _pixels;

        public override int Address => _pixels.Address;

        public override int BitsPerPixel => 2;

        public int ByteLength => (BitsPerPixel * _specs.ScreenWidth) / 8;

        public TwoBitPixelScanlineDrawBuffer(SystemMemoryBuilder memoryBuilder, Specs specs) : base(specs)
        {
            _specs = specs;
            _pixels = memoryBuilder.AddTwoBitArray(specs.ScreenWidth);
        }

        public override byte this[int index] 
        {
            get => _pixels[index];
            set => _pixels[index] = value;
        }

        public override void Clear(SystemMemory memory)
        {
            for(int i =0; i < ByteLength; i++)
            {
                memory[Address + i] = 0;
            }
        }
    }
}
