using ChompGame.Data;
using ChompGame.Data.Memory;

namespace ChompGame.GameSystem
{
    class StatusBarModule : Module, IHBlankHandler
    {
        private readonly TileModule _tileModule;

        private TileFont _scoreFont;
        private HealthGuage _healthGuage;

        private GameShort _score;
        private GameByte _lives;

        public int Score
        {
            get => _score.Value;
            set => _score.Value = (ushort)value;
        }

        public byte Health
        {
            get => _healthGuage.Health;
            set => _healthGuage.Health = value;
        }

        public byte MaxHealth
        {
            get => _healthGuage.MaxHealth;
            set => _healthGuage.MaxHealth = value;
        }

        public StatusBarModule(MainSystem mainSystem, TileModule tileModule) 
            : base(mainSystem)
        {
            _tileModule = tileModule;
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _score = memoryBuilder.AddShort();
            _lives = memoryBuilder.AddByte();
            _scoreFont = new TileFont(memoryBuilder, GameSystem.CoreGraphicsModule);
            _healthGuage = new HealthGuage(memoryBuilder, GameSystem.CoreGraphicsModule);
        }

        public void OnHBlank()
        {
            for (int x = 0; x < Specs.ScreenWidth; x++)
                GameSystem.CoreGraphicsModule.ScanlineDrawBuffer[x] = 0;

            var y = GameSystem.CoreGraphicsModule.ScreenPoint.Y;
            if (y >= 1 && y < 5)
            {
                _scoreFont.DrawString(
                    _score.Value.ToString("00000000"),
                    2,
                    (byte)(y - 1));

                _healthGuage.Draw(
                    40,
                    (byte)(y - 1));
            }
        }

        public override void OnStartup()
        {
            _scoreFont.CharStartIndex = 36;
            _healthGuage.TileIndex = 32;
        }
    }
}
