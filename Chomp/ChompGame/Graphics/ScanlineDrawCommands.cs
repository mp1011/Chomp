using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ChompGame.Graphics
{
    public class ScanlineDrawCommands
    {
        private SystemMemory _memory;
        private NBitPlane _patternTable;
        private Specs _specs;

        private DrawInstructionGroup _currentInstructionGroup;
        private DrawInstruction _currentInstruction;

        public int FirstDrawInstructionAddress { get; }
        public GameByte DrawInstructionAddressOffset { get; }
        public GameByte DrawHoldCounter { get; }
        public GameByteGridPoint PatternTablePoint { get; private set; }

        public int CurrentDrawInstructionAddress => FirstDrawInstructionAddress + DrawInstructionAddressOffset.Value;

        public ScanlineDrawCommands(SystemMemoryBuilder builder, NBitPlane patternTable, Specs specs)
        {
            _specs = specs;
            _patternTable = patternTable;

            if (specs.PatternTableWidth == 255 && specs.PatternTableHeight == 255)
            {
                PatternTablePoint = builder.AddFullGridPoint();
            }
            else
            {
                PatternTablePoint = builder.AddGridPoint(
                    (byte)specs.PatternTableWidth,
                    (byte)specs.PatternTableHeight,
                    specs.PatternTablePointMask);
            }

            DrawInstructionAddressOffset = builder.AddByte();
            DrawHoldCounter = builder.AddByte();
            FirstDrawInstructionAddress = builder.CurrentAddress;
            builder.AddBytes(specs.MaxDrawInstructions);

            _currentInstructionGroup = new DrawInstructionGroup(builder.AddByte(), builder.Memory);
            _currentInstruction = new DrawInstruction(builder.AddByte(), _currentInstructionGroup);

            _memory = builder.Memory;
        }
   
        public byte Update()
        {
            if (DrawHoldCounter == 0)
                ProcessNextDrawInstruction();

            DrawHoldCounter.Value--;

            var ptValue = _patternTable[PatternTablePoint.Index];
            if (_currentInstruction.OpCode == DrawOpcode.Advance)
                PatternTablePoint.Next();

            return ptValue;
        }

        private void ProcessNextDrawInstruction()
        {
            NextDrawInstruction();

            switch(_currentInstruction.OpCode)
            {
                case DrawOpcode.Reposition:
                    PatternTablePoint.Advance(_currentInstruction.Value);
                    NextDrawInstruction();
                    DrawHoldCounter.Value = _currentInstruction.Value;
                    break;
                case DrawOpcode.RepositionTile:
                    PatternTablePoint.Advance(_currentInstruction.Value * _specs.TileWidth);
                    NextDrawInstruction();
                    DrawHoldCounter.Value = _currentInstruction.Value;
                    break;
                default:
                    DrawHoldCounter.Value = _currentInstruction.Value;
                    return;
            }
        }

        public void BeginAddDrawInstructions()
        {
            DrawInstructionAddressOffset.Value = 0;
            _currentInstructionGroup.Address = FirstDrawInstructionAddress;
            _currentInstruction.OpCodeIndex = 0;
        }

        private void NextDrawInstruction()
        {
            if(DrawInstructionAddressOffset.Value == 255)
            {
                DrawInstructionAddressOffset.Value = 0;
                _currentInstructionGroup.Address = FirstDrawInstructionAddress + DrawInstructionAddressOffset.Value;
                _currentInstruction.OpCodeIndex = 0;                
                return;
            }

            switch (_currentInstruction.OpCodeIndex)
            {
                case 3:
                    DrawInstructionAddressOffset.Value += 5;
                    _currentInstructionGroup.Address = FirstDrawInstructionAddress + DrawInstructionAddressOffset.Value;
                    _currentInstruction.OpCodeIndex = 0;

                    break;
                default:
                    _currentInstruction.OpCodeIndex++;
                    break;
            }
        }

        //private void ProcessNextDrawInstruction()
        //{
        //    IncDrawInstruction();
        //    var drawInstruction = GetCurrentCommand();
        //    int totalAdvance = 0;

        //    while(totalAdvance < _specs.PatternTableWidth * _specs.PatternTableHeight 
        //        && drawInstruction.IsRepositionMarker)
        //    {
        //        IncDrawInstruction();
        //        drawInstruction = GetCurrentCommand();
        //        while(drawInstruction.PTMove && totalAdvance < _specs.PatternTableWidth*_specs.PatternTableHeight)
        //        {
        //            if (drawInstruction.Value == 0)
        //                break;
        //            totalAdvance += drawInstruction.Value;
        //            PatternTablePoint.Advance(drawInstruction.Value);
        //            IncDrawInstruction();
        //            drawInstruction = GetCurrentCommand();
        //        }
        //        PatternTablePoint.Advance(drawInstruction.Value);
        //        IncDrawInstruction();
        //        drawInstruction = GetCurrentCommand();
        //    }
           
        //    DrawHoldCounter.Value = drawInstruction.Value;         
        //}

        //public string[] Info_GetDrawCommandDescriptions(bool stopAtCurrentIndex)
        //{
        //    bool isReposition = false;
        //    List<string> commands = new List<string>();
        //    int offset = 0;
        //    while (true)
        //    {
        //        if (stopAtCurrentIndex && offset == DrawInstructionAddressOffset.Value)
        //            break;

        //        var cmd = new DrawCommand(FirstDrawInstructionAddress + offset, _memory);
        //        if (cmd.IsEndMarker)
        //            break;
                
        //        if (!isReposition)
        //            commands.Add(cmd.ToString());
        //        else
        //        {
        //            if (cmd.PTMove)
        //                commands.Add($"Reposition_Continue {cmd.Value}");
        //            else
        //            {
        //                commands.Add($"Reposition_Stop {cmd.Value}");
        //                isReposition = false;
        //            }
        //        }

        //        if (cmd.IsRepositionMarker)
        //            isReposition = true;

        //        offset++;
        //    }

        //    return commands.ToArray();
        //}

        //public void AddDrawCommand(bool moveIndex, byte amount)
        //{
        //    if (amount == 0)
        //        return;

        //    var cmd = GetCurrentCommand();
        //    cmd.PTMove = moveIndex;
        //    cmd.Value = amount;
        //    DrawInstructionAddressOffset.Value++;
        //}

        public void AddTileMoveCommand(int destination, int currentIndex)
        {            
            int amount = destination - currentIndex;
            amount = amount.Wrap(_specs.PatternTableWidth * _specs.PatternTableHeight);
            if (amount == 0)
                return;

            amount = amount / _specs.TileWidth;
            _currentInstruction.OpCode = DrawOpcode.RepositionTile;
            _currentInstruction.Value = (byte)amount;
            NextDrawInstruction();
        }

        public void AddDrawCommand(bool moveIndex, byte amount)
        {            
            _currentInstruction.OpCode = moveIndex ? DrawOpcode.Advance : DrawOpcode.Hold;
            _currentInstruction.Value = amount;
            NextDrawInstruction();            
        }

        //public void AddMoveCommands(int destination, int currentIndex)
        //{
        //    int amount = destination - currentIndex;
        //    amount = amount.Wrap(_specs.PatternTableWidth * _specs.PatternTableHeight);
        //    if (amount == 0)
        //        return;

        //    GetCurrentCommand().IsRepositionMarker = true;
        //    DrawInstructionAddressOffset.Value++;

        //    DrawCommand cmd;
        //    while(amount > 127)
        //    {
        //        cmd = GetCurrentCommand();
        //        amount -= 127;
        //        cmd.PTMove = true;
        //        cmd.Value = 127;
        //        DrawInstructionAddressOffset.Value++;
        //    }

        //    if (amount > 0)
        //    {
        //        cmd = GetCurrentCommand();
        //        cmd.PTMove = false;
        //        cmd.Value = (byte)amount;
        //        DrawInstructionAddressOffset.Value++;
        //    }
        //}

    }
}
