﻿using ChompGame.Data;
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

        private GameBit _sizeX;
        private GameBit _sizeY;

        private TwoBit _secondTileOffset;

        private MaskedByte _tile;

        private TwoBitEnum<GravityStrength> _gravityStrength;
        private TwoBitEnum<AnimationStyle> _animationStyle;
        private TwoBitEnum<MovementSpeed> _movementSpeed;
        private GameBit _collidesWithBackground;
        private GameBit _flipXWhenMovingLeft;

        public int SizeX => _sizeX.Value ? 2 : 1;
        public int SizeY => _sizeY.Value ? 2 : 1;

        public byte Tile
        {
            get => _tile.Value;
        }

        public byte SecondTileOffset
        {
            get => _secondTileOffset.Value;
        }

        public bool FlipXWhenMovingLeft => _flipXWhenMovingLeft.Value;

        public bool CollidesWithBackground => _collidesWithBackground.Value;

        public MovementSpeed MovementSpeed => _movementSpeed.Value;
        public GravityStrength GravityStrength => _gravityStrength.Value;
        public AnimationStyle AnimationStyle => _animationStyle.Value;

        public SpriteDefinition(SystemMemoryBuilder memoryBuilder,
            byte tile,
            byte secondTileOffset,
            int sizeX,
            int sizeY,
            GravityStrength gravityStrength,
            MovementSpeed movementSpeed,
            AnimationStyle animationStyle,
            bool collidesWithBackground,
            bool flipXWhenMovingLeft)
        {   
            _tile = memoryBuilder.AddMaskedByte(Bit.Right6);
            _secondTileOffset = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress - 1, 6);

            _gravityStrength = new TwoBitEnum<GravityStrength>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 0);
            _movementSpeed = new TwoBitEnum<MovementSpeed>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 2);
            _animationStyle = new TwoBitEnum<AnimationStyle>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 4);
            _collidesWithBackground = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit6, memoryBuilder.Memory);
            _flipXWhenMovingLeft = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit7, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            _sizeX = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit0, memoryBuilder.Memory);
            _sizeY = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit1, memoryBuilder.Memory);
            //6 bits left 
            memoryBuilder.AddByte();

            _tile.Value = tile;
            _secondTileOffset.Value = secondTileOffset;

            _sizeX.Value = (sizeX == 2);
            _sizeY.Value = (sizeY == 2);

            _animationStyle.Value = animationStyle;
            _collidesWithBackground.Value = collidesWithBackground;
            _flipXWhenMovingLeft.Value = flipXWhenMovingLeft;
            _gravityStrength.Value = gravityStrength;
            _movementSpeed.Value = movementSpeed;
        }

        public SpriteDefinition(SystemMemory memory, int address)
        {
            _tile = new MaskedByte(address, Bit.Right6, memory);
            _secondTileOffset = new TwoBit(memory, address, 6);

            _gravityStrength = new TwoBitEnum<GravityStrength>(memory, address + 1, 0);
            _movementSpeed = new TwoBitEnum<MovementSpeed>(memory, address + 1, 2);
            _animationStyle = new TwoBitEnum<AnimationStyle>(memory, address + 1, 4);
            _collidesWithBackground = new GameBit(address + 1, Bit.Bit6, memory);
            _flipXWhenMovingLeft = new GameBit(address + 1, Bit.Bit7, memory);

            _sizeX = new GameBit(address + 2, Bit.Bit0, memory);
            _sizeY = new GameBit(address + 2, Bit.Bit1, memory);
        }

        public SpriteDefinition(SpriteType spriteType, SystemMemory memory) :
            this(memory, memory.GetAddress(AddressLabels.SpriteDefinitions) + ((int) spriteType * ByteLength))
        {
        }
    }
}
