using System;
using System.Data;
using System.Windows.Forms;
using Origam.Gui;

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for DataGridColumnStyleCreator.
	/// </summary>
	public class DataGridColumnStyleCreator
	{
		public DataGridColumnStyleCreator(){}

		public static DataGridColumnStyle CreateColumnStyle(Control control, AsDataSetConsumerLoader dataDictionary, DataGridFilterFactory filterFactory)
		{
			DataGridColumnStyle result =null;
			DataColumn column= null;
			Application.DoEvents();
			string defProperty = (control as IAsControl).DefaultBindableProperty;
			
			

			if (defProperty == null)
				return null;
			if( control.DataBindings[defProperty] !=null && 
				( control.DataBindings[defProperty].DataSource is DataView ||  control.DataBindings[defProperty].DataSource is DataSet || control.DataBindings[defProperty].DataSource is DataTable) )
			{
				if (control.DataBindings[defProperty].DataSource is DataView)
				{
					column=(control.DataBindings[defProperty].DataSource as DataView).Table.Columns[control.DataBindings[defProperty].BindingMemberInfo.BindingField];
				} 
				else if(control.DataBindings[defProperty].DataSource is DataSet)
				{
					string tableName= FormGenerator.FindTableByDataMember(control.DataBindings[defProperty].DataSource as DataSet, control.DataBindings[defProperty].BindingMemberInfo.BindingPath);
					column=(control.DataBindings[defProperty].DataSource as DataSet).Tables[tableName].Columns[control.DataBindings[defProperty].BindingMemberInfo.BindingField];
				}
				else if (control.DataBindings[defProperty].DataSource is DataTable)
				{
					column=(control.DataBindings[defProperty].DataSource as DataTable).Columns[control.DataBindings[defProperty].BindingMemberInfo.BindingField];
				}
				
			}
			else if( control is AsTextBox ) 
			{
                string texBoxDataMember	= (control as AsTextBox).DataField;
				
				object dsTextBox = null;
                
				if( (control as AsTextBox).DataSource is DataSet )
				{
					dsTextBox= (control as AsTextBox).DataSource;

					string tableName		= FormGenerator.FindTableByDataMember( ((DataSet)dsTextBox), texBoxDataMember );
					string columnName		= FormGenerator.GetColumnNameFromDisplayMember( texBoxDataMember );
					column=((DataSet)dsTextBox).Tables[tableName].Columns[columnName];

				}
				else
				{
					return null;
				}
                                                        				

			}
			else
			{
				return null;
			}

			if(control is AsCombo)
			{
				result = new AsDataGridComboBoxColumn();
                
				object datasource=(control as IAsDataServiceComboConsumer).DataSource; 
				if( !(datasource is DataSet || datasource is DataTable || datasource is DataView) )
					throw new NullReferenceException("No combo Data Provided");
			

				(result as AsDataGridComboBoxColumn).AsCombo.DisplayMember  = (control as IAsDataServiceComboConsumer).DisplayMember;
				(result as AsDataGridComboBoxColumn).AsCombo.ValueMember  = (control as IAsDataServiceComboConsumer).ValueMember;
				(result as AsDataGridComboBoxColumn).AsCombo.DataSource = (control as IAsDataServiceComboConsumer).DataSource;
				(result as AsDataGridComboBoxColumn).AsCombo.DataStructureId = (control as IAsDataServiceComboConsumer).DataStructureId;


				if (control.FindForm() is AsForm)
				{

					(result as AsDataGridComboBoxColumn).AsCombo.ComboRefreshWanted +=new ComboRefresh((control.FindForm() as AsForm).FormGenerator.FormGenerator_ComboRefreshWanted);
				}

				//(result as AsDataGridComboBoxColumn).AsCombo.BindingContext = (control as AsCombo).BindingContext;
                					
			}
			else if(control is AsDateBox)
			{
				//				result= new AsTestStyle(control);
				result = new AsDataViewColumn();
				(result as AsDataViewColumn).FormatInfo =null;

				Binding bind = control.DataBindings["DateValue"];

				(result as AsDataViewColumn).AsDateBox.DataBindings.Add("DateValue", bind.DataSource, bind.BindingMemberInfo.BindingMember);

			}
			else if( control is CheckBox )
			{
				result = new AsCheckStyleColumn(column.AllowDBNull);
				Binding bind = control.DataBindings["Value"];

				(result as AsCheckStyleColumn).CheckBox.DataBindings.Add("Value", bind.DataSource, bind.BindingMemberInfo.BindingMember);
			}
			else 
			{
				
				result = new AsTextBoxStyle();
				(result as AsTextBoxStyle).FormatInfo =null;
				if (control is AsTextBox)
				{
					(result as AsTextBoxStyle).TextBox.MaxLength =(control as AsTextBox).MaxLength;
					(result as AsTextBoxStyle).AsTextBox.MaxLength=(control as AsTextBox).MaxLength;
//					(result as AsTextBoxStyle).TextBox.Multiline=(control as AsTextBox).Multiline;
//					(result as AsTextBoxStyle).AsTextBox.Multiline=(control as AsTextBox).Multiline;
					(result as AsTextBoxStyle).TextBox.Multiline = false;
					(result as AsTextBoxStyle).AsTextBox.Multiline= false;

					
					(result as AsTextBoxStyle).AsTextBox.DataType = (control as AsTextBox).DataType;
					(result as AsTextBoxStyle).AsTextBox.ErrorInfo.BeepOnError=(control as AsTextBox).ErrorInfo.BeepOnError;
					(result as AsTextBoxStyle).AsTextBox.ErrorInfo.ErrorAction=(control as AsTextBox).ErrorInfo.ErrorAction;
					(result as AsTextBoxStyle).AsTextBox.NullText=	(control as AsTextBox).NullText;
					(result as AsTextBoxStyle).AsTextBox.NumericInput=(control as AsTextBox).NumericInput;
					(result as AsTextBoxStyle).AsTextBox.AllowDbNull=(control as AsTextBox).AllowDbNull;
					(result as AsTextBoxStyle).AsTextBox.FormatType=(control as AsTextBox).FormatType;
					
					(result as AsTextBoxStyle).AsTextBox.ErrorInfo.ErrorProvider=(control as AsTextBox).ErrorInfo.ErrorProvider;
					(result as AsTextBoxStyle).AsTextBox.ErrorInfo.ErrorMessage=(control as AsTextBox).ErrorInfo.ErrorMessage;
					(result as AsTextBoxStyle).AsTextBox.ErrorInfo.ErrorMessageCaption=(control as AsTextBox).ErrorInfo.ErrorMessageCaption;
                    
					if( (result as AsTextBoxStyle).AsTextBox.DataType == typeof(string) )
					{
						(result as AsTextBoxStyle).TextBox.TextAlign = HorizontalAlignment.Left;
						(result as AsTextBoxStyle).AsTextBox.TextAlign = HorizontalAlignment.Left;
						(result as AsTextBoxStyle).AsTextBox.NumericInput = false;

					}
					else
					{
						(result as AsTextBoxStyle).TextBox.TextAlign = HorizontalAlignment.Right;
						(result as AsTextBoxStyle).AsTextBox.TextAlign = HorizontalAlignment.Right;
						(result as AsTextBoxStyle).AsTextBox.NumericInput = true;
					}

				}
//				Binding bind = control.DataBindings["Text"];
//				(result as AsTextBoxStyle).AsTextBox.DataBindings.Add("Text", bind.DataSource, bind.BindingMemberInfo.BindingMember);
			}
			
			
			result.HeaderText = SelectCaption(column, control);
			result.MappingName = column.ColumnName;
			result.ReadOnly = column.ReadOnly;
			if( (control is IAsCaptionControl) &&  (control as IAsCaptionControl).GridColumnWidth > 0 )
			{
				result.Width = (control as IAsCaptionControl).GridColumnWidth;
			}
			else
			{			
				result.Width = 100;
			}
			result.NullText = "";	//(prázdné)
			filterFactory.AddControlToPanel(control, column, result.Width);
			return result;
		}

		private static string SelectCaption(DataColumn column, Control control)
		{
			string result="";
			if( column ==null || control==null)
			{
				return result;
			}

			if( control is IAsCaptionControl )
			{
				if( ((IAsCaptionControl)control).GridColumnCaption !=null && ((IAsCaptionControl)control).GridColumnCaption.Length > 0)
					return ((IAsCaptionControl)control).GridColumnCaption;

				if( ((IAsCaptionControl)control).Caption !=null && ((IAsCaptionControl)control).Caption.Length  > 0)
					return ((IAsCaptionControl)control).Caption;
			}
			return column.Caption;
		}


		 
		public static void CopyDefaultTableStyle(DataGrid datagrid, DataGridTableStyle ts)
		{
			ts.AllowSorting = datagrid.AllowSorting;
			ts.AlternatingBackColor = datagrid.AlternatingBackColor;
			ts.BackColor = datagrid.BackColor;
			ts.ColumnHeadersVisible = datagrid.ColumnHeadersVisible;
			ts.ForeColor = datagrid.ForeColor;
			ts.GridLineColor = datagrid.GridLineColor;
			ts.GridLineStyle = datagrid.GridLineStyle;
			ts.HeaderBackColor = datagrid.HeaderBackColor;
			ts.HeaderFont = datagrid.HeaderFont;
			ts.HeaderForeColor = datagrid.HeaderForeColor;
			ts.LinkColor = datagrid.LinkColor;
			ts.PreferredColumnWidth = datagrid.PreferredColumnWidth;
			ts.PreferredRowHeight = datagrid.PreferredRowHeight;
			ts.ReadOnly = datagrid.ReadOnly;
			ts.RowHeadersVisible = datagrid.RowHeadersVisible;
			ts.RowHeaderWidth = datagrid.RowHeaderWidth;
			ts.SelectionBackColor = datagrid.SelectionBackColor;
			ts.SelectionForeColor = datagrid.SelectionForeColor;
			
		}
        


	
	
	
	}
}
