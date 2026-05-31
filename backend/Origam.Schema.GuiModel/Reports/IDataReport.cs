using Origam.Schema.EntityModel;

namespace Origam.Schema.GuiModel
{
    public interface IDataReport
    {
        public DataStructure DataStructure { get; }
        public DataStructureMethod Method { get; }
        public DataStructureSortSet SortSet { get; }
    }
}
