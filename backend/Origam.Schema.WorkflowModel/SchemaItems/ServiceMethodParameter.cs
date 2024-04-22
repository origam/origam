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

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Parameter that can be used to parametrize any kind of schema item.
/// </summary>
[SchemaItemDescription("Parameter", "Parameters", "parameter-blm.png")]
[HelpTopic("Service+Method+Parameter")]
[ClassMetaVersion("6.0.0")]
public class ServiceMethodParameter : SchemaItemParameter
{
	public ServiceMethodParameter() : base() {}
	public ServiceMethodParameter(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public ServiceMethodParameter(Key primaryKey) : base(primaryKey)	{}

	enum CallElementsEnum
	{
		ContextReference = 1,
		DataConstantReference = 2,
		DataStructureReference = 4,
		TransformationReference = 8,
		ReportReference = 16,
		WorkflowReference = 32,
		SystemFunctionCall = 64
	}

	private bool Resolve(CallElementsEnum element)
	{
			return (CallElements & (int)element) == (int)element;
		}

	private void Set(CallElementsEnum element, bool value)
	{
			bool current = Resolve(element);
			if(current == value) return;

			if(value)
			{
				CallElements += (int)element;
			}
			else
			{
				CallElements -= (int)element;
			}
		}

	#region Properties
	private int _callElements;
	[Browsable(false)]
	[XmlAttribute("callElements")]
	public int CallElements
	{
		get
		{
				return _callElements;
			}
		set
		{
				_callElements = value;
			}
	}

	[DefaultValue(false),
	 Category("Allowed Child Elements")]
	[XmlAttribute("allowContextReference")]
	public bool AllowContextReference
	{
		get
		{
				return Resolve(CallElementsEnum.ContextReference);
			}
		set
		{
				Set(CallElementsEnum.ContextReference, value);
			}
	}

	[DefaultValue(false),
	 Category("Allowed Child Elements")]
	[XmlAttribute("allowDataConstantReference")]
	public bool AllowDataConstantReference
	{
		get
		{
				return Resolve(CallElementsEnum.DataConstantReference);
			}
		set
		{
				Set(CallElementsEnum.DataConstantReference, value);
			}
	}

	[DefaultValue(false),
	 Category("Allowed Child Elements")]
	[XmlAttribute("allowDataStructureReference")]
	public bool AllowDataStructureReference
	{
		get
		{
				return Resolve(CallElementsEnum.DataStructureReference);
			}
		set
		{
				Set(CallElementsEnum.DataStructureReference, value);
			}
	}
		
	[DefaultValue(false),
	 Category("Allowed Child Elements")]
	[XmlAttribute("allowReportReference")]
	public bool AllowReportReference
	{
		get
		{
				return Resolve(CallElementsEnum.ReportReference);
			}
		set
		{
				Set(CallElementsEnum.ReportReference, value);
			}
	}

	[DefaultValue(false),
	 Category("Allowed Child Elements")]
	[XmlAttribute("allowSystemFunctionCall")]
	public bool AllowSystemFunctionCall
	{
		get
		{
				return Resolve(CallElementsEnum.SystemFunctionCall);
			}
		set
		{
				Set(CallElementsEnum.SystemFunctionCall, value);
			}
	}

	[DefaultValue(false),
	 Category("Allowed Child Elements")]
	[XmlAttribute("allowTransformationReference")]
	public bool AllowTransformationReference
	{
		get
		{
				return Resolve(CallElementsEnum.TransformationReference);
			}
		set
		{
				Set(CallElementsEnum.TransformationReference, value);
			}
	}

	[DefaultValue(false),
	 Category("Allowed Child Elements")]
	[XmlAttribute("allowWorkflowReference")]
	public bool AllowWorkflowReference
	{
		get
		{
				return Resolve(CallElementsEnum.WorkflowReference);
			}
		set
		{
				Set(CallElementsEnum.WorkflowReference, value);
			}
	}
	#endregion
}