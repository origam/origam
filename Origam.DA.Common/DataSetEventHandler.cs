using System;
using System.Data;
using System.Windows.Forms;

namespace Origam.DA
{
	/// <summary>
	/// Class Handling all necessary events
	/// </summary>
	public class DataSetEventHandler
	{

		private CurrencyManager _cm;

		public DataSetEventHandler(CurrencyManager cm)
		{
            _cm=cm;		
						
		}

	

		public void HandleDataTable(DataTable table)
		{
			if( table ==null)
				return;
            
			table.RowDeleted +=new DataRowChangeEventHandler(RowDeleted);
			table.RowDeleting +=new DataRowChangeEventHandler(RowDeleting);
			table.ColumnChanged +=new DataColumnChangeEventHandler(ColumnChanged);
		}


		public static void CheckForZeroRowCount(CurrencyManager cm)
		{
			if(cm !=null && cm.Count == 0)
			{
				if ( (cm.List as DataView).Table.ParentRelations.Count < 1 )
					cm.AddNew();
				
			}

		}


		private void RowDeleted(object sender, DataRowChangeEventArgs e)
		{
			if(e.Action == DataRowAction.Delete )
			{
				CheckForZeroRowCount(_cm);
			}

		}

		private void RowDeleting(object sender, DataRowChangeEventArgs e)
		{
			if(e.Action == DataRowAction.Delete)
			{
				CheckForZeroRowCount(_cm);
			}

		}

		bool _insertTime=false;
 		private void ColumnChanged(object sender, DataColumnChangeEventArgs e)
		{

			if(	e.Row.HasVersion(System.Data.DataRowVersion.Original) && 
				e.Row.HasVersion(System.Data.DataRowVersion.Current) && _insertTime && 
				e.Row[e.Column,System.Data.DataRowVersion.Current].ToString() != e.Row[e.Column,System.Data.DataRowVersion.Original].ToString())
			{
				_insertTime=false;
				e.Row["RecordUpdated"]= DateTime.Now;
				e.Row.EndEdit();
				_insertTime=true;
				System.Diagnostics.Debug.WriteLine(
					"Time change was changer => Column CHANGED: " + e.Column.ColumnName + 
					" original Value: " + e.Row[e.Column,System.Data.DataRowVersion.Original].ToString() +
					" current Value: " + e.Row[e.Column,System.Data.DataRowVersion.Current].ToString());
			}
			
		}




		

	}
}
