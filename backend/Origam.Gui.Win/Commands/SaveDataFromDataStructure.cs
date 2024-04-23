#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Data;
using System.Windows.Forms;
using Origam.DA;
using Origam.Schema.EntityModel;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.Services;

namespace Origam.Gui.Win.Commands;

public class SaveDataFromDataStructure : AbstractMenuCommand
{
    WorkbenchSchemaService _schemaService = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
    SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
    IServiceAgent _dataServiceAgent;

    public override bool IsEnabled
    {
        get
        {
                return Owner is DataStructure 
                       ||  Owner is DataStructureMethod;
            }
        set
        {
                throw new ArgumentException("Cannot set this property", "IsEnabled");
            }
    }

    public override void Run()
    {
            DataStructure structure;
            DataStructureMethod method = null;

            if(Owner is DataStructureMethod)
            {
                structure = (Owner as DataStructureMethod).ParentItem as DataStructure;
                method = Owner as DataStructureMethod;
            }
            else
            {
                structure = Owner as DataStructure;
            }
			

            _dataServiceAgent = (ServiceManager.Services.GetService(
                typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);

            DataStructureQuery query = new DataStructureQuery(structure.Id);
            if(method != null)	
            {
                query.MethodId = method.Id;
            }

            SaveFileDialog dialog = null;

            try
            {
                dialog = new SaveFileDialog();

                dialog.AddExtension = true;
                dialog.DefaultExt = "xml";
                dialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*" ;
                dialog.FilterIndex = 1;

                dialog.Title = strings.SaveXmlResult_Title;

                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadData(query).WriteXml(dialog.FileName, XmlWriteMode.WriteSchema);
                } 
            }
            finally
            {
                if(dialog != null) dialog.Dispose();
            }
        }

    private DataSet LoadData(DataStructureQuery query)
    {
            _dataServiceAgent.MethodName = "LoadDataByQuery";
            _dataServiceAgent.Parameters.Clear();
            _dataServiceAgent.Parameters.Add("Query", query);

            _dataServiceAgent.Run();

            return _dataServiceAgent.Result as DataSet;
        }

    public override void Dispose()
    {
            _schemaService = null;
            _dataServiceAgent = null;
        }
    public override int GetImageIndex(string icon)
    {
            return _schemaBrowser.ImageIndex(icon);
        }
}