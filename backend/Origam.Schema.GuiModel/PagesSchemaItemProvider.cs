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

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class PagesSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory
	{
		public PagesSchemaItemProvider() 
		{
			this.ChildItemTypes.Add(typeof(XsltDataPage));
			this.ChildItemTypes.Add(typeof(FileDownloadPage));
			this.ChildItemTypes.Add(typeof(ReportPage));
		}
		
		#region ISchemaItemProvider Members
		public override string RootItemType
		{
			get
			{
				return AbstractPage.CategoryConst;
			}
		}
		public override string Group
		{
			get
			{
				return "API";
			}
		}
		#endregion

		#region IBrowserNode Members

		public override string Icon
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
				return "icon_36_web-api-pages.png";
			}
		}

		public override string NodeText
		{
			get
			{
				return "Web API/Pages";
			}
			set
			{
				base.NodeText = value;
			}
		}

		public override string NodeToolTipText
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.NodeToolTipText getter implementation
				return "List of Pages";
			}
		}

		#endregion
	}
}
