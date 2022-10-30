using ChompGame.Data;
using ChompGame.GameSystem;

namespace ChompGame.MainGame
{
    class StatusBar : IHBlankHandler
    {
        private readonly TileModule _tileModule;
        private readonly CoreGraphicsModule _coreGraphicsModule;

        private GameShort _score;

        public StatusBar(TileModule tileModule)
        {
            _tileModule = tileModule;
            _coreGraphicsModule = _tileModule.GameSystem.CoreGraphicsModule;
        }

        public ushort Score
        {
            get => _score.Value;
            set => _score.Value = value;
        }
        public void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _score = memoryBuilder.AddShort();
        }

        public void OnHBlank()
        {
            if (_tileModule.ScreenPoint.Y == 0)
            {
                _tileModule.TileStartX = 0;
                _tileModule.TileStartY = 4;

                var bgPalette = _coreGraphicsModule.GetPalette(0);
                bgPalette.SetColor(0, ChompGameSpecs.Black);
                bgPalette.SetColor(1, ChompGameSpecs.White);
                bgPalette.SetColor(2, ChompGameSpecs.Blue4);
                bgPalette.SetColor(3, ChompGameSpecs.Green2);
            }

            if (_tileModule.ScreenPoint.Y == 4)
            {
                _tileModule.TileStartX = 0;
                _tileModule.TileStartY = 5;
            }
        }

        public void OnStartup()
        {

        }
    }
}
