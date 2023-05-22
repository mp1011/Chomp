using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels.SceneParts
{
    class DynamicScenePart : BaseScenePart
    {
        private DynamicBlockLocation _dynamicBlockLocation;
        public DynamicBlockState DynamicBlockState { get; }

        public DynamicScenePart(SystemMemoryBuilder builder,
          DynamicBlockType blockType,
          bool topLeft,
          bool topRight,
          bool bottomLeft,
          bool bottomRight,
          byte x,
          byte y,
          SceneDefinition definition) : base(builder, blockType switch {
              DynamicBlockType.Coin => ScenePartType.Coin,
              DynamicBlockType.DestructibleBlock => ScenePartType.DestructibleBlock,
              _ => ScenePartType.SwitchBlock }, definition)
        {
            DynamicBlockState = new DynamicBlockState(builder.Memory, Address);
            _dynamicBlockLocation = new DynamicBlockLocation(builder.Memory, Address + 1, definition, builder.Specs);
            
            _dynamicBlockLocation.TileX = x;
            _dynamicBlockLocation.TileY = y;
            DynamicBlockState.TopLeft = topLeft;
            DynamicBlockState.TopRight = topRight;
            DynamicBlockState.BottomLeft = bottomLeft;
            DynamicBlockState.BottomRight = bottomRight;
        }

        public DynamicScenePart(SystemMemory memory, int address, SceneDefinition scene, Specs specs) 
            :base(memory,address, scene, specs)
        {
            DynamicBlockState = new DynamicBlockState(memory, address);
            _dynamicBlockLocation = new DynamicBlockLocation(memory, address + 1, scene, specs);
        }

        public override byte X => _dynamicBlockLocation.TileX;

        public override byte Y => _dynamicBlockLocation.TileY;     
    }
}
