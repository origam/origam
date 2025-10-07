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
using System.Xml;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.GuiModel;
using Origam.Schema.RuleModel;
using Origam.Service.Core;

namespace Origam.Workflow;

public delegate void WorkflowHostFormEvent(object sender, WorkflowHostFormEventArgs e);

/// <summary>
/// Summary description for WorkflowHostFormEventArgs.
/// </summary>
public class WorkflowHostFormEventArgs : WorkflowHostEventArgs
{
    private IDataDocument _data;
    private string _description;
    private string _notification;
    private FormControlSet _form;
    private DataStructureRuleSet _ruleSet;
    private IEndRule _endRule;
    private Guid _taskId;
    private AbstractDataStructure _structure;
    private AbstractDataStructure _saveStructure;
    private DataStructureMethod _refreshMethod;
    private DataStructureSortSet _refreshSort;
    private bool _isFinalForm;
    private bool _allowSave;
    private bool _isAutoNext;
    private Hashtable _parameters;
    private bool _isRefreshSuppressedBeforeFirstSave;
    private IEndRule _saveConfirmationRule;
    private bool _refreshPortalAfterSave;

    public WorkflowHostFormEventArgs(
        Guid taskId,
        WorkflowEngine engine,
        IDataDocument data,
        string description,
        string notification,
        FormControlSet form,
        DataStructureRuleSet ruleSet,
        IEndRule endRule,
        AbstractDataStructure structure,
        DataStructureMethod refreshMethod,
        DataStructureSortSet refreshSort,
        AbstractDataStructure saveStructure,
        bool isFinalForm,
        bool allowSave,
        bool isAutoNext,
        Hashtable parameters,
        bool isRefreshSuppressedBeforeFirstSave,
        IEndRule saveConfirmationRule,
        bool refreshPortalAfterSave
    )
        : base(engine, null)
    {
        _taskId = taskId;
        _data = data;
        _description = description;
        _notification = notification;
        _form = form;
        _ruleSet = ruleSet;
        _endRule = endRule;
        _structure = structure;
        _refreshMethod = refreshMethod;
        _refreshSort = refreshSort;
        _saveStructure = saveStructure;
        _isFinalForm = isFinalForm;
        _allowSave = allowSave;
        _isAutoNext = isAutoNext;
        _parameters = parameters;
        _isRefreshSuppressedBeforeFirstSave = isRefreshSuppressedBeforeFirstSave;
        _saveConfirmationRule = saveConfirmationRule;
        _refreshPortalAfterSave = refreshPortalAfterSave;
    }

    public IDataDocument Data
    {
        get { return _data; }
        set { _data = value; }
    }
    public string Description
    {
        get { return _description; }
        set { _description = value; }
    }
    public string Notification
    {
        get { return _notification; }
        set { _notification = value; }
    }
    public FormControlSet Form
    {
        get { return _form; }
        set { _form = value; }
    }
    public AbstractDataStructure DataStructure
    {
        get { return _structure; }
        set { _structure = value; }
    }
    public DataStructureRuleSet RuleSet
    {
        get { return _ruleSet; }
        set { _ruleSet = value; }
    }
    public DataStructureMethod RefreshMethod
    {
        get { return _refreshMethod; }
        set { _refreshMethod = value; }
    }
    public DataStructureSortSet RefreshSort
    {
        get { return _refreshSort; }
        set { _refreshSort = value; }
    }
    public AbstractDataStructure SaveDataStructure
    {
        get { return _saveStructure; }
        set { _saveStructure = value; }
    }
    public IEndRule EndRule
    {
        get { return _endRule; }
        set { _endRule = value; }
    }
    public Guid TaskId
    {
        get { return _taskId; }
        set { _taskId = value; }
    }
    public bool IsFinalForm
    {
        get { return _isFinalForm; }
        set { _isFinalForm = value; }
    }
    public bool AllowSave
    {
        get { return _allowSave; }
        set { _allowSave = value; }
    }
    public bool IsAutoNext
    {
        get { return _isAutoNext; }
        set { _isAutoNext = value; }
    }
    public bool IsRefreshSuppressedBeforeFirstSave
    {
        get { return _isRefreshSuppressedBeforeFirstSave; }
        set { _isRefreshSuppressedBeforeFirstSave = value; }
    }
    public IEndRule SaveConfirmationRule
    {
        get { return _saveConfirmationRule; }
        set { _saveConfirmationRule = value; }
    }
    public Hashtable Parameters
    {
        get { return _parameters; }
        set { _parameters = value; }
    }
    public bool RefreshPortalAfterSave
    {
        get { return _refreshPortalAfterSave; }
        set { _refreshPortalAfterSave = value; }
    }
}
