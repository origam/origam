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
        private Node root = null;
        private Node currentNode = null;

        private readonly string whereFilterInput;
        private readonly string parameterReferenceChar;

        private readonly Dictionary<string, string> filterColumnExpressions = new Dictionary<string, string>();
        private readonly Dictionary<string, OrigamDataType> columnNameToType = new Dictionary<string, OrigamDataType>();
        private readonly AbstractFilterRenderer filterRenderer;
        private string sql;
        private string[] columns;

        public List<ParameterData> ParameterDataList { get; set; } = new List<ParameterData>();
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
            AbstractFilterRenderer filterRenderer, string whereFilterInput, 
            string parameterReferenceChar)
        {
            this.nameLeftBracket = nameLeftBracket;
            this.nameRightBracket = nameRightBracket;
            this.filterRenderer = filterRenderer;
            this.whereFilterInput = whereFilterInput;
            this.parameterReferenceChar = parameterReferenceChar;
        }

        public FilterCommandParser(string nameLeftBracket, string nameRightBracket,
            List<DataStructureColumn> dataStructureColumns,
            AbstractFilterRenderer filterRenderer, string whereFilterInput, 
            string parameterReferenceChar)
        :this(nameLeftBracket, nameRightBracket, filterRenderer,
            whereFilterInput, parameterReferenceChar)
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
                filterColumnExpressions, columnNameToType,
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

        private OrigamDataType ColumnDataType
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
        private readonly List<ParameterData> parameterDataList;
        private readonly string parameterReferenceChar;

        public Node(string nameLeftBracket, string nameRightBracket, Dictionary<string,string> lookupExpressions, 
            Dictionary<string, OrigamDataType> columnNameToType,
            AbstractFilterRenderer filterRenderer, List<ParameterData> parameterDataList,
            string parameterReferenceChar)
        {
            this.nameLeftBracket = nameLeftBracket;
            this.nameRightBracket = nameRightBracket;
            this.lookupExpressions = lookupExpressions;
            this.columnNameToType = columnNameToType;
            renderer = filterRenderer;
            this.parameterDataList = parameterDataList;
            this.parameterReferenceChar = parameterReferenceChar;
        }

        private string GetParameterName(string columnName, object value)
        {
            return parameterReferenceChar + columnName;
        }

        private string ValueToOperand(string value)
        {
            return value.Replace("\"", "");
        }

        private object ToDbValue(string value, OrigamDataType dataType)
        {
            switch(dataType)
            {
                case OrigamDataType.Integer:
                    if (!int.TryParse(value, out var intValue))
                    {
                        throw new ArgumentOutOfRangeException($"Cannot parse \"{value}\" to int");
                    }
                    return intValue;
                case OrigamDataType.Currency:
                    if (!decimal.TryParse(value, out var decimalValue))
                    {
                        throw new ArgumentOutOfRangeException($"Cannot parse \"{value}\" to decimal");
                    }
                    return decimalValue;
                case OrigamDataType.Float:
                    if (!float.TryParse(value, out var floatValue))
                    {
                        throw new ArgumentOutOfRangeException($"Cannot parse \"{value}\" to float");
                    }
                    return floatValue;
                case OrigamDataType.Boolean:
                    if (!bool.TryParse(value, out var boolValue))
                    {
                        throw new ArgumentOutOfRangeException($"Cannot parse \"{value}\" to bool");
                    }
                    return boolValue;
                case OrigamDataType.Date:
                    if (string.IsNullOrEmpty(Convert.ToString(value)))
                    {
                        return  null;
                    }
                    if (!DateTime.TryParse(value, out var dateValue))
                    {
                        throw new ArgumentOutOfRangeException($"Cannot parse \"{value}\" to DateTime");
                    }
                    return dateValue;
                case OrigamDataType.UniqueIdentifier:
                    if(value == null)
                    {
                       return Guid.Empty;
                    }
                    if (!Guid.TryParse(value, out Guid guidValue))
                    {
                        throw new ArgumentOutOfRangeException($"Cannot parse \"{value}\" to Guid");
                    }
                    return guidValue;
            }
            return value;
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

        private string GetSqlOfLeafNode(){

            string parameterName = GetParameterName(ColumnName, ColumnValue);
            var (operatorName, renderedColumnValue) =
                GetRendererInput(Operator, parameterName);    
            if (Children.Count == 0)
            {
                if (SplitValue.Length != 3)
                {
                    throw new ArgumentException("could not parse: " + Value + " to a filter node");
                }

                if (ColumnValue == null || ColumnValue.ToLower() == "null")
                {
                    return renderer.BinaryOperator(
                        leftValue: RenderedColumnName, 
                        rightValue: null, 
                        operatorName: operatorName);
                }
                parameterDataList.Add(new ParameterData
                (
                    columnName: ColumnName,
                    parameterName: ColumnName,
                    value: ToDbValue(ColumnValue, ParameterDataType),
                    dataType: ParameterDataType
                ));
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
                var parameterNames = rightHandValues
                    .Select((value, i) =>
                    {
                        string columnNameNumbered = ColumnName + "_" + i;
                        parameterDataList.Add(new ParameterData(
                            columnName: ColumnName,
                            parameterName: columnNameNumbered,
                            value: ToDbValue(value, ParameterDataType),
                            dataType: ParameterDataType));
                        return GetParameterName(columnNameNumbered, value);
                    })
                    .ToArray();
                return renderer.BinaryOperator(
                    columnName: ColumnName,
                    leftValue: RenderedColumnName, 
                    rightValues: parameterNames, 
                    operatorName: operatorName,
                    isColumnArray: columnNameToType[ColumnName] 
                                   == OrigamDataType.Array);
            }

            throw new Exception("Cannot parse filter node: " + Value + ". If this should be a binary operator prefix it with \"$\".");
        }

        public OrigamDataType ParameterDataType {
            get
            {
                if (ColumnDataType == OrigamDataType.Array)
                {
                    return OrigamDataType.String;
                }

                if (ColumnDataType == OrigamDataType.UniqueIdentifier &&
                    (Operator != "eq" || Operator != "neq"))
                {
                    return OrigamDataType.String;
                }

                return ColumnDataType;
            }
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
                case "starts": return ("Like", AppendWildCard(value));
                case "nstarts": return ("NotLike", AppendWildCard(value));
                case "ends": return ("Like", PrependWildCard(value));
                case "nends": return ("NotLike",  PrependWildCard(value));
                case "like":
                case "contains": return ("Like", PrependWildCard(AppendWildCard(value)));
                case "ncontains": return ("NotLike", PrependWildCard(AppendWildCard(value)));
                case "null": return ("Equal", null);
                case "nnull": return ("NotEqual", null);
                case "between": return ("Between", null);
                case "nbetween": return ("NotBetween", null);
                case "in": return ("In", null);
                case "nin": return ("NotIn", null);
                default: throw new NotImplementedException(operatorName);
            }
        }

        private string PrependWildCard(string value)
        {
            if (!value.StartsWith(parameterReferenceChar))
            {
                throw new ArgumentException("Cannot prepend \"%\" to a value which is not a parameter");
            }
            return "'%'+"+value;
        } 
        private string AppendWildCard(string value)
        {
            if (!value.StartsWith(parameterReferenceChar))
            {
                throw new ArgumentException("Cannot append \"%\" to a value which is not a parameter");
            }
            return value + "+'%'";
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
