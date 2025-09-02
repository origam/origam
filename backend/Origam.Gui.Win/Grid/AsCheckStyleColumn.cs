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
using System.Drawing;
using System.Data;
using Origam.Rule;

namespace Origam.Gui.Win;
public class AsCheckStyleColumn : DataGridBoolColumn
{
	private bool _readOnly = false;
	public override bool ReadOnly
	{
		get
		{
			return _readOnly;
		}
		set
		{
			_readOnly = value;
			base.ReadOnly = value;
		}
	}
	private bool _alwaysReadOnly = false;
	public bool AlwaysReadOnly
	{
		get
		{
			return _alwaysReadOnly;
		}
		set
		{
			_alwaysReadOnly = value;
			this.ReadOnly = value;
		}
	}
	protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
	{
		if(cellIsVisible)
		{
			if(! AlwaysReadOnly)
			{
				RuleEngine ruleEngine = (this.DataGridTableStyle.DataGrid.FindForm() as AsForm).FormGenerator.FormRuleEngine;
				if(ruleEngine != null)
				{
					this.ReadOnly = ! ruleEngine.EvaluateRowLevelSecurityState((source.Current as DataRowView).Row, this.MappingName, Schema.EntityModel.CredentialType.Update);
				}
			}
			else
			{
				this.ReadOnly = true;
			}
		}
		base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
	}
}
