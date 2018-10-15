using System;
using System.Data;
using System.Windows.Forms;

namespace Origam.DA
{
	/// <summary>
	/// Summary description for Debug.
	/// </summary>
	public sealed class DebugClass
	{
		private static Form _form;
		private static DataGrid dataGrid1;
		
		private DebugClass()
		{
		}

		private static void CheckInit()
		{
			if(_form ==null)
			{
				_form = new Form();
				dataGrid1 = new DataGrid();
				((System.ComponentModel.ISupportInitialize)(dataGrid1)).BeginInit();
				_form.SuspendLayout();
				// 
				// dataGrid1
				// 
				dataGrid1.DataMember = "";
				dataGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
				dataGrid1.Name = "dataGrid1";
				dataGrid1.TabIndex = 0;
				// 
				// Form2
				// 
				_form.Controls.Add(dataGrid1);
				_form.Name = "TEST_FORM";
				_form.Text = "DebugForm";
				((System.ComponentModel.ISupportInitialize)(dataGrid1)).EndInit();
				_form.ResumeLayout(false);				
			
			}
		}

		public static string DataDebug(DataSet dataSet)
		{
			string result = "";

			result = result + "**********************************Begin**ListRowErrors**********************************************" + Environment.NewLine;
			result = result + Environment.NewLine;
			result = result + ListRowErrors(dataSet) + Environment.NewLine;
			result = result + "**********************************end**ListRowErrors**********************************************" + Environment.NewLine;

			result = result + "**********************************Begin**ListUniqueColumns**********************************************" + Environment.NewLine;
			result = result + Environment.NewLine;
			result = result + ListUniqueColumns(dataSet) + Environment.NewLine;
			result = result + "**********************************end**ListUniqueColumns**********************************************" + Environment.NewLine;

			result = result + "**********************************Begin**ListConstraints**********************************************" + Environment.NewLine;
			result = result + Environment.NewLine;
			result = result + ListConstraints(dataSet) + Environment.NewLine;
			result = result + "**********************************end**ListConstraints**********************************************" + Environment.NewLine;

			return result;
		}


		public static void Show(DataSet dataSet)
		{
			CheckInit();
			dataGrid1.DataSource = dataSet;
			_form.ShowDialog();
		}

		public static string ListRowErrors(DataSet dataSet)
		{
			string result = "";

			foreach(DataTable table in dataSet.Tables)
			{
				foreach(DataRow row in table.Rows)
				{
					if(row.HasErrors)
					{
						string items="";
						foreach(object o in row.ItemArray)
						{
							items += o.ToString() + "; ";
						}
		
						result = result + "**********************************Begin************************************************" + Environment.NewLine;
						result = result + "* Table: " + table.TableName + Environment.NewLine;
						result = result + "* Error: " + row.RowError + Environment.NewLine;
						result = result + "* DataItems:" + items + Environment.NewLine;
						result = result + "***********************************End**************************************************" + Environment.NewLine;
					}
				}
			}

			return result;
		}

		public static string ListUniqueColumns(DataSet dataSet)
		{
			string result = "";

			foreach(DataTable table in dataSet.Tables)
			{
				foreach(DataColumn col in table.Columns)
				{
					if(col.Unique)
					{
						result = result + "**********************************Begin************************************************" + Environment.NewLine;
						result = result + "* Table:    " + table.TableName + Environment.NewLine;
						result = result + "* Column:   " + col.ColumnName + Environment.NewLine;
						result = result + "* DataType: "+ col.DataType.FullName.ToString() + Environment.NewLine;
						result = result + "***********************************End**************************************************" + Environment.NewLine;
					}
				}
			}

			return result;
		}


		public static string ListConstraints(DataSet dataSet)
		{
			string result = "";

			foreach(DataTable table in dataSet.Tables)
			{
				foreach(Constraint con in table.Constraints)
				{
						result = result + "**********************************Begin************************************************" + Environment.NewLine;
						result = result + "* Table:          " + table.TableName + Environment.NewLine;
						result = result + "* ConstraintName: " + con.ConstraintName + Environment.NewLine; 
						result = result + "***********************************End**************************************************" + Environment.NewLine;
				}
			}

			return result;
		}
	}
}
