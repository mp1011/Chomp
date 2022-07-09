
using Microsoft.Xna.Framework;

namespace ChompGame.Extensions
{
    public static class PointExtensions
    {
        public static Point Divide(this Point p, int i)
        {
            return new Point(p.X / i, p.Y / i);
        }

        public static Point Add(this Point p, int x, int y)
        {
            return new Point(p.X + x, p.Y + y);
        }
    }
}
