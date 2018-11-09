using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origam.DA.Service.Generators
{
    public class FilterParser
    {
        private Node root = null;
        private Node currentNode = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strFilter">input example: "[\"$AND\", [\"$OR\",[\"city_name\",\"like\",\"%Wash%\"],[\"name\",\"like\",\"%Smith%\"]], [\"age\",\"gte\",18],[\"id\",\"in\",[\"f2\",\"f3\",\"f4\"]]";
        /// </param>
        public string Parse(string strFilter)
        {
            var inpValue = CheckInput(strFilter);

            foreach (char c in inpValue)
            {
                if (c == '[')
                {
                    AddNode();
                }
                else if (c == ']')
                {
                    currentNode = currentNode.Parent;
                }
                else if (c == ' ' || currentNode.IsBinaryOperator && c == ',')
                {
                    continue;
                }
                else
                {
                    currentNode.Value += c;
                }
            }

            return root.SqlRepresentation();
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

        private static string CheckInput(string strFilter)
        {
            if (string.IsNullOrEmpty(strFilter))
            {
                throw new ArgumentException(nameof(strFilter) + " cannot be empty");
            }

            string inpValue = strFilter.Trim();
            if (inpValue[0] != '[')
            {
                throw new ArgumentException("Filter input must start with \"[\", found: \"" + inpValue[0] + "\"");
            }

            return inpValue;
        }
    }



    class Node
    {
        private string[] splitValue;
        public Node Parent { get; set; }
        public List<Node> Children { get; } = new List<Node>();
        public string Value { get; set; } = "";
        public bool IsBinaryOperator => Value.Contains("$");
        public string[] SplitValue => splitValue ?? (splitValue = Value.Split(','));
        private string LeftOperand => SplitValue[0].Replace("\"","");
        private string Operator => SplitValue[1].Replace("\"","");
        private string RightOperand => SplitValue[2].Replace("\"", "'");
        private readonly FilterRenderer renderer = new FilterRenderer();

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
            return Children
                .Select(node => node.SqlRepresentation())
                .Select(operand => "("+ operand + ")")
                .Aggregate((x, y) => x + " " + logicalOperator + " " + y);
        }

        private string GetLogicalOperator()
        {
            if (Value == "\"$AND\"") return "AND";
            if (Value == "\"$OR\"") return "OR";
            throw new Exception("Could not parse node value to logical operator: " + Value);
        }

        private string GetSqlOfLeafNode()
        {
            if (Children.Count == 0)
            {
                string operatorName = OperatorToRendererName(Operator);
                return renderer.BinaryOperator(LeftOperand, RightOperand, operatorName);
            }

            if (Children.Count == 1 && Operator == "in")
            {
                string options = Children.First()
                    .SplitValue
                    .Select(val => val.Replace("\"","'"))
                    .Aggregate((x,y) => x+", "+y);
                return LeftOperand + " IN (" + options + ")";
            }

            throw new Exception("Cannot parse filter node: " + Value);
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
