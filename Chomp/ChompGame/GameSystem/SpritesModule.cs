using ChompGame.Data;
using System.Linq;

namespace ChompGame.GameSystem
{
    class SpritesModule : Module, IHBlankHandler, IVBlankHandler
    {
        private CoreGraphicsModule _coreGraphicsModule => GameSystem.CoreGraphicsModule;
        public GameByteGridPoint ScreenPoint => _coreGraphicsModule.ScreenPoint;

        public SpritesModule(MainSystem mainSystem) : base(mainSystem)
        {
        }

        public Sprite[] Sprites { get; private set; }
        public GameByte[] ScanlineSprites { get; private set; }

        public override void OnStartup()
        {
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            Sprites = builder.AddSprite(Specs.MaxSprites);
            ScanlineSprites = builder.AddBytes(Specs.SpritesPerScanline);
        }

        public void OnHBlank()
        {
            FillScanlineSprites();
        }


        public void OnVBlank()
        {
        }

        private void FillScanlineSprites()
        {
            var scanlineSprites = Sprites.Where(p => p.Tile > 0 && p.IntersectsScanline(ScreenPoint.Y))
                .OrderBy(p => p.X)
                .Take(Specs.SpritesPerScanline)
                .ToArray();

            int sprite0Address = Sprites[0].Address;

            for (int i = 0; i < Specs.SpritesPerScanline; i++)
            {
                if (i < scanlineSprites.Length)
                    ScanlineSprites[i].Value = (byte)(scanlineSprites[i].Address - sprite0Address);
                else
                    ScanlineSprites[i].Value = 0;
            }
        }
    }
}
