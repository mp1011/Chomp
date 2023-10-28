
using ChompGame.Helpers;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.Extensions
{
    public static class PointExtensions
    {
        public static Point ClampTo(this Point p, int width, int height, int pad) =>
            new Point(p.X.Clamp(-pad, width + pad),
                p.Y.Clamp(-pad, height + pad));

        public static Point Divide(this Point p, int i)
        {
            return new Point(p.X / i, p.Y / i);
        }

        public static Point Add(this Point p, int x, int y)
        {
            return new Point(p.X + x, p.Y + y);
        }

        public static Vector2 Normalize(this Point p)
        {
            var v = new Vector2(p.X, p.Y);
            v.Normalize();
            return v;
        }

        public static Point AdjustLength(this Point p, int length)
        {
            var normalized = p.Normalize();
            return new Point((int)(normalized.X * length), (int)(normalized.Y * length));
        }

        public static int DistanceSquared(this Point start, Point target)
        {
            var xx = (start.X - target.X) * (start.X - target.X);
            var yy = (start.Y - target.Y) * (start.Y - target.Y);

            return xx + yy;
        }

        public static int Magnitude(this Point p)
        {
            var xx = p.X * p.X;
            var yy = p.Y * p.Y;
            return (int)Math.Sqrt(xx + yy);
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

        public static int Degrees(this Point pt)
        {
            var rad = Math.Atan2(pt.X, pt.Y);
            var deg = (int)MathHelper.ToDegrees((float)rad);
            return deg.NMod(360);
        }

        public static Point RotateDeg(this Point pt, int degrees)
        {
            var deg = (pt.Degrees() + degrees).NMod(360);
            var length = pt.Magnitude();
            
            return GameMathHelper.PointFromAngle(deg, length);             
        }

    }
}
