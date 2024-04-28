using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels.SceneParts
{
    class TurretScenePart : BaseScenePart
    {
        private DynamicBlockLocation _dynamicBlockLocation;

        public byte TileX => _dynamicBlockLocation.TileX;
        public byte TileY => _dynamicBlockLocation.TileY;

        public TurretScenePart(SystemMemoryBuilder builder, SceneDefinition scene, byte x, byte y) 
            : base(builder, ScenePartType.Turret, scene)
        {
            _dynamicBlockLocation = new DynamicBlockLocation(builder.Memory, Address + 1, scene, builder.Specs);
            _dynamicBlockLocation.TileX = x;
            _dynamicBlockLocation.TileY = y;

        }

        public TurretScenePart(SystemMemory memory, int address, SceneDefinition scene, Specs specs) 
            : base(memory, address, scene, specs)
        {
            _dynamicBlockLocation = new DynamicBlockLocation(memory, address + 1, scene, specs);
        }
    }
}
