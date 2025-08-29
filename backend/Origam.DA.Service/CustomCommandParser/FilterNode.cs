using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Origam.Schema;

#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

namespace Origam.DA.Service.CustomCommandParser;

class FilterNode
{
    private readonly SqlRenderer sqlRenderer;
    private readonly Dictionary<string, string> lookupExpressions;
    private readonly List<ColumnInfo> columns;
    private string[] splitValue;
    public FilterNode Parent { get; set; }
    public List<FilterNode> Children { get; } = new List<FilterNode>();
    public string Value { get; set; } = "";
    public bool IsBinaryOperator => Value.Contains("$");
    public IEnumerable<FilterNode> AllChildren
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
        return Regex.Match(value, "^[\\d,\\s\\.]+$").Success;
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
                splitValue = splitValue.Select(x => x.Trim()).ToArray();
            }
            return splitValue;
        }
    }
    internal string ColumnName => SplitValue[0].Replace("\"", "");
    private string RenderedColumnName =>
        lookupExpressions.ContainsKey(ColumnName)
            ? lookupExpressions[ColumnName]
            : sqlRenderer.NameLeftBracket + ColumnName + sqlRenderer.NameRightBracket;
    private string Operator => SplitValue?.Length > 1 ? SplitValue[1].Replace("\"", "") : null;
    private string ParameterValue => ValueToOperand(SplitValue[2]);
    private ColumnInfo Column =>
        columns.FirstOrDefault(column => column.Name == ColumnName)
        ?? throw new Exception($"Unknown column \"{ColumnName}\"");
    public OrigamDataType ParameterDataType
    {
        get
        {
            if (Column.DataType == OrigamDataType.Array)
            {
                return OrigamDataType.String;
            }
            if (
                Column.DataType == OrigamDataType.UniqueIdentifier
                && Operator != "eq"
                && Operator != "neq"
                && Operator != "in"
                && Operator != "nin"
            )
            {
                return OrigamDataType.String;
            }
            return Column.DataType;
        }
    }
    public bool IsValueNode =>
        Parent != null
        && (
            Parent.Operator == "in"
            || Parent.Operator == "nin"
            || Parent.Operator == "between"
            || Parent.Operator == "nbetween"
        );
    private readonly AbstractFilterRenderer renderer;
    private readonly List<ParameterData> parameterDataList;

    public FilterNode(
        SqlRenderer sqlRenderer,
        Dictionary<string, string> lookupExpressions,
        List<ColumnInfo> columns,
        AbstractFilterRenderer filterRenderer,
        List<ParameterData> parameterDataList
    )
    {
        this.sqlRenderer = sqlRenderer;
        this.lookupExpressions = lookupExpressions;
        this.columns = columns;
        renderer = filterRenderer;
        this.parameterDataList = parameterDataList;
    }

    private string GetParameterNameSql(string columnName)
    {
        return sqlRenderer.ParameterReferenceChar + columnName;
    }

    private string ParameterName => ColumnName + "_" + Operator;

    private string ValueToOperand(string value)
    {
        return value.Replace("\"", "");
    }

    private object ToDbValue(string value, OrigamDataType dataType)
    {
        switch (dataType)
        {
            case OrigamDataType.Integer:
            {
                if (!int.TryParse(value, out var intValue))
                {
                    throw new ArgumentOutOfRangeException($"Cannot parse \"{value}\" to int");
                }
                return intValue;
            }

            case OrigamDataType.Currency:
            {
                if (
                    !decimal.TryParse(
                        value,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out var decimalValue
                    )
                )
                {
                    throw new ArgumentOutOfRangeException($"Cannot parse \"{value}\" to decimal");
                }
                return decimalValue;
            }

            case OrigamDataType.Float:
            {
                if (
                    !float.TryParse(
                        value,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out var floatValue
                    )
                )
                {
                    throw new ArgumentOutOfRangeException($"Cannot parse \"{value}\" to float");
                }
                return floatValue;
            }

            case OrigamDataType.Boolean:
            {
                if (!bool.TryParse(value, out var boolValue))
                {
                    throw new ArgumentOutOfRangeException($"Cannot parse \"{value}\" to bool");
                }
                return boolValue;
            }

            case OrigamDataType.Date:
            {
                if (string.IsNullOrEmpty(Convert.ToString(value)))
                {
                    return null;
                }
                if (!DateTime.TryParse(value, out var dateValue))
                {
                    throw new ArgumentOutOfRangeException($"Cannot parse \"{value}\" to DateTime");
                }
                return dateValue;
            }

            case OrigamDataType.UniqueIdentifier:
            {
                if (value == null)
                {
                    return Guid.Empty;
                }
                if (!Guid.TryParse(value, out Guid guidValue))
                {
                    throw new ArgumentOutOfRangeException($"Cannot parse \"{value}\" to Guid");
                }
                return guidValue;
            }
        }
        return value;
    }

    public string SqlRepresentation()
    {
        if (IsBinaryOperator)
        {
            return GetSqlOfOperatorNode();
        }

        return GetSqlOfLeafNode();
    }

    private string GetSqlOfOperatorNode()
    {
        string logicalOperator = GetLogicalOperator();
        List<string> operands = Children.Select(node => node.SqlRepresentation()).ToList();
        return renderer.LogicalAndOr(logicalOperator, operands);
    }

    private string GetLogicalOperator()
    {
        if (Value.Trim() == "\"$AND\"")
        {
            return "AND";
        }

        if (Value.Trim() == "\"$OR\"")
        {
            return "OR";
        }

        throw new Exception("Could not parse node value to logical operator: \"" + Value + "\"");
    }

    private string GetSqlOfLeafNode()
    {
        string nodeSql = RenderNodeSql();
        nodeSql = AddIsNullToNegativeOperators(nodeSql);
        return nodeSql;
    }

    private string AddIsNullToNegativeOperators(string nodeSql)
    {
        if (
            Operator != "nnull"
            && Operator != "null"
            && Operator.StartsWith("n")
            && Column.IsNullable
        )
        {
            string isNullSql = renderer.BinaryOperator(
                leftValue: RenderedColumnName,
                rightValue: null,
                operatorName: "Equal"
            );
            return renderer.LogicalAndOr("OR", new[] { nodeSql, isNullSql });
        }
        return nodeSql;
    }

    private string RenderNodeSql()
    {
        string parameterName = GetParameterNameSql(ParameterName);
        var (operatorName, renderedColumnValue) = GetRendererInput(Operator, parameterName);
        if (Children.Count == 0)
        {
            if (SplitValue.Length != 3)
            {
                throw new ArgumentException("could not parse: " + Value + " to a filter node");
            }
            if (ParameterValue == null || ParameterValue.ToLower() == "null")
            {
                return renderer.BinaryOperator(
                    leftValue: RenderedColumnName,
                    rightValue: null,
                    operatorName: operatorName
                );
            }
            object value = ToDbValue(ParameterValue, ParameterDataType);
            if (
                (Operator == "eq" || Operator == "neq")
                && Column.DataType == OrigamDataType.Date
                && IsWholeDay((DateTime)value)
            )
            {
                return RenderDateEquals();
            }
            parameterDataList.Add(
                new ParameterData(
                    parameterName: ParameterName,
                    columnName: ColumnName,
                    value: value,
                    dataType: ParameterDataType
                )
            );
            return renderer.BinaryOperator(
                leftValue: RenderedColumnName,
                rightValue: renderedColumnValue,
                operatorName: operatorName
            );
        }
        if (
            Children.Count == 1
            && (
                Operator == "in"
                || Operator == "nin"
                || Operator == "between"
                || Operator == "nbetween"
            )
        )
        {
            var parameterNames = GetRightHandValues()
                .Select(
                    (value, i) =>
                    {
                        string parameterNameNumbered = ParameterName + "_" + i;
                        parameterDataList.Add(
                            new ParameterData(
                                parameterName: parameterNameNumbered,
                                columnName: ColumnName,
                                value: value,
                                dataType: ParameterDataType
                            )
                        );
                        return GetParameterNameSql(parameterNameNumbered);
                    }
                )
                .ToArray();
            return renderer.BinaryOperator(
                columnName: ColumnName,
                leftValue: RenderedColumnName,
                rightValues: parameterNames,
                operatorName: operatorName,
                isColumnArray: Column.DataType == OrigamDataType.Array
            );
        }
        throw new Exception(
            "Cannot parse filter node: "
                + Value
                + ". If this should be a binary operator prefix it with \"$\"."
        );
    }

    private object[] GetRightHandValues()
    {
        object[] rightHandValues = Children
            .First()
            .SplitValue.Select(ValueToOperand)
            .Select(value => ToDbValue(value, Column.DataType))
            .ToArray();
        if (
            Column.DataType == OrigamDataType.Date
            && rightHandValues.Length == 2
            && IsWholeDay((DateTime)rightHandValues[1])
        )
        {
            rightHandValues[1] = ((DateTime)rightHandValues[1]).AddDays(1).AddSeconds(-1);
        }
        return rightHandValues;
    }

    private bool IsWholeDay(DateTime dateTime)
    {
        return dateTime.Millisecond == 0
            && dateTime.Second == 0
            && dateTime.Minute == 0
            && dateTime.Hour == 0;
    }

    private string RenderDateEquals()
    {
        string actualOperator;
        switch (Operator)
        {
            case "eq":
            {
                actualOperator = "between";
                break;
            }

            case "neq":
            {
                actualOperator = "nbetween";
                break;
            }

            default:
                throw new InvalidOperationException("Operator must be eq or neq");
        }

        var (operatorName, _) = GetRendererInput(actualOperator, "");
        DateTime equalsDate = (DateTime)ToDbValue(ParameterValue, Column.DataType);
        DateTime startDate = new DateTime(equalsDate.Year, equalsDate.Month, equalsDate.Day);
        DateTime endDate = startDate.AddDays(1).AddSeconds(-1);
        var parameterNames = new[] { startDate, endDate }
            .Select(
                (value, i) =>
                {
                    string parameterNameNumbered = ParameterName + "_" + i;
                    parameterDataList.Add(
                        new ParameterData(
                            parameterName: parameterNameNumbered,
                            columnName: ColumnName,
                            value: value,
                            dataType: ParameterDataType
                        )
                    );
                    return GetParameterNameSql(parameterNameNumbered);
                }
            )
            .ToArray();
        return renderer.BinaryOperator(
            columnName: ColumnName,
            leftValue: RenderedColumnName,
            rightValues: parameterNames,
            operatorName: operatorName,
            isColumnArray: Column.DataType == OrigamDataType.Array
        );
    }

    private (string, string) GetRendererInput(string operatorName, string parameterName)
    {
        switch (operatorName)
        {
            case "gt":
                return ("GreaterThan", parameterName);
            case "lt":
                return ("LessThan", parameterName);
            case "gte":
                return ("GreaterThanOrEqual", parameterName);
            case "lte":
                return ("LessThanOrEqual", parameterName);
            case "eq":
                return ("Equal", parameterName);
            case "neq":
                return ("NotEqual", parameterName);
            case "starts":
                return ("Like", AppendWildCard(parameterName));
            case "nstarts":
                return ("NotLike", AppendWildCard(parameterName));
            case "ends":
                return ("Like", PrependWildCard(parameterName));
            case "nends":
                return ("NotLike", PrependWildCard(parameterName));
            case "like":
            case "contains":
                return ("Like", PrependWildCard(AppendWildCard(parameterName)));
            case "ncontains":
                return ("NotLike", PrependWildCard(AppendWildCard(parameterName)));
            case "null":
                return ("Equal", null);
            case "nnull":
                return ("NotEqual", null);
            case "between":
                return ("Between", null);
            case "nbetween":
                return ("NotBetween", null);
            case "in":
                return ("In", null);
            case "nin":
                return ("NotIn", null);
            default:
                throw new NotImplementedException(operatorName);
        }
    }

    private string PrependWildCard(string value)
    {
        if (!value.StartsWith(sqlRenderer.ParameterReferenceChar))
        {
            throw new ArgumentException("Cannot prepend \"%\" to a value which is not a parameter");
        }
        return "'%'" + renderer.StringConcatenationChar + value;
    }

    private string AppendWildCard(string value)
    {
        if (!value.StartsWith(sqlRenderer.ParameterReferenceChar))
        {
            throw new ArgumentException("Cannot append \"%\" to a value which is not a parameter");
        }
        return value + renderer.StringConcatenationChar + "'%'";
    }
}
