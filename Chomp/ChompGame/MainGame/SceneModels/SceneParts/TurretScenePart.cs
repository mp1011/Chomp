using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels.SceneParts
{
    class TurretScenePart : BaseScenePart
    {
        private DynamicBlockLocation _dynamicBlockLocation;
        private TwoBitEnum<Direction> _direction;

        public override byte X => _dynamicBlockLocation.TileX;
        public override byte Y => _dynamicBlockLocation.TileY;

        public Direction Direction => _direction.Value;

        public TurretScenePart(SystemMemoryBuilder builder, SceneDefinition scene, Direction direction, byte x, byte y) 
            : base(builder, ScenePartType.Turret, scene)
        {
            _direction = new TwoBitEnum<Direction>(builder.Memory, Address, shift: 4);
            _dynamicBlockLocation = new DynamicBlockLocation(builder.Memory, Address + 1, scene, builder.Specs);
            _dynamicBlockLocation.TileX = x;
            _dynamicBlockLocation.TileY = y;
            _direction.Value = direction;

        }

        public TurretScenePart(SystemMemory memory, int address, SceneDefinition scene, Specs specs) 
            : base(memory, address, scene, specs)
        {
            _dynamicBlockLocation = new DynamicBlockLocation(memory, address + 1, scene, specs);
            _direction = new TwoBitEnum<Direction>(memory, Address, shift: 4);
        }
    }
}
