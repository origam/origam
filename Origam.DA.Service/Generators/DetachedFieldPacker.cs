using System;
using System.Collections.Generic;
using System.Linq;
using Origam.Schema.EntityModel;

namespace Origam.DA.Service
{
    public interface IDetachedFieldPacker
    {
        List<object> ProcessReaderOutput(object[] values,
            ColumnsInfo columnsInfo);

        string RenderSqlExpression(DataStructureEntity entity,
            DetachedField detachedField);
    }
    
    class DetachedFieldPackerPostgre : IDetachedFieldPacker
    {
        public List<object> ProcessReaderOutput(object[] values, ColumnsInfo columnsInfo)
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
        public List<object> ProcessReaderOutput(object[] values, ColumnsInfo columnsInfo)
        {
            if (columnsInfo == null)
                throw new ArgumentNullException(nameof(columnsInfo));
            var updatedValues = new List<object>();
            for (int i = 0; i < columnsInfo.Count; i++)
            {
                if (columnsInfo.Columns[i].IsVirtual)
                {
                    if (columnsInfo.Columns[i].HasRelation)
                    {
                        updatedValues.Add(((string) values[i]).Split((char) 1));
                        continue;
                    }
                    else
                    {
                        updatedValues.Add(columnsInfo.Columns[i].DefaultValue);
                        continue;
                    }
                }
                updatedValues.Add(values[i]);
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