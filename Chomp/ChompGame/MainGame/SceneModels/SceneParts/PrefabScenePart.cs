using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using System;

namespace ChompGame.MainGame.SceneModels.SceneParts
{
    enum PrefabSize : byte
    {
        Two,
        Four,
        Six,
        Eight
    }

    enum PrefabStyle : byte
    {
        Block,
        Space,
        StairUp,
        StairDown
    }

    class PrefabScenePart : BaseScenePart
    {
        private TwoBitEnum<PrefabSize> _width, _height;

        private MaskedByte _longPosition; //4
        private MaskedByte _shortPosition; //6

        private MaskedByte _ntX; //3
        private MaskedByte _ntY; //3


        private TwoBitEnum<PrefabStyle> _shape;



        public override byte X => _scene.ScrollStyle switch {
            ScrollStyle.Horizontal => (byte)(_longPosition.Value * 4),
            ScrollStyle.NameTable => (byte)(_ntX.Value * 4),
            ScrollStyle.Vertical => (byte)(_shortPosition.Value * 4),
            _ => (byte)(_longPosition.Value * 4),
        };

        public override byte Y => _scene.ScrollStyle switch {
            ScrollStyle.Vertical => (byte)(_longPosition.Value * 4),
            ScrollStyle.NameTable => (byte)(_ntY.Value * 4),
            ScrollStyle.Horizontal => (byte)(_shortPosition.Value * 4),
            _ => (byte)(_shortPosition.Value * 4),
        };

        public byte XEnd => (byte)(X + Width);
        public byte YEnd => (byte)(Y + Height);
       

        public PrefabStyle Shape => _shape.Value;

        public byte Width => (byte)((_width.ByteValue + 1) * 2);
    
        public byte Height => (byte)((_height.ByteValue + 1) * 2);


        /// <summary>
        /// 0-31
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="scene"></param>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="origin"></param>
        /// <param name="shape"></param>
        public PrefabScenePart(SystemMemoryBuilder builder, 
            SceneDefinition scene, 
            byte x,
            byte y,
            PrefabSize width,
            PrefabSize height,
            PrefabStyle shape)
            : base(builder, ScenePartType.Prefab, scene)
        {
            _longPosition = new MaskedByte(Address + 1, Bit.Right4, builder.Memory);
            _shortPosition = new MaskedByte(Address + 1, (Bit)48, builder.Memory, 4);
            _ntX = new MaskedByte(Address + 1, Bit.Right3, builder.Memory);
            _ntY = new MaskedByte(Address + 1, (Bit)56, builder.Memory, 3);

            _width = new TwoBitEnum<PrefabSize>(builder.Memory, Address, 4);
            _height = new TwoBitEnum<PrefabSize>(builder.Memory, Address, 6);
            _shape = new TwoBitEnum<PrefabStyle>(builder.Memory, Address + 1, 6);
            _width.Value = width;
            _height.Value = height;
            _shape.Value = shape;

            switch (_scene.ScrollStyle)
            {
                case ScrollStyle.Vertical:
                    _shortPosition.Value = (byte)(x / 4);
                    _longPosition.Value = (byte)(y / 4);
                    break;
                case ScrollStyle.NameTable:
                    _ntX.Value = (byte)(x / 4);
                    _ntY.Value = (byte)(y / 4);
                    break;
                default:
                    _longPosition.Value = (byte)(x / 4);
                    _shortPosition.Value = (byte)(y / 4);
                    break;
            }
        }

        public PrefabScenePart(SystemMemory memory, int address, SceneDefinition scene, Specs specs)
           : base(memory, address, scene, specs)
        {
            _longPosition = new MaskedByte(Address + 1, Bit.Right4, memory);
            _shortPosition = new MaskedByte(Address + 1, (Bit)48, memory, 4);
            _ntX = new MaskedByte(Address + 1, Bit.Right3, memory);
            _ntY = new MaskedByte(Address + 1, (Bit)56, memory, 3);

            _width = new TwoBitEnum<PrefabSize>(memory, Address, 4);
            _height = new TwoBitEnum<PrefabSize>(memory, Address, 6);
            _shape = new TwoBitEnum<PrefabStyle>(memory, Address + 1, 6);
        }
    }
}
