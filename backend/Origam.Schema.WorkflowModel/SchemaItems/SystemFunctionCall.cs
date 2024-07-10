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
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel;
public enum SystemFunction
{
	ActiveProfileId = 0,
	ResourceIdByActiveProfile = 1
}
/// <summary>
/// Summary description for SystemFunctionCall.
/// </summary>
[SchemaItemDescription("System Function Call", "Parameters", "icon_system-function-call-ui.png")]
[HelpTopic("System+Function+Call")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class SystemFunctionCall : AbstractSchemaItem
{
	public const string CategoryConst = "SystemFunctionCall";
	public SystemFunctionCall() : base() {}
	public SystemFunctionCall(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public SystemFunctionCall(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractSchemaItem Members
	
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	#endregion
	#region Properties
	private SystemFunction _function;
	
    [XmlAttribute("function")]
	public SystemFunction Function
	{
		get
		{
			return _function;
		}
		set
		{
			_function = value;
		}
	}
	#endregion
}
