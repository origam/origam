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
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel;
/// <summary>
/// Summary description for RedirectWorkflowPageAction.
/// </summary>
[SchemaItemDescription("Redirect", "Actions", "redirect.png")]
[HelpTopic("Redirect+Action")]
[ClassMetaVersion("6.0.0")]
public class RedirectWorkflowPageAction : AbstractWorkflowPageAction
{
	public RedirectWorkflowPageAction() : base() {}
	public RedirectWorkflowPageAction(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public RedirectWorkflowPageAction(Key primaryKey) : base(primaryKey)	{}
	#region Properties
	private string _xpath;
	[Category("Result")]
	[Description("An XPath expression from the context of the data returned by the workflow. The result will be used as the URL.")]
	[XmlAttribute("xPath")]
	public string XPath
	{
		get
		{
			return _xpath;
		}
		set
		{
			_xpath = value;
		}
	}
	private bool _isUrlEscaped;
	[Category("Result")]
	[Description("Indicates if the result is already a complete URL that needs no escaping.")]
	[XmlAttribute("escapeUrl")]
	public bool IsUrlEscaped
	{
		get
		{
			return _isUrlEscaped;
		}
		set
		{
			_isUrlEscaped = value;
		}
	}
	#endregion
}
