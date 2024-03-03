using ChompGame.Data;
using ChompGame.Data.Memory;

namespace ChompGame.MainGame.SpriteModels
{
    enum SpriteType : byte
    {
        Player,
        Lizard,
        LizardBullet,
        Bird,
        Bomb,
        Door,
        Platform,
        Explosion,
        Button,
        Chomp,
        BossBullet,
        LevelBoss,
        BossJaw,
        BossArm,
        Prize,
        PlayerHead,
        Plane,
        Rocket,
        Ogre,
        OgreBullet,
        Crocodile,
        Boulder,
        Mage,
        Ufo, //22
        Max=31
    }

    public enum EnemyIndex : byte
    {        
        Lizard=0,
        Bird,
        Rocket,
        Crocodile,
        Ogre,
        Boulder,
        Mage,
        Ufo
    }

    public enum SpriteGroup : byte
    {
        Normal,
        PlaneTakeoff,
        Plane,
        Boss
    }

    public enum GravityStrength : byte
    {
        None,
        Low,
        Medium,
        High
    }

    public enum MovementSpeed : byte
    {
        VerySlow,
        Slow,
        Fast,
        VeryFast
    }

    public enum AnimationStyle : byte
    {
        NoAnimation,
        AnimateWhenMoving,
        AnimateLowerTileOnly,
        AlwaysAnimate
    }

    class SpriteDefinition
    {
        public const int ByteLength = 2;

        private GameBit _sizeX;
        private GameBit _sizeY;

        private TwoBit _secondTileOffset;

        private TwoBitEnum<GravityStrength> _gravityStrength;
        private TwoBitEnum<AnimationStyle> _animationStyle;
        private TwoBitEnum<MovementSpeed> _movementSpeed;
        private GameBit _collidesWithBackground;
        private GameBit _flipXWhenMovingLeft;
        private GameBit _stopsAtLedges;

        public int SizeX => _sizeX.Value ? 2 : 1;
        public int SizeY => _sizeY.Value ? 2 : 1;

        public byte SecondTileOffset
        {
            get => _secondTileOffset.Value;
        }

        public bool FlipXWhenMovingLeft => _flipXWhenMovingLeft.Value;

        public bool CollidesWithBackground => _collidesWithBackground.Value;

        public MovementSpeed MovementSpeed => _movementSpeed.Value;
        public GravityStrength GravityStrength => _gravityStrength.Value;
        public AnimationStyle AnimationStyle => _animationStyle.Value;

        public bool StopsAtLedges => _stopsAtLedges.Value;

        public SpriteDefinition(SystemMemoryBuilder memoryBuilder,
            byte secondTileOffset,
            int sizeX,
            int sizeY,
            GravityStrength gravityStrength,
            MovementSpeed movementSpeed,
            AnimationStyle animationStyle,
            bool collidesWithBackground,
            bool flipXWhenMovingLeft,
            bool stopsAtLedges=false)
        {   
            //byte 1
            _secondTileOffset = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 0);
            _gravityStrength = new TwoBitEnum<GravityStrength>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 2);
            _movementSpeed = new TwoBitEnum<MovementSpeed>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 4);
            _animationStyle = new TwoBitEnum<AnimationStyle>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);
            memoryBuilder.AddByte();

            //byte 2
            _collidesWithBackground = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit0, memoryBuilder.Memory);
            _flipXWhenMovingLeft = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit1, memoryBuilder.Memory);
            _sizeX = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit2, memoryBuilder.Memory);
            _sizeY = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit3, memoryBuilder.Memory);
            _stopsAtLedges = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit4, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            //3 bits free

            _secondTileOffset.Value = secondTileOffset;

            _sizeX.Value = (sizeX == 2);
            _sizeY.Value = (sizeY == 2);

            _animationStyle.Value = animationStyle;
            _collidesWithBackground.Value = collidesWithBackground;
            _flipXWhenMovingLeft.Value = flipXWhenMovingLeft;
            _gravityStrength.Value = gravityStrength;
            _movementSpeed.Value = movementSpeed;
            _stopsAtLedges.Value = stopsAtLedges;
        }

        public SpriteDefinition(SystemMemory memory, int address)
        {
            _secondTileOffset = new TwoBit(memory, address, 0);
            _gravityStrength = new TwoBitEnum<GravityStrength>(memory, address, 2);
            _movementSpeed = new TwoBitEnum<MovementSpeed>(memory, address, 4);
            _animationStyle = new TwoBitEnum<AnimationStyle>(memory, address, 6);

            _collidesWithBackground = new GameBit(address + 1, Bit.Bit0, memory);
            _flipXWhenMovingLeft = new GameBit(address + 1, Bit.Bit1, memory);
            _sizeX = new GameBit(address + 1, Bit.Bit2, memory);
            _sizeY = new GameBit(address + 1, Bit.Bit3, memory);
            _stopsAtLedges = new GameBit(address + 1, Bit.Bit4, memory);
        }

        public SpriteDefinition(SpriteType spriteType, SystemMemory memory) :
            this(memory, memory.GetAddress(AddressLabels.SpriteDefinitions) + ((int) spriteType * ByteLength))
        {
        }
    }
}
