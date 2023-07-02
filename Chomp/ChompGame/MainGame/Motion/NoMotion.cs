namespace ChompGame.MainGame.Motion
{
    class NoMotion : IMotion
    {
        public int XSpeed { get => 0; set { } }
        public int YSpeed { get => 0; set { } }

        public int TargetXSpeed => 0;

        public void Apply(WorldSprite sprite) { }

        public void Stop() { }
    }
}
