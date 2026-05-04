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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using MoreLinq;
using Origam;
using Origam.BI.CrystalReports;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Gui;
using Origam.Gui.Win;
using Origam.OrigamEngine;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.GuiModel;
using Origam.Schema.ItemCollection;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.Pads;
using Origam.Workbench.Services;
using Origam.Workflow;
using Origam.Workflow.Gui.Win;
using core = Origam.Workbench.Services.CoreServices;

namespace OrigamArchitect.Commands;

/// <summary>
/// Edits a schema item in an editor. Schema item is passed as Owner.
/// </summary>
public class ExecuteSchemaItem : AbstractCommand
{
    private SchemaService _schemaService =
        ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
    private IPersistenceService _persistence =
        ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
        as IPersistenceService;
    private IParameterService _parameterService =
        ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
        as IParameterService;
    public readonly Hashtable Parameters = new Hashtable();
    public bool RecordEditingMode = false;

    public override void Run()
    {
        IOrigamAuthorizationProvider authorizationProvider =
            SecurityManager.GetAuthorizationProvider();
        object node = this.Owner;
#if ORIGAM_CLIENT
        if (node is IAuthorizationContextContainer)
        {
            if (
                !authorizationProvider.Authorize(
                    principal: SecurityManager.CurrentPrincipal,
                    context: (node as IAuthorizationContextContainer).AuthorizationContext
                )
            )
            {
                MessageBox.Show(text: strings.InsufficientPrivileges_Message);
                return;
            }
        }
#endif
        if (node is FormControlSet)
        {
            this.RunItem(
                item: node as FormControlSet,
                titleName: (node as FormControlSet).Name,
                isRepeatable: false
            );
        }
        else if (node is IWorkflow)
        {
            this.RunItem(
                item: node as IWorkflow,
                titleName: (node as IWorkflow).Name,
                isRepeatable: false
            );
        }
        else if (node is AbstractReport)
        {
            this.RunItem(
                item: node as AbstractReport,
                titleName: (node as AbstractReport).Name,
                isRepeatable: false
            );
        }
        else if (node is FormReferenceMenuItem)
        {
            this.RunItem(
                item: node as FormReferenceMenuItem,
                titleName: (node as FormReferenceMenuItem).DisplayName,
                isRepeatable: false
            );
        }
        else if (node is WorkflowReferenceMenuItem)
        {
            this.RunItem(
                item: node as WorkflowReferenceMenuItem,
                titleName: (node as WorkflowReferenceMenuItem).DisplayName,
                isRepeatable: (node as WorkflowReferenceMenuItem).IsRepeatable
            );
        }
        else if (node is ReportReferenceMenuItem)
        {
            this.RunItem(
                item: node as ReportReferenceMenuItem,
                titleName: (node as ReportReferenceMenuItem).DisplayName,
                isRepeatable: false
            );
        }
        else if (node is AbstractUpdateScriptActivity)
        {
            this.RunItem(
                item: node as AbstractUpdateScriptActivity,
                titleName: null,
                isRepeatable: false
            );
        }
        else if (node is DataConstantReferenceMenuItem)
        {
            this.RunItem(
                item: node as DataConstantReferenceMenuItem,
                titleName: (node as DataConstantReferenceMenuItem).DisplayName,
                isRepeatable: false
            );
        }
        else if (node is WorkflowSchedule)
        {
            this.RunItem(
                item: node as WorkflowSchedule,
                titleName: (node as WorkflowSchedule).Name,
                isRepeatable: false
            );
        }
        //			else
        //			{
        //				throw new ArgumentOutOfRangeException("node", node, "This type cannot be executed. Type not supported.");
        //			}
    }

    private void RunItem(ISchemaItem item, string titleName, bool isRepeatable)
    {
        WorkflowPlayerPad pad =
            WorkbenchSingleton.Workbench.GetPad(type: typeof(WorkflowPlayerPad))
            as WorkflowPlayerPad;
        FormControlSet formControlSet = item as FormControlSet;
        FormReferenceMenuItem formReferenceMenuItem = item as FormReferenceMenuItem;
        AbstractReport abstractReport = item as AbstractReport;
        if (formControlSet != null || formReferenceMenuItem != null)
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            FormGenerator generator = new FormGenerator();
            AsForm frm = new AsForm(generator: generator);
            if (formReferenceMenuItem != null)
            {
                // pass any parameters
                foreach (DictionaryEntry entry in this.Parameters)
                {
                    generator.SelectionParameters.Add(key: entry.Key, value: entry.Value);
                }
                if (RecordEditingMode)
                {
                    frm.SingleRecordEditing = true;
                }
                else
                {
                    /*********************
                     *  SELECTION DIALOG *
                     *********************/
                    if (formReferenceMenuItem.SelectionDialogPanel != null)
                    {
                        DataRow row = this.ShowSelectionDialog(
                            selectionDialogPanel: formReferenceMenuItem.SelectionDialogPanel,
                            transformationBeforeSelection: formReferenceMenuItem.TransformationBeforeSelection,
                            transformationAfterSelection: formReferenceMenuItem.TransformationAfterSelection,
                            endRule: formReferenceMenuItem.SelectionDialogEndRule
                        );
                        if (row == null)
                        {
                            return;
                        }

                        foreach (
                            var mapping in formReferenceMenuItem.ChildItemsByType<SelectionDialogParameterMapping>(
                                itemType: SelectionDialogParameterMapping.CategoryConst
                            )
                        )
                        {
                            generator.SelectionParameters.Add(
                                key: mapping.Name,
                                value: row[columnName: mapping.SelectionDialogField.Name]
                            );
                        }
                    }
                }
            }
            var schemaBrowser =
                WorkbenchSingleton.Workbench.GetPad(type: typeof(IBrowserPad)) as IBrowserPad;
            if (schemaBrowser != null && item.NodeImage == null)
            {
                (frm as Form).Icon = System.Drawing.Icon.FromHandle(
                    handle: (
                        (System.Drawing.Bitmap)
                            schemaBrowser.ImageList.Images[
                                index: schemaBrowser.ImageIndex(icon: item.Icon)
                            ]
                    ).GetHicon()
                );
            }
            else
            {
                (frm as Form).Icon = System.Drawing.Icon.FromHandle(
                    handle: item.NodeImage.ToBitmap().GetHicon()
                );
            }
            frm.TitleName = titleName;
            WorkbenchSingleton.Workbench.ShowView(content: frm);
            Application.DoEvents();
            try
            {
                frm.LoadObject(objectToLoad: item);
                (frm as Form).SelectNextControl(
                    ctl: frm as Control,
                    forward: true,
                    tabStopOnly: true,
                    nested: true,
                    wrap: false
                );
            }
            catch (Exception ex)
            {
                AsMessageBox.ShowError(
                    owner: WorkbenchSingleton.Workbench as IWin32Window,
                    text: ex.Message,
                    caption: strings.FormLoadingError_Message,
                    exception: ex
                );
                if (frm != null)
                {
                    frm.Close();
                }
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
        }
        else if (item is IWorkflow || item is WorkflowSchedule || item is WorkflowReferenceMenuItem)
        {
            ProcessWorkflow(item: item, titleName: titleName, isRepeatable: isRepeatable);
        }
        else if (item is CrystalReport)
        {
            ProcessCrystalReport(item: item, titleName: titleName);
        }
        else if (item is WebReport)
        {
            OpenBrowser(sURL: (item as WebReport).Url);
        }
        else if (abstractReport != null)
        {
            // all other reports we save
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                byte[] report = core.ReportService.GetReport(
                    reportId: abstractReport.Id,
                    data: null,
                    format: DataReportExportFormatType.PDF.ToString(),
                    parameters: this.Parameters,
                    transactionId: null
                );
                File.WriteAllBytes(path: sfd.FileName, bytes: report);
            }
        }
        else if (item is ReportReferenceMenuItem)
        {
            ReportReferenceMenuItem reportRef = item as ReportReferenceMenuItem;
            Hashtable parameters = new Hashtable();
            /*********************
            *  SELECTION DIALOG *
            *********************/
            if (reportRef.SelectionDialogPanel != null)
            {
                DataRow row = this.ShowSelectionDialog(
                    selectionDialogPanel: reportRef.SelectionDialogPanel,
                    transformationBeforeSelection: reportRef.TransformationBeforeSelection,
                    transformationAfterSelection: reportRef.TransformationAfterSelection,
                    endRule: reportRef.SelectionDialogEndRule
                );
                if (row == null)
                {
                    return;
                }

                foreach (
                    var mapping in reportRef.ChildItemsByType<SelectionDialogParameterMapping>(
                        itemType: SelectionDialogParameterMapping.CategoryConst
                    )
                )
                {
                    parameters.Add(
                        key: mapping.Name,
                        value: row[columnName: mapping.SelectionDialogField.Name]
                    );
                }
            }
            CrystalReport crReport = reportRef.Report as CrystalReport;
            WebReport webReport = reportRef.Report as WebReport;
            if (crReport != null)
            {
                ReportViewer viewer = new ReportViewer(
                    reportElement: crReport,
                    titleName: titleName,
                    parameters: parameters
                );
                viewer.LoadReport();
                WorkbenchSingleton.Workbench.ShowView(content: viewer);
            }
            else if (webReport != null)
            {
                Origam.BI.ReportHelper.PopulateDefaultValues(
                    report: reportRef.Report,
                    parameters: parameters
                );
                Origam.BI.ReportHelper.ComputeXsltValueParameters(
                    report: reportRef.Report,
                    parameters: parameters
                );
                OpenBrowser(
                    sURL: HttpTools.Instance.BuildUrl(
                        url: webReport.Url,
                        parameters: parameters,
                        forceExternal: webReport.ForceExternalUrl,
                        externalScheme: webReport.ExternalUrlScheme,
                        isUrlEscaped: webReport.IsUrlEscaped
                    )
                );
            }
            // all other reports
        }
        else if (item is AbstractUpdateScriptActivity)
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            ServiceCommandUpdateScriptActivity scriptActivity =
                (ServiceCommandUpdateScriptActivity)item;
            Platform[] platforms = settings.GetAllPlatforms();
            if (
                !platforms
                    .Where(predicate: platform =>
                        platform.GetParseEnum(dataDataService: platform.DataService)
                        == scriptActivity.DatabaseType.ToString()
                    )
                    .Any()
            )
            {
                MessageBox.Show(
                    text: "Platform is not supported for execute this script!",
                    caption: "Platform",
                    buttons: MessageBoxButtons.OK
                );
                return;
            }
            ProcessUpdateScript(item: item);
        }
        else if (item is DataConstantReferenceMenuItem)
        {
            ProcessConstant(item: item as DataConstantReferenceMenuItem, titleName: titleName);
        }
    }

    private void ProcessConstant(DataConstantReferenceMenuItem item, string titleName)
    {
        _parameterService.RefreshParameters();
        ParameterEditor editor = new ParameterEditor();
        DataConstant constant = item.Constant;
        // load non-cached element
        DataConstant copy =
            _persistence.SchemaProvider.RetrieveInstance(
                type: typeof(DataConstant),
                primaryKey: constant.PrimaryKey,
                useCache: false
            ) as DataConstant;
        // Temporarily overwrite the original lookup by the menu item's own
        // lookup. The constant will be thrown away anyway
        if (item.FinalLookup != null)
        {
            copy.DataLookup = item.FinalLookup;
        }
        // load constant into the editor
        editor.TitleName = titleName;
        editor.LoadObject(objectToLoad: copy);
        // show the editor
        WorkbenchSingleton.Workbench.ShowView(content: editor);
    }

    private static void ProcessUpdateScript(ISchemaItem item)
    {
        IDeploymentService deployment =
            ServiceManager.Services.GetService(serviceType: typeof(IDeploymentService))
            as IDeploymentService;
        deployment.ExecuteActivity(key: item.PrimaryKey);
    }

    private void ProcessWorkflow(ISchemaItem item, string titleName, bool isRepeatable)
    {
        Origam.Workbench.Commands.ViewLogPad logPad = new Origam.Workbench.Commands.ViewLogPad();
        logPad.Run();
        IWorkflow wf;
        Hashtable parameters = new Hashtable();
        WorkflowReferenceMenuItem wfRef = item as WorkflowReferenceMenuItem;
        if (wfRef != null)
        {
            wf = wfRef.Workflow;
            // set parameters
            RuleEngine ruleEngine = RuleEngine.Create(contextStores: null, transactionId: null);
            foreach (ISchemaItem parameter in item.ChildItems)
            {
                if (parameter != null)
                {
                    parameters.Add(
                        key: parameter.Name,
                        value: ruleEngine.Evaluate(item: parameter)
                    );
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
            RuleEngine ruleEngine = RuleEngine.Create(contextStores: null, transactionId: null);
            foreach (ISchemaItem parameter in schedule.ChildItems)
            {
                if (parameter != null)
                {
                    parameters.Add(
                        key: parameter.Name,
                        value: ruleEngine.Evaluate(item: parameter)
                    );
                }
            }
        }
        System.Drawing.Icon icon;
        var schemaBrowser =
            WorkbenchSingleton.Workbench.GetPad(type: typeof(IBrowserPad)) as IBrowserPad;
        if (schemaBrowser != null && item.NodeImage == null)
        {
            icon = System.Drawing.Icon.FromHandle(
                handle: (
                    (System.Drawing.Bitmap)
                        schemaBrowser.ImageList.Images[
                            index: schemaBrowser.ImageIndex(icon: item.Icon)
                        ]
                ).GetHicon()
            );
        }
        else
        {
            icon = System.Drawing.Icon.FromHandle(handle: item.NodeImage.ToBitmap().GetHicon());
        }
        WorkflowHost host = WorkflowHost.DefaultHost;
        // Initialize view for this workflow
        WorkflowForm form = Origam.Workflow.Gui.Win.WorkflowHelper.CreateWorkflowForm(
            host: host,
            icon: icon,
            titleName: titleName,
            workflowId: (Guid)wf.PrimaryKey[key: "Id"]
        );
        WorkflowEngine workflowEngine = WorkflowEngine.PrepareWorkflow(
            workflow: wf,
            parameters: parameters,
            isRepeatable: isRepeatable,
            titleName: titleName
        );
        form.WorkflowEngine = workflowEngine;
        host.ExecuteWorkflow(engine: workflowEngine);
    }

    private void ProcessCrystalReport(ISchemaItem item, string titleName)
    {
        ReportViewer viewer = new ReportViewer(
            reportElement: (item as CrystalReport),
            titleName: titleName
        );
        foreach (DictionaryEntry param in Parameters)
        {
            viewer.Parameters.Add(key: param.Key, value: param.Value);
        }
        viewer.LoadReport();
        WorkbenchSingleton.Workbench.ShowView(content: viewer);
    }

    private DataRow ShowSelectionDialog(
        PanelControlSet selectionDialogPanel,
        ITransformation transformationBeforeSelection,
        ITransformation transformationAfterSelection,
        IEndRule endRule
    )
    {
        DatasetGenerator gen = new DatasetGenerator(userDefinedParameters: true);
        FormGenerator sdGenerator = null;
        AsForm sd = null;

        try
        {
            UserProfile profile = SecurityManager.CurrentUserProfile();
            IDataDocument dataDoc;
            if (transformationBeforeSelection == null)
            {
                dataDoc = DataDocumentFactory.New(
                    dataSet: FormTools.GetSelectionDialogData(
                        entityId: selectionDialogPanel.DataSourceId,
                        transformationBeforeId: Guid.Empty,
                        createEmptyRow: false,
                        profileId: profile.Id
                    )
                );
            }
            else
            {
                dataDoc = DataDocumentFactory.New(
                    dataSet: FormTools.GetSelectionDialogData(
                        entityId: selectionDialogPanel.DataSourceId,
                        transformationBeforeId: (Guid)
                            transformationBeforeSelection.PrimaryKey[key: "Id"],
                        createEmptyRow: false,
                        profileId: profile.Id
                    )
                );
            }
            sdGenerator = new FormGenerator();
            sd =
                sdGenerator.LoadSelectionDialog(
                    xmlData: dataDoc,
                    panelDefinition: selectionDialogPanel,
                    endRule: endRule
                ) as AsForm;

            sd.AutoAddNewEntity = dataDoc.DataSet.Tables[index: 0].TableName;
            sd.SaveOnClose = false;
            DialogResult result = sd.ShowDialog(
                owner: WorkbenchSingleton.Workbench as IWin32Window
            );
            if (result == DialogResult.Cancel)
            {
                return null;
            }

            DataRow row;
            if (transformationAfterSelection == null)
            {
                row = FormTools.GetSelectionDialogResultRow(
                    entityId: selectionDialogPanel.DataSourceId,
                    transformationAfterId: Guid.Empty,
                    dataDoc: dataDoc,
                    profileId: profile.Id
                );
            }
            else
            {
                row = FormTools.GetSelectionDialogResultRow(
                    entityId: selectionDialogPanel.DataSourceId,
                    transformationAfterId: (Guid)transformationAfterSelection.PrimaryKey[key: "Id"],
                    dataDoc: dataDoc,
                    profileId: profile.Id
                );
            }
            return row;
        }
        finally
        {
            if (sd != null)
            {
                sd.Dispose();
            }

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
            sBrws = Microsoft
                .Win32.Registry.ClassesRoot.OpenSubKey(
                    name: "http\\shell\\open\\command",
                    writable: false
                )
                .GetValue(name: null)
                .ToString()
                .ToLower();
            //Trim the file path (removing everything after the ".exe") and get rid of quotation marks
            sBrws = sBrws.Substring(startIndex: 1, length: sBrws.IndexOf(value: ".exe") + 3);
            sBrws = sBrws.Replace(oldValue: "\"", newValue: "");
            //Open the URL using the path
            System.Diagnostics.Process.Start(fileName: sBrws, arguments: sURL);
        }
        catch
        {
            //Add redundant attempts to open the URL, in case the code fails
            try
            {
                //First just try using Process.Start, which will open the default browser, but might reuse IE's window
                System.Diagnostics.Process.Start(fileName: sURL);
            }
            catch
            {
                try
                {
                    //If Process.Start fails (due to known bug), try launching IE directly and passing it the URL
                    System.Diagnostics.ProcessStartInfo psi =
                        new System.Diagnostics.ProcessStartInfo(
                            fileName: "IExplore.exe",
                            arguments: sURL
                        );
                    System.Diagnostics.Process.Start(startInfo: psi);
                    psi = null;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        message: "Could not launch the browser. Details: " + ex.Message
                    );
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
        get { return true; }
        set { base.IsEnabled = value; }
    }

    public override void Run()
    {
        using (SplashScreen splash = new SplashScreen())
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
        get { return true; }
        set { base.IsEnabled = value; }
    }

    public override void Run()
    {
        WorkflowPlayerPad pad =
            WorkbenchSingleton.Workbench.GetPad(type: typeof(WorkflowPlayerPad))
            as WorkflowPlayerPad;
        if (pad != null)
        {
            WorkbenchSingleton.Workbench.ShowPad(content: pad);
        }
    }
}

public class ViewWorkQueuePad : AbstractMenuCommand
{
    public override bool IsEnabled
    {
        get { return true; }
        set { base.IsEnabled = value; }
    }

    public override void Run()
    {
        WorkQueuePad pad =
            WorkbenchSingleton.Workbench.GetPad(type: typeof(WorkQueuePad)) as WorkQueuePad;
        if (pad != null)
        {
            WorkbenchSingleton.Workbench.ShowPad(content: pad);
        }
    }
}

/// <summary>
/// Executes an active schema item.
/// </summary>
public class ExecuteActiveSchemaItem : AbstractMenuCommand
{
    WorkbenchSchemaService _schemaService =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get
        {
            return (
                _schemaService.ActiveNode is FormControlSet
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
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
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
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
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
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        Origam.Workbench.Commands.ViewOutputPad outputPad =
            new Origam.Workbench.Commands.ViewOutputPad();
        outputPad.Run();
        OutputPad p = WorkbenchSingleton.Workbench.GetPad(type: typeof(OutputPad)) as OutputPad;
        AsForm activeForm = WorkbenchSingleton.Workbench.ActiveDocument as AsForm;
        p.SetOutputText(sText: activeForm.FormGenerator.DataSet.GetXml());
    }
}

public class ShowEditorXml : AbstractMenuCommand
{
    WorkbenchSchemaService _schemaService =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return _schemaService.ActiveNode is ISchemaItem; }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        OutputPad p = WorkbenchSingleton.Workbench.GetPad(type: typeof(OutputPad)) as OutputPad;
        ISchemaItem item = _schemaService.ActiveNode as ISchemaItem;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        System.IO.StringWriter sw = new System.IO.StringWriter(sb: sb);
        XmlTextWriter xw = new XmlTextWriter(w: sw);
        xw.Formatting = Formatting.Indented;
        Origam.OrigamEngine.ModelXmlBuilders.PropertyGridBuilder.Build(item: item).WriteTo(w: xw);
        p.SetOutputText(sText: sb.ToString());
    }
}

/// <summary>
/// Displays DbCompare info
/// </summary>
public class ShowDbCompare : AbstractMenuCommand
{
    SchemaService _schemaService =
        ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
    public override bool IsEnabled
    {
        get { return _schemaService.IsSchemaLoaded; }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        SchemaCompareEditor editor = new SchemaCompareEditor();
        WorkbenchSingleton.Workbench.ShowView(content: editor);
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
    IPersistenceService _persistence =
        ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
        as IPersistenceService;
    SchemaService _schemaService =
        ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
    public override bool IsEnabled
    {
        get { return _schemaService.IsSchemaLoaded; }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        FormReferenceMenuItem formMenu = new FormReferenceMenuItem();
        formMenu.PersistenceProvider = _persistence.SchemaProvider;
        formMenu.DisplayName = strings.WorkFlowTrace_MenuItem;
        formMenu.ScreenId = new Guid(g: "75C3B61C-77BD-41AB-AE8F-39361500CEC4");
        formMenu.ListDataStructureId = new Guid(g: "309843cc-39ec-4eca-8848-8c69c885790c");
        formMenu.ListEntityId = new Guid(g: "3b041438-2f59-4c01-976b-a2f9e89038fe");
        formMenu.MethodId = new Guid(g: "77cdbb0c-430e-4552-a0c2-f494cfb0c782");
        formMenu.ListSortSet =
            _persistence.SchemaProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(
                    id: new Guid(g: "d20be524-2178-42c0-943d-7d84ee6bb53b")
                ),
                useCache: true,
                throwNotFoundException: false
            ) as DataStructureSortSet;
        formMenu.SortSet =
            _persistence.SchemaProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(
                    id: new Guid(g: "f68c419b-6130-4665-bee1-d81f2769c5ad")
                ),
                useCache: true,
                throwNotFoundException: false
            ) as DataStructureSortSet;
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
        set =>
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
    }

    public override void Run()
    {
        FormReferenceMenuItem formMenu = new FormReferenceMenuItem();
        formMenu.PersistenceProvider = _persistence.SchemaProvider;
        formMenu.DisplayName = strings.RuleTrace_MenuItem;
        formMenu.ScreenId = new Guid(g: "57dc7edd-7b9c-43f2-b94a-54ddd2d98206");
        formMenu.Roles = "*";
        formMenu.SortSet =
            _persistence.SchemaProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(
                    id: new Guid(g: "6b22f4c9-bc05-4e52-88f9-486e64dc7b1b")
                ),
                useCache: true,
                throwNotFoundException: false
            ) as DataStructureSortSet;
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
            WorkflowPlayerPad pad =
                WorkbenchSingleton.Workbench.GetPad(type: typeof(WorkflowPlayerPad))
                as WorkflowPlayerPad;

            if (pad.ebrSchemaBrowser.ActiveNode is WorkflowReferenceMenuItem)
            {
                (this.Owner as AsMenuCommand).Checked = (
                    pad.ebrSchemaBrowser.ActiveNode as WorkflowReferenceMenuItem
                ).IsRepeatable;
            }
            else
            {
                (this.Owner as AsMenuCommand).Checked = false;
            }
            return pad.ebrSchemaBrowser.ActiveNode is WorkflowReferenceMenuItem;
        }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        (this.Owner as AsMenuCommand).Checked = !(this.Owner as AsMenuCommand).Checked;
        WorkflowPlayerPad pad =
            WorkbenchSingleton.Workbench.GetPad(type: typeof(WorkflowPlayerPad))
            as WorkflowPlayerPad;
        (pad.ebrSchemaBrowser.ActiveNode as WorkflowReferenceMenuItem).IsRepeatable = (
            this.Owner as AsMenuCommand
        ).Checked;
        pad.ebrSchemaBrowser.RefreshItem(node: pad.ebrSchemaBrowser.ActiveNode);
    }
}

/// <summary>
/// Generates a localization file from the model.
/// </summary>
public class GenerateLocalizationFile : AbstractMenuCommand
{
    SchemaService _schemaService =
        ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
    public override bool IsEnabled
    {
        get { return _schemaService.IsSchemaLoaded; }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        string[] languages = settings.TranslationBuilderLanguages.Split(
            separator: ",".ToCharArray()
        );
        SchemaService ss =
            ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
        string packageName = ss.ActiveExtension.Name;
        foreach (string language in languages)
        {
            string fileName = packageName + "-" + language.Trim() + ".xml";
            string outputPath = Path.Combine(
                path1: Path.Combine(path1: settings.ModelSourceControlLocation, path2: @"..\l10n"),
                path2: fileName
            );
            if (!string.IsNullOrEmpty(value: settings.LocalizationFolder))
            {
                outputPath = Path.Combine(path1: settings.LocalizationFolder, path2: fileName);
            }
            LocalizationCache currentTranslations = null;
            if (File.Exists(path: outputPath))
            {
                currentTranslations = new LocalizationCache(filePath: outputPath);
            }
            MemoryStream ms = new MemoryStream();
            FileStream fs = null;
            try
            {
                TranslationBuilder.Build(
                    stream: ms,
                    currentTranslations: currentTranslations,
                    locale: language,
                    packageId: ss.ActiveSchemaExtensionId
                );
                fs = new FileStream(path: outputPath, mode: FileMode.Create);
                ms.Seek(offset: 0, loc: SeekOrigin.Begin);
                ms.WriteTo(stream: fs);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }

                if (ms != null)
                {
                    ms.Close();
                }
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
        get { return true; }
        set { base.IsEnabled = value; }
    }

    public override void Run()
    {
        Clipboard.SetDataObject(data: Guid.NewGuid().ToString());
    }
}

/// <summary>
/// Resets user caches
/// </summary>
public class ResetUserCaches : AbstractMenuCommand
{
    public override bool IsEnabled
    {
        get { return true; }
        set { base.IsEnabled = value; }
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
        get { return true; }
        set { base.IsEnabled = value; }
    }

    public override void Run()
    {
        WorkflowWatchPad pad =
            WorkbenchSingleton.Workbench.GetPad(type: typeof(WorkflowWatchPad)) as WorkflowWatchPad;
        if (pad != null)
        {
            WorkbenchSingleton.Workbench.ShowPad(content: pad);
        }
    }
}

public class SetServerRestart : AbstractMenuCommand
{
    private SchemaService _schemaService =
        ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
    public override bool IsEnabled
    {
        get { return _schemaService.IsSchemaLoaded; }
        set { base.IsEnabled = value; }
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
        System.Diagnostics.Process.Start(fileName: target);
    }
}

public class ShowCommunity : AbstractMenuCommand
{
    public override void Run()
    {
        string target = "http://community.origam.com";
        System.Diagnostics.Process.Start(fileName: target);
    }
}

public class ShowWebApplication : AbstractMenuCommand
{
    public override bool IsEnabled
    {
        get { return ConfigurationManager.GetActiveConfiguration() != null; }
        set { base.IsEnabled = value; }
    }

    public override void Run()
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        string target = settings.ServerUrl;
        if (string.IsNullOrEmpty(value: target))
        {
            MessageBox.Show(
                owner: WorkbenchSingleton.Workbench as IWin32Window,
                text: strings.ServerUrlNotSet_Message,
                caption: strings.ServerUrlNotSet_Title,
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
        else
        {
            System.Diagnostics.Process.Start(fileName: target);
        }
    }
}

public class CreateNewProject : AbstractMenuCommand
{
    public override void Run()
    {
        NewProjectWizard wiz = new NewProjectWizard();
        wiz.ShowDialog(owner: (IWin32Window)WorkbenchSingleton.Workbench);
    }
}

public abstract class StorageConvertor : AbstractMenuCommand
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    protected IPersistenceService CurrentPersistenceService =>
        ServiceManager.Services.GetService<IPersistenceService>();

    protected IDocumentationService CurrentDocumentationService =>
        ServiceManager.Services.GetService<IDocumentationService>();

    protected readonly SchemaService schemaService =
        ServiceManager.Services.GetService<SchemaService>();
    protected IPersistenceService newPersistenceService;
    protected IStatusBarService statusBar = ServiceManager.Services.GetService<IStatusBarService>();
    private static IEnumerable<Type> AllProviderTypes =>
        ServiceManager
            .Services.GetService<SchemaService>()
            .Providers.Select(selector: provider => provider.GetType());

    private ISchemaItemCollection GetAllItems(Type providerType) =>
        schemaService.GetProvider(type: providerType).ChildItems;

    private void UpdateStatusBar(Type type, int typeNumber)
    {
        int providerCount = AllProviderTypes.Count();
        statusBar.SetStatusText(
            text: string.Format(
                format: "Converting {0}. {1}/{2}",
                arg0: type.Name,
                arg1: typeNumber,
                arg2: providerCount
            )
        );
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
            UpdateStatusBar(type: type, typeNumber: typeNumber);
            PersistAllProviderItems(providerType: type);
            typeNumber++;
        }
        newPersistenceService.SchemaProvider.EndTransaction();
    }

    private void PersistFolders<T>()
    {
        statusBar.SetStatusText(text: "Converting " + typeof(T));
        IPersistenceService dbSvc =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        var listOfItems = dbSvc.SchemaProvider.RetrieveList<T>(filter: null);

        foreach (IPersistent item in listOfItems)
        {
            newPersistenceService.SchemaProvider.Persist(obj: item);
        }
    }

    private void PersistAllProviderItems(Type providerType)
    {
        ISchemaItemCollection allItems = GetAllItems(providerType: providerType);
        if (log.IsDebugEnabled)
        {
            log.Debug(message: $"ProviderType:{providerType}, items: {allItems.Count}");
        }
        foreach (ISchemaItem item in allItems)
        {
            Persist(item: item);
        }
    }

    private void Persist(ISchemaItem item)
    {
        newPersistenceService.SchemaProvider.Persist(obj: item);
        foreach (var ancestor in item.Ancestors)
        {
            newPersistenceService.SchemaProvider.Persist(obj: ancestor);
        }
        foreach (var child in item.ChildItems)
        {
            if (child.DerivedFrom == null && child.IsPersistable)
            {
                Persist(item: child);
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
            DataTable newTable = data.Tables[name: originalTable.TableName];
            switch (originalTable.TableName)
            {
                case "SchemaItemGroup":
                case "SchemaExtension":
                case "PackageReference":
                {
                    originalTable
                        .Rows.Cast<DataRow>()
                        .OrderBy(keySelector: row => row[columnName: "Id"])
                        .ForEach(action: row =>
                        {
                            newTable.ImportRow(row: row);
                            ImportChildRows(
                                originalTable: originalTable,
                                newTable: newTable,
                                parentRow: row
                            );
                        });
                    break;
                }

                default:
                {
                    foreach (DataRow row in originalTable.Rows)
                    {
                        newTable.ImportRow(row: row);
                    }
                    break;
                }
            }
        }
        return data;
    }

    private void ImportChildRows(DataTable originalTable, DataTable newTable, DataRow parentRow)
    {
        foreach (DataRow row in parentRow.GetChildRows(relationName: "ChildSchemaItem"))
        {
            newTable.ImportRow(row: row);
            ImportChildRows(originalTable: originalTable, newTable: newTable, parentRow: row);
        }
    }
}
