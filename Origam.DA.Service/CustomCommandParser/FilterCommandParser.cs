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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using ArgumentException = System.ArgumentException;

namespace Origam.DA.Service.CustomCommandParser
{
    public class FilterCommandParser: ICustomCommandParser
    {
        private readonly string nameLeftBracket;
        private readonly string nameRightBracket;
        private readonly SQLValueFormatter sqlValueFormatter;
        private Node root = null;
        private Node currentNode = null;

        private readonly string whereFilterInput;

        private readonly Dictionary<string, string> filterColumnExpressions = new Dictionary<string, string>();
        private readonly Dictionary<string, OrigamDataType> columnNameToType = new Dictionary<string, OrigamDataType>();
        private readonly AbstractFilterRenderer filterRenderer;
        private string sql;
        private string[] columns;
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
                if (columns == null)
                {
                    if (string.IsNullOrWhiteSpace(whereFilterInput))
                    {
                        columns = new string[0];
                        return columns;
                    }
                    var inpValue = GetCheckedInput(whereFilterInput);
                    ParseToNodeTree(inpValue);
                    columns = root.AllChildren
                        .Where(node => !node.IsBinaryOperator && !node.IsValueNode)
                        .Select(node => node.ColumnName)
                        .ToArray();
                }
                return columns;
            }
        }


        public FilterCommandParser(string nameLeftBracket, string nameRightBracket, 
            SQLValueFormatter sqlValueFormatter, AbstractFilterRenderer filterRenderer, string whereFilterInput)
        {
            this.nameLeftBracket = nameLeftBracket;
            this.nameRightBracket = nameRightBracket;
            this.sqlValueFormatter = sqlValueFormatter;
            this.filterRenderer = filterRenderer;
            this.whereFilterInput = whereFilterInput;
        }

        public FilterCommandParser(string nameLeftBracket, string nameRightBracket,
            SQLValueFormatter sqlValueFormatter, List<DataStructureColumn> dataStructureColumns,
            AbstractFilterRenderer filterRenderer, string whereFilterInput)
        :this(nameLeftBracket, nameRightBracket, sqlValueFormatter, filterRenderer, whereFilterInput)
        {
            foreach (var column in dataStructureColumns)
            {
                AddDataType(column.Name, column.DataType); 
            }
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
            Node newNode = new Node(
                nameLeftBracket, nameRightBracket,
                filterColumnExpressions, sqlValueFormatter, columnNameToType,
                filterRenderer)
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
        public void AddDataType(string columnName, OrigamDataType columnDataType)
        {
            if (columnNameToType.ContainsKey(columnName))
            {
                throw new Exception(string.Format("Dupliacate column: {0}", columnName));
            }
            columnNameToType.Add(columnName, columnDataType);
        }
    }

    class Node
    {
        private readonly string nameLeftBracket;
        private readonly string nameRightBracket;
        private readonly Dictionary<string, string> lookupExpressions;
        private readonly Dictionary<string, OrigamDataType> columnNameToType;
        private readonly SQLValueFormatter sqlValueFormatter;
        private string[] splitValue;
        public Node Parent { get; set; }
        public List<Node> Children { get; } = new List<Node>();
        public string Value { get; set; } = "";
        public bool IsBinaryOperator => Value.Contains("$");

        public IEnumerable<Node> AllChildren
        {
            get
            {
                yield return this;
                foreach (var child in Children)
                {
                    foreach (var child1 in child.AllChildren)
                    {
                        yield return child1;
                    }
                }
            }
        }

        private bool ContainsNumbersOnly(string value)
        {
            return Regex.Match(value, "^[\\d,\\s]+$").Success;
        }

        private string[] SplitValue
        {
            get
            {
                if (splitValue == null)
                {
                    splitValue = ContainsNumbersOnly(Value) 
                        ? Value.Split(',') 
                        : Regex.Split(Value, "[\\]\"]\\s*,\\s*[\\[\"]?");
                    splitValue = splitValue
                        .Select(x => x.Trim())
                        .ToArray();
                }

                return splitValue;
            }
        }

        internal string ColumnName => SplitValue[0].Replace("\"", "");

        private string RenderedColumnName =>
            lookupExpressions.ContainsKey(ColumnName)
                ? lookupExpressions[ColumnName]
                : nameLeftBracket + ColumnName + nameRightBracket;


        private string Operator => SplitValue?.Length > 1 
            ? SplitValue[1].Replace("\"","") 
            : null;
        private string ColumnValue => ValueToOperand(SplitValue[2]);

        private OrigamDataType DataType
        {
            get
            {
                if (columnNameToType.ContainsKey(ColumnName))
                {
                    return columnNameToType[ColumnName];
                }
                throw new Exception($"Data type of column \"{ColumnName}\" is unknown");
            }
        }

        public bool IsValueNode => Parent != null && 
                                   (Parent.Operator == "in" ||
                                   Parent.Operator == "nin" ||
                                   Parent.Operator == "between" ||
                                   Parent.Operator == "nbetween");

        private readonly AbstractFilterRenderer renderer;

        public Node(string nameLeftBracket, string nameRightBracket, Dictionary<string,string> lookupExpressions, 
            SQLValueFormatter sqlValueFormatter, Dictionary<string, OrigamDataType> columnNameToType,
            AbstractFilterRenderer filterRenderer)
        {
            this.nameLeftBracket = nameLeftBracket;
            this.nameRightBracket = nameRightBracket;
            this.lookupExpressions = lookupExpressions;
            this.sqlValueFormatter = sqlValueFormatter;
            this.columnNameToType = columnNameToType;
            this.renderer = filterRenderer;
        }

        private string ValueToOperand(string value)
        {
            return sqlValueFormatter.Format(DataType, value.Replace("\"", ""), Operator);
        }

        private bool IsString(string value)
        {
            string columnValue = string.Join(",", value.Split(',').Skip(2));
            return columnValue.Contains("\"");
        }
        
        private bool ContainsIsoDates(string value)
        {
            var columnValues = value
                .Split(',')
                .Skip(2)
                .Select(x => x.Replace("\"", "").Trim());

            return columnValues
                .All(colValue =>
                    DateTime.TryParseExact(colValue, "yyyy-MM-ddTHH:mm:ss.fff",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                );
        }

        public string SqlRepresentation()
        {
            if (IsBinaryOperator)
            {
                return GetSqlOfOperatorNode();
            }
            else
            {
                return GetSqlOfLeafNode();
            }
        }

        private string GetSqlOfOperatorNode()
        {
            string logicalOperator = GetLogicalOperator();
            List<string> operands = Children
                .Select(node => node.SqlRepresentation()).ToList();
            return renderer.LogicalAndOr(logicalOperator, operands);
        }

        private string GetLogicalOperator()
        {
            if (Value.Trim() == "\"$AND\"") return "AND";
            if (Value.Trim() == "\"$OR\"") return "OR";
            throw new Exception("Could not parse node value to logical operator: \"" + Value+"\"");
        }

        private string GetSqlOfLeafNode()
        {
            var (operatorName, renderedColumnValue) = GetRendererInput(Operator, ColumnValue);    
            if (Children.Count == 0)
            {
                if (SplitValue.Length != 3)
                {
                    throw new ArgumentException("could not parse: " + Value + " to a filter node");
                }

                return renderer.BinaryOperator(
                    leftValue: RenderedColumnName, 
                    rightValue: renderedColumnValue, 
                    operatorName: operatorName);
            }

            if (Children.Count == 1 && 
                (Operator == "in" ||  Operator == "nin" ||  Operator == "between" ||  Operator == "nbetween") )
            {
                string[] rightHandValues = Children.First()
                    .SplitValue
                    .Select(ValueToOperand)
                    .ToArray();
                return renderer.BinaryOperator(
                    columnName: ColumnName,
                    leftValue: RenderedColumnName, 
                    rightValues: rightHandValues, 
                    operatorName: operatorName,
                    isColumnArray: columnNameToType[ColumnName] 
                                   == OrigamDataType.Array);
            }

            throw new Exception("Cannot parse filter node: " + Value + ". If this should be a binary operator prefix it with \"$\".");
        }
        
        private (string,string) GetRendererInput(string operatorName, string value)
        {
            switch (operatorName)
            {
                case "gt": return ("GreaterThan", value);
                case "lt": return ("LessThan", value);
                case "gte": return ("GreaterThanOrEqual", value);
                case "lte": return ("LessThanOrEqual", value);
                case "eq": return ("Equal", value);
                case "neq": return ("NotEqual", value);
                case "starts": return ("Like", appendWildCard(value));
                case "nstarts": return ("NotLike", appendWildCard(value));
                case "ends": return ("Like", prependWildCard(value));
                case "nends": return ("NotLike",  prependWildCard(value));
                case "like":
                case "contains": return ("Like", prependWildCard(appendWildCard(value)));
                case "ncontains": return ("NotLike", prependWildCard(appendWildCard(value)));
                case "null": return ("Equal", null);
                case "nnull": return ("NotEqual", null);
                case "between": return ("Between", null);
                case "nbetween": return ("NotBetween", null);
                case "in": return ("In", null);
                case "nin": return ("NotIn", null);
                default: throw new NotImplementedException(operatorName);
            }
        }

        private string prependWildCard(string value)
        {
            if (!value.StartsWith("'"))
            {
                throw new ArgumentException("Cannot prepend \"%\" to a value which does not start with \"'\" (is not string)");
            }
            return value.Substring(0,1) + "%" +value.Substring(1);
        } 
        private string appendWildCard(string value)
        {
            if (!value.EndsWith("'"))
            {
                throw new ArgumentException("Cannot prepend \"%\" to a value which does not end with \"'\" (is not string)");
            }
            return value.Substring(0,value.Length-1) + "%" +value.Substring(value.Length-1);
        }
    }
}
