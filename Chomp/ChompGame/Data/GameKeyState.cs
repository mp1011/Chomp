namespace ChompGame.Data
{
    public enum GameKeyState
    {
        Up,
        Released,
        Down,
        Pressed
    }

    public static class GameKeyStateExtensions
    {
        public static bool IsDown(this GameKeyState state)
        {
            return state == GameKeyState.Down || state == GameKeyState.Pressed;
        }
    }
}
