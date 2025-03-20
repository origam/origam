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
        XmlElement actionElement =
            actionsElement.OwnerDocument.CreateElement("Action");
        actionsElement.AppendChild(actionElement);

        actionElement.SetAttribute("ShowAlways",
            XmlConvert.ToString(config.ShowAlways));
        actionElement.SetAttribute("Type", config.Type.ToString());
        actionElement.SetAttribute("Id", config.ActionId);
        actionElement.SetAttribute("GroupId", config.GroupId);
        actionElement.SetAttribute("Caption", config.Caption);
        actionElement.SetAttribute("IconUrl", config.IconUrl);
        actionElement.SetAttribute("Mode", config.Mode.ToString());
        actionElement.SetAttribute("IsDefault",
            XmlConvert.ToString(config.IsDefault));
        actionElement.SetAttribute("Placement", config.Placement.ToString());

        if (!string.IsNullOrEmpty(config.ConfirmationMessage))
        {
            actionElement.SetAttribute("ConfirmationMessage",
                config.ConfirmationMessage);
        }

        if (config.Shortcut != null && config.Shortcut.KeyCode != 0)
        {
            XmlElement shortcutElement =
                actionElement.OwnerDocument.CreateElement("KeyboardShortcut");
            actionElement.AppendChild(shortcutElement);
            shortcutElement.SetAttribute("Ctrl",
                XmlConvert.ToString(config.Shortcut.IsControl));
            shortcutElement.SetAttribute("Shift",
                XmlConvert.ToString(config.Shortcut.IsShift));
            shortcutElement.SetAttribute("Alt",
                XmlConvert.ToString(config.Shortcut.IsAlt));
            shortcutElement.SetAttribute("KeyCode",
                XmlConvert.ToString(config.Shortcut.KeyCode));
        }

        if (config.Scanner != null && config.Scanner.TerminatorCharCode != 0)
        {
            actionElement.SetAttribute("ScannerInputTerminator",
                XmlConvert.ToString(config.Scanner.TerminatorCharCode));
        }

        if (!string.IsNullOrEmpty(config.Scanner?.Parameter))
        {
            actionElement.SetAttribute("ScannerInputParameterName",
                config.Scanner.Parameter);
        }

        if (config.Parameters is { Count: > 0 })
        {
            XmlElement parametersElement =
                actionElement.OwnerDocument.CreateElement("Parameters");
            actionElement.AppendChild(parametersElement);
            foreach (DictionaryEntry entry in config.Parameters)
            {
                XmlElement parameterElement =
                    parametersElement.OwnerDocument.CreateElement("Parameter");
                parametersElement.AppendChild(parameterElement);
                parameterElement.SetAttribute("Name", (string)entry.Key);
                parameterElement.SetAttribute("FieldName", (string)entry.Value);
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