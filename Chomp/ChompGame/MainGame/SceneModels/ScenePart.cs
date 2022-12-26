using ChompGame.Data;
using ChompGame.Data.Memory;
using System;

namespace ChompGame.MainGame.SceneModels
{
    enum ScenePartType : byte
    {
        Bomb,
        EnemyType1,
        EnemyType2,
        Pit
    }

    class ScenePartsHeader
    {
        private readonly GameByte _partCount;

        private readonly BitArray _activatedParts;

        public int PartsCount => _partCount.Value;

        public int FirstPartAddress => _partCount.Address + 1 + (int)Math.Ceiling((byte)_partCount.Value / 8.0);

        public bool IsPartActived(int index) => _activatedParts[index];

        public void MarkActive(int index) => _activatedParts[index] = true;

        public ScenePartsHeader(Level level, SystemMemory memory) : this(GetAddress(level, memory), memory)
        {

        }

        public ScenePartsHeader(SystemMemoryBuilder memoryBuilder, params Func<SystemMemoryBuilder, ScenePart>[] parts)
        {
            _partCount = memoryBuilder.AddByte();
            _partCount.Value = (byte)parts.Length;

            _activatedParts = new BitArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);

            memoryBuilder.AddBytes((int)Math.Ceiling((byte)parts.Length / 8.0));

            foreach(var part in parts)
            {
                part(memoryBuilder);
            }
        }

        public ScenePartsHeader(int address, SystemMemory memory)
        {
            _partCount = new GameByte(address, memory);
            _activatedParts = new BitArray(address + 1, memory);
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

    class ScenePart
    {
        public const int Bytes = 2;

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
        }
    }
}
