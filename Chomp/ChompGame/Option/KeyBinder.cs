using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ChompGame.Option
{
    public enum GameKey
    {
        Up,
        Down,
        Left,
        Right,
        Jump,
        Throw,
        Start
    }

    internal class KeyBinder
    {
        private GameOptions _options;

        public KeyBinder(GameOptions options)
        {
            _options = options;
        }

        public bool Active { get; private set; }
      
        private string _currentPrompt = "";
        private GameKey _currentKey;
        private bool _awaitingKey = false;

        private GameKey CurrentKey
        {
            get => _currentKey;
            set
            {
                _currentKey = value;
                _currentPrompt = $"Press key for {_currentKey}";
                _awaitingKey = true;
            }
        }

        public void Activate()
        {
            Active = true;
            CurrentKey = GameKey.Up;
        }


        public bool Update()
        {
            var keyState = Keyboard.GetState();
            var padState = GamePad.GetState(PlayerIndex.One);

            var keys = keyState.GetPressedKeys();
            var padKeys = PressedPadButtons(padState);

            if (_awaitingKey)
            {
                if(keyState.IsKeyDown(Keys.Escape) || padState.IsButtonDown(Buttons.Back))
                {
                    Active = false;
                    return false;
                }

                if (keys.Any())
                {
                    _options.KeyboardBindings[CurrentKey] = keys.First();
                    _options.GamePadBindings.Remove(CurrentKey);
                    _awaitingKey = false;
                }
                else if(padKeys.Any())
                {
                    _options.GamePadBindings[CurrentKey] = padKeys.First();
                    _options.KeyboardBindings.Remove(CurrentKey);
                    _awaitingKey = false;
                }
            }
            else
            {
                if (!keys.Any() && !padKeys.Any())
                    CurrentKey++;
            }

            if(CurrentKey > GameKey.Start)
            {
                _options.Save();
                Active = false;
                return false;
            }

            return true;            
        }

        private Buttons[] PressedPadButtons(GamePadState padState)
        {
            var buttons = (Buttons[])Enum.GetValues(typeof(Buttons));
            return buttons.Where(p => padState.IsButtonDown(p)).ToArray();
        }
            
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(font, _currentPrompt, new Microsoft.Xna.Framework.Vector2(16, 16), Color.White);
            spriteBatch.End();
        }
    }

}
