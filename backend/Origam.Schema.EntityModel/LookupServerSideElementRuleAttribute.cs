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
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel.Interfaces;
using System;
using System.Data;

namespace Origam.Schema.EntityModel
{
    public class LookupServerSideElementRuleAttribute : AbstractModelElementRuleAttribute
    {
        public override Exception CheckRule(object instance)
        {
            return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
        }

        public override Exception CheckRule(object instance, string memberName)
        {
            if (memberName == String.Empty | memberName == null) CheckRule(instance);
            var iDataLookup = ((ILookupReference)instance).Lookup;

            if(iDataLookup == null || iDataLookup.IsFilteredServerside)
            {
                return null;
            }
            return new DataException("Lookup has to have property isFilteredServerSide on true.");
        }
    }
}
