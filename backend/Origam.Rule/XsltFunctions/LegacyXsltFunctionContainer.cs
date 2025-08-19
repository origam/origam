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

    private static readonly log4net.ILog log =
        log4net.LogManager.GetLogger(System.Reflection.MethodBase
            .GetCurrentMethod().DeclaringType);

    private ICounter counter;
    
    public LegacyXsltFunctionContainer()
    {
    }

    public LegacyXsltFunctionContainer(ICounter counter)
    {
        this.counter = counter;
    }

    public string GetConstant(string name)
    {
        return (string)ParameterService.GetParameterValue(name,
            OrigamDataType.String);
    }

    public string GetString(string name)
    {
        return ParameterService.GetString(name);
    }

    public string GetString(string name, string arg1)
    {
        return ParameterService.GetString(name, arg1);
    }

    public string GetString(string name, string arg1, string arg2)
    {
        return ParameterService.GetString(name, arg1, arg2);
    }

    public string GetString(string name, string arg1, string arg2, string arg3)
    {
        return ParameterService.GetString(name, arg1, arg2, arg3);
    }

    public string GetString(string name, string arg1, string arg2, string arg3,
        string arg4)
    {
        return ParameterService.GetString(name, arg1, arg2, arg3, arg4);
    }

    public string GetString(string name, string arg1, string arg2, string arg3,
        string arg4, string arg5)
    {
        return ParameterService.GetString(name, arg1, arg2, arg3, arg4, arg5);
    }

    public string GetString(string name, string arg1, string arg2, string arg3,
        string arg4, string arg5, string arg6)
    {
        return ParameterService.GetString(name, arg1, arg2, arg3, arg4, arg5,
            arg6);
    }

    public string GetString(string name, string arg1, string arg2, string arg3,
        string arg4, string arg5, string arg6, string arg7)
    {
        return ParameterService.GetString(name, arg1, arg2, arg3, arg4, arg5,
            arg6, arg7);
    }

    public string GetString(string name, string arg1, string arg2, string arg3,
        string arg4, string arg5, string arg6, string arg7, string arg8)
    {
        return ParameterService.GetString(name, arg1, arg2, arg3, arg4, arg5,
            arg6, arg7, arg8);
    }

    public string GetString(string name, string arg1, string arg2, string arg3,
        string arg4, string arg5, string arg6, string arg7, string arg8,
        string arg9)
    {
        return ParameterService.GetString(name, arg1, arg2, arg3, arg4, arg5,
            arg6, arg7, arg8, arg9);
    }

    public string GetString(string name, string arg1, string arg2, string arg3,
        string arg4, string arg5, string arg6, string arg7, string arg8,
        string arg9, string arg10)
    {
        return ParameterService.GetString(name, arg1, arg2, arg3, arg4, arg5,
            arg6, arg7, arg8, arg9, arg10);
    }

    public string GetStringOrEmpty(string name)
    {
        return ParameterService.GetString(name, false);
    }

    public string GetStringOrEmpty(string name, string arg1)
    {
        return ParameterService.GetString(name, false, arg1);
    }

    public string GetStringOrEmpty(string name, string arg1, string arg2)
    {
        return ParameterService.GetString(name, false, arg1, arg2);
    }

    public string GetStringOrEmpty(string name, string arg1, string arg2,
        string arg3)
    {
        return ParameterService.GetString(name, false, arg1, arg2, arg3);
    }

    public string GetStringOrEmpty(string name, string arg1, string arg2,
        string arg3, string arg4)
    {
        return ParameterService.GetString(name, false, arg1, arg2, arg3, arg4);
    }

    public string GetStringOrEmpty(string name, string arg1, string arg2,
        string arg3, string arg4, string arg5)
    {
        return ParameterService.GetString(name, false, arg1, arg2, arg3, arg4,
            arg5);
    }

    public string GetStringOrEmpty(string name, string arg1, string arg2,
        string arg3, string arg4, string arg5, string arg6)
    {
        return ParameterService.GetString(name, false, arg1, arg2, arg3, arg4,
            arg5, arg6);
    }

    public string GetStringOrEmpty(string name, string arg1, string arg2,
        string arg3, string arg4, string arg5, string arg6, string arg7)
    {
        return ParameterService.GetString(name, false, arg1, arg2, arg3, arg4,
            arg5, arg6, arg7);
    }

    public string GetStringOrEmpty(string name, string arg1, string arg2,
        string arg3, string arg4, string arg5, string arg6, string arg7,
        string arg8)
    {
        return ParameterService.GetString(name, false, arg1, arg2, arg3, arg4,
            arg5, arg6, arg7, arg8);
    }

    public string GetStringOrEmpty(string name, string arg1, string arg2,
        string arg3, string arg4, string arg5, string arg6, string arg7,
        string arg8, string arg9)
    {
        return ParameterService.GetString(name, false, arg1, arg2, arg3, arg4,
            arg5, arg6, arg7, arg8, arg9);
    }

    public string GetStringOrEmpty(string name, string arg1, string arg2,
        string arg3, string arg4, string arg5, string arg6, string arg7,
        string arg8, string arg9, string arg10)
    {
        return ParameterService.GetString(name, false, arg1, arg2, arg3, arg4,
            arg5, arg6, arg7, arg8, arg9, arg10);
    }

    public string Plus(string number1, string number2)
    {
        return NumberOperand(number1, number2, "PLUS");
    }

    public string Minus(string number1, string number2)
    {
        return NumberOperand(number1, number2, "MINUS");
    }

    public string Mul(string number1, string number2)
    {
        return NumberOperand(number1, number2, "MUL");
    }

    public string Mod(string number1, string number2)
    {
        return NumberOperand(number1, number2, "MOD");
    }

    public string Div(string number1, string number2)
    {
        return NumberOperand(number1, number2, "DIV");
    }

    public string NumberOperand(string number1, string number2, string operand)
    {
        decimal num1;
        decimal num2;

        try
        {
            try
            {
                num1 = XmlConvert.ToDecimal(number1);
            }
            catch
            {
                double dnum1 = Double.Parse(number1, NumberStyles.Float,
                    CultureInfo.InvariantCulture);
                num1 = Convert.ToDecimal(dnum1);
            }

            try
            {
                num2 = XmlConvert.ToDecimal(number2);
            }
            catch
            {
                double dnum2 = Double.Parse(number2, NumberStyles.Float,
                    CultureInfo.InvariantCulture);
                num2 = Convert.ToDecimal(dnum2);
            }
        }
        catch
        {
            return NotANumber;
        }

        switch (operand)
        {
            case "PLUS":
                return XmlConvert.ToString(num1 + num2);
            case "MINUS":
                return XmlConvert.ToString(num1 - num2);
            case "MUL":
                return XmlConvert.ToString(num1 * num2);
            case "MOD":
                return XmlConvert.ToString(num1 % num2);
            case "DIV":
                return XmlConvert.ToString(num1 / num2);
            default:
                throw new ArgumentOutOfRangeException("operand", operand,
                    ResourceUtils.GetString("ErrorUnsupportedOperator"));
        }
    }

    public string FormatDate(string date, string format)
    {
        if (String.IsNullOrWhiteSpace(date))
        {
            return "";
        }

        DateTime d = XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind);
        return d.ToString(format);
    }

    public string FormatNumber(string number, string format)
    {
        if (String.IsNullOrWhiteSpace(number))
        {
            return "";
        }

        decimal d = XmlConvert.ToDecimal(number);
        return d.ToString(format);
    }

    public string MinString(XPathNodeIterator iterator)
    {
        if (iterator.Count == 0)
        {
            return "";
        }
        else
        {
            List<string> sorted = SortedArray(iterator);
            return sorted[0];
        }
    }

    public string MaxString(XPathNodeIterator iterator)
    {
        if (iterator.Count == 0)
        {
            return "";
        }
        else
        {
            List<string> sorted = SortedArray(iterator);
            return sorted[sorted.Count - 1];
        }
    }

    private List<string> SortedArray(XPathNodeIterator iterator)
    {
        var result = new List<string>();

        while (iterator.MoveNext())
        {
            result.Add(iterator.Current.Value);
        }

        result.Sort();

        return result;
    }

    public string LookupValue(string lookupId, string recordId)
    {
        object result = LookupService.GetDisplayText(new Guid(lookupId),
            recordId, false, false, this.TransactionId);

        return XmlTools.FormatXmlString(result);
    }

    public string LookupValue(string lookupId, string paramName1, string param1,
        string paramName2, string param2)
    {
        var parameters = new Dictionary<string, object>
        {
            [paramName1] = param1,
            [paramName2] = param2
        };

        object result = LookupService.GetDisplayText(new Guid(lookupId),
            parameters, false, false, this.TransactionId);

        return XmlTools.FormatXmlString(result);
    }

    public string LookupValue(string lookupId, string paramName1, string param1,
        string paramName2, string param2, string paramName3, string param3)
    {
       var parameters = new Dictionary<string, object>
       {
           [paramName1] = param1,
           [paramName2] = param2,
           [paramName3] = param3
       };

       object result = LookupService.GetDisplayText(new Guid(lookupId),
            parameters, false, false, this.TransactionId);

        return XmlTools.FormatXmlString(result);
    }

    public string LookupValue(string lookupId, string paramName1, string param1,
        string paramName2, string param2, string paramName3, string param3,
        string paramName4, string param4)
    {
        var parameters = new Dictionary<string, object>
        {
            [paramName1] = param1,
            [paramName2] = param2,
            [paramName3] = param3,
            [paramName4] = param4
        };

        object result = LookupService.GetDisplayText(new Guid(lookupId),
            parameters, false, false, this.TransactionId);

        return XmlTools.FormatXmlString(result);
    }

    public XPathNodeIterator LookupList(string lookupId)
    {
        return LookupList(lookupId, new Dictionary<string, object>());
    }

    public XPathNodeIterator LookupList(string lookupId, string paramName1,
        string param1)
    {
        var parameters = new Dictionary<string, object> { { paramName1, param1 } };

        return LookupList(lookupId, parameters);
    }

    public XPathNodeIterator LookupList(string lookupId, string paramName1,
        string param1, string paramName2, string param2)
    {
        var parameters = new Dictionary<string, object>
        {
            { paramName1, param1 },
            { paramName2, param2 }
        };

        return LookupList(lookupId, parameters);
    }

    public XPathNodeIterator LookupList(string lookupId, string paramName1,
        string param1, string paramName2, string param2, string paramName3,
        string param3)
    {
        var parameters = new Dictionary<string, object>
        {
            { paramName1, param1 },
            { paramName2, param2 },
            { paramName3, param3 }
        };

        return LookupList(lookupId, parameters);
    }

    private XPathNodeIterator LookupList(string lookupId, Dictionary<string, object> parameters)
    {
        DataView view = LookupService.GetList(new Guid(lookupId), parameters,
            TransactionId);

        XmlDocument resultDoc = new XmlDocument();
        XmlElement listElement = resultDoc.CreateElement("list");
        resultDoc.AppendChild(listElement);

        foreach (DataRowView rowView in view)
        {
            XmlElement itemElement = resultDoc.CreateElement("item");
            listElement.AppendChild(itemElement);

            foreach (DataColumn col in rowView.Row.Table.Columns)
            {
                if (col.ColumnMapping == MappingType.Element)
                {
                    XmlElement fieldElement =
                        resultDoc.CreateElement(col.ColumnName);
                    fieldElement.InnerText =
                        XmlTools.ConvertToString(rowView[col.ColumnName]);
                    itemElement.AppendChild(fieldElement);
                }
                else if (col.ColumnMapping == MappingType.Attribute)
                {
                    itemElement.SetAttribute(col.ColumnName,
                        XmlTools.ConvertToString(rowView[col.ColumnName]));
                }
            }
        }

        XPathNavigator nav = resultDoc.CreateNavigator();
        XPathNodeIterator result = nav.Select("/");

        return result;
    }

    public string LookupValueEx(string lookupId, XPathNavigator parameters)
    {
        Dictionary<string, object> lookupParameters = RetrieveParameters(parameters);
        object result = LookupService.GetDisplayText(
            new Guid(lookupId), lookupParameters, false, false, TransactionId);
        return XmlTools.FormatXmlString(result);
    }

    public string LookupOrCreate(string lookupId, string recordId,
        XPathNavigator createParameters)
    {
        string result = LookupValue(lookupId, recordId);
        result = CreateLookupRecord(lookupId, createParameters, result);
        return result;
    }

    public string LookupOrCreateEx(string lookupId, XPathNavigator parameters,
        XPathNavigator createParameters)
    {
        string result = LookupValueEx(lookupId, parameters);
        result = CreateLookupRecord(lookupId, createParameters, result);
        return result;
    }

    private string CreateLookupRecord(string lookupId,
        XPathNavigator createParameters, string result)
    {
        if (result == string.Empty)
        {
            result = XmlTools.FormatXmlString(LookupService.CreateRecord(
                new Guid(lookupId), RetrieveParameters(createParameters),
                TransactionId));
        }

        return result;
    }

    private static Dictionary<string, object> RetrieveParameters(XPathNavigator parameters)
    {
        XPathNodeIterator iter =
            ((XPathNodeIterator)parameters.Evaluate("parameter"));
        var lookupParameters = new Dictionary<string, object>();
        while (iter.MoveNext())
        {
            XPathNodeIterator keyIterator =
                (XPathNodeIterator)iter.Current.Evaluate("@key");
            if (keyIterator == null || keyIterator.Count == 0)
                throw new Exception(
                    "'key' attribute not present in the parameters.");
            keyIterator.MoveNext();
            string key = keyIterator.Current.Value.ToString();

            XPathNodeIterator valueIterator =
                (XPathNodeIterator)iter.Current.Evaluate("@value");
            if (valueIterator == null || valueIterator.Count == 0)
                throw new Exception(
                    "'value' attribute not present in the parameters.");
            valueIterator.MoveNext();
            object value = valueIterator.Current.Value;

            lookupParameters[key] = value;
        }

        return lookupParameters;
    }

    public string NormalRound(string num, string place)
    {
        return NormalStaticRound(num, place);
    }

    public static string NormalStaticRound(string num, string place)
    {
        decimal num1;
        int num2;
        try
        {
            try
            {
                num1 = XmlConvert.ToDecimal(num);
            }
            catch
            {
                double dnum1 = Double.Parse(num, NumberStyles.Float,
                    CultureInfo.InvariantCulture);
                num1 = Convert.ToDecimal(dnum1);
            }

            num2 = XmlConvert.ToInt32(place);
        }
        catch
        {
            return NotANumber;
        }

        decimal res = Round(num1, "0b58b6b8-5d68-42bd-bf23-c698a9c78cbf",
            (decimal)Math.Pow(10, -num2));
        return XmlConvert.ToString(res);
    }

    public string OrigamRound(string num, string origamRounding)
    {
        return OrigamRound(XmlConvert.ToDecimal(num),
            XmlTools.XPathArgToString(origamRounding));
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
            decimal precision = (decimal)LookupService.GetDisplayText(
                new Guid("7d3d6933-648b-42cb-8947-0d2cb700152b"),
                origamRounding, this.TransactionId);
            string type =
                this.LookupValue("994608ad-9634-439b-975a-484067f5b5a6",
                    origamRounding);

            result = Round(num, type, precision);
        }

        return XmlConvert.ToString(result);
    }

    public static decimal Round(decimal num, string type, decimal precision)
    {
        switch (type)
        {
            case "9ecc0d91-f4bd-411e-936d-e4a8066b38dd": // up
                return RoundUD(true, precision, num);
            case "970da659-63b1-42e5-9c5b-bfff0216a976": //down
                return RoundUD(false, precision, num);
            case "0b58b6b8-5d68-42bd-bf23-c698a9c78cbf": //arithmetic
                decimal lvalue =
                    decimal.ToInt64((num / precision) +
                                    (0.5m * Math.Sign(num))) * precision;
                return lvalue;
            //return decimal.Round(num, GetDecimalPlaces(precision));
        }

        throw new ArgumentOutOfRangeException("type", type,
            ResourceUtils.GetString("ErrorUnknownRoundingType"));
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
            else
            {
                try
                {
                    price = XmlConvert.ToDecimal(amount);
                }
                catch
                {
                    double dnum1 = Double.Parse(amount, NumberStyles.Float,
                        CultureInfo.InvariantCulture);
                    price = Convert.ToDecimal(dnum1);
                }
            }
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorRoundAmountInvalid"), ex);
        }

        return NormalStaticRound(amount, "0");
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
            else
            {
                try
                {
                    price = XmlConvert.ToDecimal(amount);
                }
                catch
                {
                    double dnum1 = Double.Parse(amount, NumberStyles.Float,
                        CultureInfo.InvariantCulture);
                    price = Convert.ToDecimal(dnum1);
                }
            }
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorCeilingAmountInvalid"), ex);
        }

        retVal = XmlConvert.ToString(System.Math.Ceiling(price));
        return retVal;
    }

    private static decimal RoundUD(bool bRoundUp, decimal dbUnit, decimal dbVal)
    {
        const int ROUND_DP = 10;
        decimal dbValInUnit = dbVal / dbUnit;
        dbValInUnit = decimal.Round(dbValInUnit, ROUND_DP);
        if (bRoundUp) // round up
        {
            if (GetDecimalPlaces(dbValInUnit) > 0)
            {
                dbValInUnit = decimal.Floor(dbValInUnit + 1); // ceiling
            }
        }
        else // round down
        {
            dbValInUnit = decimal.Floor(dbValInUnit);
        }

        return (dbValInUnit * dbUnit);
    }

    private static int GetDecimalPlaces(decimal val)
    {
        const int MAX_DP = 10;
        decimal THRES = pow(0.1m, MAX_DP);
        if (val == 0) return 0;
        int nDecimal = 0;
        while (val - decimal.Floor(val) > THRES && nDecimal < MAX_DP)
        {
            val *= 10;
            nDecimal++;
        }

        return nDecimal;
    }

    private static decimal Pow(decimal basis, int power)
    {
        return pow(basis, power);
    }

    private static decimal pow(decimal basis, int power)
    {
        decimal res = 1;
        for (int i = 0; i < power; i++, res *= basis) ;
        return res;
    }

    public static string ResizeImage(string inputData, int width, int height)
    {
        byte[] inBytes = System.Convert.FromBase64String(inputData);


        return System.Convert.ToBase64String(
            ImageResizer.FixedSizeBytesInBytesOut(
                inBytes, width, height
            )
        );
    }

    public static string ResizeImage(string inputData, int width, int height,
        string keepAspectRatio, string outFormat)
    {
        byte[] inBytes = Convert.FromBase64String(inputData);

        bool keepRatio;
        // check input parameters format
        try
        {
            keepRatio = XmlConvert.ToBoolean(keepAspectRatio);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                "'keepAspectRatio' parameter isn't in a bool format", ex);
        }

        return Convert.ToBase64String(
            ImageResizer.ResizeBytesInBytesOut(
                inBytes, width, height, keepRatio, outFormat
            )
        );
    }

    public static XPathNodeIterator GetImageDimensions(string inputData)
    {
        byte[] inBytes = Convert.FromBase64String(inputData);
        int[] dimensions = ImageResizer.GetImageDimensions(inBytes);
        XmlDocument resultDoc = new XmlDocument();
        XmlElement rootElement = resultDoc.CreateElement("ROOT");
        resultDoc.AppendChild(rootElement);
        XmlElement dimenstionElement = resultDoc.CreateElement("Dimensions");
        // width
        XmlAttribute widthAttr = resultDoc.CreateAttribute("Width");
        widthAttr.Value = dimensions[0].ToString();
        dimenstionElement.Attributes.Append(widthAttr);
        // height
        XmlAttribute heightAttr = resultDoc.CreateAttribute("Height");
        heightAttr.Value = dimensions[1].ToString();
        dimenstionElement.Attributes.Append(heightAttr);
        // add dimensoun element
        rootElement.AppendChild(dimenstionElement);

        XPathNavigator nav = resultDoc.CreateNavigator();
        XPathNodeIterator result = nav.Select("/");

        return result;
    }

    public static string isnull(string value1, string value2)
    {
        return (value1 == "" ? value2 : value1);
    }

    public static string isnull(string value1, string value2, string value3)
    {
        string part1 = isnull(value1, value2);

        return (part1 == "" ? value3 : part1);
    }

    public static string isnull(string value1, string value2, string value3,
        string value4)
    {
        string part1 = isnull(value1, value2, value3);

        return (part1 == "" ? value4 : part1);
    }

    public static string iif(object condition, string trueResult,
        string falseResult)
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
        return MyUri.EscapeDataString(input);
    }

    public static string DecodeDataFromUri(string input)
    {
        return MyUri.UnescapeDataString(input);
    }
    
    public bool IsUriValid(string url)
    {
        try
        {
            Uri u = new Uri(url);
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

        result = XmlTools.FormatXmlDateTime(XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind)
            .AddDays(XmlConvert.ToDouble(days)));

        if (log.IsDebugEnabled)
        {
            if (result == "" | result == null) log.Debug("AddDays: empty");
        }

        return result;
    }

    public static string AddHours(string date, string hours)
    {
        string result;

        result = XmlTools.FormatXmlDateTime(XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind)
            .AddHours(XmlConvert.ToDouble(hours)));

        if (log.IsDebugEnabled)
        {
            if (result == "" | result == null) log.Debug("AddHours: empty");
        }

        return result;
    }

    public static string AddMinutes(string date, string minutes)
    {
        string result;

        result = XmlTools.FormatXmlDateTime(XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind)
            .AddMinutes(XmlConvert.ToDouble(minutes)));

        if (log.IsDebugEnabled)
        {
            if (result == "" | result == null) log.Debug("AddMinutes: empty");
        }

        return result;
    }

    public static string AddYears(string date, string years)
    {
        string result;

        result = XmlTools.FormatXmlDateTime(XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind)
            .AddYears(XmlConvert.ToInt32(years)));

        if (log.IsDebugEnabled)
        {
            if (result == "" | result == null) log.Debug("AddYears: empty");
        }

        return result;
    }

    public static string AddSeconds(string date, string seconds)
    {
        string result;

        result = XmlTools.FormatXmlDateTime(XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind)
            .AddSeconds(XmlConvert.ToDouble(seconds)));

        if (log.IsDebugEnabled)
        {
            if (result == "" | result == null) log.Debug("AddSeconds: empty");
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
            testDate = XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorAddMonthsDateInvalid"), ex);
        }

        try
        {
            numMonth = XmlConvert.ToInt32(months);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorAddMonthsMonthsInvalid"), ex);
        }


        retVal = XmlTools.FormatXmlDateTime(testDate.AddMonths(numMonth));

        if (log.IsDebugEnabled)
        {
            if (retVal == "" | retVal == null) log.Debug("AddMonths: empty");
        }

        return retVal;
    }

    public static int DifferenceInDays(string periodStart, string periodEnd)
    {
        DateTime periodStartDate;
        DateTime periodEndDate;

        try
        {
            periodStartDate = XmlConvert.ToDateTime(periodStart, XmlDateTimeSerializationMode.RoundtripKind);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorPeriodStartInvalid"), ex);
        }

        try
        {
            periodEndDate = XmlConvert.ToDateTime(periodEnd, XmlDateTimeSerializationMode.RoundtripKind);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorPeriodEndInvalid"), ex);
        }

        TimeSpan span = (periodEndDate - periodStartDate);

        return span.Days;
    }

    public static double DifferenceInSeconds(
        string periodStart, string periodEnd)
    {
        DateTime periodStartDate;
        DateTime periodEndDate;
        try
        {
            periodStartDate = XmlConvert.ToDateTime(periodStart, XmlDateTimeSerializationMode.RoundtripKind);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorPeriodStartInvalid"), ex);
        }

        try
        {
            periodEndDate = XmlConvert.ToDateTime(periodEnd, XmlDateTimeSerializationMode.RoundtripKind);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorPeriodEndInvalid"), ex);
        }

        TimeSpan span = (periodEndDate - periodStartDate);
        return span.TotalSeconds;
    }

    public static double DifferenceInMinutes(string periodStart,
        string periodEnd)
    {
        DateTime dateFrom;
        DateTime dateTo;
        // check input parameters format

        try
        {
            dateFrom = XmlConvert.ToDateTime(periodStart, XmlDateTimeSerializationMode.RoundtripKind);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorPeriodStartInvalid"), ex);
        }

        try
        {
            dateTo = XmlConvert.ToDateTime(periodEnd, XmlDateTimeSerializationMode.RoundtripKind);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorPeriodEndInvalid"), ex);
        }

        TimeSpan span = (dateTo - dateFrom);

        return span.TotalMinutes;
    }

    public static string UTCDateTime()
    {
        return XmlConvert.ToString(
            DateTime.Now, XmlDateTimeSerializationMode.Utc);
    }

    public static string LocalDateTime()
    {
        return XmlConvert.ToString(
            DateTime.Now, XmlDateTimeSerializationMode.Local);
    }

    public bool IsFeatureOn(string featureCode)
    {
        return ParameterService.IsFeatureOn(featureCode);
    }

    public bool IsInRole(string roleName)
    {
        return AuthorizationProvider.Authorize(SecurityManager.CurrentPrincipal,
            roleName);
    }

    public bool IsInState(string entityId, string fieldId,
        string currentStateValue, string targetStateId)
    {
        return StateMachineService.IsInState(new Guid(entityId),
            new Guid(fieldId), currentStateValue, new Guid(targetStateId));
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
        return ToXml(value, "/");
    }

    public static XPathNodeIterator ToXml(string value, string xpath)
    {
        XmlDocument doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        doc.LoadXml(value);

        XPathNavigator nav = doc.CreateNavigator();

        XPathNodeIterator result = nav.Select(xpath);

        return result;
    }

    public string GenerateSerial(string counterCode)
    {
        if (counter == null)
        {
            counter = new Counter(BusinessService);
        }

        return counter.GetNewCounter(counterCode, DateTime.MinValue,
            this.TransactionId);
    }

    public string GenerateSerial(string counterCode, string dateString)
    {
        if (counter == null)
        {
            counter = new Counter(BusinessService);
        }

        DateTime date = XmlConvert.ToDateTime(dateString, XmlDateTimeSerializationMode.RoundtripKind);
        return counter.GetNewCounter(counterCode, date, this.TransactionId);
    }

    public static string FormatLink(string url, string text)
    {
        return "<a href=\"" + url +
               "\" target=\"_blank\"><u><font color=\"#0000ff\">" + text +
               "</font></u></a>";
    }

    public bool IsUserLockedOut(string userId)
    {
        IServiceAgent identityServiceAgent =
            BusinessService.GetAgent("IdentityService", null, null);
        identityServiceAgent.MethodName = "IsLockedOut";
        identityServiceAgent.Parameters.Clear();
        identityServiceAgent.Parameters["UserId"] = new Guid(userId);
        identityServiceAgent.Run();
        return (bool)identityServiceAgent.Result;
    }

    public bool IsUserEmailConfirmed(string userId)
    {
        IServiceAgent identityServiceAgent =
            BusinessService.GetAgent("IdentityService", null, null);
        identityServiceAgent.MethodName = "IsEmailConfirmed";
        identityServiceAgent.Parameters.Clear();
        identityServiceAgent.Parameters["UserId"] = new Guid(userId);
        identityServiceAgent.Run();
        return (bool)identityServiceAgent.Result;
    }

    public bool Is2FAEnforced(string userId)
    {
        IServiceAgent identityServiceAgent
            = BusinessService.GetAgent(
                "IdentityService", null, null);
        identityServiceAgent.MethodName = "Is2FAEnforced";
        identityServiceAgent.Parameters.Clear();
        identityServiceAgent.Parameters["UserId"] = new Guid(userId);
        identityServiceAgent.Run();
        return (bool)identityServiceAgent.Result;
    }

    [XsltFunction("null")]
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
            log.RunHandled(() =>
            {
                log.DebugFormat("Percent complete: {0}",
                    _statusPosition / _statusTotal * 100);
            });
        }
    }

    Hashtable _positions = new Hashtable();

    public void InitPosition(string id, decimal startPosition)
    {
        _positions[id] = startPosition;
    }

    public decimal NextPosition(string id, decimal increment)
    {
        if (!_positions.Contains(id))
        {
            throw new ArgumentOutOfRangeException("id", id,
                ResourceUtils.GetString("ErrorIncrementFailure"));
        }

        decimal result = (decimal)_positions[id];
        result += increment;

        _positions[id] = result;
        ;

        return result;
    }

    public void DestroyPosition(string id)
    {
        if (!_positions.Contains(id))
        {
            throw new ArgumentOutOfRangeException("id", id,
                ResourceUtils.GetString("ErrorRemoveFailure"));
        }

        _positions.Remove(id);
    }

    public string GetMenuId(string lookupId, string value)
    {
        return LookupService.GetMenuBinding(new Guid(lookupId), value).MenuId;
    }


    /// <summary>
    /// Decodes number in signed overpunch format 
    /// https://en.wikipedia.org/wiki/Signed_overpunch
    /// </summary>
    /// <param name="stringToDecode"></param>
    /// <param name="decimalPlaces"></param>
    /// <returns></returns>
    public static double DecodeSignedOverpunch(string stringToDecode,
        int decimalPlaces)
    {
        CheckIsInSignedOverpunchFormat(stringToDecode, decimalPlaces);

        char codeChar = stringToDecode.ToUpper()[stringToDecode.Length - 1];
        string numberChars =
            stringToDecode.Substring(0, stringToDecode.Length - 1);

        string incompleteNumber = AddDecimalPoint(numberChars, decimalPlaces);

        (int sign, char lastDigit) = GetSignAndLastChar(codeChar);

        string resultStr = incompleteNumber + lastDigit;
        bool parseFailed = !double.TryParse(
            resultStr,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var parsedNum);
        if (parseFailed)
        {
            throw new Exception(
                $"double.Parse failed. Input to DecodeSignedOverpunch: {stringToDecode}");
        }

        return sign * parsedNum;
    }

    private static void CheckIsInSignedOverpunchFormat(string stringToDecode,
        int decimalPlaces)
    {
        if (string.IsNullOrEmpty(stringToDecode))
        {
            throw new ArgumentException("cannot parse null or empty string");
        }

        if (stringToDecode.Length < decimalPlaces)
        {
            throw new ArgumentException(
                "Number of decimal places has to be " +
                "less than total number of characters to parse");
        }

        var invalidDigitCharFound = stringToDecode
            .Take(stringToDecode.Length - 1)
            .Any(numChar => !char.IsDigit(numChar));

        if (invalidDigitCharFound)
        {
            throw new ArgumentException(
                $"\"{stringToDecode}\" is not in Signed Overpunch format");
        }
    }

    private static string AddDecimalPoint(string numberChars,
        int decimalPlaces)
    {
        if (decimalPlaces == 0) return numberChars;

        var length = numberChars.Length;
        var splitIndex =
            length + 1 -
            decimalPlaces; // +1 is here because numberChars are one char shorter than the actual number
        var beforeDecPoint = numberChars.Substring(0, splitIndex);
        var afterDecPoint = numberChars.Substring(splitIndex);
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
                lastDigit = '0';
                sign = -1;
                break;
            case '{':
                lastDigit = '0';
                sign = +1;
                break;
            case 'A':
            case 'B':
            case 'C':
            case 'D':
            case 'E':
            case 'F':
            case 'G':
            case 'H':
            case 'I':
                sign = +1;
                lastDigit = (char)(codeChar - positiveDiff);
                break;
            case 'J':
            case 'K':
            case 'L':
            case 'M':
            case 'N':
            case 'O':
            case 'P':
            case 'Q':
            case 'R':
                sign = -1;
                lastDigit = (char)(codeChar - negativeDiff);
                break;
            default:
                throw new ArgumentException(
                    $"\"{codeChar}\" is not a valid Signed overpunch character");
        }

        return (sign, lastDigit);
    }

    public XPathNodeIterator RandomlyDistributeValues(XPathNavigator parameters)
    {
        XPathNodeIterator iter =
            ((XPathNodeIterator)parameters.Evaluate("/parameter"));

        int total = 0;
        var parameterList = new List<Tuple<string, int>>(iter.Count);

        while (iter.MoveNext())
        {
            XPathNodeIterator keyIterator =
                (XPathNodeIterator)iter.Current.Evaluate("@value");
            if (keyIterator == null || keyIterator.Count == 0)
                throw new Exception(
                    "'value' attribute not present in the parameters.");
            keyIterator.MoveNext();
            string key = keyIterator.Current.Value.ToString();

            XPathNodeIterator valueIterator =
                (XPathNodeIterator)iter.Current.Evaluate("@quantity");
            if (valueIterator == null || valueIterator.Count == 0)
                throw new Exception(
                    "'quantity' attribute not present in the parameters.");
            valueIterator.MoveNext();
            int value = Convert.ToInt32(valueIterator.Current.Value);
            total += value;

            parameterList.Add(new Tuple<string, int>(key, value));
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
                int random = RandomNumber(min, max);
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
        XmlNode values = doc.AppendChild(doc.CreateElement("values"));
        for (int i = 0; i < total; i++)
        {
            XmlElement value =
                (XmlElement)values.AppendChild(doc.CreateElement("value"));
            value.InnerText = result[i];
        }

        XPathNavigator nav = doc.CreateNavigator();

        XPathNodeIterator resultIterator = nav.Select("/");

        return resultIterator;
    }

    private int RandomNumber(int min, int max)
    {
        Random rndNum = new Random(int.Parse(
            Guid.NewGuid().ToString().Substring(0, 8),
            System.Globalization.NumberStyles.HexNumber));
        return rndNum.Next(min, max);
    }
    
    public double Random()
    {
        Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), NumberStyles.HexNumber));
        return rndNum.NextDouble();
    }
    
    public long ReferenceCount(string entityId, string value)
    {
        return DataService.ReferenceCount(new Guid(entityId), value, this.TransactionId);
    }
    
    public string GenerateId()
    {
    	return Guid.NewGuid().ToString();
    }
    
    public static XPathNodeIterator ListDays(string startDate, string endDate)
    {
        DateTime start = XmlConvert.ToDateTime(startDate, XmlDateTimeSerializationMode.RoundtripKind);
        DateTime end = XmlConvert.ToDateTime(endDate, XmlDateTimeSerializationMode.RoundtripKind);
        XmlDocument resultDoc = new XmlDocument();
        XmlElement listElement = resultDoc.CreateElement("list");
        resultDoc.AppendChild(listElement);

        for(DateTime date = start; date.Date <= end.Date; date = date.AddDays(1))
        {
            XmlElement itemElement = resultDoc.CreateElement("item");
            itemElement.InnerText = XmlConvert.ToString(date, XmlDateTimeSerializationMode.RoundtripKind);
            listElement.AppendChild(itemElement);
        }

        XPathNavigator nav = resultDoc.CreateNavigator();
        XPathNodeIterator result = nav.Select("/");

        return result;
    }
    
    public static bool IsDateBetween(string date, string startDate, string endDate)
    {
        DateTime d = XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind);
        DateTime start = XmlConvert.ToDateTime(startDate, XmlDateTimeSerializationMode.RoundtripKind);
        DateTime end = XmlConvert.ToDateTime(endDate, XmlDateTimeSerializationMode.RoundtripKind);

        return d >= start && d <= end;
    }
    
    public string AddWorkingDays(string date, string days, string calendarId)
    {
        Guid calendarGuid = new Guid(calendarId);
        decimal shift = XmlConvert.ToDecimal(days);

        DateTime result = XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind);

        // load holidays
        DataStructureQuery q = new DataStructureQuery(
            new Guid("24b52598-7679-487b-a15c-e0ab3b37b21c"), 
            new Guid("8225b839-f336-4171-b05d-7b9aa5d39afc"));

        q.Parameters.Add(new QueryParameter("OrigamCalendar_parId", calendarGuid));

        DataSet ds = LoadData(q);
        CalendarDataset calendar = new CalendarDataset();
        calendar.Merge(ds);
        ds = null;

        CalendarDataset.OrigamCalendarRow calendarRow = null;

        if(calendar.OrigamCalendar.Rows.Count > 0) calendarRow = calendar.OrigamCalendar[0];

        if(shift == 0)
        {
            if(IsWorkingDay(result, calendarRow))
            {
                return date;
            }
            else
            {
                shift = 1;
            }
        }

        int direction = shift > 0 ? 1 : -1;

        while(shift != 0)
        {
            result = result.AddDays(direction);

            if(IsWorkingDay(result, calendarRow))
            {
                shift = shift - direction;
            }
        }

        return XmlTools.FormatXmlDateTime(result);
    }
    
    private DataSet LoadData(DataStructureQuery query)
    {
        var dataServiceAgent = BusinessService.GetAgent("DataService", null, null);
        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add("Query", query);

        dataServiceAgent.Run();

        return dataServiceAgent.Result as DataSet;
    }
    
    private bool IsWorkingDay(DateTime date, CalendarDataset.OrigamCalendarRow calendar)
    {
        // if there is no calendar, we always return that this is a working day
        if(calendar == null) return true;

        // if the day is not marked as working day, we return false directly
        switch(date.DayOfWeek)
        {
            case DayOfWeek.Sunday:
                if(! calendar.IsSundayWorkingDay) return false;
                break;
            case DayOfWeek.Monday:
                if(! calendar.IsMondayWorkingDay) return false;
                break;
            case DayOfWeek.Tuesday:
                if(! calendar.IsTuesdayWorkingDay) return false;
                break;
            case DayOfWeek.Wednesday:
                if(! calendar.IsWednesdayWorkingDay) return false;
                break;
            case DayOfWeek.Thursday:
                if(! calendar.IsThursdayWorkingDay) return false;
                break;
            case DayOfWeek.Friday:
                if(! calendar.IsFridayWorkingDay) return false;
                break;
            case DayOfWeek.Saturday:
                if(! calendar.IsSaturdayWorkingDay) return false;
                break;
        }

        // the weekday is working day, so we check if it is a holiday
        foreach(CalendarDataset.OrigamCalendarDetailRow holiday in calendar.GetOrigamCalendarDetailRows())
        {
            if(holiday.Date == date) return false;
        }

        return true;
    }
    
    public string FirstDayNextMonthDate(string date)
    {
        return XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind).AddMonths(1).ToString("yyyy-MM-01");;
    }
    
    public string LastDayOfMonth(string date)
    {
        System.DateTime testDate;
        string retVal;

        try
        {
            testDate = XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind);
        }
        catch(Exception ex)
        {
            throw new FormatException(ResourceUtils.GetString("ErrorDateInvalid"), ex);
        }

        int daysinMonth = DateTime.DaysInMonth(testDate.Year, testDate.Month);
        testDate = testDate.AddDays(daysinMonth - testDate.Day);
			
        retVal = XmlConvert.ToString(testDate, "yyy-MM-dd");
        return retVal;
    }
    
    public string Year(string date)
    {
        return XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind).ToString("yyyy");;
    }

    public string Month(string date)
    {
        return XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.RoundtripKind).ToString("MM");;
    }
    
    public string DecimalNumber(string number)
    {
        decimal num1;

        try
        {
            num1 = XmlConvert.ToDecimal(number);
        }
        catch
        {
            return NotANumber;
        }

        return XmlConvert.ToString(num1);
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

        if(count == 0)
        {
            return NotANumber;
        }

        try
        { 
            while(iterator.MoveNext())
            {
                sum += XmlConvert.ToDecimal(iterator.Current.Value);
            }
				
        }
        catch(FormatException)
        {
            return NotANumber;
        }			 

        return XmlConvert.ToString(sum / count); 
    }
    
    public string Sum(XPathNodeIterator iterator)
    {
        return sum(iterator);
    }

    public string sum(XPathNodeIterator iterator)
    {
        decimal sum = 0; 
        int count = iterator.Count;

        if(count == 0)
        {
            return "0";
        }

        try
        { 
            while(iterator.MoveNext())
            {
                sum += XmlConvert.ToDecimal(iterator.Current.Value);
            }
				
        }
        catch(FormatException)
        {
            return NotANumber;
        }			 

        return XmlConvert.ToString(sum); 
    }
    
    public string Min(XPathNodeIterator iterator)
    {
        return min(iterator);
    }

    public string min(XPathNodeIterator iterator)
    {
        decimal result = decimal.MaxValue; 
        int count = iterator.Count;

        if(count == 0)
        {
            return NotANumber;
        }

        try
        { 
            while(iterator.MoveNext())
            {
                result = Math.Min(XmlConvert.ToDecimal(iterator.Current.Value), result);
            }
				
        }
        catch(FormatException)
        {
            return NotANumber;
        }			 

        return XmlConvert.ToString(result);
    }

    public string Max(XPathNodeIterator iterator)
    {
        return max(iterator);
    }

    public string max(XPathNodeIterator iterator)
    {
        decimal result = decimal.MinValue; 
        int count = iterator.Count;

        if(count == 0)
        {
            return NotANumber;
        }

        try
        { 
            while(iterator.MoveNext())
            {
                result = Math.Max(XmlConvert.ToDecimal(iterator.Current.Value), result);
            }
				
        }
        catch(FormatException)
        {
            return NotANumber;
        }			 

        return XmlConvert.ToString(result);
    }
    
    public void Trace(string trace)
    {
        if(log.IsDebugEnabled)
        {
            log.Debug(trace);
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
            new Guid("9268abd0-a08e-4c97-b5f7-219eacf171c0"), 
            new Guid("c2cd04cd-9a47-49d8-aa03-2e07044b3c7c"), 
            Guid.Empty, 
            new Guid("26b8f31b-a6ce-4a0a-905d-0915855cd934"), 
            this.TransactionId, 
            "OrigamCharacterTranslationDetail_parOrigamCharacterTranslationId", 
            new Guid(dictionaryId));

        foreach(DataRow row in dictionary.Tables[0].Rows)
        {
            text = text.Replace((string)row["Source"], (string)row["Target"]);
        }

        return text;
    }
    public XPathNodeIterator NextStates(string entityId, string fieldId, string currentStateValue, XPathNodeIterator row)
    {
        IXmlContainer doc = new XmlContainer();
        XmlElement el = doc.Xml.CreateElement("row");
        doc.Xml.AppendChild(el);
			
        // child atributes
        XPathNodeIterator attributes = row.Clone();
        attributes.MoveNext();
        if(attributes.Current.MoveToFirstAttribute())
        {
            do
            {
                el.SetAttribute(attributes.Current.Name, attributes.Current.Value);
            } while(attributes.Current.MoveToNextAttribute());
        }

        // child elements
        row.MoveNext();
        if(row.Current.MoveToFirstChild())
        {
            do
            {
                if(row.Current.NodeType == XPathNodeType.Element)
                {
                    XmlElement childElement = doc.Xml.CreateElement(row.Current.Name);
                    childElement.InnerText = row.Current.Value;
                    el.AppendChild(childElement);
                }
            } while(row.Current.MoveToNext());
        }
			
        object[] states = StateMachineService.AllowedStateValues(new Guid(entityId), new Guid(fieldId), currentStateValue, doc, null);

        XmlDocument resultDoc = new XmlDocument();
        XmlElement statesElement = resultDoc.CreateElement("states");
        resultDoc.AppendChild(statesElement);

        foreach(object state in states)
        {
            XmlElement stateElement = resultDoc.CreateElement("state");
            stateElement.SetAttribute("id", state.ToString());
            statesElement.AppendChild(stateElement);
        }

        XPathNavigator nav = resultDoc.CreateNavigator();
        XPathNodeIterator result = nav.Select("/");

        return result;
    }
    
    public static XPathNodeIterator NodeSet(XPathNavigator nav)
    {
        XPathNodeIterator result = nav.Select("/");

        return result;
    }
    
    public static string NodeToString(XPathNodeIterator node)
    {
        return NodeToString(node, true);
    }
    
    public static string NodeToString(XPathNodeIterator node, bool indent)
    {
        node.MoveNext();
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        AsXmlTextWriter xtw = new AsXmlTextWriter(sw);
        if (indent)
        {
            xtw.Formatting = Formatting.Indented;
        }
        xtw.WriteNode(node.Current);
        return sb.ToString();
    }
    
    public static string DecodeTextFromBase64(string input, string encoding)
    {
        byte[] blob = Convert.FromBase64String(input);
			
        if(encoding.ToUpper() != "UTF-8")
        {
            blob = Encoding.Convert(Encoding.GetEncoding(encoding), Encoding.UTF8, blob);
        }
        return Encoding.UTF8.GetString(blob);
    }
    
    public static string PointFromJtsk(double x, double y)
    {
        Coordinates coordinates = CoordinateConverter.JtskToWgs(x, y);
        return
            $"POINT({XmlConvert.ToString(coordinates.Longitude)} {XmlConvert.ToString(coordinates.Latitude)})";			
    }
    
    public static string abs(string num)
    {
        decimal retDecValue;
        try
        {	
            try
            {
                retDecValue = XmlConvert.ToDecimal(num);
            }
            catch
            {
                double dnum1 = Double.Parse(num, NumberStyles.Float, CultureInfo.InvariantCulture);
                retDecValue = Convert.ToDecimal(dnum1);
            }				
        }
        catch (Exception ex)
        {
            throw new FormatException(ResourceUtils.GetString("ErrorAbsAmountInvalid"), ex);
        }
        return XmlConvert.ToString(Math.Abs(retDecValue));
    }
    
    public string EvaluateXPath(object nodeset, string xpath)
    {
        return XpathEvaluator.Evaluate(nodeset, xpath);
    }
    public  string HttpRequest(string url)
	{
		return HttpRequest(url, null, null, null, null);
	}

    public  string HttpRequest(string url, string authenticationType,
        string userName, string password)
    {
        return HttpRequest(url, null, null, null, null, authenticationType, 
            userName, password);
    }

    public  string HttpRequest(string url, string method, string content,
        string contentType, XPathNavigator headers)
    {
        return HttpRequest(url, method, content, contentType, headers, null, null, null);
    }

	public string HttpRequest(string url, string method, string content, 
        string contentType, XPathNavigator headers, string authenticationType,
        string userName, string password)
	{
		Hashtable headersCollection = new Hashtable();
        if (content == "")
        {
            content = null;
        }
		if(headers != null)
		{
			XPathNodeIterator iter = ((XPathNodeIterator)headers.Evaluate("/header"));
			while(iter.MoveNext())
			{
				XPathNodeIterator keyIterator = (XPathNodeIterator)iter.Current.Evaluate("@name");
				if(keyIterator == null || keyIterator.Count == 0) throw new Exception("'name' attribute not present in the parameters.");
				keyIterator.MoveNext();
				string name = keyIterator.Current.Value.ToString();

				XPathNodeIterator valueIterator = (XPathNodeIterator)iter.Current.Evaluate("@value");
				if(valueIterator == null || valueIterator.Count == 0) throw new Exception("'value' attribute not present in the parameters.");
				valueIterator.MoveNext();
				object value = valueIterator.Current.Value;

				headersCollection[name] = value;
			}
		}

		// SEND REQUEST
		object result = HttpTools.SendRequest(
            new Request (
                url:url,
                method: method, 
                content: content, 
                contentType: contentType,
                headers: headersCollection, 
                authenticationType: authenticationType, 
                userName: userName,
                password: password)
            ).Content;

		string stringResult = result as string;
		byte[] byteResult = result as byte[];
        IXmlContainer xmlResult = result as IXmlContainer;

		if(stringResult != null)
		{
			return stringResult;
		}
		else if(byteResult != null)
		{
			return Convert.ToBase64String(byteResult);
		}
        else if (xmlResult != null)
        {
            return xmlResult.Xml.OuterXml;
        }
		else
		{
			throw new ArgumentOutOfRangeException("result", result, "Unknown http request result type");
		}
	}
    
    public string ProcessMarkdown(string text)
    {
        MarkdownSharp.Markdown md = new MarkdownSharp.Markdown();
        return md.Transform(text);
    }
    
    public static XPathNodeIterator Diff(string oldText, string newText)
    {
        var diffBuilder = new InlineDiffBuilder(new Differ());
        var diff = diffBuilder.BuildDiffModel(oldText, newText);
        XmlDocument resultDoc = new XmlDocument();
        XmlElement linesElement = resultDoc.CreateElement("lines");
        resultDoc.AppendChild(linesElement);
        foreach (var line in diff.Lines)
        {
            XmlElement lineElement = resultDoc.CreateElement("line");
            lineElement.SetAttribute("changeType", line.Type.ToString());
            if (line.Position.HasValue)
            {
                lineElement.SetAttribute("position", line.Position.ToString());
            }
            lineElement.InnerText = XmlTools.ConvertToString(line.Text);
            linesElement.AppendChild(lineElement);
        }
        XPathNavigator nav = resultDoc.CreateNavigator();
        XPathNodeIterator result = nav.Select("/");
        return result;
    }

    public string ResourceIdByActiveProfile()
    {
        return ResourceTools.ResourceIdByActiveProfile();
    }
}