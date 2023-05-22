using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels.SceneParts
{
    class PitScenePart : BaseScenePart
    {
        private GameByte _start;
        private HighNibble _width;

        public override byte X => _scene.ScrollStyle switch {
            ScrollStyle.Vertical => 0,
            _ => _start.Value
        };

        public override byte Y => _scene.ScrollStyle switch {
            ScrollStyle.Vertical => _start.Value,
            _ => 0
        };

        public byte Width => (byte)(_width.Value * 2);

        public PitScenePart(SystemMemoryBuilder memoryBuilder, byte start, byte width, SceneDefinition sceneDefinition) 
            :base(memoryBuilder, ScenePartType.Pit, sceneDefinition)
        {
            _start = new GameByte(Address + 1, memoryBuilder.Memory);
            _width = new HighNibble(Address, memoryBuilder.Memory);

            _start.Value = start;
            _width.Value = (byte)(width / 2);
        }

        public PitScenePart(SystemMemory memory, int address, SceneDefinition scene, Specs specs)
            : base(memory, address, scene, specs)
        {
            _start = new GameByte(Address + 1, memory);
            _width = new HighNibble(Address, memory);
        }
    }
}
