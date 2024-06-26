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
using System.Data;
using System.Linq;
using Origam.DA.ObjectPersistence;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel;
[AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
public class NoDuplicateNamesInDataConstantRuleAtribute : AbstractModelElementRuleAttribute 
{
    public NoDuplicateNamesInDataConstantRuleAtribute()
    {
    }
    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
    }
    public override Exception CheckRule(object instance, string memberName)
    {
        if(string.IsNullOrEmpty(memberName)) CheckRule(instance);
        if(memberName != "Name") throw new Exception(nameof(NoDuplicateNamesInDataConstantRuleAtribute) +" can be only applied to Name properties");  
        if (!(instance is DataConstant dataconstant)) return null;
        if (dataconstant.RootProvider == null)
        {
            
        }
        string instanceName = (string)Reflector.GetValue(instance.GetType(), instance, memberName);
        var itemWithDuplicateName = dataconstant
            .RootProvider.ChildItems
            .ToGeneric()
            .Where(item => item is DataConstant)
            .Where(item => item.Name == instanceName)
            .FirstOrDefault(item => item.Id != dataconstant.Id);
        if (itemWithDuplicateName != null)
        {
            return new DataException(dataconstant.Name+" contains duplicate  names ");
        }
        return null;
    }
}
