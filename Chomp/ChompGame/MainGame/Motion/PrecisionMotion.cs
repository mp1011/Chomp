using ChompGame.Data;
using ChompGame.Data.Memory;

namespace ChompGame.MainGame
{
    class PrecisionMotion : IMotion
    {
        public const int Bytes = 4;
        public int Address { get; }

        private byte _motionScale = 4;
        private ByteVector _motion;
        private GameByte _subPixelX;
        private GameByte _subPixelY;
        
        public int XSpeed
        {
            get => _motion.X;
            set => _motion.X = value;
        }

        public int YSpeed
        {
            get => _motion.Y;
            set => _motion.Y = value;
        }

        public int TargetXSpeed => XSpeed;

        public PrecisionMotion(SystemMemoryBuilder memoryBuilder)
        {
            Address = memoryBuilder.CurrentAddress;
            _motion = new ByteVector(memoryBuilder.AddByte(), memoryBuilder.AddByte());
            _subPixelX = memoryBuilder.AddByte();
            _subPixelY = memoryBuilder.AddByte();
        }

        public PrecisionMotion(SystemMemory memory, int address)
        {
            Address = address;
            _motion = new ByteVector(new GameByte(address, memory), new GameByte(address + 1, memory));
            _subPixelX = new GameByte(address + 2, memory);
            _subPixelY = new GameByte(address + 3, memory);
        }

        public void Apply(IWithPosition sprite)
        {
            int sx = _subPixelX.Value;
            sx += _motion.X * _motionScale;

            var pixelX = 0;
            if (sx >= 256)
                pixelX = 1;
            else if (sx < 0)
                pixelX = -1;

            _subPixelX.Value = (byte)(sx % 256);
            if (pixelX != 0)
            {
                sprite.X = sprite.X + pixelX;
            }

            int sy = _subPixelY.Value;
            sy += _motion.Y * _motionScale;

            var pixelY = 0;
            if (sy >= 256)
                pixelY = 1;
            else if (sy < 0)
                pixelY = -1;

            _subPixelY.Value = (byte)(sy % 256);
            if (pixelY != 0)
            {
                sprite.Y = sprite.Y + pixelY;
            }
        }

        public void Stop()
        {
            XSpeed = 0;
            YSpeed = 0;
        }
    }

}
