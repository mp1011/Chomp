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

        public GameByte PaletteIndex { get; private set; }
        public GameByte CurrentPaletteSwitch { get; private set; }

        public int FirstDrawInstructionAddress { get; }
        public GameByte DrawInstructionAddressOffset { get; }
        public GameByte DrawHoldCounter { get; }
        public GameByteGridPoint PatternTablePoint { get; private set; }

        public int CurrentDrawInstructionAddress => FirstDrawInstructionAddress + DrawInstructionAddressOffset.Value;

        public ScanlineDrawCommands(SystemMemoryBuilder builder, NBitPlane patternTable, Specs specs)
        {
            _specs = specs;
            _patternTable = patternTable;

            if (specs.PatternTableWidth == 256 && specs.PatternTableHeight == 256)
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
            CurrentPaletteSwitch = builder.AddByte();

            //todo, configurable
            builder.AddBytes(4);

            PaletteIndex = builder.AddByte();

            FirstDrawInstructionAddress = builder.CurrentAddress;
            builder.AddBytes(specs.MaxDrawInstructions);

            _currentInstructionGroup = new DrawInstructionGroup(builder.AddByte(), builder.Memory);
            _currentInstruction = new DrawInstruction(builder.AddByte(), _currentInstructionGroup);

            _memory = builder.Memory;
        }
   
        public byte Update()
        {
            if (DrawHoldCounter == 0)
            {
                var paletteSwitch = new PaletteSwitch(CurrentPaletteSwitch.Address + CurrentPaletteSwitch.Value + 1, _memory);
                if (paletteSwitch.CommandCount == 0)
                {
                    CurrentPaletteSwitch.Value++;
                    PaletteIndex.Value = paletteSwitch.Palette;
                }
                else
                    paletteSwitch.CommandCount--;

                ProcessNextDrawInstruction();
            }

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

        public int AddTileMoveCommand(int destination, int currentIndex)
        {            
            int amount = destination - currentIndex;
            amount = amount.Wrap(_specs.PatternTableWidth * _specs.PatternTableHeight);
            if (amount == 0)
                return 0;

            amount = amount / _specs.TileWidth;
            _currentInstruction.OpCode = DrawOpcode.RepositionTile;
            _currentInstruction.Value = (byte)amount;
            NextDrawInstruction();
            return 1;
        }

        public int AddDrawCommand(bool moveIndex, byte amount)
        {            
            _currentInstruction.OpCode = moveIndex ? DrawOpcode.Advance : DrawOpcode.Hold;
            _currentInstruction.Value = amount;
            NextDrawInstruction();
            return 1;
        }
    }
}
