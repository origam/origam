using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Origam.Gui.Win;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema.EntityModel;

namespace Origam.Gui
{
	/// <summary>
	/// Summary description for FormGenerator.
	/// </summary>
	public class FormGenerator
	{

		private IDataService _dataService;
		private Guid _version;

		#region Properties
		private DataSet _ds;
		public DataSet DataSet
		{
			get{return _ds;}
			//set{_ds=value;}
		}

		#endregion

		#region PublicMethods

		public Control LoadFormWithData(FormControlSet formControlSet, IDataService dataService)
		{

			if(formControlSet == null || dataService ==null)
				return null;

			_dataService=dataService;

			ControlSetItem rootItem=formControlSet.ChildItemsByType(ControlSetItem.ItemTypeConst)[0] as ControlSetItem;
			
			if(rootItem== null)
				return null;

			DataStructure ds=formControlSet.DataStructure;

			if(ds==null)
				throw new NullReferenceException("Provided FormControSet doesn't contin definition for DataStructure");

			_version=(Guid)ds.PrimaryKey["SchemaVersionId"];
			DataStructureQuery qry=new DataStructureQuery((Guid)ds.PrimaryKey["Id"],(Guid)ds.PrimaryKey["SchemaVersionId"]);

			if(qry==null)
				throw new NullReferenceException("Can't create Query object");

			_ds=_dataService.LoadDataSet(qry,null);

			return this.LoadControl(rootItem);
		}

		public Control LoadControl(ControlSetItem cntrlSet)
		{
			return this.LoadControl(cntrlSet,null);
		}

		public Control LoadControl(ControlSetItem cntrlSet, string dataMember)
		{
			//create control
			Control cntrl = CreateInstance(cntrlSet, dataMember);
            
			if(cntrl.Tag is ControlSetItem)
			{
				//adding child controls
				SchemaItemCollection col= (cntrl.Tag as ControlSetItem).ChildItemsByType("ControlSetItem");
				foreach(ControlSetItem childItem in col)
				{
					Control addingControl;

					string dataMemeberForChildControls=null;

					if(dataMember !=null && dataMember.Length>0)
					{
						dataMemeberForChildControls =dataMember;
					}
					else if(cntrl is AsPanel)
					{
						dataMemeberForChildControls =(cntrl as AsPanel).DataMember;
					}
					else
					{
						addingControl=LoadControl(childItem);
					}

					addingControl=LoadControl(childItem, dataMemeberForChildControls);

					if(addingControl !=null)
						cntrl.Controls.Add(addingControl);                
				}
			}
			//Tag must be null
			cntrl.Tag =null;
			return cntrl;
		}
		#endregion

		#region PrivateMethods
		
		private Control CreateInstance(ControlSetItem cntrlSet, string dataMember)
		{
			object result=null;

			if(cntrlSet.ControlItem.IsComplexType)
			{
				IControlSet panel=cntrlSet.ControlItem.PanelControlSet as IControlSet;
				if(panel !=null)
				{
					SchemaItemCollection  col = panel.ChildItemsByType("ControlSetItem");
					if(col !=null && col.Count == 1)
					{
						//we must find right DataMember

						//TODO: Should be optimalized... 
						AsPanel testPanel=new AsPanel();
                        testPanel.Tag = cntrlSet;
						ControlProperties(testPanel);

						//founded datamember provide to controlcreation
						result=LoadControl(col[0] as ControlSetItem,testPanel.DataMember) as Control;
						testPanel=null;
						(result as Control).Tag=cntrlSet;
						if(_ds !=null && result is AsPanel)
							(result as AsPanel).DataSource = _ds;
						ControlProperties(result as Control);
						return (result as Control);
					}
				}
				
			}
			if( cntrlSet==null || 
				cntrlSet.ControlItem == null || 
				cntrlSet.ControlItem.ControlType == null ||
				cntrlSet.ControlItem.ControlNamespace == null)
				throw new ArgumentException("Parametr is null or inner parameters are null", "cntrlSet");
            
			result=Origam.Reflector.InvokeObject(cntrlSet.ControlItem.ControlType, cntrlSet.ControlItem.ControlNamespace);

			if(result==null || (!(result is Control)))
				throw new NullReferenceException("Unsupported type: " + cntrlSet.ControlItem.ControlType);

			(result as Control).Tag=cntrlSet;
			ControlProperties(result as Control, dataMember);
			return (result as Control);
		}

	

		private void ControlProperties(Control cntrl)
		{
			this.ControlProperties(cntrl,null);
		}

		private void ControlProperties(Control cntrl, string dataMember)
		{
			if(cntrl.Tag is ControlSetItem)
			{
				ControlSetItem cntrSetItem=cntrl.Tag as ControlSetItem;
				foreach(PropertyValueItem propValItem in cntrSetItem.ChildItemsByType(PropertyValueItem.ItemTypeConst))
				{
					//addinng default properties to control set item
					Type t=cntrl.GetType();
					PropertyInfo property = t.GetProperty(propValItem.Name);
						
					if(propValItem.Value !=null)
					{
						object val=null;
						System.Xml.Serialization.XmlSerializer ser  = new System.Xml.Serialization.XmlSerializer(property.PropertyType);

						if(propValItem.Value !=null)
						{
							System.IO.StringReader reader = new System.IO.StringReader(propValItem.Value);
							val=ser.Deserialize(reader);
							property.SetValue(cntrl,val,new object[0]);
						}
					}
				}

				if(cntrl is AsCombo)
				{
					(cntrl as AsCombo).DataStructureSchemaVersionId =_version;
					(cntrl as AsCombo).DataService = _dataService;

				}

                
				//if datamember is set try to set also databindings
				if(	dataMember ==null || dataMember.Length < 1 ||
					_ds ==null || _ds.Tables.Count < 1)
					return;

				foreach(PropertyBindingInfo bindItem in cntrSetItem.ChildItemsByType(PropertyBindingInfo.ItemTypeConst))
				{
					//addinng default properties to control set item
					Type t=cntrl.GetType();
					PropertyInfo property = t.GetProperty(bindItem.Name);
					if(property !=null)
					{
						Binding bind= new Binding(bindItem.Name,_ds, dataMember + "." + bindItem.Value);
						cntrl.DataBindings.Add(bind);
						
						// In case that dataMember is a path through relations, we find the last table
						// so we can take a caption out of it
						string tableName;
						if(dataMember.IndexOf(".") > 0)
						{
							string[] path = dataMember.Split(".".ToCharArray());
							DataTable table = _ds.Tables[path[0]];
							for(int i = 1; i < path.Length; i++)
							{
								table = table.ChildRelations[path[i]].ChildTable;
							}
							tableName = table.TableName;
						}
						else
							tableName = dataMember;

						if( (cntrl is AsTextBox) && ( (cntrl as AsTextBox).Caption =="LABEL" || (cntrl as AsTextBox).Caption ==""))
							(cntrl as AsTextBox).Caption = _ds.Tables[tableName].Columns[bindItem.Value].Caption;
                        
					}
				}
			}
		}		

		private PropertyValueItem FindPropertyValueItem (ControlSetItem controlSetItem, ControlPropertyItem propertyToFind)
		{
			PropertyValueItem result=null;
			foreach(PropertyValueItem item in controlSetItem.ChildItemsByType("PropertyValueItem"))
			{
				if(item.ControlPropertyItem.PrimaryKey.Equals(propertyToFind.PrimaryKey))
				{
					result=item;
					break;
				}
			}
			return result;
		}
		#endregion
	}
}
