using System;
using System.Data;
using System.Collections;
using System.Windows.Forms;
using Origam.DA;



namespace Origam.Gui.Win
{
	public class AsDataViewContext : IDisposable
	{
		private Hashtable _dataTables;

		#region Properties
		private DataSet _data;
		public DataSet Data
		{
			get
			{
				return _data;
			}
			set
			{
				_data = value;
			}
		}

		private bool _isLoaded = false;
		public bool IsLoaded
		{
			get
			{
				return _isLoaded;
			}
			set
			{
				_isLoaded = value;
			}
		}
		#endregion

		#region Functions
		public DataView GetDataView(string tableName, int contextValue)
		{
			if(contextValue<0 || Data ==null || (!Data.Tables.Contains(tableName)) )
				return null;

			if(_dataTables==null)
				_dataTables = new Hashtable();

			DataView result;
			Hashtable context;
			
			if(_dataTables.Contains(tableName))
				context=_dataTables[tableName] as Hashtable;
			else
			{
				context = new Hashtable();
				if(contextValue==0)
				{
					result =Data.Tables[tableName].DefaultView;
					context.Add(contextValue, result);
				}
				else
				{
					result=new DataView(Data.Tables[tableName]);
					context.Add(contextValue, result);
				}
				
				_dataTables.Add(tableName, context);
				return result;
			}

            
			if( context.Contains(contextValue) )
				return (context[contextValue] as DataView);
			else
			{
				result = new DataView(Data.Tables[tableName]);
				context.Add(contextValue, result);
				return result;
			}
		}
		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if(this._dataTables != null) this._dataTables.Clear();
			_dataTables = null;
			
			// clear is not possible if DataSet has a XmlDataDocument, which we have on workflows
			//this.Data.Clear();
			
			this.Data.Dispose();
			this.Data = null;
		}

		#endregion
	}
}
