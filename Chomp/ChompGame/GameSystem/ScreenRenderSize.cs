using Microsoft.Xna.Framework;

namespace ChompGame.GameSystem
{
    class ScreenRenderSize
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public void Update(GameWindow window, double aspectRatio)
        {
            Width = window.ClientBounds.Width;
            Height = (int)(Width * aspectRatio);

            if (Height > window.ClientBounds.Height)
            {
                Height = window.ClientBounds.Height;
                Width = (int)(Height / aspectRatio);
            }

            X = 16 + (window.ClientBounds.Width - Width) / 2;
            Y = (window.ClientBounds.Height - Height) / 2;
        }
    }
}
