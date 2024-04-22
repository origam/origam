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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;


namespace Origam.Schema.GuiModel;

[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public abstract class AbstractChart : AbstractSchemaItem
{
	public const string CategoryConst = "Chart";

	public AbstractChart() : base() {Init();}
	public AbstractChart(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public AbstractChart(Key primaryKey) : base(primaryKey) {Init();}

	private void Init()
	{
			this.ChildItemTypes.Add(typeof(ChartFormMapping));
		}

	#region Properties
	private string _caption = "";
	[Category("User Interface")]
	[StringNotEmptyModelElementRule()]
	[Localizable(true)]
	[XmlAttribute("label")]
	public string Caption
	{
		get
		{
				return _caption;
			}
		set
		{
				_caption = value;
			}
	}

	public override string ItemType
	{
		get
		{
				return CategoryConst;
			}
	}
	#endregion			
}