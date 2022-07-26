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
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Rule.XsltFunctions;

public class LegacyXsltFunctionContainer : AbstractOrigamDependentXsltFunctionContainer
{
    private const string NotANumber = "NaN";

    private static readonly log4net.ILog log =
        log4net.LogManager.GetLogger(System.Reflection.MethodBase
            .GetCurrentMethod().DeclaringType);

    private ICounter counter;
    
    public string TransactionId { get; set; } = null;

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

        DateTime d = XmlConvert.ToDateTime(date);
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
            ArrayList sorted = SortedArray(iterator);
            return (string)sorted[0];
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
            ArrayList sorted = SortedArray(iterator);
            return (string)sorted[sorted.Count - 1];
        }
    }

    private ArrayList SortedArray(XPathNodeIterator iterator)
    {
        ArrayList result = new ArrayList();

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
        Hashtable parameters = new Hashtable(3);
        parameters[paramName1] = param1;
        parameters[paramName2] = param2;

        object result = LookupService.GetDisplayText(new Guid(lookupId),
            parameters, false, false, this.TransactionId);

        return XmlTools.FormatXmlString(result);
    }

    public string LookupValue(string lookupId, string paramName1, string param1,
        string paramName2, string param2, string paramName3, string param3)
    {
        Hashtable parameters = new Hashtable(3);
        parameters[paramName1] = param1;
        parameters[paramName2] = param2;
        parameters[paramName3] = param3;

        object result = LookupService.GetDisplayText(new Guid(lookupId),
            parameters, false, false, this.TransactionId);

        return XmlTools.FormatXmlString(result);
    }

    public string LookupValue(string lookupId, string paramName1, string param1,
        string paramName2, string param2, string paramName3, string param3,
        string paramName4, string param4)
    {
        Hashtable parameters = new Hashtable(4);
        parameters[paramName1] = param1;
        parameters[paramName2] = param2;
        parameters[paramName3] = param3;
        parameters[paramName4] = param4;

        object result = LookupService.GetDisplayText(new Guid(lookupId),
            parameters, false, false, this.TransactionId);

        return XmlTools.FormatXmlString(result);
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
        return OrigamRound(Convert.ToDecimal(num),
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


    public static string AddDays(string date, string days)
    {
        string result;

        result = XmlTools.FormatXmlDateTime(XmlConvert.ToDateTime(date)
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

        result = XmlTools.FormatXmlDateTime(XmlConvert.ToDateTime(date)
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

        result = XmlTools.FormatXmlDateTime(XmlConvert.ToDateTime(date)
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

        result = XmlTools.FormatXmlDateTime(XmlConvert.ToDateTime(date)
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

        result = XmlTools.FormatXmlDateTime(XmlConvert.ToDateTime(date)
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
            testDate = XmlConvert.ToDateTime(date);
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
            periodStartDate = XmlConvert.ToDateTime(periodStart);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorPeriodStartInvalid"), ex);
        }

        try
        {
            periodEndDate = XmlConvert.ToDateTime(periodEnd);
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
            periodStartDate = XmlConvert.ToDateTime(periodStart);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorPeriodStartInvalid"), ex);
        }

        try
        {
            periodEndDate = XmlConvert.ToDateTime(periodEnd);
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
            dateFrom = XmlConvert.ToDateTime(periodStart);
        }
        catch (Exception ex)
        {
            throw new FormatException(
                ResourceUtils.GetString("ErrorPeriodStartInvalid"), ex);
        }

        try
        {
            dateTo = XmlConvert.ToDateTime(periodEnd);
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

        DateTime date = XmlConvert.ToDateTime(dateString);
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
}