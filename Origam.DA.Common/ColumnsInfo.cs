using System.Collections.Generic;
using System.Linq;

namespace Origam.DA
{
    public class ColumnsInfo
    {
        public static readonly ColumnsInfo Empty = new ColumnsInfo();
        public bool RenderSqlForDetachedFields { get; }

        private ColumnsInfo()
        {
        }
        
        public ColumnsInfo(string columnName): this(columnName, false)
        {
        }
        
        public ColumnsInfo(string columnName, bool renderSqlForDetachedFields)
        {
            RenderSqlForDetachedFields = renderSqlForDetachedFields;
            if (columnName == null)
            {
                Columns = new List<ColumnData>();
                return;
            }

            Columns = columnName
                .Split(';')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => new ColumnData(x))
                .ToList();
        }

        public ColumnsInfo(List<ColumnData> columns, bool renderSqlForDetachedFields)
        {
            Columns = columns.ToList();
            RenderSqlForDetachedFields = renderSqlForDetachedFields;
        }
        

        public List<ColumnData> Columns { get; } = new List<ColumnData>();

        public int Count => Columns.Count;
        public bool IsEmpty => Count == 0;
        public List<string> ColumnNames => Columns.Select(x => x.Name).ToList();


        public override string ToString()
        {
            return string.Join(";", ColumnNames);
        }
    }
    
    public class ColumnData
    {
        public ColumnData(string name, bool isVirtual)
        {
            Name = name;
            IsVirtual = isVirtual;
        }

        public ColumnData(string name)
        {
            Name = name;
        }

        public string Name { get;}
        public bool IsVirtual { get;  }
    }
}