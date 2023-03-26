using ChompGame.GameSystem;
using ChompGame.MainGame;
using System;
using System.Linq;

namespace ChompGame.Data.Memory
{
    public class SystemMemoryBuilder
    {
        public DynamicMemoryBlock Bytes { get; }

        private SystemMemory _systemMemory;
        private Specs _specs;

        public int CurrentAddress => Bytes.CurrentAddress;

        public SystemMemory Memory => _systemMemory;

        public SystemMemoryBuilder(SystemMemory systemMemory, Specs specs, GameRAM gameRAM)
        {
            _specs = specs;
            _systemMemory = systemMemory;
            Bytes = new GameRamMemoryBlock(systemMemory, specs, gameRAM);
        }

        public SystemMemoryBuilder(SystemMemory systemMemory, Specs specs)
        {
            _specs = specs;
            _systemMemory = systemMemory;
            Bytes = new ListMemoryBlock();
        }


        public void AddLabel(AddressLabels label)
        {
            _systemMemory.AddLabel(label, CurrentAddress);
        }

        public FixedMemoryBlock Build()
        {
            return Bytes.ToFixed();
        }

        public GameByte[] AddBytes(int count)
        {
            return Enumerable.Range(0, count)
                .Select(p => AddByte(0))
                .ToArray();
        }

        public BitPlane AddBitPlane(int width, int height)
        {
            var bitPlane = new BitPlane(CurrentAddress, _systemMemory, width, height);
            AddBytes(bitPlane.Bytes);
            return bitPlane;
        }

        public NBitPlane AddNBitPlane(int planes, int width, int height)
        {
            var bitPlane = NBitPlane.Create(CurrentAddress, _systemMemory, planes, width, height);
            AddBytes(bitPlane.Bytes);
            return bitPlane;
        }


        public ByteAddressWithFPad AddByteAddressWithFPad()
        {
            return new ByteAddressWithFPad(AddByte(), _systemMemory);
        }

        public GameByte AddByte(byte value = 0)
        {
            var b = new GameByte(CurrentAddress, _systemMemory);
            Bytes.Add(value);
            return b;
        }

        public MaskedByte AddMaskedByte(Bit mask)
        {
            var m = new MaskedByte(CurrentAddress, mask, _systemMemory);
            AddByte();
            return m;
        }

        public void AddNibbles(ref LowNibble low, ref HighNibble high)
        {
            low = new LowNibble(this);
            high = new HighNibble(this);
            AddByte();
        }

        public NibblePoint AddNibblePoint()
        {
            var n = new NibblePoint(CurrentAddress, Memory);
            AddByte();
            return n;
        }

        public GameShort AddShort()
        {
            var s = new GameShort(CurrentAddress, _systemMemory);
            Bytes.Add(0);
            Bytes.Add(0);
            return s;
        }

        public GameInteger AddInteger()
        {
            var s = new GameInteger(CurrentAddress, _systemMemory);
            Bytes.Add(0);
            Bytes.Add(0);
            Bytes.Add(0);
            Bytes.Add(0);
            return s;
        }

        public GameByteGridPoint AddGridPoint(byte width, byte height)
        {
            var b = new GameByteGridPoint(
                AddByte(),
                AddByte(),
                width,
                height);

            return b;
        }

        public BitArray AddBitArray(int length)
        {
            var bitArray = new BitArray(CurrentAddress, _systemMemory);
            var bytes = (int)(Math.Ceiling((float)length / 8));
            AddBytes(bytes);
            return bitArray;
        }

        public TwoBitArray AddTwoBitArray(int length)
        {
            return new TwoBitArray(AddBitArray(length), AddBitArray(length));
        }

        public GameByteGridPoint AddGridPoint(byte width, byte height, Bit mask)
            => AddGridPoint(width, height, mask, mask);

        public GameByteGridPoint AddGridPoint(byte width, byte height, Bit xMask, Bit yMask)
        {
            var b = new GameByteGridPoint(
                AddByte().WithMask(xMask),
                AddByte().WithMask(yMask),
                width,
                height);

            return b;
        }

        /// <summary>
        /// Note - leaves higher 6 bits of last byte free
        /// </summary>
        /// <returns></returns>
        public ExtendedPoint AddExtendedPoint()
        {
            return new ExtendedPoint(this);
        }

        public Sprite[] AddSprite(int count, SpritesModule spritesModule)
        {
            return Enumerable.Range(0, count)
                .Select(p =>
                {
                    return new Sprite(this, _specs, spritesModule.Scroll);
                })
                .ToArray();
        }
    }

}
