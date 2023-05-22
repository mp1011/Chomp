using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels.SceneParts
{
    interface IScenePart
    {
        int Address { get; }
        byte X { get; }
        byte Y { get; }
        ScenePartType Type { get; }
        byte DestroyBitsRequired { get; }
    }


    class BaseScenePart : IScenePart
    {
        public const int Bytes = 2;

        private FourBitEnum<ScenePartType> _type;
        protected readonly SceneDefinition _scene;

        public int Address => _type.Address;

        public virtual byte X => 0;

        public virtual byte Y => 0;

        public ScenePartType Type => _type.Value;

        public byte DestroyBitsRequired => Type.DestroyBitsRequired();

        public BaseScenePart(SystemMemoryBuilder builder, ScenePartType type, SceneDefinition scene)
        {
            int address = builder.CurrentAddress;
            builder.AddBytes(ScenePartsHeader.ScenePartBytes);

            _type = new FourBitEnum<ScenePartType>(builder.Memory, address, true);
            _scene = scene;
            
            _type.Value = type;
        }

        public BaseScenePart(SystemMemory memory, int address, SceneDefinition scene, Specs specs)
        {
            _type = new FourBitEnum<ScenePartType>(memory, address, true);
            _scene = scene;
        }
    }
}
