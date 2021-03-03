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
using Origam.Schema;
using Origam.Schema.EntityModel;
using ArgumentException = System.ArgumentException;

namespace Origam.DA.Service.CustomCommandParser
{
    public class FilterCommandParser: ICustomCommandParser
    {
        private readonly List<ColumnInfo> columns;
        private readonly string nameLeftBracket;
        private readonly string nameRightBracket;
        private FilterNode root = null;
        private FilterNode currentNode = null;
        private readonly string whereFilterInput;
        private readonly string parameterReferenceChar;
        private readonly Dictionary<string, string> filterColumnExpressions = new Dictionary<string, string>();
        private readonly AbstractFilterRenderer filterRenderer;
        private string sql;
        private string[] columnsNames;

        public List<ParameterData> ParameterDataList { get; } = new List<ParameterData>();
        public string Sql
        {
            get
            {
                if (sql == null && !string.IsNullOrWhiteSpace(whereFilterInput))
                {
                    var inpValue = GetCheckedInput(whereFilterInput);
                    ParseToNodeTree(inpValue);
                    sql = root.SqlRepresentation();
                }
                return sql;
            }
        }

        public string[] Columns
        {
            get {
                if (columnsNames == null)
                {
                    if (string.IsNullOrWhiteSpace(whereFilterInput))
                    {
                        columnsNames = new string[0];
                        return columnsNames;
                    }
                    var inpValue = GetCheckedInput(whereFilterInput);
                    ParseToNodeTree(inpValue);
                    columnsNames = root.AllChildren
                        .Where(node => !node.IsBinaryOperator && !node.IsValueNode)
                        .Select(node => node.ColumnName)
                        .ToArray();
                }
                return columnsNames;
            }
        }

        public FilterCommandParser(string nameLeftBracket, string nameRightBracket, 
            AbstractFilterRenderer filterRenderer, string whereFilterInput, 
            string parameterReferenceChar, List<ColumnInfo> columns)
        {
            this.nameLeftBracket = nameLeftBracket;
            this.nameRightBracket = nameRightBracket;
            this.filterRenderer = filterRenderer;
            this.whereFilterInput = whereFilterInput;
            this.parameterReferenceChar = parameterReferenceChar;
            this.columns = columns;
        }

        public FilterCommandParser(string nameLeftBracket, string nameRightBracket,
            List<DataStructureColumn> dataStructureColumns,
            AbstractFilterRenderer filterRenderer, string whereFilterInput, 
            string parameterReferenceChar)
        :this(nameLeftBracket, nameRightBracket, filterRenderer,
            whereFilterInput, parameterReferenceChar, 
            dataStructureColumns
                .Select(column => new ColumnInfo
                {
                    Name = column.Name,
                    DataType = column.DataType,
                    IsNullable = column.Field.AllowNulls
                })
                .ToList())
        {
        }
        
        public void SetColumnExpressionsIfMissing(string columnName, string[] expressions)
        {
            if (expressions == null || expressions.Length != 1)
            {
                throw new NotImplementedException("Can only handle single expression for a single column.");
            }
            filterColumnExpressions[columnName] = expressions[0];
        }

        private void ParseToNodeTree(string filter)
        {
            root = null;
            currentNode = null;
            foreach (char c in filter)
            {
                if (c == '[')
                {
                    AddNode();
                }
                else if (c == ']')
                {
                    currentNode = currentNode.Parent;
                }
                else if (currentNode.IsBinaryOperator && c == ',')
                {
                    continue;
                }
                else
                {
                    currentNode.Value += c;
                }
            }
        }

        private void AddNode()
        {
            FilterNode newNode = new FilterNode(
                nameLeftBracket, nameRightBracket,
                filterColumnExpressions, columns,
                filterRenderer, ParameterDataList, parameterReferenceChar)
            {
                Parent = currentNode
            };
            currentNode?.Children.Add(newNode);
            currentNode = newNode;
            if (root == null)
            {
                root = newNode;
            }
        }

        private static string GetCheckedInput(string strFilter)
        {
            if (strFilter == null)
            {
                throw new ArgumentException(nameof(strFilter) + " cannot be null");
            }

            string inpValue = strFilter.Trim();
            if (inpValue[0] != '[')
            {
                throw new ArgumentException("Filter input must start with \"[\", found: \"" + inpValue[0] + "\"");
            }

            if (inpValue.Last() != ']')
            {
                throw new ArgumentException("Filter input must end with \"]\", found: \"" + inpValue.Last() + "\"");
            }

            if (inpValue.Last() != ']')
            {
                throw new ArgumentException("Filter input must end with \"]\", found: \"" + inpValue.Last() + "\"");
            }

            if (inpValue.Count(x => x == ']') != inpValue.Count(x => x == ']'))
            {
                throw new ArgumentException("Filter input must contain the same number of \"[\" and \"]\", input is: \"" + inpValue + "\"");
            }

            return inpValue;
        }
    }

    public class ParameterData
    {
        public string ParameterName { get;  }
        public string ColumnName { get;  }
        public object Value { get;  }
        public OrigamDataType DataType { get; }

        public ParameterData(string parameterName, string columnName, 
            object value, OrigamDataType dataType)
        {
            ParameterName = parameterName;
            ColumnName = columnName;
            Value = value;
            DataType = dataType;
        }
    }
}
