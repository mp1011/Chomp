using ChompGame.Data;
using ChompGame.Data.Memory;

namespace ChompGame.MainGame
{
    class PrecisionMotion : IMotion
    {
        public const int Bytes = 4;
        public int Address { get; }

        private ByteVector _motion;
        private ByteVector _subPixel;
        
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
            _subPixel = new ByteVector(memoryBuilder.AddByte(), memoryBuilder.AddByte());
            _motion.X = 0;
            _motion.Y = 0;
            _subPixel.X = 0;
            _subPixel.Y = 0;
        }

        public PrecisionMotion(SystemMemory memory, int address)
        {
            Address = address;
            _motion = new ByteVector(new GameByte(address, memory), new GameByte(address + 1, memory));
            _motion = new ByteVector(new GameByte(address + 2, memory), new GameByte(address + 3, memory));
            _motion.X = 0;
            _motion.Y = 0;
            _subPixel.X = 0;
            _subPixel.Y = 0;
        }

        public void Apply(IWithPosition sprite)
        {
            int moveX = 0;
            int newSubX = _subPixel.X + _motion.X;
            if(newSubX >= 128)
            {
                moveX = 2;
                newSubX -= 128;
            }
            else if(newSubX >= 64)
            {
                moveX = 1;
                newSubX -= 64;
            }
            else if (newSubX <= -128)
            {
                moveX = -2;
                newSubX += 128;
            }
            else if (newSubX <= -64)
            {
                moveX = -1;
                newSubX += 64;
            }

            _subPixel.X = newSubX;
            sprite.X += moveX;


            int moveY = 0;
            int newSubY = _subPixel.Y + _motion.Y;
            if (newSubY >= 128)
            {
                moveY = 2;
                newSubY -= 128;
            }
            else if (newSubY >= 64)
            {
                moveY = 1;
                newSubY -= 64;
            }
            else if (newSubY <= -128)
            {
                moveY = -2;
                newSubY += 128;
            }
            else if (newSubY <= -64)
            {
                moveY = -1;
                newSubY += 64;
            }

            _subPixel.Y = newSubY;
            sprite.Y += moveY;

        }

        public void Stop()
        {
            XSpeed = 0;
            YSpeed = 0;
        }
    }

}
