#region license

/*
Copyright 2005 - 2022 Advantage Solutions, s. r. o.

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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using DiffPlex;
using DiffPlex.DiffBuilder;
using Origam.DA;
using Origam.Extensions;
using Origam.Schema;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Rule.XsltFunctions;

public class LegacyXsltFunctionContainer : AbstractOrigamDependentXsltFunctionContainer
{
    private const string NotANumber = "NaN";

    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    private ICounter counter;

    public LegacyXsltFunctionContainer() { }

    public LegacyXsltFunctionContainer(ICounter counter)
    {
        this.counter = counter;
    }

    public string GetConstant(string name)
    {
        return (string)
            ParameterService.GetParameterValue(
                parameterName: name,
                targetType: OrigamDataType.String
            );
    }

    public string GetString(string name)
    {
        return ParameterService.GetString(name: name);
    }

    public string GetString(string name, string arg1)
    {
        return ParameterService.GetString(name: name, args: arg1);
    }

    public string GetString(string name, string arg1, string arg2)
    {
        return ParameterService.GetString(name: name, args: [arg1, arg2]);
    }

    public string GetString(string name, string arg1, string arg2, string arg3)
    {
        return ParameterService.GetString(name: name, args: [arg1, arg2, arg3]);
    }

    public string GetString(string name, string arg1, string arg2, string arg3, string arg4)
    {
        return ParameterService.GetString(name: name, args: [arg1, arg2, arg3, arg4]);
    }

    public string GetString(
        string name,
        string arg1,
        string arg2,
        string arg3,
        string arg4,
        string arg5
    )
    {
        return ParameterService.GetString(name: name, args: [arg1, arg2, arg3, arg4, arg5]);
    }

    public string GetString(
        string name,
        string arg1,
        string arg2,
        string arg3,
        string arg4,
        string arg5,
        string arg6
    )
    {
        return ParameterService.GetString(name: name, args: [arg1, arg2, arg3, arg4, arg5, arg6]);
    }

    public string GetString(
        string name,
        string arg1,
        string arg2,
        string arg3,
        string arg4,
        string arg5,
        string arg6,
        string arg7
    )
    {
        return ParameterService.GetString(
            name: name,
            args: [arg1, arg2, arg3, arg4, arg5, arg6, arg7]
        );
    }

    public string GetString(
        string name,
        string arg1,
        string arg2,
        string arg3,
        string arg4,
        string arg5,
        string arg6,
        string arg7,
        string arg8
    )
    {
        return ParameterService.GetString(
            name: name,
            args: [arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8]
        );
    }

    public string GetString(
        string name,
        string arg1,
        string arg2,
        string arg3,
        string arg4,
        string arg5,
        string arg6,
        string arg7,
        string arg8,
        string arg9
    )
    {
        return ParameterService.GetString(
            name: name,
            args: [arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9]
        );
    }

    public string GetString(
        string name,
        string arg1,
        string arg2,
        string arg3,
        string arg4,
        string arg5,
        string arg6,
        string arg7,
        string arg8,
        string arg9,
        string arg10
    )
    {
        return ParameterService.GetString(
            name: name,
            args: [arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10]
        );
    }

    public string GetStringOrEmpty(string name)
    {
        return ParameterService.GetString(name: name, throwException: false);
    }

    public string GetStringOrEmpty(string name, string arg1)
    {
        return ParameterService.GetString(name: name, throwException: false, args: arg1);
    }

    public string GetStringOrEmpty(string name, string arg1, string arg2)
    {
        return ParameterService.GetString(name: name, throwException: false, args: [arg1, arg2]);
    }

    public string GetStringOrEmpty(string name, string arg1, string arg2, string arg3)
    {
        return ParameterService.GetString(
            name: name,
            throwException: false,
            args: [arg1, arg2, arg3]
        );
    }

    public string GetStringOrEmpty(string name, string arg1, string arg2, string arg3, string arg4)
    {
        return ParameterService.GetString(
            name: name,
            throwException: false,
            args: [arg1, arg2, arg3, arg4]
        );
    }

    public string GetStringOrEmpty(
        string name,
        string arg1,
        string arg2,
        string arg3,
        string arg4,
        string arg5
    )
    {
        return ParameterService.GetString(
            name: name,
            throwException: false,
            args: [arg1, arg2, arg3, arg4, arg5]
        );
    }

    public string GetStringOrEmpty(
        string name,
        string arg1,
        string arg2,
        string arg3,
        string arg4,
        string arg5,
        string arg6
    )
    {
        return ParameterService.GetString(
            name: name,
            throwException: false,
            args: [arg1, arg2, arg3, arg4, arg5, arg6]
        );
    }

    public string GetStringOrEmpty(
        string name,
        string arg1,
        string arg2,
        string arg3,
        string arg4,
        string arg5,
        string arg6,
        string arg7
    )
    {
        return ParameterService.GetString(
            name: name,
            throwException: false,
            args: [arg1, arg2, arg3, arg4, arg5, arg6, arg7]
        );
    }

    public string GetStringOrEmpty(
        string name,
        string arg1,
        string arg2,
        string arg3,
        string arg4,
        string arg5,
        string arg6,
        string arg7,
        string arg8
    )
    {
        return ParameterService.GetString(
            name: name,
            throwException: false,
            args: [arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8]
        );
    }

    public string GetStringOrEmpty(
        string name,
        string arg1,
        string arg2,
        string arg3,
        string arg4,
        string arg5,
        string arg6,
        string arg7,
        string arg8,
        string arg9
    )
    {
        return ParameterService.GetString(
            name: name,
            throwException: false,
            args: [arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9]
        );
    }

    public string GetStringOrEmpty(
        string name,
        string arg1,
        string arg2,
        string arg3,
        string arg4,
        string arg5,
        string arg6,
        string arg7,
        string arg8,
        string arg9,
        string arg10
    )
    {
        return ParameterService.GetString(
            name: name,
            throwException: false,
            args: [arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10]
        );
    }

    public string Plus(string number1, string number2)
    {
        return NumberOperand(number1: number1, number2: number2, operand: "PLUS");
    }

    public string Minus(string number1, string number2)
    {
        return NumberOperand(number1: number1, number2: number2, operand: "MINUS");
    }

    public string Mul(string number1, string number2)
    {
        return NumberOperand(number1: number1, number2: number2, operand: "MUL");
    }

    public string Mod(string number1, string number2)
    {
        return NumberOperand(number1: number1, number2: number2, operand: "MOD");
    }

    public string Div(string number1, string number2)
    {
        return NumberOperand(number1: number1, number2: number2, operand: "DIV");
    }

    public string NumberOperand(string number1, string number2, string operand)
    {
        decimal num1;
        decimal num2;

        try
        {
            try
            {
                num1 = XmlConvert.ToDecimal(s: number1);
            }
            catch
            {
                double dnum1 = Double.Parse(
                    s: number1,
                    style: NumberStyles.Float,
                    provider: CultureInfo.InvariantCulture
                );
                num1 = Convert.ToDecimal(value: dnum1);
            }

            try
            {
                num2 = XmlConvert.ToDecimal(s: number2);
            }
            catch
            {
                double dnum2 = Double.Parse(
                    s: number2,
                    style: NumberStyles.Float,
                    provider: CultureInfo.InvariantCulture
                );
                num2 = Convert.ToDecimal(value: dnum2);
            }
        }
        catch
        {
            return NotANumber;
        }

        switch (operand)
        {
            case "PLUS":
            {
                return XmlConvert.ToString(value: num1 + num2);
            }
            case "MINUS":
            {
                return XmlConvert.ToString(value: num1 - num2);
            }
            case "MUL":
            {
                return XmlConvert.ToString(value: num1 * num2);
            }
            case "MOD":
            {
                return XmlConvert.ToString(value: num1 % num2);
            }
            case "DIV":
            {
                return XmlConvert.ToString(value: num1 / num2);
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "operand",
                    actualValue: operand,
                    message: ResourceUtils.GetString(key: "ErrorUnsupportedOperator")
                );
            }
        }
    }

    public string FormatDate(string date, string format)
    {
        if (String.IsNullOrWhiteSpace(value: date))
        {
            return "";
        }

        DateTime d = XmlConvert.ToDateTime(
            s: date,
            dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
        );
        return d.ToString(format: format);
    }

    public string FormatNumber(string number, string format)
    {
        if (String.IsNullOrWhiteSpace(value: number))
        {
            return "";
        }

        decimal d = XmlConvert.ToDecimal(s: number);
        return d.ToString(format: format);
    }

    public string MinString(XPathNodeIterator iterator)
    {
        if (iterator.Count == 0)
        {
            return "";
        }
        List<string> sorted = SortedArray(iterator: iterator);

        return sorted[index: 0];
    }

    public string MaxString(XPathNodeIterator iterator)
    {
        if (iterator.Count == 0)
        {
            return "";
        }
        List<string> sorted = SortedArray(iterator: iterator);

        return sorted[index: sorted.Count - 1];
    }

    private List<string> SortedArray(XPathNodeIterator iterator)
    {
        var result = new List<string>();

        while (iterator.MoveNext())
        {
            result.Add(item: iterator.Current.Value);
        }

        result.Sort();

        return result;
    }

    public string LookupValue(string lookupId, string recordId)
    {
        object result = LookupService.GetDisplayText(
            lookupId: new Guid(g: lookupId),
            lookupValue: recordId,
            useCache: false,
            returnMessageIfNull: false,
            transactionId: this.TransactionId
        );

        return XmlTools.FormatXmlString(value: result);
    }

    public string LookupValue(
        string lookupId,
        string paramName1,
        string param1,
        string paramName2,
        string param2
    )
    {
        var parameters = new Dictionary<string, object>
        {
            [key: paramName1] = param1,
            [key: paramName2] = param2,
        };

        object result = LookupService.GetDisplayText(
            lookupId: new Guid(g: lookupId),
            parameters: parameters,
            useCache: false,
            returnMessageIfNull: false,
            transactionId: this.TransactionId
        );

        return XmlTools.FormatXmlString(value: result);
    }

    public string LookupValue(
        string lookupId,
        string paramName1,
        string param1,
        string paramName2,
        string param2,
        string paramName3,
        string param3
    )
    {
        var parameters = new Dictionary<string, object>
        {
            [key: paramName1] = param1,
            [key: paramName2] = param2,
            [key: paramName3] = param3,
        };

        object result = LookupService.GetDisplayText(
            lookupId: new Guid(g: lookupId),
            parameters: parameters,
            useCache: false,
            returnMessageIfNull: false,
            transactionId: this.TransactionId
        );

        return XmlTools.FormatXmlString(value: result);
    }

    public string LookupValue(
        string lookupId,
        string paramName1,
        string param1,
        string paramName2,
        string param2,
        string paramName3,
        string param3,
        string paramName4,
        string param4
    )
    {
        var parameters = new Dictionary<string, object>
        {
            [key: paramName1] = param1,
            [key: paramName2] = param2,
            [key: paramName3] = param3,
            [key: paramName4] = param4,
        };

        object result = LookupService.GetDisplayText(
            lookupId: new Guid(g: lookupId),
            parameters: parameters,
            useCache: false,
            returnMessageIfNull: false,
            transactionId: this.TransactionId
        );

        return XmlTools.FormatXmlString(value: result);
    }

    public XPathNodeIterator LookupList(string lookupId)
    {
        return LookupList(lookupId: lookupId, parameters: new Dictionary<string, object>());
    }

    public XPathNodeIterator LookupList(string lookupId, string paramName1, string param1)
    {
        var parameters = new Dictionary<string, object> { { paramName1, param1 } };

        return LookupList(lookupId: lookupId, parameters: parameters);
    }

    public XPathNodeIterator LookupList(
        string lookupId,
        string paramName1,
        string param1,
        string paramName2,
        string param2
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { paramName1, param1 },
            { paramName2, param2 },
        };

        return LookupList(lookupId: lookupId, parameters: parameters);
    }

    public XPathNodeIterator LookupList(
        string lookupId,
        string paramName1,
        string param1,
        string paramName2,
        string param2,
        string paramName3,
        string param3
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { paramName1, param1 },
            { paramName2, param2 },
            { paramName3, param3 },
        };

        return LookupList(lookupId: lookupId, parameters: parameters);
    }

    private XPathNodeIterator LookupList(string lookupId, Dictionary<string, object> parameters)
    {
        DataView view = LookupService.GetList(
            lookupId: new Guid(g: lookupId),
            parameters: parameters,
            transactionId: TransactionId
        );

        XmlDocument resultDoc = new XmlDocument();
        XmlElement listElement = resultDoc.CreateElement(name: "list");
        resultDoc.AppendChild(newChild: listElement);

        foreach (DataRowView rowView in view)
        {
            XmlElement itemElement = resultDoc.CreateElement(name: "item");
            listElement.AppendChild(newChild: itemElement);

            foreach (DataColumn col in rowView.Row.Table.Columns)
            {
                if (col.ColumnMapping == MappingType.Element)
                {
                    XmlElement fieldElement = resultDoc.CreateElement(name: col.ColumnName);
                    fieldElement.InnerText = XmlTools.ConvertToString(
                        val: rowView[property: col.ColumnName]
                    );
                    itemElement.AppendChild(newChild: fieldElement);
                }
                else if (col.ColumnMapping == MappingType.Attribute)
                {
                    itemElement.SetAttribute(
                        name: col.ColumnName,
                        value: XmlTools.ConvertToString(val: rowView[property: col.ColumnName])
                    );
                }
            }
        }

        XPathNavigator nav = resultDoc.CreateNavigator();
        XPathNodeIterator result = nav.Select(xpath: "/");

        return result;
    }

    public string LookupValueEx(string lookupId, XPathNavigator parameters)
    {
        Dictionary<string, object> lookupParameters = RetrieveParameters(parameters: parameters);
        object result = LookupService.GetDisplayText(
            lookupId: new Guid(g: lookupId),
            parameters: lookupParameters,
            useCache: false,
            returnMessageIfNull: false,
            transactionId: TransactionId
        );
        return XmlTools.FormatXmlString(value: result);
    }

    public string LookupOrCreate(string lookupId, string recordId, XPathNavigator createParameters)
    {
        string result = LookupValue(lookupId: lookupId, recordId: recordId);
        result = CreateLookupRecord(
            lookupId: lookupId,
            createParameters: createParameters,
            result: result
        );
        return result;
    }

    public string LookupOrCreateEx(
        string lookupId,
        XPathNavigator parameters,
        XPathNavigator createParameters
    )
    {
        string result = LookupValueEx(lookupId: lookupId, parameters: parameters);
        result = CreateLookupRecord(
            lookupId: lookupId,
            createParameters: createParameters,
            result: result
        );
        return result;
    }

    private string CreateLookupRecord(
        string lookupId,
        XPathNavigator createParameters,
        string result
    )
    {
        if (result == string.Empty)
        {
            result = XmlTools.FormatXmlString(
                value: LookupService.CreateRecord(
                    lookupId: new Guid(g: lookupId),
                    values: RetrieveParameters(parameters: createParameters),
                    transactionId: TransactionId
                )
            );
        }

        return result;
    }

    private static Dictionary<string, object> RetrieveParameters(XPathNavigator parameters)
    {
        XPathNodeIterator iter = ((XPathNodeIterator)parameters.Evaluate(xpath: "parameter"));
        var lookupParameters = new Dictionary<string, object>();
        while (iter.MoveNext())
        {
            XPathNodeIterator keyIterator = (XPathNodeIterator)iter.Current.Evaluate(xpath: "@key");
            if (keyIterator == null || keyIterator.Count == 0)
            {
                throw new Exception(message: "'key' attribute not present in the parameters.");
            }

            keyIterator.MoveNext();
            string key = keyIterator.Current.Value.ToString();

            XPathNodeIterator valueIterator = (XPathNodeIterator)
                iter.Current.Evaluate(xpath: "@value");
            if (valueIterator == null || valueIterator.Count == 0)
            {
                throw new Exception(message: "'value' attribute not present in the parameters.");
            }

            valueIterator.MoveNext();
            object value = valueIterator.Current.Value;

            lookupParameters[key: key] = value;
        }

        return lookupParameters;
    }

    public string NormalRound(string num, string place)
    {
        return NormalStaticRound(num: num, place: place);
    }

    public static string NormalStaticRound(string num, string place)
    {
        decimal num1;
        int num2;
        try
        {
            try
            {
                num1 = XmlConvert.ToDecimal(s: num);
            }
            catch
            {
                double dnum1 = Double.Parse(
                    s: num,
                    style: NumberStyles.Float,
                    provider: CultureInfo.InvariantCulture
                );
                num1 = Convert.ToDecimal(value: dnum1);
            }

            num2 = XmlConvert.ToInt32(s: place);
        }
        catch
        {
            return NotANumber;
        }

        decimal res = Round(
            num: num1,
            type: "0b58b6b8-5d68-42bd-bf23-c698a9c78cbf",
            precision: (decimal)Math.Pow(x: 10, y: -num2)
        );
        return XmlConvert.ToString(value: res);
    }

    public string OrigamRound(string num, string origamRounding)
    {
        return OrigamRound(
            num: XmlConvert.ToDecimal(s: num),
            origamRounding: XmlTools.XPathArgToString(arg: origamRounding)
        );
    }

    private string OrigamRound(decimal num, string origamRounding)
    {
        decimal result;

        if (origamRounding == "")
        {
            result = num;
        }
        else
        {
            decimal precision = (decimal)
                LookupService.GetDisplayText(
                    lookupId: new Guid(g: "7d3d6933-648b-42cb-8947-0d2cb700152b"),
                    lookupValue: origamRounding,
                    transactionId: this.TransactionId
                );
            string type = this.LookupValue(
                lookupId: "994608ad-9634-439b-975a-484067f5b5a6",
                recordId: origamRounding
            );

            result = Round(num: num, type: type, precision: precision);
        }

        return XmlConvert.ToString(value: result);
    }

    public static decimal Round(decimal num, string type, decimal precision)
    {
        switch (type)
        {
            case "9ecc0d91-f4bd-411e-936d-e4a8066b38dd": // up
            {
                return RoundUD(bRoundUp: true, dbUnit: precision, dbVal: num);
            }
            case "970da659-63b1-42e5-9c5b-bfff0216a976": //down
            {
                return RoundUD(bRoundUp: false, dbUnit: precision, dbVal: num);
            }
            case "0b58b6b8-5d68-42bd-bf23-c698a9c78cbf": //arithmetic
            {
                decimal lvalue =
                    decimal.ToInt64(d: (num / precision) + (0.5m * Math.Sign(value: num)))
                    * precision;
                return lvalue;
            }
            //return decimal.Round(num, GetDecimalPlaces(precision));
        }

        throw new ArgumentOutOfRangeException(
            paramName: "type",
            actualValue: type,
            message: ResourceUtils.GetString(key: "ErrorUnknownRoundingType")
        );
    }

    public string Round(string amount)
    {
        decimal price;
        try
        {
            if (amount == "")
            {
                return "0";
            }

            try
            {
                price = XmlConvert.ToDecimal(s: amount);
            }
            catch
            {
                double dnum1 = Double.Parse(
                    s: amount,
                    style: NumberStyles.Float,
                    provider: CultureInfo.InvariantCulture
                );
                price = Convert.ToDecimal(value: dnum1);
            }
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: ResourceUtils.GetString(key: "ErrorRoundAmountInvalid"),
                innerException: ex
            );
        }

        return NormalStaticRound(num: amount, place: "0");
    }

    public string Ceiling(string amount)
    {
        string retVal;

        decimal price;
        try
        {
            if (amount == "")
            {
                return "0";
            }

            try
            {
                price = XmlConvert.ToDecimal(s: amount);
            }
            catch
            {
                double dnum1 = Double.Parse(
                    s: amount,
                    style: NumberStyles.Float,
                    provider: CultureInfo.InvariantCulture
                );
                price = Convert.ToDecimal(value: dnum1);
            }
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: ResourceUtils.GetString(key: "ErrorCeilingAmountInvalid"),
                innerException: ex
            );
        }

        retVal = XmlConvert.ToString(value: System.Math.Ceiling(d: price));
        return retVal;
    }

    private static decimal RoundUD(bool bRoundUp, decimal dbUnit, decimal dbVal)
    {
        const int ROUND_DP = 10;
        decimal dbValInUnit = dbVal / dbUnit;
        dbValInUnit = decimal.Round(d: dbValInUnit, decimals: ROUND_DP);
        if (bRoundUp) // round up
        {
            if (GetDecimalPlaces(val: dbValInUnit) > 0)
            {
                dbValInUnit = decimal.Floor(d: dbValInUnit + 1); // ceiling
            }
        }
        else // round down
        {
            dbValInUnit = decimal.Floor(d: dbValInUnit);
        }

        return (dbValInUnit * dbUnit);
    }

    private static int GetDecimalPlaces(decimal val)
    {
        const int MAX_DP = 10;
        decimal THRES = pow(basis: 0.1m, power: MAX_DP);
        if (val == 0)
        {
            return 0;
        }

        int nDecimal = 0;
        while (val - decimal.Floor(d: val) > THRES && nDecimal < MAX_DP)
        {
            val *= 10;
            nDecimal++;
        }

        return nDecimal;
    }

    private static decimal Pow(decimal basis, int power)
    {
        return pow(basis: basis, power: power);
    }

    private static decimal pow(decimal basis, int power)
    {
        decimal res = 1;
        for (int i = 0; i < power; i++, res *= basis)
        {
            ;
        }

        return res;
    }

    public static string ResizeImage(string inputData, int width, int height)
    {
        byte[] inBytes = System.Convert.FromBase64String(s: inputData);

        return System.Convert.ToBase64String(
            inArray: ImageResizer.FixedSizeBytesInBytesOut(
                imgBytes: inBytes,
                width: width,
                height: height
            )
        );
    }

    public static string ResizeImage(
        string inputData,
        int width,
        int height,
        string keepAspectRatio,
        string outFormat
    )
    {
        byte[] inBytes = Convert.FromBase64String(s: inputData);

        bool keepRatio;
        // check input parameters format
        try
        {
            keepRatio = XmlConvert.ToBoolean(s: keepAspectRatio);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: "'keepAspectRatio' parameter isn't in a bool format",
                innerException: ex
            );
        }

        return Convert.ToBase64String(
            inArray: ImageResizer.ResizeBytesInBytesOut(
                imgBytes: inBytes,
                width: width,
                height: height,
                keepAspectRatio: keepRatio,
                outFormat: outFormat
            )
        );
    }

    public static XPathNodeIterator GetImageDimensions(string inputData)
    {
        byte[] inBytes = Convert.FromBase64String(s: inputData);
        int[] dimensions = ImageResizer.GetImageDimensions(imgBytes: inBytes);
        XmlDocument resultDoc = new XmlDocument();
        XmlElement rootElement = resultDoc.CreateElement(name: "ROOT");
        resultDoc.AppendChild(newChild: rootElement);
        XmlElement dimenstionElement = resultDoc.CreateElement(name: "Dimensions");
        // width
        XmlAttribute widthAttr = resultDoc.CreateAttribute(name: "Width");
        widthAttr.Value = dimensions[0].ToString();
        dimenstionElement.Attributes.Append(node: widthAttr);
        // height
        XmlAttribute heightAttr = resultDoc.CreateAttribute(name: "Height");
        heightAttr.Value = dimensions[1].ToString();
        dimenstionElement.Attributes.Append(node: heightAttr);
        // add dimensoun element
        rootElement.AppendChild(newChild: dimenstionElement);

        XPathNavigator nav = resultDoc.CreateNavigator();
        XPathNodeIterator result = nav.Select(xpath: "/");

        return result;
    }

    public static string isnull(string value1, string value2)
    {
        return (value1 == "" ? value2 : value1);
    }

    public static string isnull(string value1, string value2, string value3)
    {
        string part1 = isnull(value1: value1, value2: value2);

        return (part1 == "" ? value3 : part1);
    }

    public static string isnull(string value1, string value2, string value3, string value4)
    {
        string part1 = isnull(value1: value1, value2: value2, value3: value3);

        return (part1 == "" ? value4 : part1);
    }

    public static string iif(object condition, string trueResult, string falseResult)
    {
        bool boolCondition = false;

        if (condition is bool)
        {
            boolCondition = (bool)condition;
        }
        else
        {
            string s = condition.ToString().Trim();
            if ((s == "1") || (s == "true"))
            {
                boolCondition = true;
            }

            if (!(s == "0") && !(s == "false"))
            {
                boolCondition = (condition.ToString() == "" ? false : true);
            }
        }

        return (boolCondition ? trueResult : falseResult);
    }

    public static string EncodeDataForUri(string input)
    {
        return MyUri.EscapeDataString(stringToEscape: input);
    }

    public static string DecodeDataFromUri(string input)
    {
        return MyUri.UnescapeDataString(stringToUnescape: input);
    }

    public bool IsUriValid(string url)
    {
        try
        {
            Uri u = new Uri(uriString: url);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public static string AddDays(string date, string days)
    {
        string result;

        result = XmlTools.FormatXmlDateTime(
            date: XmlConvert
                .ToDateTime(s: date, dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind)
                .AddDays(value: XmlConvert.ToDouble(s: days))
        );

        if (log.IsDebugEnabled)
        {
            if (result == "" | result == null)
            {
                log.Debug(message: "AddDays: empty");
            }
        }

        return result;
    }

    public static string AddHours(string date, string hours)
    {
        string result;

        result = XmlTools.FormatXmlDateTime(
            date: XmlConvert
                .ToDateTime(s: date, dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind)
                .AddHours(value: XmlConvert.ToDouble(s: hours))
        );

        if (log.IsDebugEnabled)
        {
            if (result == "" | result == null)
            {
                log.Debug(message: "AddHours: empty");
            }
        }

        return result;
    }

    public static string AddMinutes(string date, string minutes)
    {
        string result;

        result = XmlTools.FormatXmlDateTime(
            date: XmlConvert
                .ToDateTime(s: date, dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind)
                .AddMinutes(value: XmlConvert.ToDouble(s: minutes))
        );

        if (log.IsDebugEnabled)
        {
            if (result == "" | result == null)
            {
                log.Debug(message: "AddMinutes: empty");
            }
        }

        return result;
    }

    public static string AddYears(string date, string years)
    {
        string result;

        result = XmlTools.FormatXmlDateTime(
            date: XmlConvert
                .ToDateTime(s: date, dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind)
                .AddYears(value: XmlConvert.ToInt32(s: years))
        );

        if (log.IsDebugEnabled)
        {
            if (result == "" | result == null)
            {
                log.Debug(message: "AddYears: empty");
            }
        }

        return result;
    }

    public static string AddSeconds(string date, string seconds)
    {
        string result;

        result = XmlTools.FormatXmlDateTime(
            date: XmlConvert
                .ToDateTime(s: date, dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind)
                .AddSeconds(value: XmlConvert.ToDouble(s: seconds))
        );

        if (log.IsDebugEnabled)
        {
            if (result == "" | result == null)
            {
                log.Debug(message: "AddSeconds: empty");
            }
        }

        return result;
    }

    public static string AddMonths(string date, string months)
    {
        System.DateTime testDate;
        int numMonth;
        string retVal;

        try
        {
            testDate = XmlConvert.ToDateTime(
                s: date,
                dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
            );
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: ResourceUtils.GetString(key: "ErrorAddMonthsDateInvalid"),
                innerException: ex
            );
        }

        try
        {
            numMonth = XmlConvert.ToInt32(s: months);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: ResourceUtils.GetString(key: "ErrorAddMonthsMonthsInvalid"),
                innerException: ex
            );
        }

        retVal = XmlTools.FormatXmlDateTime(date: testDate.AddMonths(months: numMonth));

        if (log.IsDebugEnabled)
        {
            if (retVal == "" | retVal == null)
            {
                log.Debug(message: "AddMonths: empty");
            }
        }

        return retVal;
    }

    public static int DifferenceInDays(string periodStart, string periodEnd)
    {
        DateTime periodStartDate;
        DateTime periodEndDate;

        try
        {
            periodStartDate = XmlConvert.ToDateTime(
                s: periodStart,
                dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
            );
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: ResourceUtils.GetString(key: "ErrorPeriodStartInvalid"),
                innerException: ex
            );
        }

        try
        {
            periodEndDate = XmlConvert.ToDateTime(
                s: periodEnd,
                dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
            );
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: ResourceUtils.GetString(key: "ErrorPeriodEndInvalid"),
                innerException: ex
            );
        }

        TimeSpan span = (periodEndDate - periodStartDate);

        return span.Days;
    }

    public static double DifferenceInSeconds(string periodStart, string periodEnd)
    {
        DateTime periodStartDate;
        DateTime periodEndDate;
        try
        {
            periodStartDate = XmlConvert.ToDateTime(
                s: periodStart,
                dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
            );
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: ResourceUtils.GetString(key: "ErrorPeriodStartInvalid"),
                innerException: ex
            );
        }

        try
        {
            periodEndDate = XmlConvert.ToDateTime(
                s: periodEnd,
                dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
            );
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: ResourceUtils.GetString(key: "ErrorPeriodEndInvalid"),
                innerException: ex
            );
        }

        TimeSpan span = (periodEndDate - periodStartDate);
        return span.TotalSeconds;
    }

    public static double DifferenceInMinutes(string periodStart, string periodEnd)
    {
        DateTime dateFrom;
        DateTime dateTo;
        // check input parameters format

        try
        {
            dateFrom = XmlConvert.ToDateTime(
                s: periodStart,
                dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
            );
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: ResourceUtils.GetString(key: "ErrorPeriodStartInvalid"),
                innerException: ex
            );
        }

        try
        {
            dateTo = XmlConvert.ToDateTime(
                s: periodEnd,
                dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
            );
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: ResourceUtils.GetString(key: "ErrorPeriodEndInvalid"),
                innerException: ex
            );
        }

        TimeSpan span = (dateTo - dateFrom);

        return span.TotalMinutes;
    }

    public static string UTCDateTime()
    {
        return XmlConvert.ToString(
            value: DateTime.Now,
            dateTimeOption: XmlDateTimeSerializationMode.Utc
        );
    }

    public static string LocalDateTime()
    {
        return XmlConvert.ToString(
            value: DateTime.Now,
            dateTimeOption: XmlDateTimeSerializationMode.Local
        );
    }

    public bool IsFeatureOn(string featureCode)
    {
        return ParameterService.IsFeatureOn(featureCode: featureCode);
    }

    public bool IsInRole(string roleName)
    {
        return AuthorizationProvider.Authorize(
            principal: SecurityManager.CurrentPrincipal,
            context: roleName
        );
    }

    public bool IsInState(
        string entityId,
        string fieldId,
        string currentStateValue,
        string targetStateId
    )
    {
        return StateMachineService.IsInState(
            entityId: new Guid(g: entityId),
            fieldId: new Guid(g: fieldId),
            currentStateValue: currentStateValue,
            targetStateId: new Guid(g: targetStateId)
        );
    }

    public string ActiveProfileBusinessUnitId()
    {
        UserProfile profile = UserProfileGetter();
        return profile.BusinessUnitId.ToString();
    }

    public string ActiveProfileOrganizationId()
    {
        UserProfile profile = UserProfileGetter();
        return profile.OrganizationId.ToString();
    }

    public string ActiveProfileId()
    {
        UserProfile profile = UserProfileGetter();
        return profile.Id.ToString();
    }

    public bool IsUserAuthenticated()
    {
        return SecurityManager.CurrentPrincipal.Identity.IsAuthenticated;
    }

    public string UserName()
    {
        return SecurityManager.CurrentPrincipal.Identity.Name;
    }

    public static XPathNodeIterator ToXml(string value)
    {
        return ToXml(value: value, xpath: "/");
    }

    public static XPathNodeIterator ToXml(string value, string xpath)
    {
        XmlDocument doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        doc.LoadXml(xml: value);

        XPathNavigator nav = doc.CreateNavigator();

        XPathNodeIterator result = nav.Select(xpath: xpath);

        return result;
    }

    public string GenerateSerial(string counterCode)
    {
        if (counter == null)
        {
            counter = new Counter(businessService: BusinessService);
        }

        return counter.GetNewCounter(
            counterCode: counterCode,
            date: DateTime.MinValue,
            transactionId: this.TransactionId
        );
    }

    public string GenerateSerial(string counterCode, string dateString)
    {
        if (counter == null)
        {
            counter = new Counter(businessService: BusinessService);
        }

        DateTime date = XmlConvert.ToDateTime(
            s: dateString,
            dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
        );
        return counter.GetNewCounter(
            counterCode: counterCode,
            date: date,
            transactionId: this.TransactionId
        );
    }

    public static string FormatLink(string url, string text)
    {
        return "<a href=\""
            + url
            + "\" target=\"_blank\"><u><font color=\"#0000ff\">"
            + text
            + "</font></u></a>";
    }

    public bool IsUserLockedOut(string userId)
    {
        IServiceAgent identityServiceAgent = BusinessService.GetAgent(
            serviceType: "IdentityService",
            ruleEngine: null,
            workflowEngine: null
        );
        identityServiceAgent.MethodName = "IsLockedOut";
        identityServiceAgent.Parameters.Clear();
        identityServiceAgent.Parameters[key: "UserId"] = new Guid(g: userId);
        identityServiceAgent.Run();
        return (bool)identityServiceAgent.Result;
    }

    public bool IsUserEmailConfirmed(string userId)
    {
        IServiceAgent identityServiceAgent = BusinessService.GetAgent(
            serviceType: "IdentityService",
            ruleEngine: null,
            workflowEngine: null
        );
        identityServiceAgent.MethodName = "IsEmailConfirmed";
        identityServiceAgent.Parameters.Clear();
        identityServiceAgent.Parameters[key: "UserId"] = new Guid(g: userId);
        identityServiceAgent.Run();
        return (bool)identityServiceAgent.Result;
    }

    public bool Is2FAEnforced(string userId)
    {
        IServiceAgent identityServiceAgent = BusinessService.GetAgent(
            serviceType: "IdentityService",
            ruleEngine: null,
            workflowEngine: null
        );
        identityServiceAgent.MethodName = "Is2FAEnforced";
        identityServiceAgent.Parameters.Clear();
        identityServiceAgent.Parameters[key: "UserId"] = new Guid(g: userId);
        identityServiceAgent.Run();
        return (bool)identityServiceAgent.Result;
    }

    [XsltFunction(xsltName: "null")]
    public string Null()
    {
        return null;
    }

    private decimal _statusTotal = 0;

    public void SetStatusTotal(decimal total)
    {
        _statusTotal = total;
    }

    private decimal _statusPosition = 0;

    public void IncrementStatusPosition()
    {
        _statusPosition++;
        if (log.IsDebugEnabled)
        {
            log.RunHandled(loggingAction: () =>
            {
                log.DebugFormat(
                    format: "Percent complete: {0}",
                    arg0: _statusPosition / _statusTotal * 100
                );
            });
        }
    }

    Hashtable _positions = new Hashtable();

    public void InitPosition(string id, decimal startPosition)
    {
        _positions[key: id] = startPosition;
    }

    public decimal NextPosition(string id, decimal increment)
    {
        if (!_positions.Contains(key: id))
        {
            throw new ArgumentOutOfRangeException(
                paramName: "id",
                actualValue: id,
                message: ResourceUtils.GetString(key: "ErrorIncrementFailure")
            );
        }

        decimal result = (decimal)_positions[key: id];
        result += increment;

        _positions[key: id] = result;
        ;

        return result;
    }

    public void DestroyPosition(string id)
    {
        if (!_positions.Contains(key: id))
        {
            throw new ArgumentOutOfRangeException(
                paramName: "id",
                actualValue: id,
                message: ResourceUtils.GetString(key: "ErrorRemoveFailure")
            );
        }

        _positions.Remove(key: id);
    }

    public string GetMenuId(string lookupId, string value)
    {
        return LookupService.GetMenuBinding(lookupId: new Guid(g: lookupId), value: value).MenuId;
    }

    /// <summary>
    /// Decodes number in signed overpunch format
    /// https://en.wikipedia.org/wiki/Signed_overpunch
    /// </summary>
    /// <param name="stringToDecode"></param>
    /// <param name="decimalPlaces"></param>
    /// <returns></returns>
    public static double DecodeSignedOverpunch(string stringToDecode, int decimalPlaces)
    {
        CheckIsInSignedOverpunchFormat(
            stringToDecode: stringToDecode,
            decimalPlaces: decimalPlaces
        );

        char codeChar = stringToDecode.ToUpper()[index: stringToDecode.Length - 1];
        string numberChars = stringToDecode.Substring(
            startIndex: 0,
            length: stringToDecode.Length - 1
        );

        string incompleteNumber = AddDecimalPoint(
            numberChars: numberChars,
            decimalPlaces: decimalPlaces
        );

        (int sign, char lastDigit) = GetSignAndLastChar(codeChar: codeChar);

        string resultStr = incompleteNumber + lastDigit;
        bool parseFailed = !double.TryParse(
            s: resultStr,
            style: NumberStyles.Any,
            provider: CultureInfo.InvariantCulture,
            result: out var parsedNum
        );
        if (parseFailed)
        {
            throw new Exception(
                message: $"double.Parse failed. Input to DecodeSignedOverpunch: {stringToDecode}"
            );
        }

        return sign * parsedNum;
    }

    private static void CheckIsInSignedOverpunchFormat(string stringToDecode, int decimalPlaces)
    {
        if (string.IsNullOrEmpty(value: stringToDecode))
        {
            throw new ArgumentException(message: "cannot parse null or empty string");
        }

        if (stringToDecode.Length < decimalPlaces)
        {
            throw new ArgumentException(
                message: "Number of decimal places has to be "
                    + "less than total number of characters to parse"
            );
        }

        var invalidDigitCharFound = stringToDecode
            .Take(count: stringToDecode.Length - 1)
            .Any(predicate: numChar => !char.IsDigit(c: numChar));

        if (invalidDigitCharFound)
        {
            throw new ArgumentException(
                message: $"\"{stringToDecode}\" is not in Signed Overpunch format"
            );
        }
    }

    private static string AddDecimalPoint(string numberChars, int decimalPlaces)
    {
        if (decimalPlaces == 0)
        {
            return numberChars;
        }

        var length = numberChars.Length;
        var splitIndex = length + 1 - decimalPlaces; // +1 is here because numberChars are one char shorter than the actual number
        var beforeDecPoint = numberChars.Substring(startIndex: 0, length: splitIndex);
        var afterDecPoint = numberChars.Substring(startIndex: splitIndex);
        return $"{beforeDecPoint}.{afterDecPoint}";
    }

    private static (int sign, char lastDigit) GetSignAndLastChar(char codeChar)
    {
        const int positiveDiff = 'A' - '1';
        const int negativeDiff = 'J' - '1';

        int sign;
        char lastDigit;

        switch (codeChar)
        {
            case '}':
            {
                lastDigit = '0';
                sign = -1;
                break;
            }

            case '{':
            {
                lastDigit = '0';
                sign = +1;
                break;
            }

            case 'A':
            case 'B':
            case 'C':
            case 'D':
            case 'E':
            case 'F':
            case 'G':
            case 'H':
            case 'I':
            {
                sign = +1;
                lastDigit = (char)(codeChar - positiveDiff);
                break;
            }

            case 'J':
            case 'K':
            case 'L':
            case 'M':
            case 'N':
            case 'O':
            case 'P':
            case 'Q':
            case 'R':
            {
                sign = -1;
                lastDigit = (char)(codeChar - negativeDiff);
                break;
            }

            default:
            {
                throw new ArgumentException(
                    message: $"\"{codeChar}\" is not a valid Signed overpunch character"
                );
            }
        }

        return (sign, lastDigit);
    }

    public XPathNodeIterator RandomlyDistributeValues(XPathNavigator parameters)
    {
        XPathNodeIterator iter = ((XPathNodeIterator)parameters.Evaluate(xpath: "/parameter"));

        int total = 0;
        var parameterList = new List<Tuple<string, int>>(capacity: iter.Count);

        while (iter.MoveNext())
        {
            XPathNodeIterator keyIterator = (XPathNodeIterator)
                iter.Current.Evaluate(xpath: "@value");
            if (keyIterator == null || keyIterator.Count == 0)
            {
                throw new Exception(message: "'value' attribute not present in the parameters.");
            }

            keyIterator.MoveNext();
            string key = keyIterator.Current.Value.ToString();

            XPathNodeIterator valueIterator = (XPathNodeIterator)
                iter.Current.Evaluate(xpath: "@quantity");
            if (valueIterator == null || valueIterator.Count == 0)
            {
                throw new Exception(message: "'quantity' attribute not present in the parameters.");
            }

            valueIterator.MoveNext();
            int value = Convert.ToInt32(value: valueIterator.Current.Value);
            total += value;

            parameterList.Add(item: new Tuple<string, int>(item1: key, item2: value));
        }

        string[] result = new string[total];
        int min = 0;
        int max = total - 1;

        foreach (var entry in parameterList)
        {
            (string key, int quantity) = entry;
            int used = 0;

            while (used < quantity)
            {
                int random = RandomNumber(min: min, max: max);
                if (result[random] == null)
                {
                    result[random] = key;
                    used++;
                    if (random == min && used <= quantity)
                    {
                        // set new min
                        for (int i = min; i <= max; i++)
                        {
                            if (result[i] == null)
                            {
                                min = i;
                                break;
                            }
                        }
                    }

                    if (random == max && used <= quantity)
                    {
                        // set new max
                        for (int i = max; i >= min; i--)
                        {
                            if (result[i] == null)
                            {
                                max = i;
                                break;
                            }
                        }
                    }
                }
            }
        }

        XmlDocument doc = new XmlDocument();
        XmlNode values = doc.AppendChild(newChild: doc.CreateElement(name: "values"));
        for (int i = 0; i < total; i++)
        {
            XmlElement value = (XmlElement)
                values.AppendChild(newChild: doc.CreateElement(name: "value"));
            value.InnerText = result[i];
        }

        XPathNavigator nav = doc.CreateNavigator();

        XPathNodeIterator resultIterator = nav.Select(xpath: "/");

        return resultIterator;
    }

    private int RandomNumber(int min, int max)
    {
        Random rndNum = new Random(
            Seed: int.Parse(
                s: Guid.NewGuid().ToString().Substring(startIndex: 0, length: 8),
                style: System.Globalization.NumberStyles.HexNumber
            )
        );
        return rndNum.Next(minValue: min, maxValue: max);
    }

    public double Random()
    {
        Random rndNum = new Random(
            Seed: int.Parse(
                s: Guid.NewGuid().ToString().Substring(startIndex: 0, length: 8),
                style: NumberStyles.HexNumber
            )
        );
        return rndNum.NextDouble();
    }

    public long ReferenceCount(string entityId, string value)
    {
        return DataService.ReferenceCount(
            entityId: new Guid(g: entityId),
            value: value,
            transactionId: this.TransactionId
        );
    }

    public string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }

    public static XPathNodeIterator ListDays(string startDate, string endDate)
    {
        DateTime start = XmlConvert.ToDateTime(
            s: startDate,
            dateTimeOption: XmlDateTimeSerializationMode.Local
        );
        DateTime end = XmlConvert.ToDateTime(
            s: endDate,
            dateTimeOption: XmlDateTimeSerializationMode.Local
        );
        XmlDocument resultDoc = new XmlDocument();
        XmlElement listElement = resultDoc.CreateElement(name: "list");
        resultDoc.AppendChild(newChild: listElement);

        for (DateTime date = start; date.Date <= end.Date; date = date.AddDays(value: 1))
        {
            XmlElement itemElement = resultDoc.CreateElement(name: "item");
            itemElement.InnerText = XmlConvert.ToString(
                value: date,
                dateTimeOption: XmlDateTimeSerializationMode.Local
            );
            listElement.AppendChild(newChild: itemElement);
        }

        XPathNavigator nav = resultDoc.CreateNavigator();
        XPathNodeIterator result = nav.Select(xpath: "/");

        return result;
    }

    public static bool IsDateBetween(string date, string startDate, string endDate)
    {
        DateTime d = XmlConvert.ToDateTime(
            s: date,
            dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
        );
        DateTime start = XmlConvert.ToDateTime(
            s: startDate,
            dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
        );
        DateTime end = XmlConvert.ToDateTime(
            s: endDate,
            dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
        );

        return d >= start && d <= end;
    }

    public string AddWorkingDays(string date, string days, string calendarId)
    {
        Guid calendarGuid = new Guid(g: calendarId);
        decimal shift = XmlConvert.ToDecimal(s: days);

        DateTime result = XmlConvert.ToDateTime(
            s: date,
            dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
        );

        // load holidays
        DataStructureQuery q = new DataStructureQuery(
            dataStructureId: new Guid(g: "24b52598-7679-487b-a15c-e0ab3b37b21c"),
            methodId: new Guid(g: "8225b839-f336-4171-b05d-7b9aa5d39afc")
        );

        q.Parameters.Add(
            value: new QueryParameter(_parameterName: "OrigamCalendar_parId", value: calendarGuid)
        );

        DataSet ds = LoadData(query: q);
        CalendarDataset calendar = new CalendarDataset();
        calendar.Merge(dataSet: ds);
        ds = null;

        CalendarDataset.OrigamCalendarRow calendarRow = null;

        if (calendar.OrigamCalendar.Rows.Count > 0)
        {
            calendarRow = calendar.OrigamCalendar[index: 0];
        }

        if (shift == 0)
        {
            if (IsWorkingDay(date: result, calendar: calendarRow))
            {
                return date;
            }

            shift = 1;
        }

        int direction = shift > 0 ? 1 : -1;

        while (shift != 0)
        {
            result = result.AddDays(value: direction);

            if (IsWorkingDay(date: result, calendar: calendarRow))
            {
                shift = shift - direction;
            }
        }

        return XmlTools.FormatXmlDateTime(date: result);
    }

    private DataSet LoadData(DataStructureQuery query)
    {
        var dataServiceAgent = BusinessService.GetAgent(
            serviceType: "DataService",
            ruleEngine: null,
            workflowEngine: null
        );
        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query);

        dataServiceAgent.Run();

        return dataServiceAgent.Result as DataSet;
    }

    private bool IsWorkingDay(DateTime date, CalendarDataset.OrigamCalendarRow calendar)
    {
        // if there is no calendar, we always return that this is a working day
        if (calendar == null)
        {
            return true;
        }

        // if the day is not marked as working day, we return false directly
        switch (date.DayOfWeek)
        {
            case DayOfWeek.Sunday:
            {
                if (!calendar.IsSundayWorkingDay)
                {
                    return false;
                }

                break;
            }

            case DayOfWeek.Monday:
            {
                if (!calendar.IsMondayWorkingDay)
                {
                    return false;
                }

                break;
            }

            case DayOfWeek.Tuesday:
            {
                if (!calendar.IsTuesdayWorkingDay)
                {
                    return false;
                }

                break;
            }

            case DayOfWeek.Wednesday:
            {
                if (!calendar.IsWednesdayWorkingDay)
                {
                    return false;
                }

                break;
            }

            case DayOfWeek.Thursday:
            {
                if (!calendar.IsThursdayWorkingDay)
                {
                    return false;
                }

                break;
            }

            case DayOfWeek.Friday:
            {
                if (!calendar.IsFridayWorkingDay)
                {
                    return false;
                }

                break;
            }

            case DayOfWeek.Saturday:
            {
                if (!calendar.IsSaturdayWorkingDay)
                {
                    return false;
                }

                break;
            }
        }

        // the weekday is working day, so we check if it is a holiday
        foreach (
            CalendarDataset.OrigamCalendarDetailRow holiday in calendar.GetOrigamCalendarDetailRows()
        )
        {
            if (holiday.Date == date)
            {
                return false;
            }
        }

        return true;
    }

    public string FirstDayNextMonthDate(string date)
    {
        return XmlConvert
            .ToDateTime(s: date, dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind)
            .AddMonths(months: 1)
            .ToString(format: "yyyy-MM-01");
        ;
    }

    public string LastDayOfMonth(string date)
    {
        System.DateTime testDate;
        string retVal;

        try
        {
            testDate = XmlConvert.ToDateTime(
                s: date,
                dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
            );
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: ResourceUtils.GetString(key: "ErrorDateInvalid"),
                innerException: ex
            );
        }

        int daysinMonth = DateTime.DaysInMonth(year: testDate.Year, month: testDate.Month);
        testDate = testDate.AddDays(value: daysinMonth - testDate.Day);

        retVal = XmlConvert.ToString(value: testDate, format: "yyy-MM-dd");
        return retVal;
    }

    public string Year(string date)
    {
        return XmlConvert
            .ToDateTime(s: date, dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind)
            .ToString(format: "yyyy");
        ;
    }

    public string Month(string date)
    {
        return XmlConvert
            .ToDateTime(s: date, dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind)
            .ToString(format: "MM");
        ;
    }

    public string DecimalNumber(string number)
    {
        decimal num1;

        try
        {
            num1 = XmlConvert.ToDecimal(s: number);
        }
        catch
        {
            return NotANumber;
        }

        return XmlConvert.ToString(value: num1);
    }

    /// <summary>
    /// Implements the following function
    ///    number avg(node-set)
    /// </summary>
    /// <param name="iterator"></param>
    /// <returns>The average of all the value of all the nodes in the
    /// node set</returns>
    /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
    public string avg(XPathNodeIterator iterator)
    {
        decimal sum = 0;
        int count = iterator.Count;

        if (count == 0)
        {
            return NotANumber;
        }

        try
        {
            while (iterator.MoveNext())
            {
                sum += XmlConvert.ToDecimal(s: iterator.Current.Value);
            }
        }
        catch (FormatException)
        {
            return NotANumber;
        }

        return XmlConvert.ToString(value: sum / count);
    }

    public string Sum(XPathNodeIterator iterator)
    {
        return sum(iterator: iterator);
    }

    public string sum(XPathNodeIterator iterator)
    {
        decimal sum = 0;
        int count = iterator.Count;

        if (count == 0)
        {
            return "0";
        }

        try
        {
            while (iterator.MoveNext())
            {
                sum += XmlConvert.ToDecimal(s: iterator.Current.Value);
            }
        }
        catch (FormatException)
        {
            return NotANumber;
        }

        return XmlConvert.ToString(value: sum);
    }

    public string Min(XPathNodeIterator iterator)
    {
        return min(iterator: iterator);
    }

    public string min(XPathNodeIterator iterator)
    {
        decimal result = decimal.MaxValue;
        int count = iterator.Count;

        if (count == 0)
        {
            return NotANumber;
        }

        try
        {
            while (iterator.MoveNext())
            {
                result = Math.Min(
                    val1: XmlConvert.ToDecimal(s: iterator.Current.Value),
                    val2: result
                );
            }
        }
        catch (FormatException)
        {
            return NotANumber;
        }

        return XmlConvert.ToString(value: result);
    }

    public string Max(XPathNodeIterator iterator)
    {
        return max(iterator: iterator);
    }

    public string max(XPathNodeIterator iterator)
    {
        decimal result = decimal.MinValue;
        int count = iterator.Count;

        if (count == 0)
        {
            return NotANumber;
        }

        try
        {
            while (iterator.MoveNext())
            {
                result = Math.Max(
                    val1: XmlConvert.ToDecimal(s: iterator.Current.Value),
                    val2: result
                );
            }
        }
        catch (FormatException)
        {
            return NotANumber;
        }

        return XmlConvert.ToString(value: result);
    }

    public void Trace(string trace)
    {
        if (log.IsDebugEnabled)
        {
            log.Debug(message: trace);
        }
    }

    public string Uppercase(string text)
    {
        return text.ToUpper();
    }

    public string Lowercase(string text)
    {
        return text.ToLower();
    }

    public string Translate(string dictionaryId, string text)
    {
        string result = text;

        DataSet dictionary = DataService.LoadData(
            dataStructureId: new Guid(g: "9268abd0-a08e-4c97-b5f7-219eacf171c0"),
            methodId: new Guid(g: "c2cd04cd-9a47-49d8-aa03-2e07044b3c7c"),
            defaultSetId: Guid.Empty,
            sortSetId: new Guid(g: "26b8f31b-a6ce-4a0a-905d-0915855cd934"),
            transactionId: this.TransactionId,
            paramName1: "OrigamCharacterTranslationDetail_parOrigamCharacterTranslationId",
            paramValue1: new Guid(g: dictionaryId)
        );

        foreach (DataRow row in dictionary.Tables[index: 0].Rows)
        {
            text = text.Replace(
                oldValue: (string)row[columnName: "Source"],
                newValue: (string)row[columnName: "Target"]
            );
        }

        return text;
    }

    public XPathNodeIterator NextStates(
        string entityId,
        string fieldId,
        string currentStateValue,
        XPathNodeIterator row
    )
    {
        IXmlContainer doc = new XmlContainer();
        XmlElement el = doc.Xml.CreateElement(name: "row");
        doc.Xml.AppendChild(newChild: el);

        // child atributes
        XPathNodeIterator attributes = row.Clone();
        attributes.MoveNext();
        if (attributes.Current.MoveToFirstAttribute())
        {
            do
            {
                el.SetAttribute(name: attributes.Current.Name, value: attributes.Current.Value);
            } while (attributes.Current.MoveToNextAttribute());
        }

        // child elements
        row.MoveNext();
        if (row.Current.MoveToFirstChild())
        {
            do
            {
                if (row.Current.NodeType == XPathNodeType.Element)
                {
                    XmlElement childElement = doc.Xml.CreateElement(name: row.Current.Name);
                    childElement.InnerText = row.Current.Value;
                    el.AppendChild(newChild: childElement);
                }
            } while (row.Current.MoveToNext());
        }

        object[] states = StateMachineService.AllowedStateValues(
            entityId: new Guid(g: entityId),
            fieldId: new Guid(g: fieldId),
            currentStateValue: currentStateValue,
            dataRow: doc,
            transactionId: null
        );

        XmlDocument resultDoc = new XmlDocument();
        XmlElement statesElement = resultDoc.CreateElement(name: "states");
        resultDoc.AppendChild(newChild: statesElement);

        foreach (object state in states)
        {
            XmlElement stateElement = resultDoc.CreateElement(name: "state");
            stateElement.SetAttribute(name: "id", value: state.ToString());
            statesElement.AppendChild(newChild: stateElement);
        }

        XPathNavigator nav = resultDoc.CreateNavigator();
        XPathNodeIterator result = nav.Select(xpath: "/");

        return result;
    }

    public static XPathNodeIterator NodeSet(XPathNavigator nav)
    {
        XPathNodeIterator result = nav.Select(xpath: "/");

        return result;
    }

    public static string NodeToString(XPathNodeIterator node)
    {
        return NodeToString(node: node, indent: true);
    }

    public static string NodeToString(XPathNodeIterator node, bool indent)
    {
        node.MoveNext();
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb: sb);
        AsXmlTextWriter xtw = new AsXmlTextWriter(tw: sw);
        if (indent)
        {
            xtw.Formatting = Formatting.Indented;
        }
        xtw.WriteNode(navigator: node.Current);
        return sb.ToString();
    }

    public static string DecodeTextFromBase64(string input, string encoding)
    {
        byte[] blob = Convert.FromBase64String(s: input);

        if (encoding.ToUpper() != "UTF-8")
        {
            blob = Encoding.Convert(
                srcEncoding: Encoding.GetEncoding(name: encoding),
                dstEncoding: Encoding.UTF8,
                bytes: blob
            );
        }
        return Encoding.UTF8.GetString(bytes: blob);
    }

    public static string PointFromJtsk(double x, double y)
    {
        Coordinates coordinates = CoordinateConverter.JtskToWgs(x: x, y: y);
        return $"POINT({XmlConvert.ToString(value: coordinates.Longitude)} {XmlConvert.ToString(value: coordinates.Latitude)})";
    }

    public static string abs(string num)
    {
        decimal retDecValue;
        try
        {
            try
            {
                retDecValue = XmlConvert.ToDecimal(s: num);
            }
            catch
            {
                double dnum1 = Double.Parse(
                    s: num,
                    style: NumberStyles.Float,
                    provider: CultureInfo.InvariantCulture
                );
                retDecValue = Convert.ToDecimal(value: dnum1);
            }
        }
        catch (Exception ex)
        {
            throw new FormatException(
                message: ResourceUtils.GetString(key: "ErrorAbsAmountInvalid"),
                innerException: ex
            );
        }
        return XmlConvert.ToString(value: Math.Abs(value: retDecValue));
    }

    public string EvaluateXPath(object nodeset, string xpath)
    {
        return XpathEvaluator.Evaluate(nodeset: nodeset, xpath: xpath);
    }

    public string HttpRequest(string url)
    {
        return HttpRequest(url: url, method: null, content: null, contentType: null, headers: null);
    }

    public string HttpRequest(
        string url,
        string authenticationType,
        string userName,
        string password
    )
    {
        return HttpRequest(
            url: url,
            method: null,
            content: null,
            contentType: null,
            headers: null,
            authenticationType: authenticationType,
            userName: userName,
            password: password
        );
    }

    public string HttpRequest(
        string url,
        string method,
        string content,
        string contentType,
        XPathNavigator headers
    )
    {
        return HttpRequest(
            url: url,
            method: method,
            content: content,
            contentType: contentType,
            headers: headers,
            authenticationType: null,
            userName: null,
            password: null
        );
    }

    public string HttpRequest(
        string url,
        string method,
        string content,
        string contentType,
        XPathNavigator headers,
        string authenticationType,
        string userName,
        string password
    )
    {
        Hashtable headersCollection = new Hashtable();
        if (content == "")
        {
            content = null;
        }
        if (headers != null)
        {
            XPathNodeIterator iter = ((XPathNodeIterator)headers.Evaluate(xpath: "/header"));
            while (iter.MoveNext())
            {
                XPathNodeIterator keyIterator = (XPathNodeIterator)
                    iter.Current.Evaluate(xpath: "@name");
                if (keyIterator == null || keyIterator.Count == 0)
                {
                    throw new Exception(message: "'name' attribute not present in the parameters.");
                }

                keyIterator.MoveNext();
                string name = keyIterator.Current.Value.ToString();

                XPathNodeIterator valueIterator = (XPathNodeIterator)
                    iter.Current.Evaluate(xpath: "@value");
                if (valueIterator == null || valueIterator.Count == 0)
                {
                    throw new Exception(
                        message: "'value' attribute not present in the parameters."
                    );
                }

                valueIterator.MoveNext();
                object value = valueIterator.Current.Value;

                headersCollection[key: name] = value;
            }
        }

        // SEND REQUEST
        object result = HttpTools
            .SendRequest(
                request: new Request(
                    url: url,
                    method: method,
                    content: content,
                    contentType: contentType,
                    headers: headersCollection,
                    authenticationType: authenticationType,
                    userName: userName,
                    password: password
                )
            )
            .Content;

        string stringResult = result as string;
        byte[] byteResult = result as byte[];
        IXmlContainer xmlResult = result as IXmlContainer;

        if (stringResult != null)
        {
            return stringResult;
        }

        if (byteResult != null)
        {
            return Convert.ToBase64String(inArray: byteResult);
        }

        if (xmlResult != null)
        {
            return xmlResult.Xml.OuterXml;
        }

        throw new ArgumentOutOfRangeException(
            paramName: "result",
            actualValue: result,
            message: "Unknown http request result type"
        );
    }

    public string ProcessMarkdown(string text)
    {
        MarkdownSharp.Markdown md = new MarkdownSharp.Markdown();
        return md.Transform(text: text);
    }

    public static XPathNodeIterator Diff(string oldText, string newText)
    {
        var diffBuilder = new InlineDiffBuilder(differ: new Differ());
        var diff = diffBuilder.BuildDiffModel(oldText: oldText, newText: newText);
        XmlDocument resultDoc = new XmlDocument();
        XmlElement linesElement = resultDoc.CreateElement(name: "lines");
        resultDoc.AppendChild(newChild: linesElement);
        foreach (var line in diff.Lines)
        {
            XmlElement lineElement = resultDoc.CreateElement(name: "line");
            lineElement.SetAttribute(name: "changeType", value: line.Type.ToString());
            if (line.Position.HasValue)
            {
                lineElement.SetAttribute(name: "position", value: line.Position.ToString());
            }
            lineElement.InnerText = XmlTools.ConvertToString(val: line.Text);
            linesElement.AppendChild(newChild: lineElement);
        }
        XPathNavigator nav = resultDoc.CreateNavigator();
        XPathNodeIterator result = nav.Select(xpath: "/");
        return result;
    }

    public string ResourceIdByActiveProfile()
    {
        return ResourceTools.ResourceIdByActiveProfile();
    }
}
