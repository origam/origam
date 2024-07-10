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
using System.Collections.Generic;
using Origam.DA.ObjectPersistence;

/// <summary>
/// Is defined on DataStructureRuleSetReference.RuleSet field.
/// Chekcs the root ruleset for recursion (whether there aren't duplicate rules)
/// </summary>
namespace Origam.Schema.EntityModel;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class RuleSetRuleUniqnessModelElementRuleAttribute : AbstractModelElementRuleAttribute
{
    public RuleSetRuleUniqnessModelElementRuleAttribute()
    {
    }
    public override Exception CheckRule(object instance)
    {
        DataStructureRuleSetReference currentRuleSetReference = instance as DataStructureRuleSetReference;
        if (currentRuleSetReference.RuleSet == null)
        {
            return null;
        }
        
        // get the datastructure 
        DataStructure ds = currentRuleSetReference.RootItem as DataStructure;
        HashSet<Guid> ruleSetUniqIds = new HashSet<Guid>();
        // examine all root rulesets for circural ruleset references
        foreach (DataStructureRuleSet ruleSet in ds.RuleSets)
        {
            try
            {
                ruleSet.AddUniqueRuleSetIds(ruleSetUniqIds, currentRuleSetReference);
            }
            catch (Exception ex)
            {
                return ex;
            }    
            // reset before checking next root ruleset
            ruleSetUniqIds.Clear();
        }
        return null;
    }
    public override Exception CheckRule(object instance, string memberName)
    {
        return CheckRule(instance);
    }
}
