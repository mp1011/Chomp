using Microsoft.Xna.Framework;
using System;

namespace ChompGame.Helpers
{
    public static class GameMathHelper
    {
        public static Point PointFromAngle(int degrees, int magnitude)
        {
            var rad = MathHelper.ToRadians(degrees);

            var x = Math.Sin(rad) * magnitude;
            var y = Math.Cos(rad) * magnitude;

            return new Point((int)x, (int)y);
        }
    }
}
