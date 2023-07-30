using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels.SceneParts
{

    class AutoscrollScenePart : IScenePart
    {
        private LowNibble _spawnPosition;
        private NibbleEnum<ScenePartType> _type;
        private LowNibble _timer;

        public byte Delay => _timer.Value;

        public int Address => _spawnPosition.Address;

        public byte X => 0;

        public byte Y => _spawnPosition.Value;

        public ScenePartType Type => _type.Value;

        public byte DestroyBitsRequired => 0;

        public AutoscrollScenePart(SystemMemoryBuilder builder, ScenePartType type, byte position, byte delay, SceneDefinition scene)
        {
            _spawnPosition = new LowNibble(builder);
            _type = new NibbleEnum<ScenePartType>(new HighNibble(builder));
            builder.AddByte();

            _timer = new LowNibble(builder);

            //todo, other stuff
            builder.AddByte();

            _spawnPosition.Value = position;
            _type.Value = type;
            _timer.Value = delay;
        }
        public AutoscrollScenePart(SystemMemory memory, int address, SceneDefinition scene, Specs specs)
        {
            _spawnPosition = new LowNibble(address, memory);
            _type = new NibbleEnum<ScenePartType>(new HighNibble(address, memory));           
            _timer = new LowNibble(address + 1, memory);
        }
    }
}
