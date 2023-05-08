
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

        public static int DistanceSquared(this Point start, Point target)
        {
            var xx = (start.X - target.X) * (start.X - target.X);
            var yy = (start.Y - target.Y) * (start.Y - target.Y);

            return xx + yy;
        }

        public static Point GetVectorTo(this Point start, Point target, int speed)
        {
            Vector2 vector = new Vector2(target.X - start.X, target.Y - start.Y);
            vector.Normalize();

            var p = new Point(
                (int)(vector.X * (speed / 1.0)),
                (int)(vector.Y * (speed / 1.0)));

            return p;            
        }
    }
}
