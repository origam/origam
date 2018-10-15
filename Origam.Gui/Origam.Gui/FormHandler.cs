using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Collections;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Reflection;


namespace Origam.Gui
{
	public class FormHandler 
	{

		public const string NULL_GUID =  "00000000-0000-0000-0000-000000000000";
		private ArrayList _defaultProperties= new ArrayList();
		
		
		public FormHandler()
		{
			_dsGUI = new dsOrigamGui();
			_propertiesToDesign = new ArrayList();
			_propertiesToDesign.Add("Location");
			LoadDefaultProperties(_defaultProperties);
		}

		private void LoadDefaultProperties(ArrayList _dp)
		{

			//Default properties shlould be saved in database
			_dp.Add("Name");
			_dp.Add("Text");
			_dp.Add("Location");
			_dp.Add("Size");

		}

	

		private dsOrigamGui _dsGUI;
		public dsOrigamGui DataSource
		{
			get { return _dsGUI; }
			set { _dsGUI = value;}
		}

		private DataSet _dsORIGAM;
		public DataSet OrigamDataSource
		{
			get { return _dsORIGAM; }
			set { _dsORIGAM = value;}
		}

        private ArrayList _propertiesToDesign;
		public ArrayList PropertiesToDesign
		{
			get { return _propertiesToDesign;}
			set{ _propertiesToDesign = value;}
		}

		private DataRow GetRow (string TableName, object[] rowId)
		{
			return _dsGUI.Tables[TableName].Rows.Find(rowId);
		}

		#region "Loading Form"
		public object LoadForm(Guid controlSetGuid)
		{
			dsOrigamGui.ControlSetRow row = ((dsOrigamGui.ControlSetRow)_dsGUI.ControlSet.Rows.Find(controlSetGuid));

			if(row!=null)
			{
				dsOrigamGui.ControlSetItemRow[] chldRows = 
					((dsOrigamGui.ControlSetItemRow[])_dsGUI.ControlSetItem.Select("ControlSetGuid='" + row.ControlSetGuid.ToString() 
						+ "' and ParentControlGuid='00000000-0000-0000-0000-000000000000'"));
				return LoadControl(chldRows[0]);
			}
			else
			{
				//throw new ArgumentException("Provided Guid " + controlSetGuid.ToString() + " doesn't exist in DataSource",controlSetGuid);
			}

			return null;
		}

		private Control LoadControl(dsOrigamGui.ControlSetItemRow currentRow)
		{
			//create control
			Control cntrl = CreateInstance(currentRow);
            
			//Get child rows
			dsOrigamGui.ControlSetItemRow[] childControls = 
				((dsOrigamGui.ControlSetItemRow[])
				_dsGUI.ControlSetItem.Select
				("ParentControlGuid='"+currentRow.ControlSetItemGuid.ToString()+"'"));
			
			//adding child controls
			foreach(dsOrigamGui.ControlSetItemRow row in 	childControls)
			{
				cntrl.Controls.Add(LoadControl(row));                
			}
			return cntrl;
		}

		private Control CreateInstance(dsOrigamGui.ControlSetItemRow cntrlItemRow)
		{
			object o;
			string cntrlType="";
			string endType;
			string nameSpace;

			try
			{
				cntrlType=((dsOrigamGui.ControlRow)cntrlItemRow.GetParentRow("ControlControlSetItem")).ControlType;
				endType = ((string[])cntrlType.Split(".".ToCharArray()))[cntrlType.Split(".".ToCharArray()).Length-1].ToString();
				nameSpace = cntrlType.Substring(0,cntrlType.Length-endType.Length -1);
            
				//Creating Instance 

				//Assembly asm = Assembly.Load(nameSpace);
				Assembly asm = Assembly.LoadWithPartialName(nameSpace);
				o= asm.CreateInstance(cntrlType); 		
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(cntrlType + ex.ToString());
				throw new ArgumentException("Can't render control", "cntrlItemRow",ex);
			}

			SettingProperties(o as Control,cntrlItemRow);
			return o as Control;
		}


		private bool SettingProperties(Control cntrl, dsOrigamGui.ControlSetItemRow row)
		{
			bool rslt=true;

			DataRow[] rsc1 = row.GetChildRows("ControlSetItemItemProperty");
			DataRow[] rsc2 = _dsGUI.ItemProperty.Select("MasterControlSetItemGuid='" + row.ControlSetItemGuid.ToString() + "'");
			DataRow[] result= RowResult(rsc1,rsc2);
                        
			foreach(DataRow rw in result)
			{
				dsOrigamGui.ItemPropertyRow itmRw=((dsOrigamGui.ItemPropertyRow)rw);
				dsOrigamGui.PropertyValueRow[] prop=
					((dsOrigamGui.PropertyValueRow[])itmRw.GetChildRows("ItemPropertyPropertyValue"));
				Type t=cntrl.GetType();
				PropertyInfo property = t.GetProperty(prop[0].Name);

				property.SetValue(cntrl,ConvertingValue(prop[0].Name,prop[0].Value),null);
			}

			return rslt;
		}

		private object ConvertingValue(string propName, string val)
		{
			string X,Y,tmp;
			object result;
			switch(propName.ToUpper())
			{
				case "LOCATION":
					tmp=val.Replace("{","");
					tmp=tmp.Replace("}","");
					tmp=tmp.Replace("X=","");
					tmp=tmp.Replace("Y=","");
					X=((string[])tmp.Split(",".ToCharArray()))[0];
					Y=((string[])tmp.Split(",".ToCharArray()))[1];
					result = new System.Drawing.Point(System.Convert.ToInt32(X),System.Convert.ToInt32(Y));					
					break;

				case "SIZE":
					tmp=val.Replace("{","");
					tmp=tmp.Replace("}","");
					tmp=tmp.Replace("Width=","");
					tmp=tmp.Replace("Height=","");
					X=((string[])tmp.Split(",".ToCharArray()))[0];
					Y=((string[])tmp.Split(",".ToCharArray()))[1];
					result = new System.Drawing.Size(System.Convert.ToInt32(X),System.Convert.ToInt32(Y));					
					break;

				default:            
					result = val;
					break;
			}
			return result;
		}


		private DataRow[] RowResult(DataRow[] rsc1, DataRow[] rsc2)
		{
			DataRow[] result = new DataRow[0];
			if(rsc1==null && rsc2==null)
			{
				return null;
			}
			else
			{
				//one of them are null
				if(rsc2==null)
				{
					result=rsc1;
				}
				else if(rsc2==null)
				{
					result=rsc1;
				}
				
				else
				{
					//both are instance  (we copy into results)
					result = new DataRow[rsc1.Length+rsc2.Length];
					Array.Copy(rsc1,0,result,0,rsc1.Length);
					Array.Copy(rsc2,0,result,rsc1.Length,rsc2.Length);
				}
			}
			return result;
			
		}

    	#endregion


		#region "Saving form"

		
		public void SaveForm(Form frm)
		{
			ScanControl(frm as Control,null);
           
		}

		private void ScanControl(Control ctrl, Control parentCtrl)
		{
			SaveControl(ctrl,parentCtrl);
			foreach(Control con in ctrl.Controls)
			{
				ScanControl(con,ctrl);
			}
		}

		private bool SaveControl(Control control, Control parentControl)
		{
			bool result=true;
			bool isRoot = (parentControl == null);
			OrigamControlIdentifier gCntrl= new OrigamControlIdentifier();
			try
			{
				gCntrl = GetOrigamControlIdentifier(control);
			}
			catch(ArgumentException ex)
			{
				result=false;
				System.Diagnostics.Debug.WriteLine("Control : " + control.Name + 
					"(Type: " + control.GetType().ToString() + ") doesn't contains OrigamControlIdentifier OrigamControlIdentifier in its tag property" + ex.Message); 
			}
			
			if(!result){return false;}

			OrigamControlIdentifier gPrntCntrl; 
			if(!isRoot)
			{
				gPrntCntrl=GetOrigamControlIdentifier(parentControl);
			}
			else
			{
				//Saving Control Set
				SaveControlSet(gCntrl);
			}

			//Adding new row to controlSetItemTable			
			dsOrigamGui.ControlSetItemRow CtrlRow = _dsGUI.ControlSetItem.NewControlSetItemRow();
			
			CtrlRow.ControlSetItemGuid = gCntrl.ControlSetItemGuid;
			CtrlRow.ControlName = control.Name.ToString();
			CtrlRow.ParentControlGuid = gCntrl.ControlSetItemParentGuid;
			CtrlRow.ControlGuid = GetOrigamControlType(control,gCntrl);
			CtrlRow.ControlSetGuid = gCntrl.ControlSetGuid;
			
			_dsGUI.ControlSetItem.Rows.Add(CtrlRow);
    
			//Saving All Properties Values		
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(control);
			PropertyDescriptor propertyDescriptor;
			ArrayList props = GetControlProperties(gCntrl);
			foreach (string propName in props)
			{
				//gets descriptor for current property
				propertyDescriptor = properties[propName];

				//new Item property row
				dsOrigamGui.ItemPropertyRow itemRow = _dsGUI.ItemProperty.NewItemPropertyRow();

				itemRow.ItemPropertyGuid = NewGuid();
				itemRow.ControlSetItemGuid = gCntrl.ControlSetItemGuid;
				itemRow.MasterControlSetItemGuid = gCntrl.ControlSetGuid;
                				
				_dsGUI.ItemProperty.Rows.Add(itemRow);

				//new PropertyValue row
				dsOrigamGui.PropertyValueRow row = _dsGUI.PropertyValue.NewPropertyValueRow();
				
				row.PropertyValueGuid = NewGuid();
				row.Value = propertyDescriptor.GetValue(control).ToString();
				row.ControlPropertyGuid = GetPropertyGuid(gCntrl,propName);
				row.ItemPropertyGuid  = itemRow.ItemPropertyGuid;
				row.Name = propName;
				
				_dsGUI.PropertyValue.AddPropertyValueRow(row);
			}
			System.Diagnostics.Debug.WriteLine("*****************************************************************");
			System.Diagnostics.Debug.WriteLine("* Control SetItem GUID:" + gCntrl.ControlSetItemGuid);
			System.Diagnostics.Debug.WriteLine("* Parent  SetItem GUID:" + gCntrl.ControlSetItemParentGuid.ToString());
			System.Diagnostics.Debug.WriteLine("* Name " + control.Name);
			System.Diagnostics.Debug.WriteLine("* Text " + control.Text);
			System.Diagnostics.Debug.WriteLine("* Type " + control.GetType().ToString()); 
			System.Diagnostics.Debug.WriteLine("* Location" + control.Location.ToString());
			System.Diagnostics.Debug.WriteLine("* Size" + control.Size.ToString());
			System.Diagnostics.Debug.WriteLine("*****************************************************************");

			return true;
		}

		
        
		private ArrayList GetControlProperties(OrigamControlIdentifier ident)
		{
			ArrayList result= new ArrayList();
			DataRow[] rows = _dsGUI.ControlProperty.Select("ControlGuid='" + ident.ControlGuid.ToString()+"'");
			if(rows.Length >0)					
			{
				foreach(dsOrigamGui.ControlPropertyRow row in rows as dsOrigamGui.ControlPropertyRow[])
				{
					result.Add(row.PropertyName);
				}

			}
			else
			{
				//to be replaced just generate TestData
				// replace this with Throw error
				result.Add("Name");
				result.Add("Location");
				result.Add("Size");
				result.Add("Text");
				foreach(string prop in result)
				{
					GetPropertyGuid(ident, prop);
				}
			}
			return result;

		}



		#endregion

		private void ShowDS(dsOrigamGui ds)
		{
			frmDs frm = new frmDs();
			frm.ds = ds;
			frm.ShowDialog();


		}

		private void DsForm(dsOrigamGui ds)
		{
			DsForm frm = new DsForm();
			frm.dataGrid1.DataSource = ds;
			frm.ShowDialog();
			frm = null;
		
		}

		private Guid GetPropertyGuid(OrigamControlIdentifier ident, string propertyName)
		{
			DataRow[] rows =_dsGUI.ControlProperty.Select("PropertyName='" + propertyName + 
				"' and ControlGuid='" + 
				ident.ControlGuid.ToString() + "'" );

			if( rows.Length != 1)
			{
				dsOrigamGui.ControlPropertyRow newRow = _dsGUI.ControlProperty.NewControlPropertyRow();
				newRow.ControlPropertyGuid = NewGuid();
				newRow.ControlGuid = ident.ControlGuid;
				newRow.PropertyName = propertyName;
				_dsGUI.ControlProperty.AddControlPropertyRow(newRow);
				return newRow.ControlPropertyGuid;
			}
			else
			{
				return ((dsOrigamGui.ControlPropertyRow)rows[0]).ControlPropertyGuid;
			}

		}
        
		private void SaveControlSet(OrigamControlIdentifier origamCntrl)
		{
			dsOrigamGui.ControlSetRow row = _dsGUI.ControlSet.Rows.Find(origamCntrl.ControlSetGuid) as dsOrigamGui.ControlSetRow;
			if(row!=null)
			{
				//row exists in dataset
				dsOrigamGui.ControlSetItemRow[] childRows = row.GetControlSetItemRows();
				foreach(dsOrigamGui.ControlSetItemRow childRow in childRows)
				{
					_dsGUI.ControlSetItem.Rows.Remove(childRow);
				}
				//Editing the row
				row.ControlSetGuid = origamCntrl.ControlSetGuid;
				row.ContainerName =  origamCntrl.ControlGuid.ToString();
			}
			else
			{	//creating new row
				row = _dsGUI.ControlSet.NewControlSetRow();
				row.ControlSetGuid = origamCntrl.ControlSetGuid;
				row.ContainerName =  origamCntrl.ControlGuid.ToString();
				_dsGUI.ControlSet.AddControlSetRow(row);
			}
		}

		private Guid GetOrigamControlType (Control control,OrigamControlIdentifier origamId)
		{
			Guid result = NewGuid();
			string where;
			dsOrigamGui.ControlRow row ;
			if(control is Form) 
			{
				where= "System.Windows.Forms.Form";
			}
			else
			{
				where = control.GetType().ToString();
			}
			                        
			DataRow[] rows=_dsGUI.Control.Select("ControlType='" + where + "'");
			if(rows.GetLength(0) > 0)
			{
				row=((dsOrigamGui.ControlRow)rows[0]);
				result = row.ControlGuid;
			}
			else
			{
				row= _dsGUI.Control.NewControlRow();
				row.ControlGuid = origamId.ControlGuid;
				row.ControlType = where;
				row.ControlName = "Test " + control.Name.ToString();
				row.IsComplexType = false;
				_dsGUI.Control.AddControlRow(row);
				result=row.ControlGuid;

				//TODO: dodelat saving properties

			}
			return 	result;		
		}


		private OrigamControlIdentifier GetOrigamControlIdentifier(Control control)
		{
			OrigamControlIdentifier origam;

			if (control.Tag is OrigamControlIdentifier)
			{
				origam=((OrigamControlIdentifier)control.Tag);
			}
			else
			{
				//				origam.ControlSetItemGuid = new Guid();
				//				System.Diagnostics.Debug.WriteLine("Control : " + control.Name + 
				//						"(Type: " + control.GetType().ToString() + ") doesn't contains OrigamControlIdentifier OrigamControlIdentifier in its tag property"); 
				throw new ArgumentException("Control " + control.Name + " doesn't contains OrigamControlIdentifier in its tag property");               
			}
			return origam;
		}

		private bool GoodType(string Name)
		{
			bool result;
			switch (Name)
			{
				case "System.Windows.Forms.NumericUpDown":
					result = false;
					break;

				default:
					result = true;
					break;
			}
			return result;
		}

		
		private System.Guid NewGuid()
		{
			return System.Guid.NewGuid();
		}


		//class end
	}

//namespace end
}

