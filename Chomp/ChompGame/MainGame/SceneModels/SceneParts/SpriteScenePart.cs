using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels.SceneParts
{
    class SpriteScenePart : BaseScenePart
    {
        private HighNibble _xBase;
        private LowNibble _yBase;
        private HighNibble _positionExtra;
        private GameBit _xExtra;
        private GameBit _yExtra;
        private TwoBit _xExtra2;
        private TwoBit _yExtra2;

        public SpriteScenePart(SystemMemoryBuilder builder,
          ScenePartType type,
          byte x,
          byte y,
          SceneDefinition definition) : base(builder, type, definition)
        {
            _xBase = new HighNibble(Address, builder.Memory);

            _yBase = new LowNibble(Address + 1, builder.Memory);
            _positionExtra = new HighNibble(Address + 1, builder.Memory);

            _xExtra = new GameBit(_positionExtra.Address, Bit.Bit5, builder.Memory);
            _yExtra = new GameBit(_positionExtra.Address, Bit.Bit6, builder.Memory);

            _xExtra2 = new TwoBit(builder.Memory, _positionExtra.Address, 4);
            _yExtra2 = new TwoBit(builder.Memory, _positionExtra.Address, 6);

            XPos = x;
            YPos = y;
        }

        public SpriteScenePart(SystemMemory memory, int address, SceneDefinition scene, Specs specs)
            : base(memory, address, scene,specs)
        {
            _xBase = new HighNibble(Address, memory);

            _yBase = new LowNibble(Address + 1, memory);
            _positionExtra = new HighNibble(Address + 1, memory);

            _xExtra = new GameBit(_positionExtra.Address, Bit.Bit5, memory);
            _yExtra = new GameBit(_positionExtra.Address, Bit.Bit6, memory);

            _xExtra2 = new TwoBit(memory, _positionExtra.Address, 4);
            _yExtra2 = new TwoBit(memory, _positionExtra.Address, 6);
        }

        public override byte X => XPos;
        public override byte Y => YPos;

        private byte XPos
        {
            get
            {
                return _scene.ScrollStyle switch {
                    ScrollStyle.NameTable => (byte)(_xBase.Value + (_xExtra.Value ? 16 : 0)),
                    ScrollStyle.Horizontal => (byte)(_xBase.Value + (_xExtra2.Value * 16)),
                    _ => _xBase.Value
                };
            }

            set
            {
                switch (_scene.ScrollStyle)
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

        private byte YPos
        {
            get
            {
                return _scene.ScrollStyle switch {
                    ScrollStyle.NameTable => (byte)(_yBase.Value + (_yExtra.Value ? 16 : 0)),
                    ScrollStyle.Vertical => (byte)(_yBase.Value + (_yExtra2.Value * 16)),
                    _ => _yBase.Value
                };
            }

            set
            {
                switch (_scene.ScrollStyle)
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

    }
}