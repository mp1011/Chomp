using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels.SceneParts
{
    enum PlatformDistance : byte
    {
        Len16,
        Len24,
        Len32,
        Len48            
    }

    class PlatformScenePart : BaseScenePart
    {
        private Specs _specs;
        private DynamicBlockLocation _location;
        private TwoBitEnum<PlatformDistance> _len;

        public PlatformDistance Length => _len.Value;

        public override byte X => _location.TileX;
        public override byte Y => _location.TileY;

        public PlatformScenePart(SystemMemoryBuilder builder, ScenePartType type, PlatformDistance length, byte x, byte y, SceneDefinition scene)
            : base(builder, type, scene)
        {
            _specs = builder.Specs;
            _len = new TwoBitEnum<PlatformDistance>(builder.Memory, Address, 4);
            _location = new DynamicBlockLocation(builder.Memory, Address + 1, scene, builder.Specs);
            _location.TileX = x;
            _location.TileY = y;
            _len.Value = length;
        }

        public PlatformScenePart(SystemMemory memory, int address, SceneDefinition scene, Specs specs)
           : base(memory, address, scene, specs)
        {
            _specs = specs;
            _len = new TwoBitEnum<PlatformDistance>(memory, address, 4);
            _location = new DynamicBlockLocation(memory, Address + 1, scene, specs);
        }
    }
}
