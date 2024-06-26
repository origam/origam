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
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;

namespace Origam.DA.EntityModel;
[AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
public class NoLengthLimitOnMemoFieldRule : AbstractModelElementRuleAttribute 
{
    public override Exception CheckRule(object instance)
    {
        if (!(instance is AbstractDataEntityColumn dataEntityColumn))
        {
            throw new Exception(
                $"{nameof(NoLengthLimitOnMemoFieldRule)} can be only applied to type {nameof(AbstractDataEntityColumn)}");  
        }
        if (dataEntityColumn.DataType == OrigamDataType.Memo &&
            dataEntityColumn.DataLength != 0)
        {
            return new Exception(
                $"{nameof(AbstractDataEntityColumn.DataLength)} cannot be se to anything other than 0 if the {nameof(AbstractDataEntityColumn.DataType)} is {nameof(OrigamDataType.Memo)}. The memo length is unlimited by definition.");
        }
        return null;
    }
    public override Exception CheckRule(object instance, string memberName)
    {
        return CheckRule(instance);
    }
}
