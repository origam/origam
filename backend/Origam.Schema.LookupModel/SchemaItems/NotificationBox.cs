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

using Origam.DA.Common;
using System;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.LookupModel;

[SchemaItemDescription("Notification Box", "icon_notification-box.png")]
[HelpTopic("Notification+Boxes+And+Tooltips")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class NotificationBox : AbstractSchemaItem
{
	public const string CategoryConst = "NotificationBox";

	public NotificationBox() {}

	public NotificationBox(Guid schemaExtensionId) 
		: base(schemaExtensionId) {}

	public NotificationBox(Key primaryKey) : base(primaryKey) {}
	
	#region Properties
	private NotificationBoxType _type = NotificationBoxType.Logo;
	[Description("One of the predefined notification box types.")]
	[XmlAttribute("type")]
	public NotificationBoxType Type
	{
		get => _type;
		set => _type = value;
	}

	private int _refreshInterval = 0;
	[Description("Refresh interval in seconds.")]
	[XmlAttribute("refreshInterval")]
	public int RefreshInterval
	{
		get => _refreshInterval;
		set => _refreshInterval = value;
	}
	#endregion

	#region Overriden AbstractDataEntityColumn Members
	public override string ItemType => CategoryConst;

	public override bool UseFolders => false;

	#endregion

	#region ISchemaItemFactory Members

	public override Type[] NewItemTypes => new[] 
		{
			typeof(DataServiceDataTooltip)
		};

	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
			return base.NewItem<T>(schemaExtensionId, group, 
				typeof(T) == typeof(DataServiceDataTooltip) ?
					"NewDataServiceDataTooltip" : null);
		}

	#endregion
}