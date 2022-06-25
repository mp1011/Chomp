using System;

namespace Chomp.Models
{
    [Flags]
    public enum RenderAttributes : byte
    {
        FlipX=1,
        FlipY=2,
        Priority=4,
        Visible=8
    }

    public enum BitOffset : byte
    {
        Zero,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven
    }


}
