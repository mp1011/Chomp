using Microsoft.Xna.Framework.Graphics;

namespace Chomp.Services.Interfaces
{
    public interface IRenderService
    {
        void Render(SpriteBatch sprite, Texture2D canvas);
    }
}
