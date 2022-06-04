using Chomp.SystemModels;

namespace Chomp.Models
{

    public class NameTable : MemoryGrid<Nibble>
    {
        public NameTable(TableSpecs tableSpecs, Nibble firstCell) : base(tableSpecs, firstCell)
        {
        }
    }
}
