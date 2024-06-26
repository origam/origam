#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Text;
using CSharpFunctionalExtensions;

namespace Origam.DA.Service;
public abstract class AbstractFilterRenderer
{
    public string In(string leftOperand, IEnumerable<string> options)
    {
        return leftOperand+" IN (" + string.Join(", ",options) + ")";
    }
    public string LogicalAndOr(string functionName, IList<string> arguments)
    {
        if (arguments.Count < 2)
        {
            throw new ArgumentOutOfRangeException("At least 2 arguments must be present fpr AND/OR.");
        }
        int i = 0;
        StringBuilder logicalBuilder = new StringBuilder();
        logicalBuilder.Append("(");
        foreach (string arg in arguments)
        {
            if (arg != string.Empty)
            {
                if (i > 0)
                {
                    logicalBuilder.Append(" " + GetOperator(functionName) + " ");
                }
                logicalBuilder.Append(arg);
            }
            i++;
        }
        logicalBuilder.Append(")");
        return logicalBuilder.ToString();
    }
    
    public string BinaryOperator(
        string columnName,
        string leftValue,
        string[] rightValues, string operatorName,
        bool isColumnArray)
    {
        switch (operatorName)
        {
            case "Between":
                CheckArgumentLength("Between", rightValues, 2);
                return $"{leftValue} BETWEEN {rightValues[0]} AND {rightValues[1]}";
            case "NotBetween":
                CheckArgumentLength("NotBetween", rightValues, 2);
                return $"{leftValue} NOT BETWEEN {rightValues[0]} AND {rightValues[1]}";                
            case "In":
                if (isColumnArray)
                {
                    return ColumnArray(columnName, "IN", rightValues);     
                }
                else
                {
                    return leftValue + " IN (" + string.Join(", ", rightValues) + ")";                
                }
            case "NotIn":
                if (isColumnArray)
                {
                    return ColumnArray(columnName, "NOT IN", rightValues);
                }
                else
                {
                    return leftValue + " NOT IN (" + string.Join(", ", rightValues) + ")";
                }
            default:
                if (rightValues.Length == 1)
                {
                    return BinaryOperator(leftValue, rightValues[0], operatorName);
                }
                throw new ArgumentException($"Cannot process operator {operatorName} with {rightValues.Length} arguments");
        }
    }
    protected abstract string ColumnArray(string columnName, string operand, string[] rightValues);
    public string BinaryOperator(string leftValue,
        string rightValue, string operatorName)
    {
        switch (operatorName)
        {
            case "Equal":
                return Equal(leftValue, rightValue);
            case "NotEqual":
                return NotEqual(leftValue, rightValue);
            default:
                CheckArgumentEmpty("leftValue", leftValue);
                CheckArgumentEmpty("rightValue", rightValue);
                return string.Format("({0} {1} {2})",
                    leftValue, GetOperator(operatorName), rightValue);
        }
    }
    public string NotEqual(string leftValue, string rightValue)
    {
        CheckArgumentEmpty("leftValue", leftValue);
        if (rightValue == null)
        {
            return string.Format("{0} IS NOT NULL", leftValue);
        }
        else
        {
            return string.Format("{0} <> {1}", leftValue, rightValue);
        }
    }
    public string Equal(string leftValue, string rightValue)
    {
        CheckArgumentEmpty("leftValue", leftValue);
        if (rightValue == null || rightValue.ToLower() == "null")
        {
            return string.Format("{0} IS NULL", leftValue);
        }
        else
        {
            return string.Format("{0} = {1}", leftValue, rightValue);
        }
    }
    public string Not(string argument)
    {
        CheckArgumentEmpty("argument", argument);
        return string.Format("NOT({0})", argument);
    }
    private string GetOperator(string functionName)
    {
        switch (functionName)
        {
            case "NotEqual":
                return "<>";
            case "Equal":
                return "=";
            case "Like":
                return LikeOperator();
            case "NotLike":
                return "NOT "+ LikeOperator();
            case "Add":
                return "+";
            case "Deduct":
                return "-";
            case "Multiply":
                return "*";
            case "Divide":
                return "/";
            case "LessThan":
                return "<";
            case "LessThanOrEqual":
                return "<=";
            case "GreaterThan":
                return ">";
            case "GreaterThanOrEqual":
                return ">=";
            case "OR":
            case "LogicalOr":
                return "OR";
            case "AND":
            case "LogicalAnd":
                return "AND";
            default:
                throw new ArgumentOutOfRangeException("functionName", functionName, ResourceUtils.GetString("UnsupportedOperator"));
        }
    }
    protected abstract string LikeOperator();
    public abstract string StringConcatenationChar { get; }
    private static void CheckArgumentEmpty(string name, string argument)
    {
        if (argument == null)
        {
            throw new ArgumentOutOfRangeException("name", name, "Argument cannot be empty.");
        }
    }
    private static void CheckArgumentLength(string operatorName, string[] arguments, int length)
    {
        if (arguments?.Length != length)
        {
            throw new ArgumentOutOfRangeException("operator", operatorName, $"Operator needs exactly {length} number of right hand arguments.");
        }
    }
}
