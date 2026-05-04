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
using System.Xml;
using System.Xml.XPath;
using Origam.Rule.Xslt;
using Origam.Schema;

namespace Origam.Rule;

public class XpathEvaluator : IXpathEvaluator
{
    public static XpathEvaluator Instance { get; } = new();

    private XpathEvaluator() { }

    public string Evaluate(object nodeset, string xpath)
    {
        if (nodeset is XPathNodeIterator)
        {
            return Evaluate(iterator: nodeset as XPathNodeIterator, xpath: xpath);
        }

        if (nodeset is XPathNavigator)
        {
            return Evaluate(navigator: nodeset as XPathNavigator, xpath: xpath);
        }

        throw new ArgumentOutOfRangeException(
            paramName: "nodeset",
            actualValue: nodeset,
            message: "Invalid type."
        );
    }

    private string Evaluate(XPathNodeIterator iterator, string xpath)
    {
        return (string)Evaluate(navigator: iterator.Current, xpath: xpath);
    }

    private string Evaluate(XPathNavigator navigator, string xpath)
    {
        return (string)Evaluate(
            xpath: xpath,
            isPathRelative: false,
            returnDataType: OrigamDataType.String,
            nav: navigator,
            contextPosition: null,
            transactionId: null
        );
    }

    public object Evaluate(
        string xpath,
        bool isPathRelative,
        OrigamDataType returnDataType,
        XPathNavigator nav,
        XPathNodeIterator contextPosition,
        string transactionId
    )
    {
        XPathExpression expr;
        expr = nav.Compile(xpath: xpath);
        OrigamXsltContext ctx = OrigamXsltContext.Create(
            nameTable: new NameTable(),
            transactionId: transactionId
        );
        expr.SetContext(nsManager: ctx);

        object result;

        if (isPathRelative & contextPosition != null)
        {
            result = nav.Evaluate(expr: expr, context: contextPosition);
        }
        else
        {
            result = nav.Evaluate(expr: expr);
        }

        if (result is XPathNodeIterator)
        {
            XPathNodeIterator iterator = result as XPathNodeIterator;

            if (iterator.Count == 0)
            {
                result = null;
            }
            else
            {
                iterator.MoveNext();
                result = iterator.Current.Value;
            }
        }

        try
        {
            switch (returnDataType)
            {
                case OrigamDataType.Boolean:
                {
                    if (result == null)
                    {
                        return false;
                    }

                    if (result is String)
                    {
                        if (
                            (string)result == ""
                            | (string)result == "false"
                            | (string)result == "0"
                        )
                        {
                            return false;
                        }

                        return true;
                    }

                    if (result is double)
                    {
                        if ((double)result == 0)
                        {
                            return false;
                        }

                        return true;
                    }

                    if (result is bool)
                    {
                        return result;
                    }

                    throw new Exception(
                        message: ResourceUtils.GetString(key: "ErrorConvertToBool")
                    );
                }

                case OrigamDataType.UniqueIdentifier:
                {
                    if (result == null || result.ToString() == "")
                    {
                        return DBNull.Value;
                    }

                    return new Guid(g: result.ToString());
                }

                case OrigamDataType.Date:
                {
                    if (result == null || result.ToString() == "")
                    {
                        return DBNull.Value;
                    }

                    return XmlConvert.ToDateTime(
                        s: result.ToString(),
                        dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
                    );
                }

                case OrigamDataType.Long:
                {
                    if (result == null || result.ToString() == "")
                    {
                        return DBNull.Value;
                    }

                    return Convert.ToInt64(
                        value: result,
                        provider: new System.Globalization.NumberFormatInfo()
                    );
                }

                case OrigamDataType.Integer:
                {
                    if (result == null || result.ToString() == "")
                    {
                        return DBNull.Value;
                    }

                    return Convert.ToInt32(
                        value: result,
                        provider: new System.Globalization.NumberFormatInfo()
                    );
                }

                case OrigamDataType.Float:
                {
                    if (result == null || result.ToString() == "")
                    {
                        return DBNull.Value;
                    }

                    return Convert.ToDecimal(
                        value: result,
                        provider: new System.Globalization.NumberFormatInfo()
                    );
                }

                case OrigamDataType.Currency:
                {
                    return Convert.ToDecimal(
                        value: result,
                        provider: new System.Globalization.NumberFormatInfo()
                    );
                }

                case OrigamDataType.String:
                {
                    if (result == null)
                    {
                        return DBNull.Value;
                    }

                    return XmlTools.ConvertToString(val: result);
                }

                default:
                {
                    throw new Exception(message: "Data type not supported by rule evaluation.");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception(
                message: ResourceUtils.GetString(key: "ErrorConvertToType0")
                    + Environment.NewLine
                    + ResourceUtils.GetString(key: "ErrorConvertToType1")
                    + returnDataType.ToString()
                    + Environment.NewLine
                    + ResourceUtils.GetString(key: "ErrorConvertToType2", args: result.ToString()),
                innerException: ex
            );
        }
    }
}
