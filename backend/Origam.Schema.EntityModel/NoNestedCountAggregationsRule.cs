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
using Origam.Schema.EntityModel;

namespace Origam.DA.EntityModel
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public class NoNestedCountAggregationsRule : AbstractModelElementRuleAttribute 
    {
        public override Exception CheckRule(object instance)
        {
            if (!(instance is AggregatedColumn aggregatedColumn))
            {
                throw new Exception(
                    $"{nameof(NoNestedCountAggregationsRule)} can be only applied to type {nameof(AggregatedColumn)}");  
            }
            if (!(aggregatedColumn.Field is AggregatedColumn referencedColumn))
            {
                return null;
            }

            if (aggregatedColumn.AggregationType != referencedColumn.AggregationType &&
                aggregatedColumn.AggregationType != AggregationType.Sum && 
                referencedColumn.AggregationType != AggregationType.Count ||
                aggregatedColumn.AggregationType == AggregationType.Count && 
                referencedColumn.AggregationType == AggregationType.Count)
            {
                return new Exception(
                    $"Nested aggregation error. Column must have the property {nameof(AggregationType)} set to the same\n" +
                           $"value as {nameof(AggregationType)} of the field it references \"{referencedColumn.Name}\"\n" +
                           "The only exception is a combination of Count(Count()) which is not allowed and should be\n" +
                           "replaced by Sum(Count()) to calculate the total count.");
            }
            
            return null;
        }

        public override Exception CheckRule(object instance, string memberName)
        {
            return CheckRule(instance);
        }
    }
}