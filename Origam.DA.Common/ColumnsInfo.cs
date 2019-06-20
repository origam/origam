using System.Collections.Generic;
using System.Linq;

namespace Origam.DA
{
    public class ColumnsInfo
    {
        private readonly List<ColumnData> columns = new List<ColumnData>();
        public static readonly ColumnsInfo Empty = new ColumnsInfo();

        private ColumnsInfo()
        {
        }

        public ColumnsInfo(string columnName)
        {
            if (columnName == null)
            {
                columns = new List<ColumnData>();
                return;
            }

            columns = columnName
                .Split(';')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => new ColumnData(x))
                .ToList();
        }

        public ColumnsInfo(List<ColumnData> columnNames)
        {
            columns = columnNames;
        }

        public int Count => columns.Count;
        public bool IsEmpty => Count == 0;
        public IEnumerable<string> ColumnNames => columns.Select(x => x.Name);


        public override string ToString()
        {
            return string.Join(";", columns.Select(column => column.Name));
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