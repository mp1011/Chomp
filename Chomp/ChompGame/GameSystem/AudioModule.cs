using ChompGame.Data;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChompGame.GameSystem
{
    class AudioModule : Module, ILogicUpdateHandler
    {
        public AudioChannel[] Channels { get; private set; }

        public AudioModule(MainSystem mainSystem) : base(mainSystem)
        {
        }

        public override void OnStartup()
        {
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            Channels = new AudioChannel[GameSystem.Specs.AudioChannels];

            var playing = memoryBuilder.AddByte();

            if (Channels.Length > 1)
                throw new Exception("fix me");

            for (int i = 0; i < Channels.Length; i++)
            {
                Channels[i] = new AudioChannel(
                    value: memoryBuilder.AddShort(),
                    playing: new GameBit(playing.Address, Bit.Bit0, memoryBuilder.Memory));
            }
        }

        public void OnLogicUpdate()
        {
            foreach (var channel in Channels)
                channel.Update();
        }       
    }
}
