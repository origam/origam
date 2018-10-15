using System;
using System.Data;

namespace Origam.DA
{
    public class OrigamDataTable : DataTable
    {
        public OrigamDataTable()
        {
           
        }
        public OrigamDataTable(string tableName) : base(tableName)
        {
        }

        override protected Type GetRowType()
        {
            return typeof(OrigamDataRow);
        }

        override protected DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new OrigamDataRow(builder);
        }

        protected override void OnRowChanged(DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Commit)
            {
                ((OrigamDataRow)e.Row).HasChangedOnce = false;
            }
            else if (e.Action == DataRowAction.Nothing)
            {
                return;
            }
            base.OnRowChanged(e);
            if (!IsLoading)
            {
                base.OnRowChanged(e);
            }
        }

        public bool IsLoading { get; set; }
    }
}
