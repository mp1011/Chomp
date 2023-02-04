using ChompGame.Data;

namespace ChompGame.MainGame
{
    interface IMotion
    {
        public int XSpeed { get; set; }

        public int YSpeed { get; set; }

        void Apply(MovingWorldSprite sprite);
    }
}
