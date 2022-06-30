using ChompGame.Data;
using ChompGame.Graphics;
using ChompGame.ROM;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace ChompGame.GameSystem
{
    public class CoreGraphicsModule : Module
    {
        private Color[] _screenData;
        public GameByteGridPoint ScreenPoint { get; private set; }
        public GameByte CurrentColorIndex { get; private set; }
        public ScanlineDrawCommands[] ScanlineDrawCommands { get; private set; }
        public NBitPlane PatternTable { get; private set; }
      
        public CoreGraphicsModule(MainSystem gameSystem) : base(gameSystem) 
        {
            _screenData = new Color[gameSystem.Specs.ScreenWidth * gameSystem.Specs.ScreenHeight];
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            CurrentColorIndex = builder.AddByte();
            PatternTable = builder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            
            if(Specs.ScreenWidth==256 && Specs.ScreenHeight==256)
                ScreenPoint = builder.AddFullGridPoint();
            else 
                ScreenPoint = builder.AddGridPoint((byte)Specs.ScreenWidth, (byte)Specs.ScreenHeight, Specs.ScreenPointMask);

            ScanlineDrawCommands = Enumerable.Range(0, Specs.ScanlineDrawPlanes)
                .Select(p => new ScanlineDrawCommands(builder, PatternTable, Specs))
                .ToArray();
        }

        public override void OnStartup()
        { 
        }

        public void DrawFrame(SpriteBatch spriteBatch, Texture2D canvas)
        {
            ScreenPoint.Reset();
            GameSystem.OnHBlank();
            byte colorIndex = 0;
            byte planeColor = 0;

            for (int i = 0; i < _screenData.Length; i++)
            {
                //todo, optimize/generalize
                if (ScanlineDrawCommands.Length == 2)
                {
                    colorIndex = ScanlineDrawCommands[1].Update();
                    planeColor = ScanlineDrawCommands[0].Update();
                }
                else
                {
                    colorIndex = ScanlineDrawCommands[0].Update();
                }

                if (colorIndex == 0)
                    colorIndex = planeColor;

                _screenData[i] = Specs.SystemColors[colorIndex];

                if (ScreenPoint.Next())
                    GameSystem.OnHBlank();
            }

            GameSystem.OnVBlank();
            canvas.SetData(_screenData);
            spriteBatch.Draw(canvas, Vector2.Zero, Color.White);
        }
    }
}
