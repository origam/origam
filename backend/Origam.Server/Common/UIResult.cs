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
using Origam.Gui;

namespace Origam.Server;

public class UIResult
{
    private Guid _sessionId;
    private string _formDefinitionId;
    private string _title;
    private IDictionary<string, object> _data;
    private IList<AbstractPanelConfig> _panelConfigurations = new List<AbstractPanelConfig>();
    private IList<LookupConfig> _lookupMenuMappings = new List<LookupConfig>();
    private IList<string> _externalModules = new List<string>();
    private string _formDefinition;
    private IList<FormNotification> _notifications;
    private string _currentRecordId;
    private HelpTooltip _tooltip;
    private bool _hasPartialData;
    private IDictionary<string, IDictionary> _variables;
    private string _workflowTaskId;

    public UIResult(
        Guid sessionId,
        IDictionary<string, object> data,
        IDictionary<string, IDictionary> variables,
        bool isDirty
    )
    {
        IsDirty = isDirty;
        SessionId = sessionId;
        Data = data;
        if (variables != null)
        {
            Variables = variables;
        }
    }

    public bool IsDirty { get; }
    public string FormDefinition
    {
        get { return _formDefinition; }
        set { _formDefinition = value; }
    }

    public Guid SessionId
    {
        get { return _sessionId; }
        set { _sessionId = value; }
    }
    public string FormDefinitionId
    {
        get { return _formDefinitionId; }
        set { _formDefinitionId = value; }
    }
    public string WorkflowTaskId
    {
        get { return _workflowTaskId; }
        set { _workflowTaskId = value; }
    }
    public string Title
    {
        get { return _title; }
        set { _title = value; }
    }
    public IDictionary<string, object> Data
    {
        get { return _data; }
        set { _data = value; }
    }
    public IList<AbstractPanelConfig> PanelConfigurations
    {
        get { return _panelConfigurations; }
        set { _panelConfigurations = value; }
    }
    public IList<LookupConfig> LookupMenuMappings
    {
        get { return _lookupMenuMappings; }
        set { _lookupMenuMappings = value; }
    }
    public IList<string> ExternalModules
    {
        get { return _externalModules; }
        set { _externalModules = value; }
    }
    public IList<FormNotification> Notifications
    {
        get { return _notifications; }
        set { _notifications = value; }
    }
    public IDictionary<string, IDictionary> Variables
    {
        get { return _variables; }
        set { _variables = value; }
    }

    /// <summary>
    /// Compatibility with Flash Client v. 1486
    /// </summary>
    //TODO: Remove after Flash Client v. 1486 will not be in use
    public string[] Notification
    {
        get
        {
            if (Notifications == null)
            {
                return new string[0];
            }
            string[] result = new string[Notifications.Count];
            for (int i = 0; i < Notifications.Count; i++)
            {
                result[i] = Notifications[i].Text;
            }
            return result;
        }
    }
    public string CurrentRecordId
    {
        get { return _currentRecordId; }
        set { _currentRecordId = value; }
    }
    public HelpTooltip Tooltip
    {
        get { return _tooltip; }
        set { _tooltip = value; }
    }

    /// <summary>
    /// Indicates to the client that only a list of primary keys is returned and the client has to request data as they are displayed.
    /// </summary>
    public bool HasPartialData
    {
        get { return _hasPartialData; }
        set { _hasPartialData = value; }
    }
}

public class LookupConfig
{
    private Guid _lookupId;
    private string _menuId;
    private bool _dependsOnValue;
    private string _selectionPanelId;

    public LookupConfig(Guid lookupId, string menuId, bool dependsOnValue, string selectionPanelId)
    {
        _lookupId = lookupId;
        _menuId = menuId;
        _dependsOnValue = dependsOnValue;
        _selectionPanelId = selectionPanelId;
    }

    public Guid LookupId
    {
        get { return _lookupId; }
        set { _lookupId = value; }
    }
    public string MenuId
    {
        get { return _menuId; }
        set { _menuId = value; }
    }
    public bool DependsOnValue
    {
        get { return _dependsOnValue; }
        set { _dependsOnValue = value; }
    }
    public string SelectionPanelId
    {
        get { return _selectionPanelId; }
        set { _selectionPanelId = value; }
    }
}

public class UIElement
{
    private Guid _id;
    public Guid Id
    {
        get { return _id; }
        set { _id = value; }
    }
}

public class UIPanel : UIElement
{
    private Guid _instanceId;
    private string _entity;
    private OrigamPanelViewMode _defaultPanelView;
    public OrigamPanelViewMode DefaultPanelView
    {
        get { return _defaultPanelView; }
        set { _defaultPanelView = value; }
    }
    public string Entity
    {
        get { return _entity; }
        set { _entity = value; }
    }
    public Guid InstanceId
    {
        get { return _instanceId; }
        set { _instanceId = value; }
    }
}

public class AbstractPanelConfig
{
    private UIPanel _panel;
    public UIPanel Panel
    {
        get { return _panel; }
        set { _panel = value; }
    }
}

public class UIPanelConfig : AbstractPanelConfig
{
    private UIGridFilterConfiguration _initialFilter;
    private IList<UIGridFilterConfiguration> _filters = new List<UIGridFilterConfiguration>();
    private IList<UIGridColumnConfiguration> _columnConfigurations =
        new List<UIGridColumnConfiguration>();
    private IList<UIGridSortConfiguration> _defaultSort = new List<UIGridSortConfiguration>();
    private bool _allowCreate = true;
    public UIGridFilterConfiguration InitialFilter
    {
        get { return _initialFilter; }
        set { _initialFilter = value; }
    }
    public IList<UIGridFilterConfiguration> Filters
    {
        get { return _filters; }
    }
    public IList<UIGridColumnConfiguration> ColumnConfigurations
    {
        get { return _columnConfigurations; }
    }
    public IList<UIGridSortConfiguration> DefaultSort
    {
        get { return _defaultSort; }
    }
    public bool AllowCreate
    {
        get { return _allowCreate; }
        set { _allowCreate = value; }
    }
}

public class UISplitConfig : AbstractPanelConfig
{
    private int _position;
    public int Position
    {
        get { return _position; }
        set { _position = value; }
    }
}

public class UIGridColumnConfiguration
{
    private string _property;
    private int _width;
    private bool _isHidden;

    public UIGridColumnConfiguration() { }

    public UIGridColumnConfiguration(string property, int width, bool isHidden)
    {
        Property = property;
        Width = width;
        IsHidden = isHidden;
    }

    public string Property
    {
        get { return _property; }
        set { _property = value; }
    }
    public int Width
    {
        get { return _width; }
        set { _width = value; }
    }
    public bool IsHidden
    {
        get { return _isHidden; }
        set { _isHidden = value; }
    }
}

public class UIGridFilterConfiguration
{
    private string _name;
    private bool _isGlobal;
    private Guid _id;

    public UIGridFilterConfiguration() { }

    public UIGridFilterConfiguration(Guid id, string name, bool isGlobal)
    {
        _id = id;
        _name = name;
        _isGlobal = isGlobal;
    }

    public Guid Id
    {
        get { return _id; }
        set { _id = value; }
    }

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }
    public bool IsGlobal
    {
        get { return _isGlobal; }
        set { _isGlobal = value; }
    }

    public List<UIGridFilterFieldConfiguration> Details { get; set; } = new();
}

public class UIGridFilterFieldConfiguration
{
    private string _property;
    private object _value1;
    private object _value2;
    private int _operator;

    public UIGridFilterFieldConfiguration() { }

    public UIGridFilterFieldConfiguration(string property, object value1, object value2, int oper)
    {
        _property = property;
        _value1 = value1;
        _value2 = value2;
        _operator = oper;
    }

    public string Property
    {
        get { return _property; }
        set { _property = value; }
    }
    public object Value1
    {
        get { return _value1; }
        set { _value1 = value; }
    }
    public object Value2
    {
        get { return _value2; }
        set { _value2 = value; }
    }
    public int Operator
    {
        get { return _operator; }
        set { _operator = value; }
    }
}
