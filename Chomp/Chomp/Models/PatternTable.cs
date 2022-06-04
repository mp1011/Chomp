using Chomp.SystemModels;

namespace Chomp.Models
{
    public class PatternTable : MemoryGrid<TwoBit>
    {
        public PatternTable(TableSpecs tableSpecs, TwoBit firstCell) : base(tableSpecs, firstCell)
        {
        }
    }
}
