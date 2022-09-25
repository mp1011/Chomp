using ChompGame.Data;
using ChompGame.GameSystem;

namespace ChompGame.MainGame
{
    class PlayerController
    {
        private readonly SpritesModule _spritesModule;
        public AcceleratedMotion Motion { get; }

        public Sprite GetSprite() => _spritesModule.GetSprite(0);

        public PlayerController(SpritesModule spritesModule, SystemMemoryBuilder memoryBuilder)
        {
            _spritesModule = spritesModule;
            Motion = new AcceleratedMotion(memoryBuilder);
        }

        public void Update()
        {
            Motion.Apply(GetSprite());
        }
    }
}
