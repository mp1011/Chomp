namespace ChompGame.Audio
{
    public enum AudioAction : byte
    {
        PlayA = 0,
        PlayASharp = 1,
        PlayB = 2,
        PlayC = 3,
        PlayCSharp = 4,
        PlayD = 5,
        PlayDSharp = 6,
        PlayE = 7,
        PlayF = 8,
        PlayFSharp = 9,
        PlayG = 10,
        PlayGSharp = 11,
        OctaveUp = 12,
        OctaveDown = 13,
        Unused = 14, //maybe switch between square and saw?
        Rest= 15
    }
}
