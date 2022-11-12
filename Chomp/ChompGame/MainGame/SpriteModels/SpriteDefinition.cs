using ChompGame.Data;

namespace ChompGame.MainGame.SpriteModels
{
    enum SpriteType : byte
    {
        Player,
        Lizard,
        LizardBullet,
        Max=31
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
        public const int ByteLength = 3;

        private GameEnum2<Orientation> _orientation;
        private TwoBit _palette;
        private TwoBit _secondTileOffset;

        private MaskedByte _tile;
        private GameByteEnum<SpriteType> _spriteType;

        private TwoBitEnum<GravityStrength> _gravityStrength;
        private TwoBitEnum<AnimationStyle> _animationStyle;
        private TwoBitEnum<MovementSpeed> _movementSpeed;
        private GameBit _collidesWithBackground;
        private GameBit _flipXWhenMovingLeft;

        public Orientation Orientation
        {
            get => _orientation.Value;
            set => _orientation.Value = value;
        }

        public byte Palette
        {
            get => _palette.Value;
        }

        public byte Tile
        {
            get => _tile.Value;
        }

        public byte SecondTileOffset
        {
            get => _secondTileOffset.Value;
        }

        public SpriteType SpriteType
        {
            get => _spriteType.Value;
        }

        public bool FlipXWhenMovingLeft => _flipXWhenMovingLeft.Value;

        public bool CollidesWithBackground => _collidesWithBackground.Value;

        public MovementSpeed MovementSpeed => _movementSpeed.Value;
        public GravityStrength GravityStrength => _gravityStrength.Value;
        public AnimationStyle AnimationStyle => _animationStyle.Value;

        public SpriteDefinition(SystemMemoryBuilder memoryBuilder,
            SpriteType spriteType,
            byte tile,
            byte secondTileOffset,
            byte palette,
            Orientation orientation,
            GravityStrength gravityStrength,
            MovementSpeed movementSpeed,
            AnimationStyle animationStyle,
            bool collidesWithBackground,
            bool flipXWhenMovingLeft)
        {
            _spriteType = new GameByteEnum<SpriteType>(
                memoryBuilder.AddMaskedByte(Bit.Right5));

            _palette = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress - 1, 5);
            _orientation = new GameEnum2<Orientation>(memoryBuilder.CurrentAddress - 1, Bit.Bit7, memoryBuilder.Memory);

            _tile = memoryBuilder.AddMaskedByte(Bit.Right6);
            _secondTileOffset = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress - 1, 6);

            _gravityStrength = new TwoBitEnum<GravityStrength>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 0);
            _movementSpeed = new TwoBitEnum<MovementSpeed>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 2);
            _animationStyle = new TwoBitEnum<AnimationStyle>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 4);
            _collidesWithBackground = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit6, memoryBuilder.Memory);
            _flipXWhenMovingLeft = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit7, memoryBuilder.Memory);

            memoryBuilder.AddByte();

            _spriteType.Value = spriteType;
            _tile.Value = tile;
            _secondTileOffset.Value = secondTileOffset;
            _palette.Value = palette;
            _orientation.Value = orientation;
            _animationStyle.Value = animationStyle;
            _collidesWithBackground.Value = collidesWithBackground;
            _flipXWhenMovingLeft.Value = flipXWhenMovingLeft;
            _gravityStrength.Value = gravityStrength;
            _movementSpeed.Value = movementSpeed;
        }

        public SpriteDefinition(SystemMemory memory, int address)
        {
            _spriteType = new GameByteEnum<SpriteType>(new MaskedByte(address, Bit.Right5, memory));

            _palette = new TwoBit(memory, address, 5);
            _orientation = new GameEnum2<Orientation>(address, Bit.Bit7, memory);

            _tile = new MaskedByte(address + 1, Bit.Right6, memory);
            _secondTileOffset = new TwoBit(memory, address + 1, 6);

            _gravityStrength = new TwoBitEnum<GravityStrength>(memory, address + 2, 0);
            _movementSpeed = new TwoBitEnum<MovementSpeed>(memory, address + 2, 2);
            _animationStyle = new TwoBitEnum<AnimationStyle>(memory, address + 2, 4);
            _collidesWithBackground = new GameBit(address + 2, Bit.Bit6, memory);
            _flipXWhenMovingLeft = new GameBit(address + 2, Bit.Bit7, memory);
        }

        public SpriteDefinition(SpriteType spriteType, SystemMemory memory) :
            this(memory, memory.GetAddress(AddressLabels.SpriteDefinitions) + ((int) spriteType * ByteLength))
        {
        }
    }
}
