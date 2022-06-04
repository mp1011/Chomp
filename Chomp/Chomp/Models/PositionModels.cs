using Chomp.SystemModels;

namespace Chomp.Models
{
    public interface IPoint
    {
        int X { get; }
        int Y { get; }
    }
    public interface IBytePoint
    {
        byte X { get; }
        byte Y { get;}
    }

    public record Point(SignedInt X, SignedInt Y) : IPoint, IAddressable 
    {
        public int Address => throw new System.NotImplementedException();

        int IPoint.X { get => throw new System.NotImplementedException(); }
        int IPoint.Y { get => throw new System.NotImplementedException();  }
    }

    public class GridPoint : IBytePoint, IAddressable
    {
        public byte X => throw new System.NotImplementedException();

        public byte Y => throw new System.NotImplementedException();

        public int Address => throw new System.NotImplementedException();
    }

    public record GridPointRecord(byte X, byte Y) : IBytePoint
    {
    }
}
