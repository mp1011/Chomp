using Microsoft.Xna.Framework;

namespace ChompGame.MainGame
{
    interface IWithPosition
    {
        int X { get; set; }
        int Y { get; set; }
    }

    interface IWithBounds
    {
        Rectangle Bounds { get; }
    }
}
