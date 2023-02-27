using System.Collections.Generic;
using System.Data;
using Origam.DA;

namespace Origam.Workflow.WorkQueue
{


    public partial class WorkQueueData
    {
        partial class WorkQueueEntryRow
        {
            public WorkQueueEntryRow Clone()
            {
                DataSet oneRowDataSet = DatasetTools.CloneDataSet(Table.DataSet);
                DatasetTools.GetDataSlice(oneRowDataSet, new List<DataRow> { this });
                DataRow oneRow = oneRowDataSet.Tables[0].Rows[0];
                return (WorkQueueEntryRow)oneRow;
            }
        }
    }
}
