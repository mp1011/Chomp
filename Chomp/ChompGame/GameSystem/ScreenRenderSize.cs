using Microsoft.Xna.Framework;

namespace ChompGame.GameSystem
{
    public class ScreenRenderSize
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public void Update(int windowWidth, int windowHeight, double aspectRatio)
        {
            Width = windowWidth;
            Height = (int)(Width * aspectRatio);

            if (Height > windowHeight)
            {
                Height = windowHeight;
                Width = (int)(Height / aspectRatio);
            }

            X = (windowWidth - Width) / 2;
            Y = (windowHeight - Height) / 2;
        }
    }
}
