namespace ChompGame.Data
{
    public class NibblePoint
    {
        private readonly NibbleArray _bounds;

        public NibblePoint(NibbleArray bounds)
        {
            _bounds = bounds;
        }

        public NibblePoint(int address, SystemMemory systemMemory)
        {
            _bounds = new NibbleArray(address, systemMemory);
        }

        public byte X
        {
            get => _bounds[0];
            set => _bounds[0] = value;
        }

        public byte Y
        {
            get => _bounds[1];
            set => _bounds[1] = value;
        }
    }
}
