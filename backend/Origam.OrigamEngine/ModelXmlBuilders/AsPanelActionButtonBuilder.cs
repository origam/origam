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

/// <summary>
/// Summary description for AsPanelActionButtonBuilder.
/// </summary>
public class AsPanelActionButtonBuilder
{
	public static void Build(XmlElement actionsElement, PanelActionType type, PanelActionMode mode,
		ActionButtonPlacement placement, string actionId, string groupId, string caption,
		string iconUrl, bool isDefault, Hashtable parameters, string confirmationMessage)
	{
			Build(actionsElement, type, mode, placement, actionId, groupId, caption, iconUrl, isDefault, parameters,
				false, false, false, 0, "", 0, confirmationMessage);
		}

	public static void Build(XmlElement actionsElement, PanelActionType type, PanelActionMode mode,
		ActionButtonPlacement placement, string actionId, string groupId, string caption,
		string iconUrl, bool isDefault, Hashtable parameters, bool shortcutIsShift, 
		bool shortcutIsControl,	bool shortcutIsAlt, int shortcutKeyCode, string scannerParameter,
		int terminatorCharCode, string confirmationMessage)
	{
			XmlElement actionElement = actionsElement.OwnerDocument.CreateElement("Action");
			actionsElement.AppendChild(actionElement);

			actionElement.SetAttribute("Type", type.ToString());
			actionElement.SetAttribute("Id", actionId);
			actionElement.SetAttribute("GroupId", groupId);
			actionElement.SetAttribute("Caption", caption);
			actionElement.SetAttribute("IconUrl", iconUrl);
			actionElement.SetAttribute("Mode", mode.ToString());
			actionElement.SetAttribute("IsDefault", XmlConvert.ToString(isDefault));
			actionElement.SetAttribute("Placement", placement.ToString());

			if(confirmationMessage != null)
			{
				actionElement.SetAttribute("ConfirmationMessage", confirmationMessage);
			}

			if(shortcutKeyCode != 0)
			{
				XmlElement shortcutElement = actionElement.OwnerDocument.CreateElement("KeyboardShortcut");
				actionElement.AppendChild(shortcutElement);

				shortcutElement.SetAttribute("Ctrl", XmlConvert.ToString(shortcutIsControl));
				shortcutElement.SetAttribute("Shift", XmlConvert.ToString(shortcutIsShift));
				shortcutElement.SetAttribute("Alt", XmlConvert.ToString(shortcutIsAlt));
				shortcutElement.SetAttribute("KeyCode", XmlConvert.ToString(shortcutKeyCode));
			}

			if(terminatorCharCode != 0)
			{
				actionElement.SetAttribute("ScannerInputTerminator", XmlConvert.ToString(terminatorCharCode));
			}

			if(scannerParameter != "" && scannerParameter != null)
			{
				actionElement.SetAttribute("ScannerInputParameterName", scannerParameter);
			}

			if(parameters.Count > 0)
			{
				XmlElement parametersElement = actionElement.OwnerDocument.CreateElement("Parameters");
				actionElement.AppendChild(parametersElement);

				foreach(DictionaryEntry entry in parameters)
				{
					XmlElement parameterElement = parametersElement.OwnerDocument.CreateElement("Parameter");
					parametersElement.AppendChild(parameterElement);

					parameterElement.SetAttribute("Name", (string)entry.Key);
					parameterElement.SetAttribute("FieldName", (string)entry.Value);
				}
			}
		}
}