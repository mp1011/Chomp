using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels.SceneParts
{
    class ExitScenePart : BaseScenePart
    {
        private NibbleEnum<ExitType> _exitType;
        private LowNibble _exitOffset;

        public override byte X => throw new System.NotImplementedException();

        public override byte Y => throw new System.NotImplementedException();

        public ExitType ExitType
        {
            get => _exitType.Value;
            set => _exitType.Value = value;
        }

        public int ExitLevelOffset
        {
            get
            {
                if (ExitType == ExitType.DoorForward)
                    return 1;
                else if (ExitType == ExitType.DoorBack)
                    return -1;
                else if (_exitOffset.Value < 8)
                    return _exitOffset.Value + 1;
                else
                    return -((_exitOffset.Value & 7) + 1);
            }
        }

        public ExitScenePart(SystemMemoryBuilder memoryBuilder, ExitType exitType, int exitOffset, SceneDefinition scene)
            : base(memoryBuilder, exitType switch {
                ExitType.DoorBack => throw new System.Exception("Use SpriteScenePart"),
                ExitType.DoorForward => throw new System.Exception("Use SpriteScenePart"),
                _ => ScenePartType.SideExit
            }, scene)
        {
            _exitType = new NibbleEnum<ExitType>(new HighNibble(Address, memoryBuilder.Memory));
            _exitOffset = new LowNibble(Address + 1, memoryBuilder.Memory);
            _exitOffset.Value = GetExitOffsetByte(exitOffset);
            ExitType = exitType;
        }

        public ExitScenePart(SystemMemoryBuilder memoryBuilder, ExitType exitType, int exitOffset, byte exitPosition, SceneDefinition scene)
            : this(memoryBuilder, exitType, exitOffset, scene)
        {
            //todo-scenepart
        }

        public ExitScenePart(SystemMemory memory, int address, SceneDefinition scene, Specs specs)
            : base(memory, address, scene, specs)
        {
            _exitType = new NibbleEnum<ExitType>(new HighNibble(Address, memory));
            _exitOffset = new LowNibble(Address + 1, memory);
        }

        private static byte GetExitOffsetByte(int offset)
        {
            if (offset > 0)
                return (byte)(offset - 1);
            else
                return (byte)((-offset - 1) | 8);
        }
    }
}
