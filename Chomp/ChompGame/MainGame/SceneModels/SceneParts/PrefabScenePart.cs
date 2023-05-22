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


    class PrefabScenePart : BaseScenePart
    {
        private TwoBitEnum<PrefabSize> _width, _height;

        private MaskedByte _position; //5
        private GameBit _origin; //1
        private TwoBit _style; //todo

        public PrefabOrigin Origin => _origin.Value ? PrefabOrigin.TopOrRight : PrefabOrigin.BottomOrLeft;

        public override byte X => _scene.ScrollStyle switch {
            ScrollStyle.Horizontal => (byte)(_position.Value * 2),
             _ => 0 };

        public override byte Y => _scene.ScrollStyle switch {
            ScrollStyle.Vertical => (byte)(_position.Value * 2),
            ScrollStyle.Horizontal => 
                Origin == PrefabOrigin.TopOrRight
                    ? (byte)_scene.GetParallaxLayerTile(ParallaxLayer.ForegroundStart, false)
                    : (byte)(_scene.LevelTileHeight - _scene.BottomTiles - Height),
            _ => 0
        };

        public byte XEnd => (byte)(X + Width);
        public byte YEnd => (byte)(Y + Height);

        private int HeightIncrement => (_scene.LevelTileHeight - _scene.GetParallaxLayerTile(ParallaxLayer.ForegroundStart, false)) / 4;

        //todo, more options
        public byte Width => _scene.ScrollStyle switch {
            ScrollStyle.Vertical => (byte)_scene.LevelTileWidth,
            ScrollStyle.Horizontal => (byte)(4 * ((byte)_width.Value + 1)),
            _ => 0 };

    
        public byte Height => _scene.ScrollStyle switch {
            ScrollStyle.Vertical => (byte)(4 * ((byte)_width.Value + 1)),
            ScrollStyle.Horizontal => (byte)(HeightIncrement + (HeightIncrement * (byte)_height.Value)),
            _ => 0
        };

        public PrefabScenePart(SystemMemoryBuilder builder, 
            SceneDefinition scene, 
            byte position,  
            PrefabSize width,
            PrefabSize height,
            PrefabOrigin origin)
            : base(builder, ScenePartType.Prefab, scene)
        {
            _position = new MaskedByte(Address + 1, Bit.Right5, builder.Memory);           
            _width = new TwoBitEnum<PrefabSize>(builder.Memory, Address, 4);
            _height = new TwoBitEnum<PrefabSize>(builder.Memory, Address, 6);
            _origin = new GameBit(Address + 1, Bit.Bit5, builder.Memory);

            _position.Value = (byte)(position / 2);
            _width.Value = width;
            _height.Value = height;
            _origin.Value = origin == PrefabOrigin.TopOrRight;
        }

        public PrefabScenePart(SystemMemory memory, int address, SceneDefinition scene, Specs specs)
           : base(memory, address, scene, specs)
        {
            _position = new MaskedByte(Address + 1, Bit.Right5, memory);           
            _width = new TwoBitEnum<PrefabSize>(memory, Address, 4);
            _height = new TwoBitEnum<PrefabSize>(memory, Address, 6);
            _origin = new GameBit(Address + 1, Bit.Bit5, memory);
        }
    }
}
