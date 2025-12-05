using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace ChompGame.Menu
{
    public class MenuItem
    {
        public const int Height = 16;
        public const int Pad = 1;

        private Action _onClick;

        public string Text { get; }
        public Rectangle Area { get; }

        public bool Visible { get; set; }

        public bool MouseOver { get; private set; }

        public bool Activated { get; private set; }


        public MenuItem(int yOrder, string text, int width, Action onClick)
        {
            _onClick = onClick;
            Text = text;
            Area = new Rectangle(0, (Height + Pad) * yOrder, width, Height);
        }

        public void Update(MouseState mouse)
        {
            MouseOver = Area.Contains(mouse.Position);
            if (!MouseOver)
                return;

            if (mouse.LeftButton == ButtonState.Pressed && !Activated)
            {
                Activated = true;
                _onClick();
            }
            else if (mouse.LeftButton == ButtonState.Released)
                Activated = false;
        }
        
    }
}
