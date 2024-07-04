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
using System.Xml;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

using Origam;
using Origam.OrigamEngine;
using Origam.DA.Service;
using Origam.Schema.DeploymentModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using Origam.Gui.Win;
using Origam.UI;
using Origam.Rule;
using Origam.Workflow;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;
using Origam.Schema;
using Origam.Workbench;
using Origam.Workbench.Pads;
using Origam.Workflow.Gui.Win;
using Origam.DA.ObjectPersistence;
using Origam.Schema.RuleModel;
using core = Origam.Workbench.Services.CoreServices;
using System.IO;
using System.Linq;
using Origam.BI.CrystalReports;
using MoreLinq;
using Origam.Extensions;
using Origam.Gui;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.ItemCollection;
using Origam.Service.Core;

namespace OrigamArchitect.Commands;
/// <summary>
/// Edits a schema item in an editor. Schema item is passed as Owner.
/// </summary>
public class ExecuteSchemaItem : AbstractCommand
{
	private SchemaService _schemaService = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
	private IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
	private IParameterService _parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
	public readonly Hashtable Parameters = new Hashtable();
	public bool RecordEditingMode = false;
	public override void Run()
	{	
		IOrigamAuthorizationProvider authorizationProvider = SecurityManager.GetAuthorizationProvider();
		object node = this.Owner;
#if ORIGAM_CLIENT
		if(node is IAuthorizationContextContainer)
		{
			if(! authorizationProvider.Authorize(SecurityManager.CurrentPrincipal, (node as IAuthorizationContextContainer).AuthorizationContext))
			{
				MessageBox.Show(strings.InsufficientPrivileges_Message);
				return;
			}
		}
#endif
		if(node is FormControlSet)
		{
			this.RunItem(node as FormControlSet, (node as FormControlSet).Name, false);
		}
		else if(node is IWorkflow)
		{
			this.RunItem(node as IWorkflow, (node as IWorkflow).Name, false);
		}
		else if(node is AbstractReport)
		{
			this.RunItem(node as AbstractReport, (node as AbstractReport).Name, false);
		}
		else if(node is FormReferenceMenuItem)
		{
			this.RunItem(node as FormReferenceMenuItem, (node as FormReferenceMenuItem).DisplayName, false);
		}
		else if(node is WorkflowReferenceMenuItem)
		{
			this.RunItem(node as WorkflowReferenceMenuItem, (node as WorkflowReferenceMenuItem).DisplayName, (node as WorkflowReferenceMenuItem).IsRepeatable);
		}
		else if(node is ReportReferenceMenuItem)
		{
			this.RunItem(node as ReportReferenceMenuItem, (node as ReportReferenceMenuItem).DisplayName, false);
		}
		else if(node is AbstractUpdateScriptActivity)
		{
			this.RunItem(node as AbstractUpdateScriptActivity, null, false);
		}
		else if(node is DataConstantReferenceMenuItem)
		{
			this.RunItem(node as DataConstantReferenceMenuItem, (node as DataConstantReferenceMenuItem).DisplayName, false);
		}
		else if(node is WorkflowSchedule)
		{
			this.RunItem(node as WorkflowSchedule, (node as WorkflowSchedule).Name, false);
		}
		//			else
//			{
//				throw new ArgumentOutOfRangeException("node", node, "This type cannot be executed. Type not supported.");
//			}
	}
	private void RunItem(ISchemaItem item, string titleName, bool isRepeatable)
	{
		WorkflowPlayerPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(WorkflowPlayerPad)) as WorkflowPlayerPad;
        FormControlSet formControlSet = item as FormControlSet;
        FormReferenceMenuItem formReferenceMenuItem = item as FormReferenceMenuItem;
        AbstractReport abstractReport = item as AbstractReport;
		if(formControlSet != null || formReferenceMenuItem != null)
		{
			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			FormGenerator generator = new FormGenerator();
			AsForm frm = new AsForm(generator);
			if(formReferenceMenuItem != null)
			{
				// pass any parameters
				foreach(DictionaryEntry entry in this.Parameters)
				{
					generator.SelectionParameters.Add(entry.Key, entry.Value);
				}
				if(RecordEditingMode)
				{
					frm.SingleRecordEditing = true;
				}
				else
				{
					/*********************
					 *  SELECTION DIALOG *
					 *********************/
					if(formReferenceMenuItem.SelectionDialogPanel != null)
					{
						DataRow row = this.ShowSelectionDialog(
                            formReferenceMenuItem.SelectionDialogPanel,
                            formReferenceMenuItem.TransformationBeforeSelection, 
                            formReferenceMenuItem.TransformationAfterSelection, 
                            formReferenceMenuItem.SelectionDialogEndRule);
						if(row == null)
						{
							return;
						}
						else
						{
							foreach(SelectionDialogParameterMapping mapping in 
                                formReferenceMenuItem.ChildItemsByType(SelectionDialogParameterMapping.CategoryConst))
							{
								generator.SelectionParameters.Add(mapping.Name, row[mapping.SelectionDialogField.Name]);
							}
						}
					}
				}
			}
            var schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(IBrowserPad)) as IBrowserPad;
            if (schemaBrowser != null && item.NodeImage == null)
			{
                (frm as Form).Icon = System.Drawing.Icon.FromHandle((
                    (System.Drawing.Bitmap)schemaBrowser.ImageList.Images[schemaBrowser.ImageIndex(item.Icon)]).GetHicon());
			}
			else
			{
				(frm as Form).Icon = System.Drawing.Icon.FromHandle(item.NodeImage.ToBitmap().GetHicon());
			}
			frm.TitleName = titleName;
			WorkbenchSingleton.Workbench.ShowView(frm);
			Application.DoEvents();
			try
			{
				frm.LoadObject(item);
                (frm as Form).SelectNextControl(frm as Control, true, true, true, false);
            }
			catch(Exception ex)
			{
				AsMessageBox.ShowError(WorkbenchSingleton.Workbench as IWin32Window, ex.Message, strings.FormLoadingError_Message, ex);
				if(frm != null) frm.Close();
			}
            finally
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
		}
		else if(item is IWorkflow || item is WorkflowSchedule 
            || item is WorkflowReferenceMenuItem)
		{
            ProcessWorkflow(item, titleName, isRepeatable);
		}
		else if(item is CrystalReport)
		{
            ProcessCrystalReport(item, titleName);
		}
		else if(item is WebReport)
		{
			OpenBrowser((item as WebReport).Url);
		}
        else if (abstractReport != null)
        {
            // all other reports we save
            SaveFileDialog sfd = new SaveFileDialog();
            if(sfd.ShowDialog() == DialogResult.OK)
            {
                byte[] report = core.ReportService.GetReport(abstractReport.Id,
                    null, DataReportExportFormatType.PDF.ToString(), this.Parameters, null);
                File.WriteAllBytes(sfd.FileName, report);
            }
        }
		else if (item is ReportReferenceMenuItem)
		{
			ReportReferenceMenuItem reportRef = item as ReportReferenceMenuItem;
			Hashtable parameters = new Hashtable();
			/*********************
			*  SELECTION DIALOG *
			*********************/
			if(reportRef.SelectionDialogPanel != null)
			{
				DataRow row = this.ShowSelectionDialog(reportRef.SelectionDialogPanel, reportRef.TransformationBeforeSelection, reportRef.TransformationAfterSelection, reportRef.SelectionDialogEndRule);
				if(row == null)
				{
					return;
				}
				else
				{
					foreach(SelectionDialogParameterMapping mapping in reportRef.ChildItemsByType(SelectionDialogParameterMapping.CategoryConst))
					{
						parameters.Add(mapping.Name, row[mapping.SelectionDialogField.Name]);
					}
				}
			}
			CrystalReport crReport = reportRef.Report as CrystalReport;
			WebReport webReport = reportRef.Report as WebReport;
			if(crReport != null)
			{
				ReportViewer viewer = new ReportViewer(crReport, titleName, parameters);
				viewer.LoadReport();
				WorkbenchSingleton.Workbench.ShowView(viewer);
			}
			else if(webReport != null)
			{
				Origam.BI.ReportHelper.PopulateDefaultValues(reportRef.Report, parameters);
                Origam.BI.ReportHelper.ComputeXsltValueParameters(reportRef.Report, parameters);
                OpenBrowser(HttpTools.Instance.BuildUrl(webReport.Url, parameters, webReport.ForceExternalUrl, webReport.ExternalUrlScheme, webReport.IsUrlEscaped));
			}
            else
            {
                // all other reports
            }
		}		 
		else if(item is AbstractUpdateScriptActivity)
		{
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            ServiceCommandUpdateScriptActivity scriptActivity = (ServiceCommandUpdateScriptActivity)item;
            Platform[] platforms = settings.GetAllPlatform();
            if(!platforms
                .Where(platform => platform.GetParseEnum(platform.DataService) == scriptActivity.DatabaseType.ToString()).Any())
            {
                MessageBox.Show("Platform is not supported for execute this script!","Platform",MessageBoxButtons.OK);
                return;
            }
            ProcessUpdateScript(item);
		}
		else if(item is DataConstantReferenceMenuItem)
		{
            ProcessConstant(item as DataConstantReferenceMenuItem, titleName);
		}
	}
    private void ProcessConstant(DataConstantReferenceMenuItem item, string titleName)
    {
        _parameterService.RefreshParameters();
        ParameterEditor editor = new ParameterEditor();
        DataConstant constant = item.Constant;
        // load non-cached element
        DataConstant copy = _persistence.SchemaProvider.RetrieveInstance(
            typeof(DataConstant), constant.PrimaryKey, false) as DataConstant;
        // Temporarily overwrite the original lookup by the menu item's own
        // lookup. The constant will be thrown away anyway
        if (item.FinalLookup != null)
        {
            copy.DataLookup = item.FinalLookup;
        }
        // load constant into the editor
        editor.TitleName = titleName;
        editor.LoadObject(copy);
        // show the editor
        WorkbenchSingleton.Workbench.ShowView(editor);
    }
    private static void ProcessUpdateScript(ISchemaItem item)
    {
        IDeploymentService deployment = ServiceManager.Services.GetService(typeof(IDeploymentService)) as IDeploymentService;
        deployment.ExecuteActivity(item.PrimaryKey);
    }
    private void ProcessWorkflow(ISchemaItem item, string titleName, 
        bool isRepeatable)
    {
        Origam.Workbench.Commands.ViewLogPad logPad =
            new Origam.Workbench.Commands.ViewLogPad();
        logPad.Run();
        IWorkflow wf;
        Hashtable parameters = new Hashtable();
        WorkflowReferenceMenuItem wfRef = item as WorkflowReferenceMenuItem;
        if (wfRef != null)
        {
            wf = wfRef.Workflow;
            // set parameters
            RuleEngine ruleEngine = RuleEngine.Create(null, null);
            foreach (AbstractSchemaItem parameter in item.ChildItems)
            {
                if (parameter != null)
                {
                    parameters.Add(parameter.Name, ruleEngine.Evaluate(parameter));
                }
            }
        }
        else if (item is IWorkflow workflow)
        {
            wf = workflow;
            parameters = Parameters;
        }
        else
        {
            WorkflowSchedule schedule = item as WorkflowSchedule;
            wf = schedule.Workflow;
            // set parameters
            RuleEngine ruleEngine = RuleEngine.Create(null, null);
            foreach (AbstractSchemaItem parameter in schedule.ChildItems)
            {
                if (parameter != null)
                {
                    parameters.Add(parameter.Name, ruleEngine.Evaluate(parameter));
                }
            }
        }
        System.Drawing.Icon icon;
        var schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(IBrowserPad)) as IBrowserPad;
        if (schemaBrowser != null && item.NodeImage == null)
        {
            icon = System.Drawing.Icon.FromHandle((
                (System.Drawing.Bitmap)schemaBrowser.ImageList.Images[schemaBrowser.ImageIndex(item.Icon)]).GetHicon());
        }
        else
        {
            icon = System.Drawing.Icon.FromHandle(item.NodeImage.ToBitmap().GetHicon());
        }
        WorkflowHost host = WorkflowHost.DefaultHost;
        // Initialize view for this workflow
        WorkflowForm form = Origam.Workflow.Gui.Win.WorkflowHelper.CreateWorkflowForm(host, icon, titleName, (Guid)wf.PrimaryKey["Id"]);
        WorkflowEngine workflowEngine = WorkflowEngine.PrepareWorkflow(wf, parameters, isRepeatable, titleName);
        form.WorkflowEngine = workflowEngine;
        host.ExecuteWorkflow(workflowEngine);
    }
    private void ProcessCrystalReport(ISchemaItem item, string titleName)
    {
        ReportViewer viewer = new ReportViewer((item as CrystalReport), titleName);
        foreach (DictionaryEntry param in Parameters)
        {
            viewer.Parameters.Add(param.Key, param.Value);
        }
        viewer.LoadReport();
        WorkbenchSingleton.Workbench.ShowView(viewer);
    }
	private DataRow ShowSelectionDialog(PanelControlSet selectionDialogPanel, ITransformation transformationBeforeSelection, ITransformation transformationAfterSelection, IEndRule endRule)
	{
		DatasetGenerator gen = new DatasetGenerator(true);
		FormGenerator sdGenerator = null;
		AsForm sd = null;
					
		try
		{
            UserProfile profile = SecurityManager.CurrentUserProfile();
			IDataDocument dataDoc;
			if(transformationBeforeSelection == null)
			{
				dataDoc = DataDocumentFactory.New(FormTools.GetSelectionDialogData(selectionDialogPanel.DataSourceId, Guid.Empty, false, profile.Id));
			}
			else
			{
				dataDoc = DataDocumentFactory.New(FormTools.GetSelectionDialogData(selectionDialogPanel.DataSourceId, (Guid)transformationBeforeSelection.PrimaryKey["Id"], false, profile.Id));
			}
			sdGenerator = new FormGenerator();
			sd = sdGenerator.LoadSelectionDialog(dataDoc, selectionDialogPanel, endRule) as AsForm;
			
			sd.AutoAddNewEntity = dataDoc.DataSet.Tables[0].TableName;
			sd.SaveOnClose = false;
			DialogResult result = sd.ShowDialog(WorkbenchSingleton.Workbench as IWin32Window);
			if(result == DialogResult.Cancel) return null;
			DataRow row;
			if(transformationAfterSelection == null)
			{
				row = FormTools.GetSelectionDialogResultRow(selectionDialogPanel.DataSourceId, Guid.Empty, dataDoc, profile.Id);
			}
			else
			{
				row = FormTools.GetSelectionDialogResultRow(selectionDialogPanel.DataSourceId, (Guid)transformationAfterSelection.PrimaryKey["Id"], dataDoc, profile.Id);
			}
			return row;
		}
		finally
		{
			if(sd != null) sd.Dispose();
			sd = null;
			sdGenerator = null;
		}
	}
	public void OpenBrowser(string sURL)
	{
		string sBrws = null;
		try 
		{
			//Return the path used to access http from the registry
			sBrws = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("http\\shell\\open\\command", false).GetValue(null).ToString().ToLower();
			//Trim the file path (removing everything after the ".exe") and get rid of quotation marks
			sBrws = sBrws.Substring(1, sBrws.IndexOf(".exe") + 3);
			sBrws = sBrws.Replace("\"", "");
			//Open the URL using the path
			System.Diagnostics.Process.Start(sBrws, sURL);
		} 
		catch 
		{
			//Add redundant attempts to open the URL, in case the code fails 
			try 
			{
				//First just try using Process.Start, which will open the default browser, but might reuse IE's window
				System.Diagnostics.Process.Start(sURL);
			} 
			catch 
			{
				try 
				{
					//If Process.Start fails (due to known bug), try launching IE directly and passing it the URL
					System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("IExplore.exe", sURL);
					System.Diagnostics.Process.Start(psi);
					psi = null;
				} 
				catch (Exception ex) 
				{
					throw new Exception("Could not launch the browser. Details: " + ex.Message);
				}
			}
		}
	}
}
/// <summary>
/// Shows the About screen
/// </summary>
public class ViewAboutScreen : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return true;
		}
		set
		{
			base.IsEnabled = value;
		}
	}
	public override void Run()
	{
		using(SplashScreen splash = new SplashScreen())
		{
			splash.ShowOkButton = true;
			splash.ShowDialog();
		}
	}		
}
/// <summary>
/// Shows the Schema Output pad
/// </summary>
public class ViewProcessBrowserPad : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return true;
		}
		set
		{
			base.IsEnabled = value;
		}
	}
	public override void Run()
	{
		WorkflowPlayerPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(WorkflowPlayerPad)) as WorkflowPlayerPad;
		if(pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
	}		
}

public class ViewWorkQueuePad : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return true;
		}
		set
		{
			base.IsEnabled = value;
		}
	}
	public override void Run()
	{
		WorkQueuePad pad = WorkbenchSingleton.Workbench.GetPad(typeof(WorkQueuePad)) as WorkQueuePad;
		if(pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
	}		
}
/// <summary>
/// Executes an active schema item.
/// </summary>
public class ExecuteActiveSchemaItem : AbstractMenuCommand
{
	WorkbenchSchemaService _schemaService = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
	public override bool IsEnabled
	{
		get
		{
			return (_schemaService.ActiveNode is FormControlSet
				| _schemaService.ActiveNode is IWorkflow
				| _schemaService.ActiveNode is AbstractReport
				| _schemaService.ActiveNode is ReportReferenceMenuItem
				| _schemaService.ActiveNode is FormReferenceMenuItem
				| _schemaService.ActiveNode is DataConstantReferenceMenuItem
				| _schemaService.ActiveNode is WorkflowReferenceMenuItem
				| _schemaService.ActiveNode is AbstractUpdateScriptActivity
				| _schemaService.ActiveNode is WorkflowSchedule
				);
		}
		set
		{
			throw new ArgumentException("Cannot set this property", "IsEnabled");
		}
	}
	public override void Run()
	{
		ExecuteSchemaItem cmd = new ExecuteSchemaItem();
		cmd.Owner = _schemaService.ActiveSchemaItem;
		cmd.Run();
	}
	public override void Dispose()
	{
		_schemaService = null;
	}
}
public class FinishWorkflowTask : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return WorkbenchSingleton.Workbench.ActiveDocument != null 
				&& WorkbenchSingleton.Workbench.ActiveDocument is WorkflowForm
				&& (WorkbenchSingleton.Workbench.ActiveDocument as WorkflowForm).CanFinishTask;
		}
		set
		{
			throw new ArgumentException("Cannot set this property", "IsEnabled");
		}
	}
	public override void Run()
	{
		(WorkbenchSingleton.Workbench.ActiveDocument as WorkflowForm).FinishTask();
	}
}
public class DumpWindowXml : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return WorkbenchSingleton.Workbench.ActiveDocument != null 
				&& WorkbenchSingleton.Workbench.ActiveDocument is AsForm;
		}
		set
		{
			throw new ArgumentException("Cannot set this property", "IsEnabled");
		}
	}
	public override void Run()
	{
        Origam.Workbench.Commands.ViewOutputPad outputPad =
            new Origam.Workbench.Commands.ViewOutputPad();
        outputPad.Run();
        OutputPad p = WorkbenchSingleton.Workbench.GetPad(typeof(OutputPad)) as OutputPad;
		AsForm activeForm = WorkbenchSingleton.Workbench.ActiveDocument as AsForm;
		p.SetOutputText(activeForm.FormGenerator.DataSet.GetXml());
	}
}
public class ShowEditorXml : AbstractMenuCommand
{
	WorkbenchSchemaService _schemaService = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
	public override bool IsEnabled
	{
		get
		{
			return _schemaService.ActiveNode is AbstractSchemaItem;
		}
		set
		{
			throw new ArgumentException("Cannot set this property", "IsEnabled");
		}
	}
	public override void Run()
	{
		OutputPad p = WorkbenchSingleton.Workbench.GetPad(typeof(OutputPad)) as OutputPad;
		AbstractSchemaItem item = _schemaService.ActiveNode as AbstractSchemaItem;
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		System.IO.StringWriter sw = new System.IO.StringWriter(sb);
		XmlTextWriter xw = new XmlTextWriter(sw);
		xw.Formatting = Formatting.Indented;
		Origam.OrigamEngine.ModelXmlBuilders.PropertyGridBuilder.Build(item).WriteTo(xw);
		p.SetOutputText(sb.ToString());
	}
}
/// <summary>
/// Displays DbCompare info
/// </summary>
public class ShowDbCompare : AbstractMenuCommand
{
	SchemaService _schemaService = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
	public override bool IsEnabled
	{
		get
		{
			return _schemaService.IsSchemaLoaded;
		}
		set
		{
			throw new ArgumentException("Cannot set this property", "IsEnabled");
		}
	}
	public override void Run()
	{
		SchemaCompareEditor editor = new SchemaCompareEditor();
		WorkbenchSingleton.Workbench.ShowView(editor);
	}
	public override void Dispose()
	{
		_schemaService = null;
	}
}
/// <summary>
/// Runs Explorer with Debug path displayed
/// </summary>
public class ShowTrace : AbstractMenuCommand
{
	IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
	SchemaService _schemaService = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
	public override bool IsEnabled
	{
		get
		{
			return _schemaService.IsSchemaLoaded;
		}
		set
		{
			throw new ArgumentException("Cannot set this property", "IsEnabled");
		}
	}
	public override void Run()
	{
		FormReferenceMenuItem formMenu = new FormReferenceMenuItem();
		formMenu.PersistenceProvider = _persistence.SchemaProvider;
		formMenu.DisplayName = strings.WorkFlowTrace_MenuItem;
		formMenu.ScreenId 
            = new Guid("75C3B61C-77BD-41AB-AE8F-39361500CEC4");
		formMenu.ListDataStructureId 
            = new Guid("309843cc-39ec-4eca-8848-8c69c885790c");
		formMenu.ListEntityId 
            = new Guid("3b041438-2f59-4c01-976b-a2f9e89038fe");
		formMenu.MethodId 
            = new Guid("77cdbb0c-430e-4552-a0c2-f494cfb0c782");
        formMenu.ListSortSet = _persistence.SchemaProvider.RetrieveInstance(
            typeof(AbstractSchemaItem), new ModelElementKey(
                new Guid("d20be524-2178-42c0-943d-7d84ee6bb53b")), 
            true, false) as DataStructureSortSet;
        formMenu.SortSet = _persistence.SchemaProvider.RetrieveInstance(
            typeof(AbstractSchemaItem), new ModelElementKey(
                new Guid("f68c419b-6130-4665-bee1-d81f2769c5ad")), 
            true, false) as DataStructureSortSet;
		formMenu.Roles = "*";
		ExecuteSchemaItem cmd = new ExecuteSchemaItem();
		cmd.Owner = formMenu;
		cmd.Run();
	}
	public override void Dispose()
	{
		_persistence = null;
		_schemaService = null;
	}
}

public class ShowRuleTrace : AbstractMenuCommand
{
	IPersistenceService _persistence = ServiceManager.Services.GetService<IPersistenceService>();
	SchemaService _schemaService = ServiceManager.Services.GetService<SchemaService>();
	public override bool IsEnabled
	{
		get => _schemaService.IsSchemaLoaded;
		set => throw new ArgumentException("Cannot set this property", "IsEnabled");
	}
	public override void Run()
	{
		FormReferenceMenuItem formMenu = new FormReferenceMenuItem();
		formMenu.PersistenceProvider = _persistence.SchemaProvider;
		formMenu.DisplayName = strings.RuleTrace_MenuItem;
		formMenu.ScreenId 
            = new Guid("57dc7edd-7b9c-43f2-b94a-54ddd2d98206");
		formMenu.Roles = "*";
		formMenu.SortSet = _persistence.SchemaProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), new ModelElementKey(
				new Guid("6b22f4c9-bc05-4e52-88f9-486e64dc7b1b")), 
			true, false) as DataStructureSortSet;
		ExecuteSchemaItem cmd = new ExecuteSchemaItem();
		cmd.Owner = formMenu;
		cmd.Run();
	}
	public override void Dispose()
	{
		_persistence = null;
		_schemaService = null;
	}
}
/// <summary>
/// Runs Explorer with Debug path displayed
/// </summary>
public class MakeWorkflowRecurring : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			WorkflowPlayerPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(WorkflowPlayerPad)) as WorkflowPlayerPad;
			
			if(pad.ebrSchemaBrowser.ActiveNode is WorkflowReferenceMenuItem)
			{
				(this.Owner as AsMenuCommand).Checked = (pad.ebrSchemaBrowser.ActiveNode as WorkflowReferenceMenuItem).IsRepeatable;
			}
			else
			{
				(this.Owner as AsMenuCommand).Checked = false;
			}
			return pad.ebrSchemaBrowser.ActiveNode is WorkflowReferenceMenuItem;
		}
		set
		{
			throw new ArgumentException("Cannot set this property", "IsEnabled");
		}
	}
	public override void Run()
	{
		(this.Owner as AsMenuCommand).Checked = !(this.Owner as AsMenuCommand).Checked;
		WorkflowPlayerPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(WorkflowPlayerPad)) as WorkflowPlayerPad;
		(pad.ebrSchemaBrowser.ActiveNode as WorkflowReferenceMenuItem).IsRepeatable = (this.Owner as AsMenuCommand).Checked;
		pad.ebrSchemaBrowser.RefreshItem(pad.ebrSchemaBrowser.ActiveNode);
	}
}

/// <summary>
/// Generates a localization file from the model.
/// </summary>
public class GenerateLocalizationFile : AbstractMenuCommand
{
	SchemaService _schemaService = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
	public override bool IsEnabled
	{
		get
		{
			return _schemaService.IsSchemaLoaded;
		}
		set
		{
			throw new ArgumentException("Cannot set this property", "IsEnabled");
		}
	}
	public override void Run()
	{
		OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
		string[] languages = settings.TranslationBuilderLanguages.Split(",".ToCharArray());
		SchemaService ss = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
		string packageName = ss.ActiveExtension.Name;
		foreach(string language in languages)
		{
			string fileName = packageName + "-" + language.Trim() + ".xml";
			string outputPath = Path.Combine(Path.Combine(settings.ModelSourceControlLocation, @"..\l10n"), fileName);
			if (!string.IsNullOrEmpty(settings.LocalizationFolder))
			{
				outputPath = Path.Combine(settings.LocalizationFolder, fileName);
			}
			LocalizationCache currentTranslations = null;
			if(File.Exists(outputPath))
			{
				currentTranslations = new LocalizationCache(outputPath);
			}
			MemoryStream ms = new MemoryStream();
            FileStream fs = null;
			try
			{
				TranslationBuilder.Build(ms, currentTranslations, language, ss.ActiveSchemaExtensionId);
				fs = new FileStream(outputPath, FileMode.Create);
				ms.Seek(0, SeekOrigin.Begin);
				ms.WriteTo(fs);
			}
			finally
			{
				if(fs != null)	fs.Close();
				if (ms != null) ms.Close();
			}
		}
	}		
}
/// <summary>
/// Generates GUID and copies it to the Clipboard
/// </summary>
public class GenerateGuid : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return true;
		}
		set
		{
			base.IsEnabled = value;
		}
	}
	public override void Run()
	{
		Clipboard.SetDataObject(Guid.NewGuid().ToString());
	}		
}
/// <summary>
/// Resets user caches
/// </summary>
public class ResetUserCaches : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return true;
		}
		set
		{
			base.IsEnabled = value;
		}
	}
	public override void Run()
	{
		OrigamUserContext.Reset();
	}		
}
/// <summary>
/// Shows the Workflow Watch pad
/// </summary>
public class ViewWorkflowWatchPad : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return true;
		}
		set
		{
			base.IsEnabled = value;
		}
	}
	public override void Run()
	{
		WorkflowWatchPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(WorkflowWatchPad)) as WorkflowWatchPad;
		if(pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
	}		
}
public class SetServerRestart : AbstractMenuCommand
{
	private SchemaService _schemaService = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
	public override bool IsEnabled
	{
		get
		{
			return _schemaService.IsSchemaLoaded;
		}
		set
		{
			base.IsEnabled = value;
		}
	}
	public override void Run()
	{
		OrigamEngine.SetRestart();
	}		
}
public class ShowHelp : AbstractMenuCommand
{
    public override void Run()
    {
        string target = "https://origam.com/doc/display/architect/";
        if (WorkbenchSingleton.Workbench.ActiveDocument != null)
        {
            target += WorkbenchSingleton.Workbench.ActiveDocument.HelpTopic;
        }            
        System.Diagnostics.Process.Start(target);
    }
}
public class ShowCommunity : AbstractMenuCommand
{
    public override void Run()
    {
        string target = "http://community.origam.com";
        System.Diagnostics.Process.Start(target);
    }
}
public class ShowWebApplication : AbstractMenuCommand
{
    public override bool IsEnabled
    {
        get
        {
            return ConfigurationManager.GetActiveConfiguration() != null;
        }
        set
        {
            base.IsEnabled = value;
        }
    }
    public override void Run()
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
        string target = settings.ServerUrl;
        if (string.IsNullOrEmpty(target))
        {
            MessageBox.Show(WorkbenchSingleton.Workbench as IWin32Window,
                strings.ServerUrlNotSet_Message, strings.ServerUrlNotSet_Title, 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            System.Diagnostics.Process.Start(target);
        }
    }
}
public class CreateNewProject : AbstractMenuCommand
{
    public override void Run()
    {
        NewProjectWizard wiz = new NewProjectWizard();
        wiz.ShowDialog((IWin32Window)WorkbenchSingleton.Workbench);
    }
}
public abstract class StorageConvertor : AbstractMenuCommand
{
	private static readonly log4net.ILog log = 
		log4net.LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	protected IPersistenceService CurrentPersistenceService =>
				ServiceManager.Services.GetService<IPersistenceService>();
	
	protected IDocumentationService CurrentDocumentationService =>
		ServiceManager.Services.GetService<IDocumentationService>();
	
	protected readonly SchemaService schemaService =
		ServiceManager.Services.GetService<SchemaService>();
	protected IPersistenceService newPersistenceService;
	protected IStatusBarService statusBar =
		ServiceManager.Services.GetService<IStatusBarService>();
	private static IEnumerable<Type> AllProviderTypes =>
		ServiceManager.Services
			.GetService<SchemaService>()
			.Providers
			.Select(provider => provider.GetType());
	private ISchemaItemCollection GetAllItems(Type providerType) => 
		schemaService.GetProvider(providerType).ChildItems;
	private void UpdateStatusBar(Type type, int typeNumber)
	{
		int providerCount = AllProviderTypes.Count();
		statusBar.SetStatusText(string.Format("Converting {0}. {1}/{2}",
			type.Name, typeNumber, providerCount));
		Application.DoEvents();
	}	
	
	protected void PersistAllData()
	{
	    newPersistenceService.SchemaProvider.BeginTransaction();
        PersistFolders<Package>();
		PersistFolders<SchemaItemGroup>();
		PersistFolders<PackageReference>();
		int typeNumber = 1;
		foreach (Type type in AllProviderTypes)
		{
			UpdateStatusBar(type, typeNumber);
			PersistAllProviderItems(type);
			typeNumber++;
		}
	    newPersistenceService.SchemaProvider.EndTransaction();
    }
	private void PersistFolders<T>()
	{
		statusBar.SetStatusText("Converting "+typeof(T));
		IPersistenceService dbSvc = ServiceManager.Services.GetService(
			typeof(IPersistenceService)) as IPersistenceService;
		var listOfItems = dbSvc.SchemaProvider.RetrieveList<T>(null);
   
		foreach (IPersistent item in listOfItems)
		{
			newPersistenceService.SchemaProvider.Persist(item);
		}
	}
	private void PersistAllProviderItems(Type providerType)
	{
		ISchemaItemCollection allItems = GetAllItems(providerType); 
		if(log.IsDebugEnabled)
		{
			log.Debug($"ProviderType:{providerType}, items: {allItems.Count}");
		}
		foreach (AbstractSchemaItem item in allItems)
		{
			Persist(item);
		}
	}
	private void Persist( AbstractSchemaItem item)
	{
		newPersistenceService.SchemaProvider.Persist(item);
		foreach (var ancestor in item.Ancestors)
		{
			newPersistenceService.SchemaProvider.Persist(ancestor);
		}
		foreach (var child in item.ChildItems)
		{
			if (child.DerivedFrom == null && child.IsPersistable)
			{
				Persist(child);
			}
		}
	}
}

public class SchemaExtensionSorter : IDatasetFormatter
{
	public DataSet Format(DataSet unsortedData)
	{
		DataSet data = unsortedData.Clone();
		data.EnforceConstraints = false;
		foreach (DataTable originalTable in unsortedData.Tables)
		{
			DataTable newTable = data.Tables[originalTable.TableName];
			switch (originalTable.TableName)
			{
				case "SchemaItemGroup":
				case "SchemaExtension":
				case "PackageReference":
					originalTable.Rows
						.Cast<DataRow>()
						.OrderBy(row => row["Id"])
						.ForEach(row =>
						{
							newTable.ImportRow(row);
							ImportChildRows(originalTable, newTable, row);
						});
					break;
				default:
					foreach (DataRow row in originalTable.Rows)
					{
						newTable.ImportRow(row);
					}
					break;
			}
		}
		return data;
	}
	private void ImportChildRows(DataTable originalTable, DataTable newTable, DataRow parentRow)
	{
		foreach(DataRow row in parentRow.GetChildRows("ChildSchemaItem"))
		{
			newTable.ImportRow(row);
			ImportChildRows(originalTable, newTable, row);
		}
	}
}
