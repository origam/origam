#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.Linq;
using Origam.Schema.EntityModel;

namespace Origam.DA.Service
{
    public interface IDetachedFieldPacker
    {
        List<KeyValuePair<string, object>> ProcessReaderOutput(KeyValuePair<string, object>[] values,
            List<ColumnData> columnData);

        string RenderSqlExpression(DataStructureEntity entity,
            DetachedField detachedField);
    }
    
    class DetachedFieldPackerPostgre : IDetachedFieldPacker
    {
        public List<KeyValuePair<string, object>> ProcessReaderOutput(
            KeyValuePair<string, object>[] values, List<ColumnData> columnData)
        {
            throw new NotImplementedException();
        }

        public string RenderSqlExpression(DataStructureEntity entity,
            DetachedField detachedField)
        {
            throw new NotImplementedException();
        }
    }

    public class DetachedFieldPackerMs : IDetachedFieldPacker
    {
        public List<KeyValuePair<string, object>> ProcessReaderOutput(
            KeyValuePair<string, object>[] values, List<ColumnData> columnData)
        {
            if (columnData == null)
                throw new ArgumentNullException(nameof(columnData));
            var updatedValues = new List<KeyValuePair<string, object>>();
            
            for (int i = 0; i < columnData.Count; i++)
            {
                if (columnData[i].IsVirtual)
                {
                    if (columnData[i].HasRelation && values[i].Value != null && values[i].Value != DBNull.Value)
                    {
                        updatedValues.Add(new KeyValuePair<string, object>(
                            values[i].Key, ((string) values[i].Value).Split((char) 1)));
                        continue;
                    }
                    else
                    {
                        updatedValues.Add(new KeyValuePair<string, object>
                            (values[i].Key, columnData[i].DefaultValue));
                        continue;
                    }
                }
                updatedValues.Add(new KeyValuePair<string, object>(
                    values[i].Key, values[i].Value));
            }

            return updatedValues;
        }

        public string RenderSqlExpression(DataStructureEntity entity,
            DetachedField detachedField)
        {
            if (detachedField.ArrayRelation == null)
            {
                return "";
            }

            var relationPairItem = detachedField.ArrayRelation.ChildItems
                                       .ToGeneric()
                                       .OfType<EntityRelationColumnPairItem>()
                                       .SingleOrDefault()
                                   ?? throw new InvalidOperationException(
                                       $"Relation {detachedField.ArrayRelation.Id} does not have exactly one {nameof(EntityRelationColumnPairItem)}");
            return
                $"(SELECT STRING_AGG(CAST([{detachedField.ArrayValueField.Name}] as varchar(max)), CHAR(1)) " +
                $"FROM [{detachedField.ArrayRelation.Name}] " +
                $"Where {relationPairItem.RelatedEntityField.Name} = [{entity.Name}].[{relationPairItem.BaseEntityField.Name}]) ";
        }
    }
}