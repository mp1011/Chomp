using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels.SceneParts;
using System;

namespace ChompGame.MainGame.SceneModels
{
    enum ScenePartType : byte
    {
        Bomb=0,
        EnemyType1=1,
        EnemyType2=2,
        Turret=3,
        SideExit=4,
        DoorFowardExit=5,
        DoorBackExit=6,
        Platform_LeftRight=7,
        Platform_UpDown=8,
        Platform_Falling=9,
        Platform_Vanishing=10,
        Coin=11,
        DestructibleBlock=12,
        SwitchBlock=13,
        Button=14,
        Prefab= 15,
        Max = 15
    }

    class ScenePartsHeader
    {
        public const int ScenePartBytes = 2;

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

        public ScenePartsHeader(SystemMemoryBuilder memoryBuilder, params Func<SystemMemoryBuilder, IScenePart>[] parts)
        {
            _memory = memoryBuilder.Memory;
            _partCount = memoryBuilder.AddByte();
            _partCount.Value = (byte)parts.Length;
           
            foreach(var part in parts)
            {
                part(memoryBuilder);
            }
        }

        public ScenePartsHeader(SystemMemoryBuilder memoryBuilder, SceneDefinition scene, params Func<SystemMemoryBuilder, SceneDefinition, IScenePart>[] parts)
        {
            _memory = memoryBuilder.Memory;
            _partCount = memoryBuilder.AddByte();
            _partCount.Value = (byte)parts.Length;

            foreach (var part in parts)
            {
                part(memoryBuilder, scene);
            }
        }

        public ScenePartsHeader(int address, SystemMemory memory)
        {
            _partCount = new GameByte(address, memory);
            _memory = memory;
        }

        public IScenePart GetScenePart(int index, SceneDefinition sceneDefinition, Specs specs)
        {
            int address = FirstPartAddress + (BaseScenePart.Bytes * index);
            var basePart = new BaseScenePart(_memory, address, sceneDefinition, specs);
            return basePart.Type switch {
                ScenePartType.DestructibleBlock => new DynamicScenePart(_memory, address, sceneDefinition, specs),
                ScenePartType.Coin => new DynamicScenePart(_memory, address, sceneDefinition, specs),
                ScenePartType.SideExit => new ExitScenePart(_memory, address, sceneDefinition, specs),
                ScenePartType.Prefab => new PrefabScenePart(_memory, address, sceneDefinition, specs),
                ScenePartType.Turret => new TurretScenePart(_memory, address, sceneDefinition, specs),
                _ => new SpriteScenePart(_memory, address, sceneDefinition, specs),
            };
        }

        public DynamicScenePart GetDynamicScenePart(int index, SceneDefinition sceneDefinition, Specs specs)
        {
            return new DynamicScenePart(_memory, FirstPartAddress + (BaseScenePart.Bytes * index), sceneDefinition, specs);
        }

        public ExitScenePart GetExitScenePart(int index, SceneDefinition sceneDefinition, Specs specs)
        {
            return new ExitScenePart(_memory, FirstPartAddress + (BaseScenePart.Bytes * index), sceneDefinition, specs);
        }

        public SpriteScenePart GetSpriteScenePart(int index, SceneDefinition sceneDefinition, Specs specs)
        {
            return new SpriteScenePart(_memory, FirstPartAddress + (BaseScenePart.Bytes * index), sceneDefinition, specs);
        }

        public AutoscrollScenePart GeAutoScrollScenePart(int index, SceneDefinition sceneDefinition, Specs specs)
        {
            return new AutoscrollScenePart(_memory, FirstPartAddress + (BaseScenePart.Bytes * index), sceneDefinition, specs);
        }

        public BaseScenePart GetSpriteOrPlatformScenePart(int index, SceneDefinition sceneDefinition, Specs specs)
        {
            var s = new SpriteScenePart(_memory, FirstPartAddress + (BaseScenePart.Bytes * index), sceneDefinition, specs);
            switch(s.Type)
            {
                case ScenePartType.Platform_Falling:
                case ScenePartType.Platform_LeftRight:
                case ScenePartType.Platform_UpDown:
                case ScenePartType.Platform_Vanishing:
                    return new PlatformScenePart(_memory, FirstPartAddress + (BaseScenePart.Bytes * index), sceneDefinition, specs);
                default:
                    return s;
            }
        }

        public PrefabScenePart GetPrefabScenePart(int index, SceneDefinition sceneDefinition, Specs specs)
        {
            return new PrefabScenePart(_memory, FirstPartAddress + (BaseScenePart.Bytes * index), sceneDefinition, specs);
        }

        public TurretScenePart GetTurretScenePart(int index, SceneDefinition sceneDefinition, Specs specs)
        {
            return new TurretScenePart(_memory, FirstPartAddress + (BaseScenePart.Bytes * index), sceneDefinition, specs);
        }

        public byte GetScenePartDestroyBitsRequired(int index)
        {
            int address = FirstPartAddress + (ScenePartBytes * index);
            var partType = new FourBitEnum<ScenePartType>(_memory, address, true);
            return partType.Value.DestroyBitsRequired();
        }

        private static int GetAddress(Level level, SystemMemory memory)
        {
            int address = memory.GetAddress(AddressLabels.SceneParts);
            int index = 0;

            while(index < (int)level)
            {
                var header = new ScenePartsHeader(address, memory);
                address = header.FirstPartAddress + (header.PartsCount * ScenePartsHeader.ScenePartBytes);

                index++;
            }

            return address;
        }

        public int DestroyBitsNeeded(SceneDefinition scene, Specs specs)
        {
            int destroyBitsNeeded = 0;

            for (int p = 0; p < PartsCount; p++)
            {
                destroyBitsNeeded += GetScenePart(p, scene, specs).DestroyBitsRequired;
            }

            return destroyBitsNeeded;
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

            memoryBuilder.AddBytes(ScenePartBytes * header.PartsCount);

            memoryBuilder.Memory.BlockCopy(header.FirstPartAddress, FirstPartAddress, ScenePartBytes * header.PartsCount);
        }

        public DynamicScenePartHeader(int address, SystemMemory memory)
        {
            _partCount = new GameByte(address, memory);
            _activatedParts = new BitArray(address + 1, memory);
        }
    }

 
    class ScenePartX    
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

        private DynamicBlockLocation _dynamicBlockLocation;
        public DynamicBlockState DynamicBlockState { get; }

        public byte DestroyBitsRequired => Type.DestroyBitsRequired();

        public ScenePartType Type => _type.Value;

       
       

        public byte X
        {
            get 
            {
                switch (_type.Value)
                {
                    case ScenePartType.Coin:
                    case ScenePartType.DestructibleBlock:
                    case ScenePartType.SwitchBlock:
                        return _dynamicBlockLocation.TileX;
                }

                return _definition.ScrollStyle switch {
                    ScrollStyle.NameTable => (byte)(_xBase.Value + (_xExtra.Value ? 16 : 0)),
                    ScrollStyle.Horizontal => (byte)(_xBase.Value + (_xExtra2.Value * 16)),
                    _ => _xBase.Value
                };
            }

            private set
            {
                switch (_type.Value)
                {
                    case ScenePartType.Coin:
                    case ScenePartType.DestructibleBlock:
                    case ScenePartType.SwitchBlock:
                        _dynamicBlockLocation.TileX = value;
                        return;
                }

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
            get
            {
                switch (_type.Value)
                {
                    case ScenePartType.Coin:
                    case ScenePartType.DestructibleBlock:
                    case ScenePartType.SwitchBlock:
                        return _dynamicBlockLocation.TileY;
                }

                return _definition.ScrollStyle switch {
                    ScrollStyle.NameTable => (byte)(_yBase.Value + (_yExtra.Value ? 16 : 0)),
                    ScrollStyle.Vertical => (byte)(_yBase.Value + (_yExtra2.Value * 16)),
                    _ => _yBase.Value
                };
            }

            private set
            {
                switch(_type.Value)
                {
                    case ScenePartType.Coin:
                    case ScenePartType.DestructibleBlock:
                    case ScenePartType.SwitchBlock:
                        _dynamicBlockLocation.TileY = value;
                        return;
                }

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

        public ScenePartX(SystemMemoryBuilder builder, 
            ScenePartType type,
            byte x,
            byte y,
            SceneDefinition definition)
        {
            _definition = definition;

            _type = new FourBitEnum<ScenePartType>(builder.Memory, builder.CurrentAddress, true);
            _xBase = new HighNibble(builder);
            //_exitType = new NibbleEnum<ExitType>(_xBase);
            DynamicBlockState = new DynamicBlockState(builder.Memory, builder.CurrentAddress);

            builder.AddByte();

            builder.AddNibbles(ref _yBase, ref _positionExtra);

            _xExtra = new GameBit(_positionExtra.Address, Bit.Bit5, builder.Memory);
            _yExtra = new GameBit(_positionExtra.Address, Bit.Bit6, builder.Memory);

            _xExtra2 = new TwoBit(builder.Memory, _positionExtra.Address, 4);
            _yExtra2 = new TwoBit(builder.Memory, _positionExtra.Address, 6);

            _dynamicBlockLocation = new DynamicBlockLocation(builder.Memory, _yBase.Address, definition, builder.Specs);

            _type.Value = type;
            X = x;
            Y = y;
        }
 
        public ScenePartX(SystemMemory memory, int address, SceneDefinition definition, Specs specs)
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

          //  _exitType = new NibbleEnum<ExitType>(_xBase);


            DynamicBlockState = new DynamicBlockState(memory, address);
            _dynamicBlockLocation = new DynamicBlockLocation(memory, _yBase.Address, definition, specs);

        }
    }
}
