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

using System.Collections;
using System.Xml;
using Origam.Schema.GuiModel;

namespace Origam.OrigamEngine.ModelXmlBuilders;

public class AsPanelActionButtonBuilder
{
    public static void Build(XmlElement actionsElement, ActionConfiguration config)
    {
        XmlElement actionElement = actionsElement.OwnerDocument.CreateElement(name: "Action");
        actionsElement.AppendChild(newChild: actionElement);

        actionElement.SetAttribute(
            name: "ShowAlways",
            value: XmlConvert.ToString(value: config.ShowAlways)
        );
        actionElement.SetAttribute(name: "Type", value: config.Type.ToString());
        actionElement.SetAttribute(name: "Id", value: config.ActionId);
        actionElement.SetAttribute(name: "GroupId", value: config.GroupId);
        actionElement.SetAttribute(name: "Caption", value: config.Caption);
        actionElement.SetAttribute(name: "IconUrl", value: config.IconUrl);
        actionElement.SetAttribute(name: "Mode", value: config.Mode.ToString());
        actionElement.SetAttribute(
            name: "IsDefault",
            value: XmlConvert.ToString(value: config.IsDefault)
        );
        actionElement.SetAttribute(name: "Placement", value: config.Placement.ToString());

        if (!string.IsNullOrEmpty(value: config.ConfirmationMessage))
        {
            actionElement.SetAttribute(
                name: "ConfirmationMessage",
                value: config.ConfirmationMessage
            );
        }

        if (config.Shortcut != null && config.Shortcut.KeyCode != 0)
        {
            XmlElement shortcutElement = actionElement.OwnerDocument.CreateElement(
                name: "KeyboardShortcut"
            );
            actionElement.AppendChild(newChild: shortcutElement);
            shortcutElement.SetAttribute(
                name: "Ctrl",
                value: XmlConvert.ToString(value: config.Shortcut.IsControl)
            );
            shortcutElement.SetAttribute(
                name: "Shift",
                value: XmlConvert.ToString(value: config.Shortcut.IsShift)
            );
            shortcutElement.SetAttribute(
                name: "Alt",
                value: XmlConvert.ToString(value: config.Shortcut.IsAlt)
            );
            shortcutElement.SetAttribute(
                name: "KeyCode",
                value: XmlConvert.ToString(value: config.Shortcut.KeyCode)
            );
        }

        if (config.Scanner != null && config.Scanner.TerminatorCharCode != 0)
        {
            actionElement.SetAttribute(
                name: "ScannerInputTerminator",
                value: XmlConvert.ToString(value: config.Scanner.TerminatorCharCode)
            );
        }

        if (!string.IsNullOrEmpty(value: config.Scanner?.Parameter))
        {
            actionElement.SetAttribute(
                name: "ScannerInputParameterName",
                value: config.Scanner.Parameter
            );
        }

        if (config.Parameters is { Count: > 0 })
        {
            XmlElement parametersElement = actionElement.OwnerDocument.CreateElement(
                name: "Parameters"
            );
            actionElement.AppendChild(newChild: parametersElement);
            foreach (DictionaryEntry entry in config.Parameters)
            {
                XmlElement parameterElement = parametersElement.OwnerDocument.CreateElement(
                    name: "Parameter"
                );
                parametersElement.AppendChild(newChild: parameterElement);
                parameterElement.SetAttribute(name: "Name", value: (string)entry.Key);
                parameterElement.SetAttribute(name: "FieldName", value: (string)entry.Value);
            }
        }
    }
}

public class ActionConfiguration
{
    public PanelActionType Type { get; set; }
    public PanelActionMode Mode { get; set; }
    public ActionButtonPlacement Placement { get; set; }
    public string ActionId { get; set; }
    public string GroupId { get; set; }
    public string Caption { get; set; }
    public string IconUrl { get; set; }
    public bool IsDefault { get; set; }
    public bool ShowAlways { get; set; }
    public string ConfirmationMessage { get; set; }
    public Hashtable Parameters { get; set; } = new Hashtable();
    public KeyShortcut Shortcut { get; set; }
    public ScannerSettings Scanner { get; set; }
}

public class KeyShortcut
{
    public bool IsShift { get; set; }
    public bool IsControl { get; set; }
    public bool IsAlt { get; set; }
    public int KeyCode { get; set; }
}

public class ScannerSettings
{
    public string Parameter { get; set; } = "";
    public int TerminatorCharCode { get; set; }
}
