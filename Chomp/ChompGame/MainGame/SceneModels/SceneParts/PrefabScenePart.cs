using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using System;

namespace ChompGame.MainGame.SceneModels.SceneParts
{
    enum PrefabSize : byte
    {
        Quarter,
        Half,
        ThreeQuarter,
        Full
    }

    enum PrefabOrigin : byte
    {
        BottomOrLeft=0,
        TopOrRight=1,
    }

    enum PrefabShape : byte
    {
        Block,
        StairLeft,
        StairRight,
        StairBoth
    }

    class PrefabScenePart : BaseScenePart
    {
        private TwoBitEnum<PrefabSize> _width, _height;

        private MaskedByte _position; //5
        private GameBit _origin; //1
        private TwoBitEnum<PrefabShape> _shape;

        public PrefabOrigin Origin => _origin.Value ? PrefabOrigin.TopOrRight : PrefabOrigin.BottomOrLeft;

        public override byte X => _scene.ScrollStyle switch {
            ScrollStyle.Horizontal => (byte)(_position.Value * 2),
            ScrollStyle.NameTable => (byte)(_position.Value * 2),
            _ => 0 };

        public override byte Y => _scene.ScrollStyle switch {
            ScrollStyle.Vertical => (byte)(_position.Value * 2),
            ScrollStyle.NameTable =>
                Origin == PrefabOrigin.TopOrRight
                    ? (byte)_scene.TopTiles
                    : (byte)(_scene.LevelTileHeight - _scene.BottomTiles - Height),
            ScrollStyle.Horizontal =>
                Origin == PrefabOrigin.TopOrRight
                    ? (byte)_scene.GetBackgroundLayerTile(BackgroundLayer.ForegroundStart, false)
                    : (byte)(_scene.LevelTileHeight - _scene.BottomTiles - Height),
            _ => 0
        };

        public byte XEnd => (byte)(X + Width);
        public byte YEnd => (byte)(Y + Height);

        private int HeightIncrement  
        {
            get
            {
                var h = (_scene.LevelTileHeight - _scene.GetBackgroundLayerTile(BackgroundLayer.ForegroundStart, false)) / 4;
                return (h / 2) * 2;
            }
        }

        private int WidthIncrement
        {
            get
            {
                return (_scene.LevelTileWidth - _scene.LeftTiles - _scene.RightTiles) / 4;
            }
        }

        public PrefabShape Shape => _shape.Value;

        public byte Width => _scene.ScrollStyle switch {
            ScrollStyle.Vertical => (byte)_scene.LevelTileWidth,
            ScrollStyle.Horizontal => (byte)(4 * ((byte)_width.Value + 1)),
            ScrollStyle.NameTable => (byte)(WidthIncrement + (WidthIncrement * (byte)_height.Value)),
            _ => 0 };

    
        public byte Height => _scene.ScrollStyle switch {
            ScrollStyle.Vertical => (byte)(4 * ((byte)_width.Value + 1)),
            ScrollStyle.Horizontal => (byte)(HeightIncrement + (HeightIncrement * (byte)_height.Value)),
            ScrollStyle.NameTable => (byte)(HeightIncrement + (HeightIncrement * (byte)_height.Value)),
            _ => 0
        };

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
            byte position,  
            PrefabSize width,
            PrefabSize height,
            PrefabOrigin origin,
            PrefabShape shape)
            : base(builder, ScenePartType.Prefab, scene)
        {
            _position = new MaskedByte(Address + 1, Bit.Right5, builder.Memory);           
            _width = new TwoBitEnum<PrefabSize>(builder.Memory, Address, 4);
            _height = new TwoBitEnum<PrefabSize>(builder.Memory, Address, 6);
            _origin = new GameBit(Address + 1, Bit.Bit5, builder.Memory);
            _shape = new TwoBitEnum<PrefabShape>(builder.Memory, Address + 1, 6);
            _position.Value = (byte)(position / 2);
            _width.Value = width;
            _height.Value = height;
            _origin.Value = origin == PrefabOrigin.TopOrRight;
            _shape.Value = shape;
        }

        public PrefabScenePart(SystemMemory memory, int address, SceneDefinition scene, Specs specs)
           : base(memory, address, scene, specs)
        {
            _position = new MaskedByte(Address + 1, Bit.Right5, memory);           
            _width = new TwoBitEnum<PrefabSize>(memory, Address, 4);
            _height = new TwoBitEnum<PrefabSize>(memory, Address, 6);
            _origin = new GameBit(Address + 1, Bit.Bit5, memory);
            _shape = new TwoBitEnum<PrefabShape>(memory, Address + 1, 6);
        }
    }
}
