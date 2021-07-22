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

using System.Windows.Forms;

using Origam.Schema.EntityModel;

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for FilterMenuItem.
	/// </summary>
	public class FilterMenuItem : MenuItem
	{
		public FilterMenuItem(string name) : base(name)	{}

		private OrigamPanelFilter.PanelFilterRow _filter;
		public OrigamPanelFilter.PanelFilterRow Filter
		{
			get
			{
				return _filter;
			}
			set
			{
				_filter = value;
			}
		}
	}

	public class TemplateMenuItem : MenuItem
	{
		public TemplateMenuItem(string name) : base(name)	{}

		public TemplateMenuItem(string name, DataStructureTemplate template) : base(name)
		{
			this.Template = template;
		}

		private DataStructureTemplate _template;
		public DataStructureTemplate Template
		{
			get
			{
				return _template;
			}
			set
			{
				_template = value;
			}
		}
	}
}
