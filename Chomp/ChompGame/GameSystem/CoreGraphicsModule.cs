using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.Graphics;
using ChompGame.ROM;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

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
            PatternTablePoint = builder.AddGridPoint(
                (byte)Specs.PatternTableWidth, 
                (byte)Specs.PatternTableHeight,
                Specs.PatternTablePointMask);

            PatternTable = builder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            ScreenPoint = builder.AddGridPoint((byte)Specs.ScreenWidth, (byte)Specs.ScreenHeight, Specs.ScreenPointMask);
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

        long dummy = 0;
        public void DrawFrame(SpriteBatch spriteBatch, Texture2D canvas)
        {
            PatternTablePoint.Reset();
            ScreenPoint.Reset();
            GameSystem.OnHBlank();

            for (int i = 0; i < _screenData.Length; i++)
            {
                if (i == 31)
                    dummy++;

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

        private DrawCommand NextDrawInstruction()
        {
            var drawInstruction = new DrawCommand(DrawInstructionAddress.Value + DrawInstructionAddressOffset.Value, GameSystem.Memory);
            IncDrawInstruction();
            return drawInstruction;
        }

        private void ProcessNextDrawInstruction()
        {
            int totalAdvance = 0;

            while(true)
            {
                var drawInstruction = NextDrawInstruction();
                if (drawInstruction.CommandType == DrawCommandType.Hold)
                {
                    DrawHoldCounter.Value = (byte)(drawInstruction.Value - 1);
                    return;
                }
                else if(drawInstruction.CommandType == DrawCommandType.MoveBrush)
                {
                    totalAdvance += (int)drawInstruction.Value;
                    PatternTablePoint.Advance(drawInstruction.Value);
                    if (totalAdvance >= PatternTable.Width * PatternTable.Height)
                        return;
                }
            }
        }

        public DrawCommand[] Info_GetDrawCommands()
        {
            List<DrawCommand> commands = new List<DrawCommand>();
            int offset = 0;
            while (GameSystem.Memory[DrawInstructionAddress.Value + offset] != 0 && offset < DrawInstructionAddressOffset.Value)
            {
                var cmd = new DrawCommand(DrawInstructionAddress.Value + offset, GameSystem.Memory);
                commands.Add(cmd);

                offset++;
            }

            return commands.ToArray();
        }

        public int Info_DrawCommandColumn()
        {
            var commands = Info_GetDrawCommands();
            return commands.Sum(p => p.Value).Wrap(Specs.ScreenWidth);
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
            int a = amount;
            while(a > 0)
            {
                if(a >= 128)
                {
                    var d = new DrawCommand(DrawInstructionAddress + DrawInstructionAddressOffset, GameSystem.Memory);
                    d.CommandType = DrawCommandType.MoveBrush;
                    d.Value = (byte)127;
                    a -= 127;
                    DrawInstructionAddressOffset.Value++;
                }
                else
                {
                    var d = new DrawCommand(DrawInstructionAddress + DrawInstructionAddressOffset, GameSystem.Memory);
                    d.CommandType = DrawCommandType.MoveBrush;
                    d.Value = (byte)a;
                    a = 0;
                    DrawInstructionAddressOffset.Value++;
                }
            }

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
