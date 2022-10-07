using System.Data;

namespace Origam.DA.ObjectPersistence
{
    public interface IDatasetFormatter
    {
        DataSet Format(DataSet data);	
    }
}