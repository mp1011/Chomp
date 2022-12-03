using ChompGame.Data;

namespace ChompGame.MainGame.SceneModels
{
    enum ScenePartType : byte
    {
        Bomb,
        EnemyType1,
        EnemyType2,
        Pit
    }

    class ScenePart
    {
        public const int Bytes = 2;

        private readonly SceneDefinition _definition;

        private FourBitEnum<ScenePartType> _type;

        private HighNibble _positionA;

        private LowNibble _positionB;

        private HighNibble _positionC;


        public ScenePartType Type => _type.Value;


        public ScenePart(SystemMemoryBuilder builder, SceneDefinition definition)
        {
            _definition = definition;

            _type = new FourBitEnum<ScenePartType>(builder.Memory, builder.CurrentAddress, true);
            _positionA = new HighNibble(builder);
            builder.AddByte();

            builder.AddNibbles(ref _positionB, ref _positionC);
        }

        public ScenePart(SystemMemory memory, int address, SceneDefinition definition)
        {
            _definition = definition;

            _type = new FourBitEnum<ScenePartType>(memory, address, true);
            _positionA = new HighNibble(address, memory);
            _positionB = new LowNibble(address+1, memory);
            _positionC = new HighNibble(address+1, memory);
        }

        public ScenePart(SystemMemory memory, byte index, SceneDefinition definition)
            :this(memory, memory.GetAddress(AddressLabels.SceneParts) + index * Bytes, definition)
        {

        }

    }
}
