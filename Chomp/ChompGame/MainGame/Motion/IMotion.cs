namespace ChompGame.MainGame
{
    interface IMotion
    {
        public int XSpeed { get; set; }

        public int YSpeed { get; set; }

        public int TargetXSpeed { get; }

        void Stop();

        void Apply(WorldSprite sprite);
    }
}
