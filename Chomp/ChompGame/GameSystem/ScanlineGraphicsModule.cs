using ChompGame.Data;

namespace ChompGame.GameSystem
{
    public abstract class ScanlineGraphicsModule : Module, IHBlankHandler, IVBlankHandler
    {
        protected CoreGraphicsModule _coreGraphicsModule => GameSystem.CoreGraphicsModule;
        
        public abstract int Layer { get; }
        public GameByteGridPoint Scroll { get; private set; }
        public GameByteGridPoint PatternTablePoint => _coreGraphicsModule.ScanlineDrawCommands[Layer].PatternTablePoint;
        public int CurrentDrawInstructionAddress => _coreGraphicsModule.ScanlineDrawCommands[Layer].CurrentDrawInstructionAddress;
        public GameByte DrawInstructionAddressOffset => _coreGraphicsModule.ScanlineDrawCommands[Layer].DrawInstructionAddressOffset;
        public GameByte DrawHoldCounter => _coreGraphicsModule.ScanlineDrawCommands[Layer].DrawHoldCounter;
        public GameByteGridPoint ScreenPoint => _coreGraphicsModule.ScreenPoint;
        public NBitPlane NameTable { get; private set; }
        public ScanlineGraphicsModule(MainSystem gameSystem) : base(gameSystem)
        {
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            Scroll = builder.AddGridPoint(
                (byte)(Specs.NameTableWidth * Specs.TileWidth),
                (byte)(Specs.NameTableHeight * Specs.TileHeight),
                Specs.ScrollXMask,
                Specs.ScrollYMask);

            NameTable = builder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);
        }

        public abstract void OnHBlank();

        public abstract void OnVBlank();
        
    }

}
