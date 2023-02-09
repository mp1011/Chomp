using ChompGame.Data;
using ChompGame.Data.Memory;
using System;

namespace ChompGame.MainGame.SceneModels
{
    enum ScenePartType : byte
    {
        Bomb=0,
        EnemyType1=1,
        EnemyType2=2,
        Pit=3,
        SideExit=4,
        DoorFowardExit=5,
        DoorBackExit=6,
        Platform=7,
        Max=15
    }

    class ScenePartsHeader
    {
        protected SystemMemory _memory;
        protected GameByte _partCount;

        public int PartsCount => _partCount.Value;

        public int FirstPartAddress => _partCount.Address + 1;
     
        protected ScenePartsHeader()
        {

        }

        public ScenePartsHeader(Level level, SystemMemory memory) : this(GetAddress(level, memory), memory)
        {

        }

        public ScenePartsHeader(SystemMemoryBuilder memoryBuilder, params Func<SystemMemoryBuilder, ScenePart>[] parts)
        {
            _memory = memoryBuilder.Memory;
            _partCount = memoryBuilder.AddByte();
            _partCount.Value = (byte)parts.Length;
           
            foreach(var part in parts)
            {
                part(memoryBuilder);
            }
        }

        public ScenePartsHeader(int address, SystemMemory memory)
        {
            _partCount = new GameByte(address, memory);
            _memory = memory;
        }

        public ScenePart GetScenePart(int index, SceneDefinition sceneDefinition)
        {
            return new ScenePart(_memory, FirstPartAddress + (ScenePart.Bytes * index), sceneDefinition);
        }

        private static int GetAddress(Level level, SystemMemory memory)
        {
            int address = memory.GetAddress(AddressLabels.SceneParts);
            int index = 0;

            while(index < (int)level)
            {
                var header = new ScenePartsHeader(address, memory);
                address = header.FirstPartAddress + (header.PartsCount * ScenePart.Bytes);

                index++;
            }

            return address;
        }

    }

    class DynamicScenePartHeader : ScenePartsHeader
    {
        private readonly BitArray _activatedParts;

        public bool IsPartActivated(int index) => _activatedParts[index];

        public void MarkActive(int index) => _activatedParts[index] = true;

        public DynamicScenePartHeader(SystemMemoryBuilder memoryBuilder, Level level)
        {
            _memory = memoryBuilder.Memory;

            var header = new ScenePartsHeader(level, memoryBuilder.Memory);

            _activatedParts = new BitArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);

            memoryBuilder.AddBytes((int)Math.Ceiling((byte)header.PartsCount / 8.0));

            _partCount = memoryBuilder.AddByte((byte)header.PartsCount);

            memoryBuilder.AddBytes(ScenePart.Bytes * header.PartsCount);

            memoryBuilder.Memory.BlockCopy(header.FirstPartAddress, FirstPartAddress, ScenePart.Bytes * header.PartsCount);
        }

        public DynamicScenePartHeader(int address, SystemMemory memory)
        {
            _partCount = new GameByte(address, memory);
            _activatedParts = new BitArray(address + 1, memory);
        }
    }


    class ScenePart
    {
        public const int Bytes = 2;

        public int Address => _type.Address;

        private readonly SceneDefinition _definition;

        private FourBitEnum<ScenePartType> _type;

        private HighNibble _xBase;

        private LowNibble _yBase;

        private HighNibble _positionExtra;

        private GameBit _xExtra;
        private GameBit _yExtra;

        private TwoBit _xExtra2;
        private TwoBit _yExtra2;

        public ScenePartType Type => _type.Value;

        private NibbleEnum<ExitType> _exitType;

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
                else if (_yBase.Value < 8)
                    return _yBase.Value + 1;
                else
                    return -((_yBase.Value & 7) + 1);
            }
        }

        public byte X
        {
            get => _definition.ScrollStyle switch {
                ScrollStyle.NameTable => (byte)(_xBase.Value + (_xExtra.Value ? 16 : 0)),
                ScrollStyle.Horizontal => (byte)(_xBase.Value + (_xExtra2.Value * 16)),
                _ => _xBase.Value
            };

            private set
            {
                switch (_definition.ScrollStyle)
                {
                    case ScrollStyle.NameTable:

                        _xBase.Value = value;
                        _xExtra.Value = value >= 16;

                        break;

                    case ScrollStyle.Horizontal:

                        _xBase.Value = value;
                        _xExtra2.Value = (byte)(value >> 4);

                        break;

                    default:
                            _xBase.Value = value;
                    break;
                }
            }
        }

        public byte Y
        {
            get => _definition.ScrollStyle switch {
                ScrollStyle.NameTable => (byte)(_yBase.Value + (_yExtra.Value ? 16 : 0)),
                ScrollStyle.Vertical => (byte)(_yBase.Value + (_yExtra2.Value * 16)),
                _ => _yBase.Value
            };

            private set
            {
                switch (_definition.ScrollStyle)
                {
                    case ScrollStyle.NameTable:

                        _yBase.Value = value;
                        _yExtra.Value = value >= 16;

                        break;

                    case ScrollStyle.Vertical:

                        _yBase.Value = value;
                        _yExtra2.Value = (byte)(value >> 4);

                        break;

                    default:
                        _yBase.Value = value;
                        break;
                }
            }
        }


        public ScenePart(SystemMemoryBuilder builder, 
            ScenePartType type,
            byte x,
            byte y,
            SceneDefinition definition)
        {
            _definition = definition;

            _type = new FourBitEnum<ScenePartType>(builder.Memory, builder.CurrentAddress, true);
            _xBase = new HighNibble(builder);
            _exitType = new NibbleEnum<ExitType>(_xBase);

            builder.AddByte();

            builder.AddNibbles(ref _yBase, ref _positionExtra);

            _xExtra = new GameBit(_positionExtra.Address, Bit.Bit5, builder.Memory);
            _yExtra = new GameBit(_positionExtra.Address, Bit.Bit6, builder.Memory);

            _xExtra2 = new TwoBit(builder.Memory, _positionExtra.Address, 4);
            _yExtra2 = new TwoBit(builder.Memory, _positionExtra.Address, 6);

            X = x;
            Y = y;
            _type.Value = type;
        }

        public ScenePart(SystemMemoryBuilder builder,
           ExitType exitType,
           int exitOffset,
           SceneDefinition definition) : this(builder, ScenePartType.SideExit, (byte)exitType, GetExitOffset(exitOffset), definition)
        {
        }


        private static byte GetExitOffset(int offset)
        {
            if (offset > 0)
                return (byte)(offset - 1);
            else
                return (byte)((-offset - 1) | 8);
        }


        public ScenePart(SystemMemory memory, int address, SceneDefinition definition)
        {
            _definition = definition;

            _type = new FourBitEnum<ScenePartType>(memory, address, true);
            _xBase = new HighNibble(address, memory);
            _yBase = new LowNibble(address+1, memory);
            _positionExtra = new HighNibble(address+1, memory);

            _xExtra = new GameBit(_positionExtra.Address, Bit.Bit5, memory);
            _yExtra = new GameBit(_positionExtra.Address, Bit.Bit6, memory);

            _xExtra2 = new TwoBit(memory, _positionExtra.Address, 4);
            _yExtra2 = new TwoBit(memory, _positionExtra.Address, 6);

            _exitType = new NibbleEnum<ExitType>(_xBase);
        }
    }
}
