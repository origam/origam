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

using Origam.Schema.EntityModel;
using System.Collections;
using System.Collections.Generic;
using Origam.Schema.EntityModel.Interfaces;

namespace Origam.Schema.RuleModel;
/// <summary>
/// Summary description for RuleSchemaItemProvider.
/// </summary>
public class RuleSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory, IRuleSchemaItemProvider
{
	public RuleSchemaItemProvider()
	{
        this.ChildItemTypes.Add(typeof(StartRule));
        this.ChildItemTypes.Add(typeof(EndRule));
        this.ChildItemTypes.Add(typeof(EndRuleLookupXPath));
        this.ChildItemTypes.Add(typeof(EntityRule));
        this.ChildItemTypes.Add(typeof(ComplexDataRule));
        this.ChildItemTypes.Add(typeof(SimpleDataRule));
    }
	#region ISchemaItemProvider Members
	public override string RootItemType
	{
		get
		{
			return AbstractRule.CategoryConst;
		}
	}
	public override bool AutoCreateFolder
	{
		get
		{
			return true;
		}
	}
	public override string Group
	{
		get
		{
			return "BL";
		}
	}
    public List<IStartRule> StartRules
    {
        get
        {
            var result = new List<IStartRule>();
            foreach (AbstractRule rule in this.ChildItems)
            {
                if(rule is StartRule startRule)
                {
                    result.Add(startRule);
                }
            }
            return result;
        }
    }
    public List<IEndRule> EndRules
    {
        get
        {
            var result = new List<IEndRule>();
            foreach (AbstractRule rule in this.ChildItems)
            {
                if (rule is IEndRule endRule)
                {
                    result.Add(endRule);
                }
            }
            return result;
        }
    }
    public List<IDataRule> DataRules
    {
        get
        {
            var result = new List<IDataRule>();
            foreach (AbstractRule rule in this.ChildItems)
            {
                if (rule is IDataRule dataRule)
                {
                    result.Add(dataRule);
                }
            }
            return result;
        }
    }
    public List<IEntityRule> EntityRules
    {
        get
        {
            var result = new List<IEntityRule>();
            foreach (AbstractRule rule in this.ChildItems)
            {
                if (rule is EntityRule entityRule)
                {
                    result.Add(entityRule);
                }
            }
            return result;
        }
    }
    #endregion
	#region IBrowserNode Members
	public override string Icon
	{
		get
		{
			// TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
			return "icon_27_rules.png";
		}
	}
	public override string NodeText
	{
		get
		{
			return "Rules";
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
			return null;
		}
	}
	#endregion
}
