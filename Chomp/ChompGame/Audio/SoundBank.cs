using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.Audio
{
    class SoundBank
    {
        private readonly SoundEffect[] _bank;

        public SoundBank(IEnumerable<SoundEffect> bank)
        {
            _bank = bank.ToArray();
        }

        public SoundEffectInstance GetSound(int index)
        {
            return _bank[index].CreateInstance();
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
