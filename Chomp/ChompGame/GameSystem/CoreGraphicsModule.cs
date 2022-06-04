using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.Graphics;
using ChompGame.ROM;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ChompGame.GameSystem
{
    public class CoreGraphicsModule : Module
    {
        private Color[] _systemPalette;
        private Color[] _screenData;
        public GameByteGridPoint ScreenPoint { get; private set; }
        public GameByte DrawInstructionAddress { get; private set; }
        public GameByte DrawInstructionAddressOffset { get; private set; }
        public GameByte DrawHoldCounter { get; private set; }
        public NBitPlane PatternTable { get; private set; }
        public GameByteGridPoint PatternTablePoint { get; private set; }

        public CoreGraphicsModule(MainSystem gameSystem) : base(gameSystem) 
        {
            _screenData = new Color[gameSystem.Specs.ScreenWidth * gameSystem.Specs.ScreenHeight];
            _systemPalette = gameSystem.Specs.SystemColors;
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            PatternTablePoint = builder.AddGridPoint((byte)Specs.PatternTableWidth, (byte)Specs.PatternTableHeight, (Bit)7);
            PatternTable = builder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            ScreenPoint = builder.AddGridPoint((byte)Specs.ScreenWidth, (byte)Specs.ScreenHeight, (Bit)31);
            DrawInstructionAddress = builder.AddByte();
            DrawInstructionAddressOffset = builder.AddByte();
            DrawHoldCounter = builder.AddByte();
            builder.AddBytes(Specs.MaxDrawInstructions);
        }

        public override void OnStartup()
        {
            DrawInstructionAddress.Value = (byte)(DrawHoldCounter.Address + 1);

            var patternTableLoader = new DiskNBitPlaneLoader();
            patternTableLoader.Load(
                new DiskFile(ContentFolder.PatternTables, "test_4color.pt"),
                PatternTable);
        }

        public void DrawFrame(SpriteBatch spriteBatch, Texture2D canvas)
        {
            PatternTablePoint.Reset();
            ScreenPoint.Reset();
            GameSystem.OnHBlank();

            for (int i = 0; i < _screenData.Length; i++)
            {
                if (DrawHoldCounter == 0)
                {
                    ProcessNextDrawInstruction();
                }
                else
                    DrawHoldCounter.Value--;

                var patternTableValue = PatternTable[PatternTablePoint.Index];
                _screenData[i] = GetColor(patternTableValue);

                PatternTablePoint.Next();
                if (ScreenPoint.Next())
                    GameSystem.OnHBlank();
            }

            GameSystem.OnVBlank();
            canvas.SetData(_screenData);
            spriteBatch.Draw(canvas, Vector2.Zero, Color.White);
        }

        private Color GetColor(byte value)
        {
            return Specs.SystemColors[value];
        }

        private void IncDrawInstruction()
        {
            DrawInstructionAddressOffset.Value++;
            if (GameSystem.Memory[DrawInstructionAddress.Value + DrawInstructionAddressOffset.Value] == 0)
                DrawInstructionAddressOffset.Value = 0;
        }

        private void ProcessNextDrawInstruction()
        {
            var drawInstruction = new DrawCommand(DrawInstructionAddress.Value + DrawInstructionAddressOffset.Value, GameSystem.Memory);
            IncDrawInstruction();

            if (drawInstruction.CommandType == DrawCommandType.Hold)
                DrawHoldCounter.Value = (byte)(drawInstruction.Value - 1);
            else
            {
                PatternTablePoint.Advance(drawInstruction.Value);
                drawInstruction = new DrawCommand(DrawInstructionAddress.Value + DrawInstructionAddressOffset.Value, GameSystem.Memory);
                if (drawInstruction.CommandType == DrawCommandType.Hold)
                    DrawHoldCounter.Value = (byte)(drawInstruction.Value - 1);

                IncDrawInstruction();
            }
        }

        public DrawCommand[] Info_GetDrawCommands()
        {
            List<DrawCommand> commands = new List<DrawCommand>();
            int offset = 0;
            while (GameSystem.Memory[DrawInstructionAddress.Value + offset] != 0)
            {
                var cmd = new DrawCommand(DrawInstructionAddress.Value + offset, GameSystem.Memory);
                commands.Add(cmd);

                offset++;
            }

            return commands.ToArray();
        }

        public int AddDrawHoldCommand(int holdAmount) => AddDrawHoldCommand((byte)holdAmount);
        public int AddDrawHoldCommand(byte holdAmount)
        {
            var d = new DrawCommand(DrawInstructionAddress + DrawInstructionAddressOffset, GameSystem.Memory);
            d.CommandType = DrawCommandType.Hold;
            d.Value = holdAmount;

            DrawInstructionAddressOffset.Value++;

            return holdAmount;
        }
        public int AddMoveBrushCommand(int amount)
        {
            var d = new DrawCommand(DrawInstructionAddress + DrawInstructionAddressOffset, GameSystem.Memory);
            d.CommandType = DrawCommandType.MoveBrush;
            d.Value = (byte)amount;

            if (amount != 0)
                DrawInstructionAddressOffset.Value++;

            return amount;
        }
        public int AddMoveBrushToCommand(int destination, int currentOffset)
        {
            int amount = destination - currentOffset;
            amount = amount.Wrap(Specs.PatternTableWidth * Specs.PatternTableHeight);

            if (amount != 0)
                AddMoveBrushCommand(amount);

            return amount;
        }
    }
}
