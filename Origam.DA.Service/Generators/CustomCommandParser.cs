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
using ArgumentException = System.ArgumentException;

namespace Origam.DA.Service.Generators
{
    public class CustomCommandParser
    {
        private Node root = null;
        private Node currentNode = null;
        private readonly ColumnOrderingRenderer columnOrderingRenderer;

        public string WhereClause { get; private set; }
        public string OrderByClause { get; private set; }

        public CustomCommandParser(string nameLeftBracket, string nameRightBracket)
        {
            columnOrderingRenderer 
                = new ColumnOrderingRenderer(nameLeftBracket, nameRightBracket);
        }

        /// <summary>
        /// returns ORDER BY clause without the "ORDER BY" keyword
        /// </summary>
        /// <returns></returns>
        public CustomCommandParser OrderBy(List<Ordering> orderings)
        {
            if (orderings != null)
            {
                OrderByClause = columnOrderingRenderer.ToSqlOrderBy(orderings);
            }
            return this;
        }

        /// <summary>
        /// returns WHERE clause without the "WHERE" keyword
        /// </summary>
        /// <param name="strFilter">input example: "[\"$AND\", [\"$OR\",[\"city_name\",\"like\",\"%Wash%\"],[\"name\",\"like\",\"%Smith%\"]], [\"age\",\"gte\",18],[\"id\",\"in\",[\"f2\",\"f3\",\"f4\"]]";
        /// </param>
        public CustomCommandParser Where(string strFilter)
        {
            if (string.IsNullOrWhiteSpace(strFilter)) return this;
            var inpValue = GetCheckedInput(strFilter);
            ParseToNodeTree(inpValue);
            WhereClause = root.SqlRepresentation();
            return this;
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
            Node newNode = new Node();
            newNode.Parent = currentNode;
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

    class ColumnOrderingRenderer
    {
        private readonly string nameLeftBracket;
        private readonly string nameRightBracket;

        public ColumnOrderingRenderer(string nameLeftBracket, string nameRightBracket)
        {
            this.nameLeftBracket = nameLeftBracket;
            this.nameRightBracket = nameRightBracket;
        }

        internal string ToSqlOrderBy(List<Ordering> orderings)
        {
            if (orderings == null) return "";
                return string.Join(", ", orderings.Select(ToSql)
            );
        }

        private string ToSql(Ordering ordering)
        {
            if (ordering.LookupId == Guid.Empty)
            {
                string orderingSql = OrderingToSQLName(ordering.Direction);
                return $"{nameLeftBracket}{ordering.ColumnName}{nameRightBracket} {orderingSql}";
            }
            return "";
        }

        private string OrderingToSQLName(string orderingName)
        {
            switch (orderingName.ToLower())
            {
                case "asc": return "ASC";
                case "desc": return "DESC";
                default: throw new NotImplementedException(orderingName);
            }
        }
    }

    class Node
    {
        private string[] splitValue;
        public Node Parent { get; set; }
        public List<Node> Children { get; } = new List<Node>();
        public string Value { get; set; } = "";
        public bool IsBinaryOperator => Value.Contains("$");

        private string[] SplitValue => splitValue ??
                                       (splitValue = Value
                                           .Split(',')
                                           .Select(x => x.Trim())
                                           .ToArray()
                                       );
        private string LeftOperand => SplitValue[0].Replace("\"","");
        private string Operator => SplitValue[1].Replace("\"","");
        private string RightOperand => SplitValue[2]
            .Replace("'", "''")
            .Replace("\"", "'");
        
        private readonly FilterRenderer renderer = new FilterRenderer();

        private string ValueToOperand(string value)
        {
            // if (string.IsNullOrWhiteSpace(value)) return value;
            // string valueWithoutDoubleQuotes = value.Replace("\"", "");
            // char firstChar = valueWithoutDoubleQuotes.First();
            // char lastChar = valueWithoutDoubleQuotes.Last();
            // return value.First() + value.Substring(1,value.Length - 2).Replace("'","''") + value.Last();

            return value
                .Replace("\"", "")
                .Replace("'", "''");
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
            if (Children.Count == 0)
            {
                if (SplitValue.Length != 3) throw new ArgumentException("could not parse: "+Value+" to a filter node");
                string operatorName = OperatorToRendererName(Operator);
                return renderer.BinaryOperator(LeftOperand, RightOperand, operatorName);
            }

            if (Children.Count == 1 && Operator == "in")
            {
                IEnumerable<string> options = Children.First()
                    .SplitValue
                    .Select(val => val.Replace("\"", "'"));
                return renderer.In(LeftOperand, options);
            }

            throw new Exception("Cannot parse filter node: " + Value + ". If this should be a binary operator prefix it with \"$\".");
        }

        private string OperatorToRendererName(string operatorName)
        {
            switch (operatorName)
            {
                case "gt": return "GreaterThan";
                case "lt": return "LessThan";
                case "gte": return "GreaterThanOrEqual";
                case "lte": return "LessThanOrEqual";
                case "eq": return "Equal";
                case "neq": return "NotEqual";
                case "like": return "Like";
                default: throw new NotImplementedException(operatorName);
            }
        }
    }
}
