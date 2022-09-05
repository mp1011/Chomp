using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.Audio
{
    class ToneBank
    {
        private readonly SoundEffect[] _bank;

        public ToneBank(IEnumerable<SoundEffect> bank)
        {
            _bank = bank.ToArray();
        }
        
        public SoundEffectInstance CreateInstance(int index)
        {
            if(index >= _bank.Length)
                return _bank[0].CreateInstance();
            else 
                return _bank[index].CreateInstance();
        }
    }
}
