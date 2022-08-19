using ChompGame.Data;

namespace ChompGame.GameSystem
{
    public abstract class ScanlineGraphicsModule : Module, IHBlankHandler, IVBlankHandler
    {
        protected CoreGraphicsModule _coreGraphicsModule => GameSystem.CoreGraphicsModule;
        
        public int Layer { get; set; }
        public GameByteGridPoint Scroll { get; private set; }
        public GameByteGridPoint ScreenPoint => _coreGraphicsModule.ScreenPoint;

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
        }

        public abstract void OnHBlank();

        public abstract void OnVBlank();
        
    }

}
