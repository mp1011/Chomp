using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.Graphics
{
    public class ScanlineDrawCommands
    {
        private SystemMemory _memory;
        private NBitPlane _patternTable;
        private Specs _specs;

        public int FirstDrawInstructionAddress { get; }
        public GameByte DrawInstructionAddressOffset { get; }
        public GameByte DrawHoldCounter { get; }
        public GameByteGridPoint PatternTablePoint { get; private set; }

        public int CurrentDrawInstructionAddress => FirstDrawInstructionAddress + DrawInstructionAddressOffset.Value;

        public ScanlineDrawCommands(SystemMemoryBuilder builder, NBitPlane patternTable, Specs specs)
        {
            _specs = specs;
            _patternTable = patternTable;
            PatternTablePoint = builder.AddGridPoint(
                (byte)specs.PatternTableWidth,
                (byte)specs.PatternTableHeight,
                specs.PatternTablePointMask);

            DrawInstructionAddressOffset = builder.AddByte();
            DrawHoldCounter = builder.AddByte();
            FirstDrawInstructionAddress = builder.CurrentAddress;
            builder.AddBytes(specs.MaxDrawInstructions);
            _memory = builder.Memory;
        }

        public DrawCommand GetCurrentCommand() => new DrawCommand(CurrentDrawInstructionAddress, _memory);

        private void IncDrawInstruction()
        {
            DrawInstructionAddressOffset.Value++;
            if (GetCurrentCommand().IsEndMarker)
                DrawInstructionAddressOffset.Value = 0;
        }

   
        public byte Update()
        {
            if (DrawHoldCounter == 0)
                ProcessNextDrawInstruction();
            
            DrawHoldCounter.Value--;

            var ptValue = _patternTable[PatternTablePoint.Index];

            var currentInstruction = GetCurrentCommand();
            if(currentInstruction.PTMove)
                PatternTablePoint.Next();

            return ptValue;
        }

        private void ProcessNextDrawInstruction()
        {
            IncDrawInstruction();
            var drawInstruction = GetCurrentCommand();
            int totalAdvance = 0;

            if(drawInstruction.IsRepositionMarker)
            {
                IncDrawInstruction();
                drawInstruction = GetCurrentCommand();
                while(drawInstruction.PTMove && totalAdvance < _specs.PatternTableWidth*_specs.PatternTableHeight)
                {
                    if (drawInstruction.Value == 0)
                        break;
                    totalAdvance += drawInstruction.Value;
                    PatternTablePoint.Advance(drawInstruction.Value);
                    IncDrawInstruction();
                    drawInstruction = GetCurrentCommand();
                }
                PatternTablePoint.Advance(drawInstruction.Value);
                IncDrawInstruction();
                drawInstruction = GetCurrentCommand();
            }
           
            DrawHoldCounter.Value = drawInstruction.Value;         
        }

        public string[] Info_GetDrawCommandDescriptions()
        {
            bool isReposition = false;
            List<string> commands = new List<string>();
            int offset = 0;
            while (true)
            {
                var cmd = new DrawCommand(FirstDrawInstructionAddress + offset, _memory);
                if (cmd.IsEndMarker)
                    break;
                
                if (!isReposition)
                    commands.Add(cmd.ToString());
                else
                {
                    if (cmd.PTMove)
                        commands.Add($"Reposition_Continue {cmd.Value}");
                    else
                    {
                        commands.Add($"Reposition_Stop {cmd.Value}");
                        isReposition = false;
                    }
                }

                if (cmd.IsRepositionMarker)
                    isReposition = true;

                offset++;
            }

            return commands.ToArray();
        }

        public void AddDrawCommand(bool moveIndex, byte amount)
        {
            var cmd = GetCurrentCommand();
            cmd.PTMove = moveIndex;
            cmd.Value = amount;
            DrawInstructionAddressOffset.Value++;
        }

        public void AddInstructionEndMarker()
        {
            var cmd = GetCurrentCommand();
            cmd.IsEndMarker = true;
            DrawInstructionAddressOffset.Value = 0;
        }

        public void AddDrawEndCommand()
        {
            var cmd = GetCurrentCommand();
            cmd.IsEndMarker = true;
        }

        public void AddMoveCommands(int destination, int currentIndex)
        {
            int amount = destination - currentIndex;
            amount = amount.Wrap(_specs.PatternTableWidth * _specs.PatternTableHeight);
            if (amount == 0)
                return;

            GetCurrentCommand().IsRepositionMarker = true;
            DrawInstructionAddressOffset.Value++;

            DrawCommand cmd;
            while(amount > 127)
            {
                cmd = GetCurrentCommand();
                amount -= 127;
                cmd.PTMove = true;
                cmd.Value = 127;
                DrawInstructionAddressOffset.Value++;
            }

            if (amount > 0)
            {
                cmd = GetCurrentCommand();
                cmd.PTMove = false;
                cmd.Value = (byte)amount;
                DrawInstructionAddressOffset.Value++;
            }
        }

    }
}
