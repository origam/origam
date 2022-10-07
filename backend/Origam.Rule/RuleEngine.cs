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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Data;
using System.Collections;
using System.Drawing;
using Origam.Services;
using Origam.UI.Common;
using Mvp.Xml.Exslt;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.RuleModel;
using Origam.Schema.WorkflowModel;
using Origam.Schema.GuiModel;
using Origam.Workbench;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;
using System.Collections.Generic;
using DiffPlex;
using DiffPlex.DiffBuilder;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Origam.Extensions;
using Origam.Service.Core;

namespace Origam.Rule
{
	/// <summary>
	/// Summary description for Functions.
	/// </summary>
	public class RuleEngine
	{
		private readonly Guid _workflowInstanceId;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static System.Xml.Serialization.XmlSerializer _ruleExceptionSerializer = 
			new System.Xml.Serialization.XmlSerializer(typeof(RuleExceptionDataCollection),
			new System.Xml.Serialization.XmlRootAttribute("RuleExceptionDataCollection"));

		private const string  NotANumber = "NaN";
		private Color NullColor = Color.FromArgb(0, 0, 0, 0);

		IXsltEngine _transformer;
		ISchemaService _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
		Counter _counter;
		IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		IDataLookupService _lookupService = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
		IServiceAgent _dataServiceAgent;

#if ORIGAM_CLIENT
		private static Hashtable _xpathRulesCache = new Hashtable();
#endif
		
		private IParameterService _parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

		public RuleEngine(Hashtable contextStores, string transactionId,
			Guid workflowInstanceId) :this(contextStores, transactionId)
		{
			_workflowInstanceId = workflowInstanceId;
		}

		public RuleEngine() : this(new Hashtable(), null)
		{
		}

		public RuleEngine(Hashtable contextStores, string transactionId) //: this()
		{
			this.TransactionId = transactionId;
			_contextStores = contextStores;

			if(_persistence == null)
			{
				throw new InvalidOperationException(ResourceUtils.GetString("ErrorInitializeEngine"));
			}

			_counter = new Counter();

#if NETSTANDARD
            XsltEngineType xsltEngineType = XsltEngineType.XslCompiledTransform;
#else
            XsltEngineType xsltEngineType = XsltEngineType.XslTransform;
#endif

            _transformer = AsTransform.GetXsltEngine(
                xsltEngineType, _persistence.SchemaProvider);
			_dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
		}

		#region Properties
        public static string ValidationNotMetMessage()
        {
            return ResourceUtils.GetString("ErrorOutputRuleFailed");
        }

        public static string ValidationContinueMessage (string message)
        {
            return ResourceUtils.GetString("DoYouWishContinue", message);
        }

        public static string ValidationWarningMessage()
        {
            return ResourceUtils.GetString("Warning");
        }

        public static void ConvertStringValueToContextValue(OrigamDataType origamDataType,
                                                    string inputString,
                                                    ref object contextValue)
        {

            if (inputString != null)
            {
                if (inputString == "" && origamDataType != OrigamDataType.String
                    && origamDataType != OrigamDataType.Memo)
                {
                    contextValue = null;
                }
                else
                {
                    switch (origamDataType)
                    {
                        case OrigamDataType.Integer:
                            contextValue = XmlConvert.ToInt32(inputString);
                            break;
                        case OrigamDataType.Long:
                            contextValue = XmlConvert.ToInt64(inputString);
                            break;
                        case OrigamDataType.UniqueIdentifier:
                            contextValue = XmlConvert.ToGuid(inputString);
                            break;
                        case OrigamDataType.Currency:
                        case OrigamDataType.Float:
                            contextValue = XmlConvert.ToDecimal(inputString);
                            break;
                        case OrigamDataType.Date:
                            contextValue = XmlConvert.ToDateTime(inputString);
                            break;
                        case OrigamDataType.Boolean:
                            contextValue = XmlConvert.ToBoolean(inputString);
                            break;
                        case OrigamDataType.String:
                        case OrigamDataType.Memo:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("dataType", origamDataType, "Unsupported data type.");
                    }
                }
            }
        }

        Hashtable _contextStores;
		private string _transactionId = null;
	
		public string TransactionId
		{
			get
			{
				return _transactionId;
			}
			set
			{
				_transactionId = value;
			}
		}
		#endregion

		#region Public Functions
		#region XSL Functions
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
			if(! _positions.Contains(id))
			{
				throw new ArgumentOutOfRangeException("id", id, ResourceUtils.GetString("ErrorIncrementFailure"));
			}

			decimal result = (decimal)_positions[id];
			result += increment;
			
			_positions[id] = result;;

			return result;
		}

		public void DestroyPosition(string id)
		{
			if(! _positions.Contains(id))
			{
				throw new ArgumentOutOfRangeException("id", id, ResourceUtils.GetString("ErrorRemoveFailure"));
			}

			_positions.Remove(id);
		}

		public string ActiveProfileId()
		{
            UserProfile profile = SecurityManager.CurrentUserProfile();
			return profile.Id.ToString();
		}

		public object ActiveProfileGuId()
		{
            UserProfile profile = SecurityManager.CurrentUserProfile();
			return profile.Id;
		}

		public string ActiveProfileBusinessUnitId()
		{
            UserProfile profile = SecurityManager.CurrentUserProfile();
		    return profile.BusinessUnitId.ToString();
		}

		public string ActiveProfileOrganizationId()
		{
            UserProfile profile = SecurityManager.CurrentUserProfile();
			return profile.OrganizationId.ToString();
		}

        public static bool IsUserAuthenticated()
        {
            return SecurityManager.CurrentPrincipal.Identity.IsAuthenticated;
        }

        public static string UserName()
        {
            return SecurityManager.CurrentPrincipal.Identity.Name;
        }

		public string GetMenuId(string lookupId, string value)
		{
			IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;

			return ls.GetMenuBinding(new Guid(lookupId), value).MenuId;
		}

		public int GetInventoryAvailability(string inventoryId, string warehouseId)
		{
			try
			{
				if(warehouseId == "") warehouseId = Guid.Empty.ToString();

				DataStructureQuery query = new DataStructureQuery(new Guid("6543c6c0-e29a-44fc-92f8-d310e179a5fc"), new Guid("782a2fa2-f5fd-4f63-99bc-5e9785d9f9d7"));

				query.Parameters.Add(new QueryParameter("InventoryAvailability_parInventory_Id", new Guid(inventoryId)));
				query.Parameters.Add(new QueryParameter("InventoryAvailability_parInventoryItem_refWarehouseId", new Guid(warehouseId)));

				DataSet ds = this.LoadData(query);

				if(ds.Tables["InventoryAvailability"].Rows.Count != 1)
				{
					return 0;
				}
				else
				{
					return (int)ds.Tables["InventoryAvailability"].Rows[0]["AvailableQuantity"];
				}
			}
			catch
			{
				return 0;
			}
		}

		public string ResourceIdByActiveProfile()
		{
			DataStructureQuery query = new DataStructureQuery(new Guid("d0d0d847-36dc-4987-95e5-43c4d8d0d78f"), new Guid("84848e4c-129c-4079-95a4-6319e21399af"));

			query.Parameters.Add(new QueryParameter("Resource_parBusinessPartnerId", ActiveProfileGuId()));

			DataSet ds = this.LoadData(query);

			if(ds.Tables["Resource"].Rows.Count == 0)
			{
				throw new Exception(ResourceUtils.GetString("ErrorNoResource"));
			}
			else if(ds.Tables["Resource"].Rows.Count > 1)
			{
				throw new Exception(ResourceUtils.GetString("ErrorMoreResources", ActiveProfileId()));
			}
			else
			{
				return ds.Tables["Resource"].Rows[0]["Id"].ToString();
			}
		}

		public static bool IsUserLockedOut(string userId)
		{
			IServiceAgent identityServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService))
				as IBusinessServicesService).GetAgent("IdentityService", null, null);
			identityServiceAgent.MethodName = "IsLockedOut";
			identityServiceAgent.Parameters.Clear();
			identityServiceAgent.Parameters["UserId"] = new Guid(userId);
			identityServiceAgent.Run();
			return (bool) identityServiceAgent.Result; 
		}

		public static bool IsUserEmailConfirmed(string userId)
		{
			IServiceAgent identityServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService))
				as IBusinessServicesService).GetAgent("IdentityService", null, null);
			identityServiceAgent.MethodName = "IsEmailConfirmed";
			identityServiceAgent.Parameters.Clear();
			identityServiceAgent.Parameters["UserId"] = new Guid(userId);
			identityServiceAgent.Run();
			return (bool)identityServiceAgent.Result;
		}

        public static bool Is2FAEnforced(string userId)
        {
            IServiceAgent identityServiceAgent 
                = (ServiceManager.Services.GetService(
                typeof(IBusinessServicesService))
                as IBusinessServicesService).GetAgent(
                "IdentityService", null, null);
            identityServiceAgent.MethodName = "Is2FAEnforced";
            identityServiceAgent.Parameters.Clear();
            identityServiceAgent.Parameters["UserId"] = new Guid(userId);
            identityServiceAgent.Run();
            return (bool)identityServiceAgent.Result;
        }

		public string GetInventoryName(string inventoryId)
		{
			DataStructureQuery query = new DataStructureQuery(new Guid("05649ca4-d692-4909-b334-bcf6fae3023b"), new Guid("764f5b79-afd3-474f-97da-2912251cfb88"));

			try
			{
				query.Parameters.Add(new QueryParameter("Inventory_parId", new Guid(inventoryId)));
			}
			catch
			{
				throw new Exception(ResourceUtils.GetString("ErrorInvalidInventoryId"));
			}

			DataSet ds = this.LoadData(query);

			if(ds.Tables["Inventory"].Rows.Count != 1)
			{
				throw new Exception(ResourceUtils.GetString("ErrorInventoryNotFound"));
			}
			else
			{
				return (string)ds.Tables["Inventory"].Rows[0]["Text"];
			}
		}

		public string InventoryEvidenceSerialNumbers (string inventoryId)
		{
			DataStructureQuery query = new DataStructureQuery(new Guid("326bc587-9629-4249-a4ac-7605fd149592"), new Guid("f4ac2fc2-978b-454a-989e-4e90badcadea"));
			string retVal;
			Guid Id;		

			try
			{
				Id = new Guid(inventoryId);
				query.Parameters.Add(new QueryParameter("Inventory_parId", new Guid(inventoryId)));
				DataSet ds = this.LoadData(query);
				retVal = XmlConvert.ToString((bool)ds.Tables["Inventory"].Rows[0]["RecordSerialNumbers"]);
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorInventoryIdInvalid"), ex);
			}
			return retVal;
		}

		
		public string HasInventoryItemMoved(string inventoryItemId, string date)
		{
			DataStructureQuery query = new DataStructureQuery(new Guid("4d88ac42-ac55-45df-9362-e2b945632ae9"), new Guid("0a829881-c28c-4279-9b48-0d8a941c4206"));

			query.Parameters.Add(new QueryParameter("InventoryOperationDetailItem_parRefInventoryItemId", new Guid(inventoryItemId)));
			query.Parameters.Add(new QueryParameter("InventoryOperationDetailItem_parRecordCreated", XmlConvert.ToDateTime(date)));

			DataSet ds = this.LoadData(query);

			if(ds.Tables["InventoryOperationDetailItem"].Rows.Count > 0)
			{
				return "true";
			}
			else
			{
				return "false";
			}
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
						double dnum1 = Double.Parse(amount, NumberStyles.Float, CultureInfo.InvariantCulture);
						price = Convert.ToDecimal(dnum1);
					}
				}
			}
			catch (Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorRoundAmountInvalid"), ex);
			}
			return NormalRound(amount, "0");
		}

		public string RoundDepreciationPlan(string amount)
		{
			return Ceiling(amount);
		}

		public string RoundPrice(string amount)
		{
			return Ceiling(amount);
		}

		public string Ceiling(string amount)
		{
			string retVal;

			decimal price;
			try
			{
				if (amount == "" )
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
						double dnum1 = Double.Parse(amount, NumberStyles.Float, CultureInfo.InvariantCulture);
						price = Convert.ToDecimal(dnum1);
					}
				}
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorCeilingAmountInvalid"), ex);
			}
			
			retVal = XmlConvert.ToString(System.Math.Ceiling(price));
			return retVal;
		}

		public string RoundAssetValue(string amount)
		{
			return Round(amount);
		}

		public static string NormalRound(string num, string place)
		{			
            decimal num1;
			int num2;
			try {
				try
				{
					num1 = XmlConvert.ToDecimal(num);
				} catch {
					double dnum1 = Double.Parse(num, NumberStyles.Float, CultureInfo.InvariantCulture);
					num1 = Convert.ToDecimal(dnum1);
				}				
				num2 = XmlConvert.ToInt32(place);
			}
			catch
			{
				return NotANumber;
			}
			decimal res = Round(num1, "0b58b6b8-5d68-42bd-bf23-c698a9c78cbf",
				(decimal) Math.Pow(10, -num2));
			return XmlConvert.ToString(res);
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

            string incompleteNumber = AddDecimalPoint(numberChars,decimalPlaces);

            (int sign, char lastDigit)= GetSignAndLastChar(codeChar);

	        string resultStr = incompleteNumber + lastDigit;
	        bool parseFailed = !double.TryParse(
		        resultStr,
		        NumberStyles.Any,
		        CultureInfo.InvariantCulture,
		        out var parsedNum);
	        if (parseFailed)
	        {
		        throw new Exception($"double.Parse failed. Input to DecodeSignedOverpunch: {stringToDecode}");
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
                    lastDigit = (char) (codeChar - positiveDiff);
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
                    lastDigit = (char) (codeChar - negativeDiff);
                    break;
                default:
                    throw new ArgumentException(
                        $"\"{codeChar}\" is not a valid Signed overpunch character");
            }
            return (sign ,lastDigit);
        }

        private static string AddDecimalPoint(string numberChars,
            int decimalPlaces)
        {
            if (decimalPlaces == 0) return numberChars;
            
            var length = numberChars.Length;
            var splitIndex = length + 1 - decimalPlaces; // +1 is here because numberChars are one char shorter than the actual number
            var beforeDecPoint = numberChars.Substring(0, splitIndex);
            var afterDecPoint = numberChars.Substring(splitIndex);
            return $"{beforeDecPoint}.{afterDecPoint}";
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

		public static string ResizeImage(string inputData, int width, int height, string keepAspectRatio, string outFormat)
		{
			byte[] inBytes = System.Convert.FromBase64String(inputData);

			bool keepRatio;
			// check input parameters format
			try
			{
				keepRatio = XmlConvert.ToBoolean(keepAspectRatio);
			}
			catch (Exception ex)
			{
				throw new FormatException("'keepAspectRatio' parameter isn't in a bool format", ex);
			}

			return System.Convert.ToBase64String(
				ImageResizer.ResizeBytesInBytesOut(
					inBytes, width, height, keepRatio, outFormat
				)
			);
		}

		public static XPathNodeIterator GetImageDimensions(string inputData)
		{
			byte[] inBytes = System.Convert.FromBase64String(inputData);
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

		public string OrigamRound(decimal num, string origamRounding)
		{
			decimal result;

			if(origamRounding == "")
			{
				result = num;
			}
			else
			{
				decimal precision = (decimal)_lookupService.GetDisplayText(new Guid("7d3d6933-648b-42cb-8947-0d2cb700152b"), origamRounding, this.TransactionId);
				string type = this.LookupValue("994608ad-9634-439b-975a-484067f5b5a6", origamRounding);

				result = Round(num, type, precision);
			}
			
			return XmlConvert.ToString(result);
		}

		public static decimal Round(decimal num, string type, decimal precision)
		{
			switch(type)
			{
				case "9ecc0d91-f4bd-411e-936d-e4a8066b38dd": // up
					return RoundUD(true, precision, num);
				case "970da659-63b1-42e5-9c5b-bfff0216a976": //down
					return RoundUD(false, precision, num);
				case "0b58b6b8-5d68-42bd-bf23-c698a9c78cbf": //arithmetic
					decimal lvalue = decimal.ToInt64((num / precision) + (0.5m * Math.Sign(num))) * precision;
					return lvalue;
					//return decimal.Round(num, GetDecimalPlaces(precision));
			}

			throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorUnknownRoundingType"));
		}

		// round up/down to certain units
		private static decimal RoundUD(bool bRoundUp, decimal dbUnit, decimal dbVal)
		{
			const int ROUND_DP = 10;
			decimal dbValInUnit = dbVal / dbUnit;
			dbValInUnit = decimal.Round(dbValInUnit, ROUND_DP);
			if (bRoundUp) // round up
			{
				if(GetDecimalPlaces(dbValInUnit) > 0)
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
		
		// get the number of decimal places
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
			for (int i = 0; i < power; i++, res *= basis);
			return res;
		}

		public static int DifferenceInDays (string periodStart, string periodEnd)
		{
			DateTime periodStartDate;
			DateTime periodEndDate;

			try
			{
				periodStartDate = XmlConvert.ToDateTime(periodStart);
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorPeriodStartInvalid"), ex);
			}

			try
			{
				periodEndDate = XmlConvert.ToDateTime(periodEnd);
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorPeriodEndInvalid"), ex);
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
			catch(Exception ex)
			{
				throw new FormatException(
                    ResourceUtils.GetString("ErrorPeriodStartInvalid"), ex);
			}
			try
			{
				periodEndDate = XmlConvert.ToDateTime(periodEnd);
			}
			catch(Exception ex)
			{
				throw new FormatException(
                    ResourceUtils.GetString("ErrorPeriodEndInvalid"), ex);
			}
			TimeSpan span = (periodEndDate - periodStartDate);
			return span.TotalSeconds;
		}

		public decimal CalculatePrice(string monthlyPrice, string periodStart, string periodEnd, string serviceBillingStart, string serviceBillingEnd, string endingService)
		{
			int dayFrom;
			int dayTo;
			DateTime periodStartDate;
			DateTime periodEndDate;
			DateTime serviceBillingStartDate;
			DateTime serviceBillingEndDate;
			bool addDay;
			decimal price;

			// check input parameters format
			try
			{
				addDay = XmlConvert.ToBoolean(endingService);
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorEndingServiceInvalid"), ex);
			}

			try
			{
				periodStartDate = XmlConvert.ToDateTime(periodStart);
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorPeriodStartInvalid"), ex);
			}

			try
			{
				periodEndDate = XmlConvert.ToDateTime(periodEnd);
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorPeriodEndInvalid"), ex);
			}

			try
			{
				serviceBillingStartDate = XmlConvert.ToDateTime(serviceBillingStart);
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorServiceBillingStartInvalid"), ex);
			}

			try
			{
				// if there is no billing end, we bill until the end of this period
				if(serviceBillingEnd == "")
				{
					serviceBillingEndDate = periodEndDate;
				}
				else
				{
					serviceBillingEndDate = XmlConvert.ToDateTime(serviceBillingEnd);
				}
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorServiceBillingStartInvalid"), ex);
			}

			try
			{
				price = XmlConvert.ToDecimal(monthlyPrice);
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorMonthlyPriceInvalid"), ex);
			}

			// if service is active for the whole month, we just return the price as it is
			if(serviceBillingStartDate <= periodStartDate & serviceBillingEndDate >= periodEndDate)
			{
				return price;
			}

			// calculate the actual billing period
			if(serviceBillingStartDate > periodStartDate)
			{
				dayFrom = serviceBillingStartDate.Day;
			}
			else
			{
				dayFrom = periodStartDate.Day;
			}

			if(serviceBillingEndDate < periodEndDate)
			{
				dayTo = serviceBillingEndDate.Day;
			}
			else
			{
				if(periodEndDate.Day > 30 & dayFrom > 30)
				{
					dayTo = periodEndDate.Day;
				}
				else
				{
					dayTo = 30;
				}
			}

			int days = (dayTo - dayFrom);

			if(days < 0)
			{
				return 0;
			}
			else
			{
				//				if(addDay)
				//					days += 2;	// we always add 1 day, because the original span is nights
				//				else
				//					days += 1;	// we always add 1 day, because the original span is nights
				//
				//				return (price / 30) * days;
				return (price / 30) * (days + 1);
			}                                   
		}

		public XPathNodeIterator LookupList(string lookupId)
		{
			return LookupList(lookupId, new Hashtable());
		}

		public XPathNodeIterator LookupList(string lookupId, string paramName1, string param1)
		{
			Hashtable parameters = new Hashtable();
			parameters.Add(paramName1, param1);

			return LookupList(lookupId, parameters);
		}

		public XPathNodeIterator LookupList(string lookupId, string paramName1, string param1, string paramName2, string param2)
		{
			Hashtable parameters = new Hashtable();
			parameters.Add(paramName1, param1);
			parameters.Add(paramName2, param2);

			return LookupList(lookupId, parameters);
		}

		public XPathNodeIterator LookupList(string lookupId, string paramName1, string param1, string paramName2, string param2, string paramName3, string param3)
		{
			Hashtable parameters = new Hashtable();
			parameters.Add(paramName1, param1);
			parameters.Add(paramName2, param2);
			parameters.Add(paramName3, param3);

			return LookupList(lookupId, parameters);
		}

		private XPathNodeIterator LookupList(string lookupId, Hashtable parameters)
		{
			DataView view = _lookupService.GetList(new Guid(lookupId), parameters, this.TransactionId);

			XmlDocument resultDoc = new XmlDocument();
			XmlElement listElement = resultDoc.CreateElement("list");
			resultDoc.AppendChild(listElement);

			foreach(DataRowView rowView in view)
			{
				XmlElement itemElement = resultDoc.CreateElement("item");
				listElement.AppendChild(itemElement);

				foreach(DataColumn col in rowView.Row.Table.Columns)
				{
					if(col.ColumnMapping == MappingType.Element)
					{
						XmlElement fieldElement = resultDoc.CreateElement(col.ColumnName);
						fieldElement.InnerText = XmlTools.ConvertToString(rowView[col.ColumnName]);
						itemElement.AppendChild(fieldElement);
					}
					else if (col.ColumnMapping == MappingType.Attribute)
					{
						itemElement.SetAttribute(col.ColumnName, XmlTools.ConvertToString(rowView[col.ColumnName]));
					}
				}
			}

			XPathNavigator nav = resultDoc.CreateNavigator();
			XPathNodeIterator result = nav.Select("/");

			return result;
		}

		public string LookupValue(string lookupId, string recordId)
		{
			object result = _lookupService.GetDisplayText(new Guid(lookupId), recordId, false, false, this.TransactionId);

			return FormatXmlString(result);
		}

		public string LookupValue(string lookupId, string paramName1, string param1, string paramName2, string param2)
		{
			Hashtable parameters = new Hashtable(3);
			parameters[paramName1] = param1;
			parameters[paramName2] = param2;

			object result = _lookupService.GetDisplayText(new Guid(lookupId), parameters, false, false, this.TransactionId);

			return FormatXmlString(result);
		}

		public string LookupValue(string lookupId, string paramName1, string param1, string paramName2, string param2, string paramName3, string param3)
		{
			Hashtable parameters = new Hashtable(3);
			parameters[paramName1] = param1;
			parameters[paramName2] = param2;
			parameters[paramName3] = param3;

			object result = _lookupService.GetDisplayText(new Guid(lookupId), parameters, false, false, this.TransactionId);

			return FormatXmlString(result);
		}

		public string LookupValue(string lookupId, string paramName1, string param1, string paramName2, string param2, string paramName3, string param3, string paramName4, string param4)
		{
			Hashtable parameters = new Hashtable(4);
			parameters[paramName1] = param1;
			parameters[paramName2] = param2;
			parameters[paramName3] = param3;
			parameters[paramName4] = param4;

			object result = _lookupService.GetDisplayText(new Guid(lookupId), parameters, false, false, this.TransactionId);

			return FormatXmlString(result);
		}

		public string LookupValueEx(string lookupId, XPathNavigator parameters)
        {
            Hashtable lookupParameters = RetrieveParameters(parameters);
            object result = _lookupService.GetDisplayText(
                new Guid(lookupId), lookupParameters, false, false, this.TransactionId);
            return FormatXmlString(result);
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

        private string CreateLookupRecord(string lookupId, XPathNavigator createParameters, string result)
        {
            if (result == string.Empty)
            {
                result = FormatXmlString(_lookupService.CreateRecord(
                    new Guid(lookupId), RetrieveParameters(createParameters), TransactionId));
            }
            return result;
        }

        private static Hashtable RetrieveParameters(XPathNavigator parameters)
        {
            XPathNodeIterator iter = ((XPathNodeIterator)parameters.Evaluate("/parameter"));
            Hashtable lookupParameters = new Hashtable(iter.Count);
            while (iter.MoveNext())
            {
                XPathNodeIterator keyIterator = (XPathNodeIterator)iter.Current.Evaluate("@key");
                if (keyIterator == null || keyIterator.Count == 0) throw new Exception("'key' attribute not present in the parameters.");
                keyIterator.MoveNext();
                string key = keyIterator.Current.Value.ToString();

                XPathNodeIterator valueIterator = (XPathNodeIterator)iter.Current.Evaluate("@value");
                if (valueIterator == null || valueIterator.Count == 0) throw new Exception("'value' attribute not present in the parameters.");
                valueIterator.MoveNext();
                object value = valueIterator.Current.Value;

                lookupParameters[key] = value;
            }
            return lookupParameters;
        }

        public XPathNodeIterator RandomlyDistributeValues(XPathNavigator parameters)
		{
			XPathNodeIterator iter = ((XPathNodeIterator)parameters.Evaluate("/parameter"));

			int total = 0;
			ArrayList parameterList = new ArrayList(iter.Count);

			while(iter.MoveNext())
			{
				XPathNodeIterator keyIterator = (XPathNodeIterator)iter.Current.Evaluate("@value");
				if(keyIterator == null || keyIterator.Count == 0) throw new Exception("'value' attribute not present in the parameters.");
				keyIterator.MoveNext();
				string key = keyIterator.Current.Value.ToString();

				XPathNodeIterator valueIterator = (XPathNodeIterator)iter.Current.Evaluate("@quantity");
				if(valueIterator == null || valueIterator.Count == 0) throw new Exception("'quantity' attribute not present in the parameters.");
				valueIterator.MoveNext();
				int value = Convert.ToInt32(valueIterator.Current.Value);
				total += value;

				parameterList.Add(new DictionaryEntry(key, value));
			}

			string[] result = new string[total];
			int min = 0;
			int max = total-1;

			foreach(DictionaryEntry entry in parameterList)
			{
				string key = (string)entry.Key;
				int quantity = (int)entry.Value;
				int used = 0;

				while(used < quantity)
				{
					int random = RandomNumber(min, max);
					if(result[random] == null)
					{
						result[random] = key;
						used++;
						if(random == min && used <= quantity)
						{
							// set new min
							for(int i = min; i <= max; i++)
							{
								if(result[i] == null)
								{
									min = i;
									break;
								}
							}
						}
						if(random == max && used <= quantity)
						{
							// set new max
							for(int i = max; i >= min; i--)
							{
								if(result[i] == null)
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
			for(int i = 0; i < total; i++)
			{
				XmlElement value = (XmlElement)values.AppendChild(doc.CreateElement("value"));
				value.InnerText = result[i];
			}

			XPathNavigator nav = doc.CreateNavigator();

			XPathNodeIterator resultIterator = nav.Select("/");

			return resultIterator;
		}

        public double Random()
        {
            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber));
            return rndNum.NextDouble();
        }

		private int RandomNumber(int min, int max)
		{
			Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber));
			return rndNum.Next(min, max);
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

		public long ReferenceCount(string entityId, string value)
		{
			return core.DataService.ReferenceCount(new Guid(entityId), value, this.TransactionId);
		}

		public static double DifferenceInMinutes(string periodStart, string periodEnd)
		{
			DateTime dateFrom;
			DateTime dateTo;
			// check input parameters format

			try
			{
				dateFrom = XmlConvert.ToDateTime(periodStart);
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorPeriodStartInvalid"), ex);
			}

			try
			{
				dateTo = XmlConvert.ToDateTime(periodEnd);
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorPeriodEndInvalid"), ex);
			}

			TimeSpan span = (dateTo - dateFrom);

			return span.TotalMinutes;
		}

		public string GenerateId()
		{
			return Guid.NewGuid().ToString();
		}

		public string GenerateSerial(string counterCode)
		{
			return _counter.GetNewCounter(counterCode, DateTime.MinValue, this.TransactionId);
		}

		public string GenerateSerial(string counterCode, string dateString)
		{
			DateTime date = XmlConvert.ToDateTime(dateString);

			return _counter.GetNewCounter(counterCode, date, this.TransactionId);
		}

		public void Trace(string trace)
		{
			if(log.IsDebugEnabled)
			{
				log.Debug(trace);
			}
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

			result = FormatXmlDateTime(XmlConvert.ToDateTime(date).AddDays(XmlConvert.ToDouble(days)));

			if(log.IsDebugEnabled)
			{
				if(result == "" | result == null) log.Debug("AddDays: empty");
			}
			return result;
		}

		public static XPathNodeIterator ListDays(string startDate, string endDate)
		{
			DateTime start = XmlConvert.ToDateTime(startDate);
			DateTime end = XmlConvert.ToDateTime(endDate);
			XmlDocument resultDoc = new XmlDocument();
			XmlElement listElement = resultDoc.CreateElement("list");
			resultDoc.AppendChild(listElement);

			for(DateTime date = start; date.Date <= end.Date; date = date.AddDays(1))
			{
				XmlElement itemElement = resultDoc.CreateElement("item");
				itemElement.InnerText = XmlConvert.ToString(date);
				listElement.AppendChild(itemElement);
			}

			XPathNavigator nav = resultDoc.CreateNavigator();
			XPathNodeIterator result = nav.Select("/");

			return result;
		}

		public static bool IsDateBetween(string date, string startDate, string endDate)
		{
			DateTime d = XmlConvert.ToDateTime(date);
			DateTime start = XmlConvert.ToDateTime(startDate);
			DateTime end = XmlConvert.ToDateTime(endDate);

			return d >= start && d <= end;
		}

		public string AddWorkingDays(string date, string days, string calendarId)
		{
			Guid calendarGuid = new Guid(calendarId);
			decimal shift = XmlConvert.ToDecimal(days);

			DateTime result = XmlConvert.ToDateTime(date);

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

			return FormatXmlDateTime(result);
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
					
		public static string AddHours(string date, string hours)
		{
			string result;

			result = FormatXmlDateTime(XmlConvert.ToDateTime(date).AddHours(XmlConvert.ToDouble(hours)));

			if(log.IsDebugEnabled)
			{
				if(result == "" | result == null) log.Debug("AddHours: empty");
			}
			return result;
		}

		public static string AddMinutes(string date, string minutes)
		{
			string result;

			result = FormatXmlDateTime(XmlConvert.ToDateTime(date).AddMinutes(XmlConvert.ToDouble(minutes)));

			if(log.IsDebugEnabled)
			{
				if(result == "" | result == null) log.Debug("AddMinutes: empty");
			}
			return result;
		}

		public static string AddYears(string date, string years)
		{
			string result;

			result = FormatXmlDateTime(XmlConvert.ToDateTime(date).AddYears(XmlConvert.ToInt32(years)));

			if(log.IsDebugEnabled)
			{
				if(result == "" | result == null) log.Debug("AddYears: empty");
			}
			return result;
		}

		public static string AddSeconds(string date, string seconds)
		{
			string result;

			result = FormatXmlDateTime(XmlConvert.ToDateTime(date).AddSeconds(XmlConvert.ToDouble(seconds)));

			if(log.IsDebugEnabled)
			{
				if(result == "" | result == null) log.Debug("AddSeconds: empty");
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
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorAddMonthsDateInvalid"), ex);
			}

			try
			{
				numMonth = XmlConvert.ToInt32(months);
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorAddMonthsMonthsInvalid"), ex);
			}
			
			
			retVal = FormatXmlDateTime(testDate.AddMonths(numMonth));
			
			if(log.IsDebugEnabled)
			{
				if(retVal == "" | retVal == null) log.Debug("AddMonths: empty");
			}
			return retVal;
		}

		public string FirstDayNextMonthDate(string date)
		{
			return XmlConvert.ToDateTime(date).AddMonths(1).ToString("yyyy-MM-01");;
		}

		public string LastDayOfMonth(string date)
		{
			System.DateTime testDate;
			string retVal;

			try
			{
				testDate = XmlConvert.ToDateTime(date);
			}
			catch(Exception ex)
			{
				throw new FormatException(ResourceUtils.GetString("ErrorDateInvalid"), ex);
			}

			int daysinMonth = DateTime.DaysInMonth(testDate.Year, testDate.Month);
			testDate = testDate.AddDays(daysinMonth - testDate.Day);
			
			retVal = XmlConvert.ToString(testDate, "yyy-MM-dd");
			return retVal;

				

			//
			//			string retVal;
			//			retVal=XmlConvert.ToDateTime(date).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
			//			return retVal;
		}

		public string Year(string date)
		{
			return XmlConvert.ToDateTime(date).ToString("yyyy");;
		}

		public string Month(string date)
		{
			return XmlConvert.ToDateTime(date).ToString("MM");;
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
					double dnum1 = Double.Parse(number1, NumberStyles.Float, CultureInfo.InvariantCulture);
					num1 = Convert.ToDecimal(dnum1);
				}
				try
				{
					num2 = XmlConvert.ToDecimal(number2);
				}
				catch
				{
					double dnum2 = Double.Parse(number2, NumberStyles.Float, CultureInfo.InvariantCulture);
					num2 = Convert.ToDecimal(dnum2);
				}
			}
			catch
			{
				return NotANumber;
			}

			switch(operand)
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
					throw new ArgumentOutOfRangeException("operand", operand, ResourceUtils.GetString("ErrorUnsupportedOperator"));
			}
		}

		public string FormatDate(string date, string format)
		{
            if(String.IsNullOrWhiteSpace(date))
            {
                return "";
            }
			DateTime d = XmlConvert.ToDateTime(date);
			return d.ToString(format);
		}

		public string FormatNumber(string number, string format)
		{
            if(String.IsNullOrWhiteSpace(number))
            {
                return "";
            }
			decimal d = XmlConvert.ToDecimal(number);
			return d.ToString(format);
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

		public string MinString(XPathNodeIterator iterator)
		{
			if(iterator.Count == 0)
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
			if(iterator.Count == 0)
			{
				return "";
			}
			else
			{
				ArrayList sorted = SortedArray(iterator);
				return (string)sorted[sorted.Count-1];
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

			DataSet dictionary = core.DataService.LoadData(
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

		private ArrayList SortedArray(XPathNodeIterator iterator)
		{
			ArrayList result = new ArrayList();

			while(iterator.MoveNext())
			{
				result.Add(iterator.Current.Value);
			}

			result.Sort();

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

		public static string isnull(string value1, string value2, string value3, string value4)
		{
			string part1 = isnull(value1, value2, value3);

			return (part1 == "" ? value4 : part1);
		}

		public static string iif(object condition, string trueResult, string falseResult)
		{
			bool boolCondition = false;

			if(condition is bool)
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

		public static bool IsFeatureOn(string featureCode)
		{
			IParameterService param = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

			return param.IsFeatureOn(featureCode);
		}

		public static bool IsInRole(string roleName)
		{
			return SecurityManager.GetAuthorizationProvider().Authorize(SecurityManager.CurrentPrincipal, roleName);
		}

		public static bool IsInState(string entityId, string fieldId, string currentStateValue, string targetStateId)
		{
			IStateMachineService sms = ServiceManager.Services.GetService(typeof(IStateMachineService)) as IStateMachineService;
			
			return sms.IsInState(new Guid(entityId), new Guid(fieldId), currentStateValue, new Guid(targetStateId));
		}

		public XPathNodeIterator NextStates(string entityId, string fieldId, string currentStateValue, XPathNodeIterator row)
		{
            IXmlContainer doc = new XmlContainer();
			XmlElement el = doc.Xml.CreateElement("row");
			doc.Xml.AppendChild(el);
			
			// child atributes
			XPathNodeIterator attributes = row.Clone();
			if(attributes.Current.MoveToFirstAttribute())
			{
				do
				{
					el.SetAttribute(attributes.Current.Name, attributes.Current.Value);
				} while(attributes.Current.MoveToNextAttribute());
			}

			// child elements
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

			IStateMachineService sms = ServiceManager.Services.GetService(typeof(IStateMachineService)) as IStateMachineService;
			
			object[] states = sms.AllowedStateValues(new Guid(entityId), new Guid(fieldId), currentStateValue, doc, null);

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
                xtw.Formatting = System.Xml.Formatting.Indented;
            }
            xtw.WriteNode(node.Current);
			return sb.ToString();
		}

		/*
		public static string DataToJson(XPathNodeIterator it)
		{
			XmlDocument resultDoc = new XmlDocument();
			it.MoveNext();
			resultDoc.ImportNode(it.Current., true);

			XmlDocument xmlData = GetXmlDocumentFromData(data);
			xmlData =

			if (rule is XPathRule) return EvaluateRule(rule as XPathRule, xmlData, contextPosition);
			if (rule is XslRule) return EvaluateRule(rule as XslRule, xmlData);

			return null;
			
		}
		*/

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
			Origam.Geo.JtskConverter.Wgs84 wgs;
			Origam.Geo.JtskConverter.Jtsk jtsk = new Origam.Geo.JtskConverter.Jtsk();
			jtsk.X = x;
			jtsk.Y = y;
			wgs = Origam.Geo.JtskConverter.JTSKtoWGS84(jtsk);

			return string.Format("POINT({0} {1})", XmlConvert.ToString(wgs.Longitude), XmlConvert.ToString(wgs.Latitude));			
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

		private static void WriteXml(XmlTextWriter xtw, XPathNavigator nav)
		{
			if(nav.HasAttributes)
			{
				nav.MoveToFirstAttribute();
				xtw.WriteAttributeString(nav.Prefix, nav.LocalName, nav.NamespaceURI, nav.Value);

				// write attributes
				while(nav.MoveToNextAttribute())
				{
					xtw.WriteAttributeString(nav.Prefix, nav.LocalName, nav.NamespaceURI, nav.Value);
				}

				// move to the parent node of the attributes
				nav.MoveToParent();
			}
			else
			{
				if(nav.NodeType == XPathNodeType.Text)
				{
					xtw.WriteString(nav.Value);
				}
			}
		}

		public string EvaluateXPath(object nodeset, string xpath)
		{
			if(nodeset is XPathNodeIterator)
			{
				return EvaluateXPath(nodeset as XPathNodeIterator, xpath);
			}
			else if (nodeset is XPathNavigator)
			{
				return EvaluateXPath(nodeset as XPathNavigator, xpath);
			}
			else
			{
				throw new ArgumentOutOfRangeException("nodeset", nodeset, "Invalid type.");
			}
		}

		private string EvaluateXPath(XPathNodeIterator iterator, string xpath)
		{
			return (string)EvaluateXPath(iterator.Current, xpath);
		}

		private string EvaluateXPath(XPathNavigator navigator, string xpath)
		{
			return (string)EvaluateXPath(xpath, false, OrigamDataType.String, navigator, null);
		}

		public static string HttpRequest(string url)
		{
			return HttpRequest(url, null, null, null, null);
		}

        public static string HttpRequest(string url, string authenticationType,
            string userName, string password)
        {
            return HttpRequest(url, null, null, null, null, authenticationType, 
                userName, password);
        }

        public static string HttpRequest(string url, string method, string content,
            string contentType, XPathNavigator headers)
        {
            return HttpRequest(url, method, content, contentType, headers, null, null, null);
        }

		public static string HttpRequest(string url, string method, string content, 
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
			object result = HttpTools.SendRequest(url, method, content, contentType,
                headersCollection, authenticationType, userName, password, false,
                null);

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

		public static string FormatLink(string url, string text)
		{
			return "<a href=\"" + url + "\" target=\"_blank\"><u><font color=\"#0000ff\">" + text + "</font></u></a>";
		}

        public static string ProcessMarkdown(string text)
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
        #endregion

        #region Other Functions

        public object EvaluateRule(IRule rule, object data,
	        XPathNodeIterator contextPosition)
        {
	       return EvaluateRule(rule, data, contextPosition, false);
        }

        public object EvaluateRule(IRule rule, object data, 
	        XPathNodeIterator contextPosition, bool parentIsTracing)
		{
			try
			{
			    IXmlContainer xmlData = GetXmlDocumentFromData(data);

			    bool ruleEvaluationDidRun = false;
			    object ruleResult = null;
			    switch (rule)
			    {
				    case XPathRule pathRule:
					    ruleResult = EvaluateRule(pathRule, xmlData, contextPosition);
					    ruleEvaluationDidRun = true;
					    break;
				    case XslRule xslRule:
					    ruleResult = EvaluateRule(xslRule, xmlData);
					    ruleEvaluationDidRun = true;
					    break;
			    }
			    
			    if ((rule.Trace == Origam.Trace.Yes ||
			         rule.Trace == Origam.Trace.InheritFromParent && parentIsTracing) &&
			        ruleEvaluationDidRun)
			    {
				    ServiceManager.Services
					    .GetService<ITracingService>()
					    .TraceRule(
						    ruleId: rule.Id, 
						    ruleName: rule.Name, 
						    ruleInput: xmlData?.Xml?.OuterXml, 
						    ruleResult: ruleResult?.ToString(),
						    workflowInstanceId: _workflowInstanceId
						);
			    }

			    return ruleResult;
			}
			catch(OrigamRuleException)
			{
				throw;
			}
			catch(Exception ex)
			{
				string errorMessage = ResourceUtils.GetString("ErrorRuleFailed", rule.Name);

				IDocumentationService svc = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;
				if(svc != null)
				{
					string doc = svc.GetDocumentation((Guid)rule.PrimaryKey["Id"], DocumentationType.RULE_EXCEPTION_MESSAGE);

					if(doc != "")
					{
						errorMessage += Environment.NewLine + doc;
					}
				}

				throw new Exception(errorMessage, ex);
			}
		}

		public RuleExceptionDataCollection EvaluateEndRule(IEndRule rule, object data)
		{
			return EvaluateEndRule(rule, data, new Hashtable(), false);
		}

		public RuleExceptionDataCollection EvaluateEndRule(IEndRule rule,
			object data, bool parentIsTracing)
		{
			return EvaluateEndRule(rule, data, new Hashtable(), parentIsTracing);
		}

		public RuleExceptionDataCollection EvaluateEndRule(IEndRule rule,
			object data, Hashtable parameters)
		{
			return EvaluateEndRule(rule, data, parameters, false);
		}

		public RuleExceptionDataCollection EvaluateEndRule(IEndRule rule,
			object data, Hashtable parameters, bool parentIsTracing)
		{
		    IXmlContainer context = GetXmlDocumentFromData(data);
		    IXmlContainer result = null;

			try
			{
				if(rule is XslRule)
				{
					XslRule xslRule = rule as XslRule;
					result = _transformer.Transform(context, xslRule.Id, parameters, this, new XsdDataStructure(), false);
				}
				else if(rule is XPathRule)
				{
					string ruleText = (string)this.EvaluateRule(rule, context, null);
					result = _transformer.Transform(context, ruleText, parameters, this, new XsdDataStructure(), false);
				}
				else
				{
					throw new Exception(ResourceUtils.GetString("ErrorOnlyXslRuleSupported"));
				}

				XmlNodeReader reader = new XmlNodeReader(result.Xml);

				RuleExceptionDataCollection exceptions = (RuleExceptionDataCollection)_ruleExceptionSerializer.Deserialize(reader);
				
				if (rule.Trace == Origam.Trace.Yes ||
				    rule.Trace == Origam.Trace.InheritFromParent && parentIsTracing)
				{
					ServiceManager.Services
						.GetService<ITracingService>()
						.TraceRule(
							ruleId: rule.Id, 
							ruleName: rule.Name, 
							ruleInput: context?.Xml?.OuterXml, 
							ruleResult: result.Xml.OuterXml,
							workflowInstanceId: _workflowInstanceId
						);
				}
				
				return exceptions;
			}
			catch(Exception ex)
			{
				throw new Exception(ResourceUtils.GetString("ErrorRuleFailed1", rule.Name), ex);
			}
		}

		public object Evaluate(AbstractSchemaItem item)
		{
			if(item is DataStructureReference)
			{
				return Evaluate(item as DataStructureReference);
			}
			else if(item is SystemFunctionCall)
			{
				return Evaluate(item as SystemFunctionCall);
			}
			else if(item is TransformationReference)
			{
				return Evaluate(item as TransformationReference);
			}
			else if(item is ReportReference)
			{
				return (item as ReportReference).ReportId;
			}
			else if(item is DataConstantReference)
			{
				return _parameterService.GetParameterValue((item as DataConstantReference).DataConstant.Id);
				//				if((item as DataConstantReference).DataConstant.DataType == OrigamDataType.Integer)
				//				{
				//					return (item as DataConstantReference).DataConstant.Value;
				//				}
				//				else
				//				{
				//					return (item as DataConstantReference).DataConstant.Value.ToString();
				//				}
			}
			else if(item is WorkflowReference)
			{
				return (item as WorkflowReference).WorkflowId;
			}
			else
			{
				throw new ArgumentOutOfRangeException("item", item, ResourceUtils.GetString("ErrorRuleInvalidType"));
			}
		}

		public string GetConstant(string name)
		{
			return (string)_parameterService.GetParameterValue(name, OrigamDataType.String);
		}

		public string GetString(string name) {return _parameterService.GetString(name);	}
		public string GetString(string name, string arg1) {	return _parameterService.GetString(name, arg1);	}
		public string GetString(string name, string arg1, string arg2) {	return _parameterService.GetString(name, arg1, arg2);	}
		public string GetString(string name, string arg1, string arg2, string arg3) {	return _parameterService.GetString(name, arg1, arg2, arg3);	}
		public string GetString(string name, string arg1, string arg2, string arg3, string arg4) {	return _parameterService.GetString(name, arg1, arg2, arg3, arg4);	}
		public string GetString(string name, string arg1, string arg2, string arg3, string arg4, string arg5) {	return _parameterService.GetString(name, arg1, arg2, arg3, arg4, arg5);	}
		public string GetString(string name, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6) {	return _parameterService.GetString(name, arg1, arg2, arg3, arg4, arg5, arg6);	}
		public string GetString(string name, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7) {	return _parameterService.GetString(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7);	}
		public string GetString(string name, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8) {	return _parameterService.GetString(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);	}
		public string GetString(string name, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9) {	return _parameterService.GetString(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);	}
		public string GetString(string name, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9, string arg10) {	return _parameterService.GetString(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);	}

        public string GetStringOrEmpty(string name) { return _parameterService.GetString(name, false); }
        public string GetStringOrEmpty(string name, string arg1) { return _parameterService.GetString(name, false, arg1); }
        public string GetStringOrEmpty(string name, string arg1, string arg2) { return _parameterService.GetString(name, false, arg1, arg2); }
        public string GetStringOrEmpty(string name, string arg1, string arg2, string arg3) { return _parameterService.GetString(name, false, arg1, arg2, arg3); }
        public string GetStringOrEmpty(string name, string arg1, string arg2, string arg3, string arg4) { return _parameterService.GetString(name, false, arg1, arg2, arg3, arg4); }
        public string GetStringOrEmpty(string name, string arg1, string arg2, string arg3, string arg4, string arg5) { return _parameterService.GetString(name, false, arg1, arg2, arg3, arg4, arg5); }
        public string GetStringOrEmpty(string name, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6) { return _parameterService.GetString(name, false, arg1, arg2, arg3, arg4, arg5, arg6); }
        public string GetStringOrEmpty(string name, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7) { return _parameterService.GetString(name, false, arg1, arg2, arg3, arg4, arg5, arg6, arg7); }
        public string GetStringOrEmpty(string name, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8) { return _parameterService.GetString(name, false, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8); }
        public string GetStringOrEmpty(string name, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9) { return _parameterService.GetString(name, false, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9); }
        public string GetStringOrEmpty(string name, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9, string arg10) { return _parameterService.GetString(name, false, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10); }

        public string GetStringVariadic(string name, bool throwException, params Object[] args) { return _parameterService.GetString(name, throwException, args); }

		public bool Merge(DataSet inout_dsTarget, DataSet in_dsSource, bool in_bTrueDelete, bool in_bPreserveChanges, bool in_bSourceIsFragment, bool preserveNewRowState)
		{
			bool result;
			DatasetTools.BeginLoadData(inout_dsTarget);
			try
			{
                MergeParams mergeParams = new MergeParams();
                mergeParams.TrueDelete = in_bTrueDelete;
                mergeParams.PreserveChanges = in_bPreserveChanges;
                mergeParams.SourceIsFragment = in_bSourceIsFragment;
                mergeParams.PreserveNewRowState = preserveNewRowState;
                mergeParams.ProfileId = ActiveProfileGuId();
				result = DatasetTools.MergeDataSet(inout_dsTarget, in_dsSource, null, mergeParams);
			}
			finally
			{
				DatasetTools.EndLoadData(inout_dsTarget);
			}

			return result;
		}
	
		public bool Merge(DataTable inout_dtTarget, DataTable in_dtSource, bool in_bTrueDelete, bool in_bPreserveChanges, bool in_bSourceIsFragment, bool preserveNewRowState)
		{
			inout_dtTarget.BeginLoadData();
			bool result;

			try
			{
                MergeParams mergeParams = new MergeParams();
                mergeParams.TrueDelete = in_bTrueDelete;
                mergeParams.PreserveChanges = in_bPreserveChanges;
                mergeParams.SourceIsFragment = in_bSourceIsFragment;
                mergeParams.PreserveNewRowState = preserveNewRowState;
                mergeParams.ProfileId = ActiveProfileGuId();
				result = DatasetTools.MergeDataTable(inout_dtTarget, in_dtSource, null, null, mergeParams);
			}
			finally
			{
				inout_dtTarget.EndLoadData();
			}

			return result;
		}
		public object EvaluateContext(string xpath, object context, OrigamDataType dataType, AbstractDataStructure targetStructure)
		{
			object result = null;

			if(!(context is XmlDocument))
			{
				if(xpath == "/")
				{
					return context;
				}
				else
				{
					// convert value to XML
					context = GetXmlDocumentFromData(context).Xml;
				}
			}
			
			if(context is XmlDocument)
			{
				if(dataType == OrigamDataType.Xml && xpath.Trim() == "/")
				{
					return context;
				}

				OrigamXsltContext ctx =  new OrigamXsltContext(new NameTable(), this);
				XPathNavigator nav = ((XmlDocument)context).CreateNavigator();
				XPathExpression expr = nav.Compile(xpath);
				expr.SetContext(ctx);
				
				if(dataType == OrigamDataType.Array)
				{
					object expressionResult = nav.Evaluate(expr);
					result = new ArrayList();

					if(expressionResult is XPathNodeIterator)
					{
						XPathNodeIterator iterator = expressionResult as XPathNodeIterator;

						while(iterator.MoveNext())
						{
							((ArrayList)result).Add(iterator.Current.Value);
						}
					}
				}
				else if(dataType != OrigamDataType.Xml)
				{
					// Result is other than XML
			
					result = nav.Evaluate(expr);

					if(result is XPathNodeIterator)
					{
						XPathNodeIterator iterator = result as XPathNodeIterator;

						if(iterator.Count == 0)
						{
							result = null;
						}
						else
						{
							iterator.MoveNext();
							result = iterator.Current.Value;
						}
					}

					switch(dataType)
					{
						case OrigamDataType.Blob:
							if(! (result is String))
							{
								throw new InvalidCastException("Only string can be converted to blob.");
							}

							result = Convert.FromBase64String((string)result);
							break;

						case OrigamDataType.Boolean:
							if(!(result is bool))
							{
								if(result == null)
								{
									result = false;
								}
								else if(result is string && 
									((string)result == "0" || (string)result == "false")
									)
								{
									result = false;
								}
								else
								{
									result = true;
								}
							}
							break;
				
						case OrigamDataType.UniqueIdentifier:
							if(result != null)
							{
								if((string)result == "")
								{
									result = null;
								}
								else
								{
									result = new Guid(result.ToString());
								}
							}
							break;

						case OrigamDataType.Integer:
							if(!(result is Int32) & result != null)
							{
								result = Convert.ToInt32(result);
							}
							break;

						case OrigamDataType.Float:
						case OrigamDataType.Currency:
							if(! (result is Decimal) && result != null)
							{
								result = XmlConvert.ToDecimal(result.ToString());
							}
							break;

						case OrigamDataType.Date:
							if(! (result is DateTime) && result != null)
							{
								result = XmlConvert.ToDateTime(result.ToString());
							}
							break;
					}
				}
				else
				{	
					// result is XML
					XmlNodeList results = new XPathNodeList(nav.Select(expr));

					XmlDocument resultDoc = new XmlDocument();
					XmlNode docElement = resultDoc.ImportNode(((XmlDocument)context).DocumentElement, false);
					resultDoc.AppendChild(docElement);
						
					foreach(XmlNode node in results)
					{
						if(node is XmlDocument)
						{
							resultDoc = node as XmlDocument;
						}
						else
						{
							docElement.AppendChild(resultDoc.ImportNode(node, true));
						}
					}

					if(targetStructure is DataStructure)
					{
						// we clone the dataset (no data, just the structure)
						DataSet dataset = new DatasetGenerator(true).CreateDataSet(targetStructure as DataStructure);
					
						dataset.EnforceConstraints = false;
						// we load the iteration data into the dataset
						try
						{
							dataset.ReadXml(new XmlNodeReader(resultDoc), XmlReadMode.IgnoreSchema);
						}
						catch(Exception ex)
						{
							throw new Exception(ResourceUtils.GetString("ErrorEvaluateContextFailed", ex.Message), ex);
						}

						// we add the context into the called engine
						result = DataDocumentFactory.New(dataset);
					}
					else
					{
						result = new XmlContainer(resultDoc);
					}
				}
			}
			else
			{
				// context is not xml document
				result = context;
			}

			return result;
		}

		private DataStructureRuleSet _ruleSet = null;
		private IDataDocument _currentRuleDocument = null;

		public void ProcessRules(IDataDocument data, DataStructureRuleSet ruleSet, DataRow contextRow)
		{
			if(data != null && data.DataSet != null)
			{
				_currentRuleDocument = data;
			}
			else
			{
				throw new ArgumentOutOfRangeException("data", data, ResourceUtils.GetString("ErrorTypeNotProcessable"));
			}

			_ruleSet = ruleSet;

			/********************************************************************
			 * Column bound rules
			 ********************************************************************/
			if(contextRow == null)	// whole dataset
			{
				Hashtable cols = new Hashtable();
				CompleteChildColumnReferences(data.DataSet, cols);
				
				EnqueueAllRows(data, ruleSet, cols);
			}
			else					// current row
			{
				Hashtable cols = new Hashtable();
				CompleteChildColumnReferences(contextRow.Table, cols);
				
				EnqueueAllRows(contextRow, data, ruleSet, cols);
			}

			/********************************************************************
			 * Row bound rules
			 ********************************************************************/
			ArrayList sortedTables = GetSortedTables(data.DataSet);

			try
			{
				foreach(DataTable table in sortedTables)
				{
					RegisterTableEvents(table);
				}

				ProcessRuleQueue();
			}
			finally
			{
				foreach(DataTable table in sortedTables)
				{
					UnregisterTableEvents(table);
				}
			
				_ruleSet = null;
			}
		}

		private void RegisterTableEvents(DataTable table)
		{
			table.RowChanged += new DataRowChangeEventHandler(table_RowChanged);
			table.ColumnChanged += new DataColumnChangeEventHandler(table_ColumnChanged);
		}

		private void UnregisterTableEvents(DataTable table)
		{
			table.RowChanged -= new DataRowChangeEventHandler(table_RowChanged);
			table.ColumnChanged -= new DataColumnChangeEventHandler(table_ColumnChanged);
		}

		private void CompleteChildColumnReferences(DataTable table, Hashtable cols)
		{
			foreach(DataColumn col in table.Columns)
			{
				if(col.ExtendedProperties.Contains("Id"))
				{
					cols[ColumnKey(col)] = col;
				}
			}

			foreach(DataRelation rel in table.ChildRelations)
			{
				CompleteChildColumnReferences(rel.ChildTable, cols);
			}
		}

		private void CompleteChildColumnReferences(DataSet data, Hashtable cols)
		{
			foreach(DataTable table in data.Tables)
			{
				foreach(DataColumn col in table.Columns)
				{
					if(col.ExtendedProperties.Contains("Id"))
					{
						cols[ColumnKey(col)] = col;
					}
				}
			}
		}

		private ArrayList GetSortedTables(DataSet dataset)
		{
			ArrayList result = new ArrayList();

			foreach(DataTable table in dataset.Tables)
			{
				if(table.ParentRelations.Count == 0)
				{
					GetChildTables(table, result);
				}
			}

			return result;
		}

		private void GetChildTables(DataTable table, ArrayList list)
		{
			foreach(DataRelation childRelation in table.ChildRelations)
			{
				GetChildTables(childRelation.ChildTable, list);
			}

			list.Add(table);
		}

		/// <summary>
		/// Processes rules after the column value has changed.
		/// </summary>
		/// <param name="rowChanged"></param>
		/// <param name="columnChanged"></param>
		/// <param name="ruleSet"></param>
		public void ProcessRules(DataRow rowChanged, IDataDocument data, DataColumn columnChanged, DataStructureRuleSet ruleSet)
		{
			ProcessRulesInternal(rowChanged, data, columnChanged, ruleSet, null, false);
		}

		internal void ProcessRules(DataRow rowChanged, IDataDocument data, ICollection columnsChanged, DataStructureRuleSet ruleSet)
		{
			ProcessRulesInternal(rowChanged, data, null, ruleSet, columnsChanged, false);
		}

		private bool ProcessRulesFromQueue(DataRow rowChanged, IDataDocument data, DataStructureRuleSet ruleSet, ICollection columnsChanged)
		{
			return ProcessRulesInternal(rowChanged, data, null, ruleSet, columnsChanged, true);
		}

		private Queue _ruleQueue = new Queue();
		private Hashtable _ruleColumnChanges = new Hashtable();

		private bool IsEntryInQueue(DataRow rowChanged, DataStructureRuleSet ruleSet)
		{
			foreach(object[] entry in _ruleQueue)
			{
				if(entry[0].Equals(rowChanged) && (
										(entry[1] != null && entry[1].Equals(ruleSet) )
										|| entry[1] == null && ruleSet == null
													)
				   )
				{
					return true;
				}
			}

			return false;
		}

		private void UpdateQueueEntries(DataRow rowChanged, DataStructureRuleSet ruleSet, DataColumn column)
		{
			foreach(object[] queueEntry in _ruleQueue)
			{
				if(! queueEntry[0].Equals(rowChanged) && (
					(queueEntry[1] != null && queueEntry[1].Equals(ruleSet) )
					|| queueEntry[1] == null && ruleSet == null
					)
					)
				{
					Hashtable h = queueEntry[2] as Hashtable;
					h[ColumnKey(column)] = column;
				}
			}
		}

		private void EnqueueEntry(DataRow rowChanged, IDataDocument data, DataStructureRuleSet ruleSet, Hashtable columns)
		{
			if(rowChanged.RowState == DataRowState.Deleted) return;

			if(columns ==  null)
			{
				// only pass other entities's column changes (e.g. from children to parents)
				columns = new Hashtable();
				foreach(DataColumn col in _ruleColumnChanges.Values)
				{
					if(! col.Table.TableName.Equals(rowChanged.Table.TableName))
					{
						columns.Add(ColumnKey(col), col);
					}
				}
			}

			object[] queueEntry = new object[4] {rowChanged, ruleSet, columns, data};
			_ruleQueue.Enqueue(queueEntry);
		}

		private static string ColumnKey(DataColumn col)
		{
			return col.Table.TableName + "_" + col.ExtendedProperties["Id"].ToString();
		}

		private void EnqueueAllRows(DataRow currentRow, IDataDocument data, DataStructureRuleSet ruleSet, Hashtable columns)
		{
			if(! IsEntryInQueue(currentRow, ruleSet))
			{
				EnqueueEntry(currentRow, data, ruleSet, columns);
			}

			EnqueueChildRows(currentRow, data, ruleSet, columns);
			EnqueueParentRows(currentRow, data, ruleSet, columns, null);
		}

		private void EnqueueAllRows(IDataDocument data, DataStructureRuleSet ruleSet, Hashtable columns)
		{
			ArrayList tables = GetSortedTables(data.DataSet);

			for(int i=tables.Count-1; i >= 0; i--)
			{
				foreach(DataRow row in ((DataTable)tables[i]).Rows)
				{
					EnqueueEntry(row, data, ruleSet, columns);
				}
			}
		}

		private void EnqueueChildRows(DataRow parentRow, IDataDocument data, DataStructureRuleSet ruleSet, Hashtable columns)
		{
            if (parentRow.RowState != DataRowState.Detached && parentRow.RowState != DataRowState.Deleted)
            {
                foreach (DataRelation childRelation in parentRow.Table.ChildRelations)
                {
                    foreach (DataRow row in parentRow.GetChildRows(childRelation))
                    {
                        if (!IsEntryInQueue(row, ruleSet))
                        {
                            EnqueueEntry(row, data, ruleSet, columns);
                        }

                        EnqueueChildRows(row, data, ruleSet, columns);
                    }
                }
            }
		}

		private void EnqueueParentRows(DataRow childRow, IDataDocument data, DataStructureRuleSet ruleSet, Hashtable columns, DataRow[] parentRows)
		{
			ArrayList rows = new ArrayList();
			if(parentRows == null)
			{
				foreach(DataRelation parentRelation in childRow.Table.ParentRelations)
				{
					foreach(DataRow row in childRow.GetParentRows(parentRelation))
					{
						rows.Add(row);
					}
				}
			}
			else
			{
				rows.AddRange(parentRows);
			}

			foreach(DataRow row in rows)
			{
				if(!IsEntryInQueue(row, ruleSet))
				{
					EnqueueEntry(row, data, ruleSet, columns);
				}

				EnqueueParentRows(row, data, ruleSet, columns, null);
			}
		}

		public void ProcessRules(DataRow rowChanged, IDataDocument data, DataStructureRuleSet ruleSet)
		{
			ProcessRules(rowChanged, data, ruleSet, null);
		}
		
		/// <summary>
		/// Processes rules after the row was commited for change.
		/// </summary>
		/// <param name="rowChanged"></param>
		/// <param name="ruleSet"></param>
		public void ProcessRules(DataRow rowChanged, IDataDocument data, DataStructureRuleSet ruleSet, DataRow[] parentRows)
		{
			bool wasQueued = false;

			if(IsEntryInQueue(rowChanged, ruleSet))
			{
				wasQueued = true;
			}
			else
			{
				EnqueueEntry(rowChanged, data, ruleSet, null);
			}

			EnqueueChildRows(rowChanged, data, ruleSet, null);

			Hashtable columns = null;
			if(rowChanged.RowState == DataRowState.Deleted)
			{
				columns = new Hashtable();
				foreach(DataColumn col in rowChanged.Table.Columns)
				{
					columns[ColumnKey(col)] = col;
				}
			}
			EnqueueParentRows(rowChanged, data, ruleSet, columns, parentRows);

			if(wasQueued) return;

			_ruleColumnChanges.Clear();

			ProcessRuleQueue();
		}


		private bool _ruleProcessingPaused = false;

		public void PauseRuleProcessing()
		{
			_ruleProcessingPaused = true;
		}

		public void ResumeRuleProcessing()
		{
			_ruleProcessingPaused = false;
		}

		public void ProcessRuleQueue()
		{
			if( ! _ruleProcessingPaused)
			{
				while(_ruleQueue.Count != 0)
				{
					object[] queueEntry = (object[])_ruleQueue.Peek();
					DataRow row = queueEntry[0] as DataRow;
					DataStructureRuleSet rs = queueEntry[1] as DataStructureRuleSet;
					Hashtable changedColumns = queueEntry[2] as Hashtable;
                    IDataDocument data = queueEntry[3] as IDataDocument;

					row.BeginEdit();

					try
					{
						// Process rules on the changed row.
						if(ProcessRulesFromQueue(row, data, rs, changedColumns.Values))
						{
							try
							{
								row.EndEdit();
							}
							catch(Exception ex)
							{
								throw new Exception(ex.Message + Environment.NewLine + ResourceUtils.GetString("RowState") + row.RowState.ToString(), ex);
							}
						}
					}
					catch
					{
						row.CancelEdit();
						_ruleQueue.Clear();
						throw;
					}

					row.CancelEdit();

					_ruleQueue.Dequeue();
				}
			}
		}
		
		private bool ProcessRulesInternal(DataRow rowChanged, IDataDocument data, DataColumn columnChanged, DataStructureRuleSet ruleSet, ICollection columnsChanged, bool isFromRuleQueue)
		{
			if(_ruleProcessingPaused) return false;
			if(columnChanged == null && columnsChanged.Count == 0) return false;

			if(! DatasetTools.HasRowValidParent(rowChanged)) return false;

			bool result = false;
			bool resultRules = false;

			var outputPad = GetOutputPad();

		    if(ruleSet != null)
			{
				ArrayList rules;

				if(columnChanged == null)
				{
					rules = ruleSet.Rules(rowChanged.Table.TableName);

					foreach(DataColumn col in columnsChanged)
					{
						// get all the rules
						if(col.ExtendedProperties.Contains("Id"))
						{
							Guid fieldId = (Guid)col.ExtendedProperties["Id"];
							ArrayList r = ruleSet.Rules(col.Table.TableName, fieldId, isFromRuleQueue);

							foreach(DataStructureRule rule in r)
							{
								if(rule.Entity.Name.Equals(rowChanged.Table.TableName))
								{
									if(!rules.Contains(rule))
									{
										rules.Add(rule);
									}
								}
							}
						}

						if(! isFromRuleQueue)
						{
							UpdateQueueEntries(rowChanged, ruleSet, col);
							_ruleColumnChanges[ColumnKey(col)] = col;
						}
					}
				}
				else
				{
					// columns we cannot recognize will not fire any events
					if(! columnChanged.ExtendedProperties.Contains("Id")) return false;

					Guid fieldId = (Guid)columnChanged.ExtendedProperties["Id"];
					rules = ruleSet.Rules(rowChanged.Table.TableName, fieldId, false);

					UpdateQueueEntries(rowChanged, ruleSet, columnChanged);
					_ruleColumnChanges[ColumnKey(columnChanged)] = columnChanged;
				}

				rules.Sort(new ProcessRuleComparer());

				if(rules.Count > 0)
				{
					if(outputPad != null)
					{
						string pk = "";
						foreach(DataColumn column in rowChanged.Table.PrimaryKey)
						{
							if(pk != "") pk += ", ";

							pk += column.ColumnName + ": " + rowChanged[column].ToString();
						}

						if(log.IsDebugEnabled)
						{
							log.Debug(ResourceUtils.GetString("PadProcessingRules", 
								DateTime.Now.ToString(), ruleSet.Name, rowChanged.Table.TableName, pk,
								(columnChanged == null ? "<none>" : columnChanged?.ColumnName)));
						}
					}			
				}

				resultRules = ProcessRulesInternalFinish(rules, data, rowChanged, outputPad, ruleSet);
			}

			// check for lookup fields changes
			if(columnChanged == null)
			{
				ArrayList copy = new ArrayList(columnsChanged);
				foreach(DataColumn col in copy)
				{
                    if (col.Table.TableName == rowChanged.Table.TableName)
                    {
                        result = ProcessRulesLookupFields(rowChanged, col.ColumnName);
                    }
				}
			}
			else
			{
				result = ProcessRulesLookupFields(rowChanged, columnChanged.ColumnName);
			}

			return result || resultRules;
		}

	    private static IOutputPad GetOutputPad()
	    {
	        IOutputPad outputPad = null;
#if !NETSTANDARD
	        if (WorkbenchSingleton.Workbench != null)
	        {
	            outputPad = WorkbenchSingleton.Workbench.GetPad(typeof(IOutputPad)) as IOutputPad;
	        }
#endif
	        return outputPad;
	    }

	    private bool ProcessRulesLookupFields(DataRow row, string columnName)
		{
			bool changed = false;
			DataTable t = row.Table;
            Guid columnFieldId = (Guid)t.Columns[columnName].ExtendedProperties["Id"];

			foreach(DataColumn column in t.Columns)
			{
				if(column.ExtendedProperties.Contains(Const.OriginalFieldId))
				{
                    Guid originalFieldId = (Guid)column.ExtendedProperties[Const.OriginalFieldId];

					// we find all columns that depend on the changed one
                    if (originalFieldId.Equals(columnFieldId))
					{
						if(column.ExtendedProperties.Contains(Const.OriginalLookupIdAttribute))
						{
							// and we reload the value by the original lookup
							Guid originalLookupId = (Guid)column.ExtendedProperties[Const.OriginalLookupIdAttribute];

							object newValue = DBNull.Value;
						
							if(! row.IsNull(columnName))
							{
								newValue = _lookupService.GetDisplayText(originalLookupId, row[columnName], false, false, this.TransactionId);
							}
						
							if(newValue == null)
							{
								newValue = DBNull.Value;
							}

							if(row[column] != newValue)
							{
								row[column] = newValue;
								changed = true;
							}
						}
						else
						{
							// or we just copy the original value (copied fields)
                            if (row[column] != row[columnName])
                            {
                                row[column] = row[columnName];
                            }
						}
					}
				}
			}
            
			return changed;
		}

		private bool ProcessRulesInternalFinish(ArrayList rules, IDataDocument data, DataRow rowChanged, IOutputPad outputPad, DataStructureRuleSet ruleSet)
		{
			bool changed = false;

			ArrayList myRules = new ArrayList(rules);

			foreach(DataStructureRule rule in myRules)
			{
				if(log.IsDebugEnabled)
				{
					log.Debug("Evaluating Rule: " + rule?.Name + ", Target Field: " + (rule?.TargetField == null ? "<none>" : rule?.TargetField.Name));
				}

				// columns which don't allow nulls will not get processed when empty
				foreach(DataStructureRuleDependency dependency in rule.RuleDependencies)
				{
					if(dependency.Entity.Name.Equals(rowChanged.Table.TableName))
					{
						if(!dependency.Field.AllowNulls && rowChanged[dependency.Field.Name] == DBNull.Value)
						{
							if(log.IsDebugEnabled)
							{
								log.Debug("   " + ResourceUtils.GetString("PadAllowNulls", dependency.Entity.Name, dependency.Field.Name));
							}
							goto nextRule;
						}
					}
				}

				XPathNodeIterator iterator = null;

				// we do a fresh slice after evaluating each rule, because data could have changed
				DataSet dataSlice = DatasetTools.CloneDataSet(rowChanged.Table.DataSet, false);

				DatasetTools.GetDataSlice(dataSlice, new List<DataRow>{rowChanged});
			    IDataDocument xmlSlice = DataDocumentFactory.New(dataSlice);

				if(rule.ValueRule.IsPathRelative)
				{
					if(data == null)
					{
						throw new NullReferenceException("Rule has IsPathRelative set but no XmlDataDocument has been provided. Cannot evaluate rule.");
					}

					// HERE WE HAVE TO USE THE SLICE, BECAUSE IF WE USED THE ORIGINA XML DOCUMENT (E.G. FROM THE FORM)
					// WE WOULD NOT GET THE ACTUAL VALUES, SINCE THEY WERE NOT COMMITED TO THE XML, YET

					// if the xml propagation would work, we would use
					//XPathNavigator nav = data.GetElementFromRow(rowChanged).CreateNavigator();
					//iterator = nav.Select(".");
					//xmlSlice = data;

					// get the path to the current row

// DOES NOT WORK FOR NEW ROWS NOT APPENDED TO THE DATATABLE
//					XPathNavigator nav = data.GetElementFromRow(rowChanged).CreateNavigator();
//					string path = nav.Name;
//
//					while(nav.MoveToParent())
//					{
//						path = nav.Name + "/" + path;
//					}

					string path = rowChanged.Table.TableName;
					DataTable t = rowChanged.Table;

					while(t.ParentRelations.Count > 0)
					{
						if(t.ParentRelations[0].Nested)
						{
							t = t.ParentRelations[0].ParentTable;
							path = t.TableName + "/" + path;
						}
						else
						{
							break;
						}
					}

					path = "/" + data.DataSet.DataSetName + "/" + path;
				
					// move to the same position in the xml slice
					XPathNavigator nav = xmlSlice.Xml.CreateNavigator();
					iterator = nav.Select(path);
					iterator.MoveNext();
				}

				// if exists, check condition, if the rule will be actually evaluated
				if(rule.ConditionRule != null)
				{
					if(rule.ConditionRule.IsPathRelative != rule.ValueRule.IsPathRelative)
					{
						throw new ArgumentOutOfRangeException("IsPathRelative", rule.ConditionRule.IsPathRelative, ResourceUtils.GetString("ErrorRuleConditionEqual", rule.Path));
					}

					if(log.IsDebugEnabled)
					{
						log.Debug("   " + ResourceUtils.GetString("PadEvaluatingCondition"));
					}

					object shouldEvaluate = this.EvaluateRule(rule.ConditionRule, xmlSlice, iterator == null ? null : iterator.Clone());

					if(shouldEvaluate is bool)
					{
						if(! (bool)shouldEvaluate) 
						{
							if(log.IsDebugEnabled)
							{
								log.Debug("   " + ResourceUtils.GetString("PadConditionFalse"));
							}
							goto nextRule;
						}
					}
					else
					{
						throw new InvalidCastException(ResourceUtils.GetString("ErrorNotBool", rule.Path));
					}
				}
				
				
//				if(outputPad != null)
//				{
//					outputPad.AddText("Rule source data:");
//					outputPad.AddText(dataSlice.GetXml());
//				}

				object result;

				try
				{
					result = this.EvaluateRule(rule.ValueRule, xmlSlice, iterator);
				}
				catch
				{
					throw;
				}

#region Processing Result
#region TRACE

			if (log.IsDebugEnabled)
			{
				log.RunHandled(() =>
				{
					if(rule.TargetField != null)
					{
						string columnName = rule.TargetField.Name;
						DataColumn col = rowChanged.Table.Columns[columnName];
						object oldValue = rowChanged[col];
						string newLookupValue = null;
						string oldLookupValue = null;

						if(col.ExtendedProperties.Contains(Const.DefaultLookupIdAttribute))
						{
							if(result != DBNull.Value && !(result is XmlDocument))
							{
								try
								{
									newLookupValue = LookupValue(
										col.ExtendedProperties[Const.DefaultLookupIdAttribute]
										.ToString(), result.ToString());
								}
								catch(Exception ex)
								{
									newLookupValue = ex.Message;
								}
							}
							if(oldValue != DBNull.Value)
							{
								try
								{
									oldLookupValue = LookupValue(
									col.ExtendedProperties[Const.DefaultLookupIdAttribute]
									.ToString(), oldValue.ToString());
								}
								catch (Exception ex)
								{
									oldLookupValue = ex.Message;
								}
							}
						}

						log.Debug("   " 
							+ ResourceUtils.GetString("PadRuleResult0") 
							+ result.ToString() 
							+ (newLookupValue == null ? "" : " (" + newLookupValue + ")")
							+ ResourceUtils.GetString("PadRuleResult1") 
							+ columnName + ResourceUtils.GetString("PadRuleResult2") 
							+ oldValue.ToString()
							+ (oldLookupValue == null ? "" : " (" + oldLookupValue + ")"));
					}
					else
					{
						log.Debug("   " + ResourceUtils.GetString("PadRuleResult0") + result.ToString());
					}
				});
			}

#endregion

				if(result is IDataDocument)
				{
					// RESULT IS DATASET
					DataTable resultTable = (result as IDataDocument).DataSet.Tables[rowChanged.Table.TableName];

					if(resultTable == null)
					{
						string message = ResourceUtils.GetString("PadRuleInvalidStructure", rowChanged.Table.TableName);

						if(log.IsDebugEnabled)
						{
							log.Debug(message);
						}
						throw new Exception(message);
					}

					// find the record in the transformed document
					DataRow resultRow = resultTable.Rows.Find(DatasetTools.PrimaryKey(rowChanged));

					if(resultRow == null)
					{
						// row was not generated by the rule, this is a problem, the row must always be returned
						string message = ResourceUtils.GetString("PadRuleInvalidNoData");
						if(log.IsDebugEnabled)
						{
							log.Debug(message);
						}
						throw new Exception(message);
					}
					else
					{
						ArrayList changedColumns = new ArrayList();
						ArrayList changedTargetColumns = new ArrayList(changedColumns.Count);

						foreach(DataColumn col in resultRow.Table.Columns)
						{
							if(rowChanged.Table.Columns.Contains(col.ColumnName) && ! (resultRow[col].Equals(rowChanged[col.ColumnName])))
							{
								changedColumns.Add(col);
								changedTargetColumns.Add(rowChanged.Table.Columns[col.ColumnName]);
							}
						}

#region TRACE
						if (log.IsDebugEnabled)
						{
							log.RunHandled(() =>
							{
								foreach(DataColumn col in changedColumns)
								{
									string newLookupValue = null;
									string oldLookupValue = null;
									object resultValue = resultRow[col];
									object oldValue = rowChanged[col.ColumnName];
									string columnName = col.ColumnName;

									if(col.ExtendedProperties.Contains(Const.DefaultLookupIdAttribute) && 
									   col.ExtendedProperties.Contains(Const.OrigamDataType) && 
									   !OrigamDataType.Array.Equals(col.ExtendedProperties[Const.OrigamDataType]))
									{
										if(resultValue != DBNull.Value)
										{
											newLookupValue = LookupValue(col.ExtendedProperties[Const.DefaultLookupIdAttribute].ToString(), resultValue.ToString());
										}
										if(oldValue != DBNull.Value)
										{
											oldLookupValue = LookupValue(col.ExtendedProperties[Const.DefaultLookupIdAttribute].ToString(), oldValue.ToString());
										}
									}

									log.Debug("   " 
										+ columnName + ": "
										+ resultValue.ToString() 
										+ (newLookupValue == null ? "" : " (" + newLookupValue + ")")
										+ ResourceUtils.GetString("PadRuleResult1") 
										+ ResourceUtils.GetString("PadRuleResult2") 
										+ oldValue.ToString()
										+ (oldLookupValue == null ? "" : " (" + oldLookupValue + ")"));
								}
							});
						}
						#endregion

						// copy the values into the source row
						PauseRuleProcessing();
						bool localChanged = DatasetTools.CopyRecordValues(
							resultRow, DataRowVersion.Current, rowChanged, 
							true);
						ResumeRuleProcessing();

						if(! changed) changed = localChanged;

						ProcessRules(rowChanged, data, changedTargetColumns, ruleSet);
					}
				}
				else if(result is XmlDocument)
				{
					// XML IS NOT SUPPORTED
					string message = ResourceUtils.GetString("PadXmlDocument");
					if (rule.ValueRule != null && rule.ValueRule is Origam.Schema.RuleModel.XslRule)
					{
						message += ResourceUtils.GetString("FixXslRuleWithDestinationDataStructure",
														   rule.ValueRule.ToString());
					}
					if(log.IsDebugEnabled)
					{
						log.Debug(message);
					}
					throw new NotSupportedException(message);
				}
				else
				{
					// SIMPLE DATA TYE (e.g. XPath Rule). TargetField must be used to return the result to a specific column.
					if(rule.TargetField == null)
					{
						string message = ResourceUtils.GetString("PadTargetField");
						if(log.IsDebugEnabled)
						{
							log.Debug(message);
						}
						throw new Exception(message);
					}

					foreach(DataColumn column in rowChanged.Table.Columns)
					{
						if(column.ExtendedProperties["Id"].Equals(rule.TargetField.PrimaryKey["Id"]))
						{
							if(! rowChanged[column].Equals(result))
							{
								rowChanged[column] = result;
								changed = true;
							}
							break;
						}
					}
				}
#endregion

			nextRule:
				;
			}

			if(log.IsDebugEnabled && myRules.Count > 0)
			{
				log.Debug(ResourceUtils.GetString("PadRuleFinished", DateTime.Now, changed.ToString()));
			}

			return changed;
		}
#endregion

#region Conditional Formatting Functions
		public EntityFormatting Formatting(XmlContainer data, Guid entityId, Guid fieldId, XPathNodeIterator contextPosition)
		{
			EntityFormatting formatting = new EntityFormatting(NullColor, NullColor);

			ArrayList entityRules = new ArrayList(); 
			IDataEntity entity = _persistence.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(entityId)) as IDataEntity;
			
			if(fieldId == Guid.Empty)
			{
				entityRules.AddRange(entity.ConditionalFormattingRules);
			}
			else
			{
				// we retrieve the column from the child-items list
				// this is very cost efficient, because when retrieving abstract columns (i.e. Id, RecordCreated, RecordUpdated), they are never cached
				IDataEntityColumn field = entity.GetChildById(fieldId) as IDataEntityColumn;

				if(field != null)
				{
					entityRules.AddRange(field.ConditionalFormattingRules);
				}
			}

			if(entityRules.Count > 0)
			{
				entityRules.Sort();

				IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;

				foreach(EntityConditionalFormatting rule in entityRules)
				{
					if(IsRuleMatching(data, rule.Rule, rule.Roles, contextPosition))
					{
						Color foreColor = rule.ForegroundColor;
						Color backColor = rule.BackgroundColor;

						object lookupParam = null;
						if(rule.DynamicColorLookupField != null) 
						{
                            EntityRule xpr = new EntityRule();
							xpr.DataType = OrigamDataType.String;
							xpr.XPath = "/row/" + (rule.DynamicColorLookupField.XmlMappingType == EntityColumnXmlMapping.Attribute ? "@" : "") + rule.DynamicColorLookupField.Name;

							lookupParam = EvaluateRule(xpr, data, contextPosition);
						}

						if(lookupParam != DBNull.Value)
						{
							if(rule.ForegroundColorLookup != null)
							{
								if(rule.DynamicColorLookupField == null) throw new Exception(ResourceUtils.GetString("ErrorNoForegroundDynamicColorLookup"));

								object color = ls.GetDisplayText(rule.ForeColorLookupId, lookupParam, false, false, null );
						
								if(color is int) foreColor = System.Drawing.Color.FromArgb((int)color);
							}

							if(rule.BackgroundColorLookup != null)
							{
								if(rule.DynamicColorLookupField == null) throw new Exception(ResourceUtils.GetString("ErrorNoBackgroundDynamicColorLookup"));

								object color = ls.GetDisplayText(rule.BackColorLookupId, lookupParam, false, false, null );
						
								if(color is int) backColor = System.Drawing.Color.FromArgb((int)color);
							}
						}

                        if (foreColor != NullColor && formatting.ForeColor == NullColor)
                        {
                            formatting.ForeColor = foreColor;
                        }
                        if (backColor != NullColor && formatting.BackColor == NullColor)
                        {
                            formatting.BackColor = backColor;
                        }

						if(formatting.ForeColor != NullColor && formatting.BackColor != NullColor)
						{
							return formatting;
						}
					}
				}
			}

			return formatting;
		}

		public string DynamicLabel(XmlContainer data, Guid entityId, Guid fieldId, XPathNodeIterator contextPosition)
		{
			ArrayList rules = new ArrayList(); 
			
			IDataEntity entity = _persistence.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(entityId)) as IDataEntity;
			IDataEntityColumn field = entity.GetChildById(fieldId) as IDataEntityColumn;
 
			if(field == null) return null;	// lookup fields in a data structure

			rules.AddRange(field.DynamicLabels);

			if(rules.Count > 0)
			{
				rules.Sort();

				IParameterService ps = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

				foreach (EntityFieldDynamicLabel rule in rules)
				{
					if(IsRuleMatching(data, rule.Rule, rule.Roles, contextPosition))
					{
						string result = (string)ps.GetParameterValue(rule.LabelConstantId, OrigamDataType.String);
						return result;
					}
				}
			}

			return null;
		}
#endregion

#region Row Level Security Functions
		public bool RowLevelSecurityState(DataRow row, string field, CredentialType type)
		{
			if(! DatasetTools.HasRowValidParent(row)) return true;

			Guid entityId = Guid.Empty;
			Guid fieldId = Guid.Empty;

			if(row.Table.ExtendedProperties.Contains("EntityId"))
			{
				XmlContainer originalData = DatasetTools.GetRowXml(row, DataRowVersion.Original);
				XmlContainer actualData = DatasetTools.GetRowXml(row, row.HasVersion(DataRowVersion.Proposed) ? DataRowVersion.Proposed : DataRowVersion.Default);

				fieldId = (Guid)row.Table.Columns[field].ExtendedProperties["Id"];
				entityId = (Guid)row.Table.ExtendedProperties["EntityId"];

				return RowLevelSecurityState(originalData, actualData, field, type, entityId, fieldId, row.RowState == DataRowState.Added || row.RowState == DataRowState.Detached);
			}
			else
			{
				return true;
			}
		}

		public RowSecurityState RowLevelSecurityState(DataRow row,
			object profileId)
		{
			return RowLevelSecurityState(row, profileId, Guid.Empty);
		}

		public RowSecurityState RowLevelSecurityState(DataRow row, object profileId, Guid formId)
		{
			if(! DatasetTools.HasRowValidParent(row)) return null;
			
			if(row.Table.ExtendedProperties.Contains("EntityId"))
			{
				Guid entityId = (Guid)row.Table.ExtendedProperties["EntityId"];
				XmlContainer originalData = DatasetTools.GetRowXml(row,
                    DataRowVersion.Original);
				XmlContainer actualData = DatasetTools.GetRowXml(row, 
                    row.HasVersion(DataRowVersion.Proposed) 
                    ? DataRowVersion.Proposed : DataRowVersion.Default);
				EntityFormatting formatting = Formatting(actualData, 
                    entityId, Guid.Empty, null);
				bool isNew = row.RowState == DataRowState.Added 
                    || row.RowState == DataRowState.Detached;
				RowSecurityState result = new RowSecurityState
                {
                    Id = DatasetTools.PrimaryKey(row)[0],
                    BackgroundColor = formatting.BackColor.ToArgb(),
                    ForegroundColor = formatting.ForeColor.ToArgb(),
                    AllowDelete = RowLevelSecurityState(originalData,
                        actualData, null, CredentialType.Delete, entityId,
                    Guid.Empty, isNew),
                    AllowCreate = RowLevelSecurityState(originalData,
                        actualData, null, CredentialType.Create, entityId,
                        Guid.Empty, isNew)
                };

				// columns
				foreach(DataColumn col in row.Table.Columns)
				{
					if(col.ExtendedProperties.Contains("Id"))
					{
						Guid fieldId = (Guid)col.ExtendedProperties["Id"];

						bool allowUpdate = RowLevelSecurityState(originalData, 
                            actualData, col.ColumnName, CredentialType.Update,
                            entityId, fieldId, isNew);
						bool allowRead = RowLevelSecurityState(originalData, 
                            actualData, col.ColumnName, CredentialType.Read, 
                            entityId, fieldId, isNew);

						EntityFormatting fieldFormatting = Formatting(actualData, 
                            entityId, fieldId, null);
						string dynamicLabel = this.DynamicLabel(actualData, 
                            entityId, fieldId, null);

						result.Columns.Add(new FieldSecurityState(col.ColumnName,
                            allowUpdate, allowRead, dynamicLabel, 
                            fieldFormatting.BackColor.ToArgb(), 
                            fieldFormatting.ForeColor.ToArgb()));
					}
				}

				// relations
                foreach (DataRelation rel in row.Table.ChildRelations)
                {
                    Guid childEntityId = (Guid)rel.ChildTable.ExtendedProperties["EntityId"];
                    bool isDummyRow = false;
                    DataRow childRow = null;
                    DataRow[] childRows = row.GetChildRows(rel);
                    try
                    {
                        if (childRows.Length > 0)
                        {
                            childRow = childRows[0];
                        }
                        else
                        {
                            isDummyRow = true;
                            childRow = DatasetTools.CreateRow(row, rel.ChildTable, rel, profileId);

                            // go through each column and lookup any looked-up column values
                            foreach (DataColumn childCol in childRow.Table.Columns)
                            {
#if !ORIGAM_SERVER
                                if (childRow.RowState != DataRowState.Unchanged
                                    && childRow.RowState != DataRowState.Detached)
                                {
#endif
                                    this.ProcessRulesLookupFields(childRow, childCol.ColumnName);
#if !ORIGAM_SERVER
                                }
#endif
                            }
                        }

                        XmlContainer originalChildData = DatasetTools.GetRowXml(
                            childRow, DataRowVersion.Original);
                        XmlContainer actualChildData = DatasetTools.GetRowXml(
                            childRow, childRow.HasVersion(DataRowVersion.Proposed)
                            ? DataRowVersion.Proposed : DataRowVersion.Default);

                        bool allowRelationCreate = RowLevelSecurityState(originalChildData,
                            actualChildData,
                            null,
                            CredentialType.Create,
                            childEntityId,
                            Guid.Empty,
                            row.RowState == DataRowState.Added || row.RowState == DataRowState.Detached
                            );

                        result.Relations.Add(new RelationSecurityState(
                            rel.ChildTable.TableName, allowRelationCreate));
                    }
                    catch (Exception ex)
                    {
                        if (log.IsErrorEnabled)
                        {
                            log.LogOrigamError(string.Format(
                                "Failed evaluating security rule for child relation {0} for entity {1}", 
                                rel?.RelationName, entityId), ex);
                        }
                        throw;
                    }
                    finally
                    {
                        if (isDummyRow && childRow != null) childRow.Delete();
                    }
                }

				// action buttons
				// TODO: resolve only those action buttons valid for a current form - passing
				// a screen/section id would be neccessary
                result.DisabledActions = GetDisabledActions(
                    originalData, actualData, entityId, formId);
				return result;
			}
			else
			{
				return null;
			}
		}

        public ArrayList GetDisabledActions(
	        XmlContainer originalData, XmlContainer actualData, Guid entityId, Guid formId)
        {
            ArrayList result = new ArrayList();
            IDataEntity entity = _persistence.SchemaProvider.RetrieveInstance(
                typeof(AbstractSchemaItem), new ModelElementKey(entityId))
                as IDataEntity;
            foreach(EntityUIAction action in entity.ChildItemsByTypeRecursive(
                EntityUIAction.CategoryConst))
            {
	            // Performance sensitive! RuleDisablesAction method should not
	            // be invoked unless it is really necessary.
	            if( 
		            IsFeatureOff(action) ||
		            IsDisabledByMode(actualData, action) || 
		            IsDisabledByScreenCondition(formId, action) || 
		            IsDisabledByScreenSectionCondition(formId, action) || 
		            IsDisabledByRoles(action) ||
		            RuleDisablesAction(originalData, actualData, action))
                {
                    result.Add(action.Id.ToString());
                }
            }
            return result;
        }

        private bool IsDisabledByRoles(EntityUIAction action)
        {
	        if (action.Roles != null & action.Roles != String.Empty)
	        {
		        return false;
	        }

	        return !SecurityManager
		        .GetAuthorizationProvider()
		        .Authorize(SecurityManager.CurrentPrincipal, action.Roles);
        }

        private bool IsDisabledByScreenSectionCondition(Guid formId,
	        EntityUIAction action)
        {
	        if (formId == Guid.Empty)
	        {
		        return false;
	        }
	        var panelIds = _persistence.SchemaProvider
		        .RetrieveInstance<FormControlSet>(formId)
		        .ChildrenRecursive
		        .OfType<ControlSetItem>()
		        .Select(controlSet => controlSet.ControlItem.PanelControlSetId)
		        .Where(panelId => panelId != Guid.Empty)
		        .ToList();
	        return action.ScreenSectionIds.Any() &&
	               panelIds.Count > 0 &&
	               !panelIds.Any(panelId => action.ScreenSectionIds.Contains(panelId));
        }

        private static bool IsDisabledByScreenCondition(Guid formId, EntityUIAction action)
        {
	        return formId != Guid.Empty &&
	               action.ScreenIds.Any() && 
	               !action.ScreenIds.Contains(formId);
        }

        private static bool IsDisabledByMode(XmlContainer actualData, EntityUIAction action)
        {
	        return action.Mode == PanelActionMode.ActiveRecord &&
	               actualData == null;
        }

        private bool IsFeatureOff(EntityUIAction action)
        {
	        return !_parameterService.IsFeatureOn(action.Features);
        }

        // Performance sensitive! RuleDisablesAction method should not
		// be invoked unless it is really necessary.
		private bool RuleDisablesAction(XmlContainer originalData,
			XmlContainer actualData, EntityUIAction action)
		{
			XmlContainer dataToUseForRule
				= action.ValueType == CredentialValueType.ActualValue
					? actualData
					: originalData;

			return  action.Rule != null &&
			        !IsRuleMatching(
				        dataToUseForRule, action.Rule, action.Roles, null);
		}

		public bool RowLevelSecurityState(XmlContainer originalData, XmlContainer actualData, string field, CredentialType type, Guid entityId, Guid fieldId, bool isNewRow)
		{
			ArrayList rules = new ArrayList();
				
			IDataEntity entity = _persistence.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(entityId)) as IDataEntity;

			// field-level rules
			if(field != null)
			{
				// we retrieve the column from the child-items list
				// this is very cost efficient, because when retrieving abstract columns (i.e. Id, RecordCreated, RecordUpdated), they are never cached
				IDataEntityColumn column = entity.GetChildById(fieldId) as IDataEntityColumn;

				// field not found, this would be e.g. a looked up column, which does not point to a real entity field id
				if(column != null)
				{
					rules.AddRange(column.RowLevelSecurityRules);
				}
			}

			// entity-level rules
			ArrayList entityRules = entity.RowLevelSecurityRules;
			if(entityRules.Count > 0)
			{
				rules.AddRange(entityRules);
			}
				
			// no rules - permit
			if(rules.Count == 0) return true;

			rules.Sort();

			foreach(AbstractEntitySecurityRule rule in rules)
			{
				EntitySecurityRule entityRule = rule as EntitySecurityRule;

				if(entityRule != null)
				{
					if(entityRule.DeleteCredential && type == CredentialType.Delete && isNewRow)
					{
						// always allow to delete new (not saved) records
						return true;
					}
					else if(
						(entityRule.UpdateCredential && type == CredentialType.Update)
						|| (entityRule.CreateCredential && type == CredentialType.Update && isNewRow)
						|| (entityRule.CreateCredential && type == CredentialType.Create)
						|| (entityRule.DeleteCredential && type == CredentialType.Delete)
						)
					{
						if(IsRowLevelSecurityRuleMatching(entityRule, entityRule.ValueType == CredentialValueType.ActualValue ? actualData : originalData))
						{
							return entityRule.Type == PermissionType.Permit;
						}
					}
				}

				EntityFieldSecurityRule fieldRule = rule as EntityFieldSecurityRule;

				if(fieldRule != null)
				{
					if(
						(fieldRule.UpdateCredential & type == CredentialType.Update)
						| (fieldRule.ReadCredential & type == CredentialType.Read)
						)
					{
						if(IsRowLevelSecurityRuleMatching(fieldRule, fieldRule.ValueType == CredentialValueType.ActualValue ? actualData : originalData))
						{
							return fieldRule.Type == PermissionType.Permit;
						}
					}
				}
			}

			// no match
			if(type == CredentialType.Read)
			{
				// permit for read
				return true;
			}
			else
			{
				// deny for all the others
				return false;
			}
		}

		private bool IsRowLevelSecurityRuleMatching(AbstractEntitySecurityRule rule, XmlContainer data)
		{
			return IsRuleMatching(data, rule.Rule, rule.Roles, null);
		}

		private bool IsRuleMatching(XmlContainer data, IRule rule, string roles, XPathNodeIterator contextPosition)
		{
			// check roles
			IOrigamAuthorizationProvider authorizationProvider = SecurityManager.GetAuthorizationProvider();
			if(! authorizationProvider.Authorize(SecurityManager.CurrentPrincipal, roles))
			{
				return false;
			}

			// check business rule
			if(rule != null)
			{
				object result = this.EvaluateRule(rule, data, contextPosition);

				if(result is bool)
				{
					return (bool)result;
				}
				else
				{
					throw new ArgumentException("Rule resulted in a result which is not boolean. Cannot evaluate non-boolean rules. Rule: " + ((AbstractSchemaItem)rule).Path);
				}
			}

			return true;
		}
#endregion
#endregion

#region Private Functions
		private string FormatXmlString(object value)
		{
			if(value is DateTime)
			{
				return FormatXmlDateTime((DateTime)value);
			}
			else if(value is decimal)
			{
				return XmlConvert.ToString((decimal)value);
			}
			else if(value is float)
			{
				return XmlConvert.ToString((float)value);
			}
			else if(value is double)
			{
				return XmlConvert.ToString((double)value);
			}
			else if(value is bool)
			{
				return XmlConvert.ToString((bool)value);
			}
			else if(value == null || value == DBNull.Value)
			{
				return String.Empty;
			}
			else
			{
				return value.ToString();
			}
		}

		public object GetContext(Key key)
		{
			return _contextStores [key];
		}

		public void SetContext(Key key, object value)
		{
			_contextStores [key] = value;
		}

		public object GetContext(IContextStore contextStore)
		{
			return GetContext(contextStore.PrimaryKey);
		}

		public ICollection ContextStoreKeys
		{
			get
			{
				return _contextStores.Keys;
			}
		}

        public IXmlContainer GetXmlDocumentFromData(object inputData)
		{
		    IXmlContainer doc = inputData as XmlContainer;
            if (doc != null)
            {
                return doc;
            }
			object data = inputData;
			IContextStore contextStore = data as IContextStore;
			if (contextStore != null) {
				// Get the rule's context store
				data = GetContext(contextStore);
			}
		    IXmlContainer xmlDocument = data as IXmlContainer;
			if(xmlDocument != null)
			{
				doc = xmlDocument;
                return doc;
			}
            
            System.Xml.XmlDocument xmlDoc = data as XmlDocument;
            if (xmlDoc != null)
            {
                // this shouldn't happen. XmlContainer should be as and input all the time.
                // But if it was XmlDocument, we convert it here and log it.
                log.ErrorFormat("GetXmlDocumentFromData called with System.Xml.XmlDataDocuement." +
                    "This isn't expected. Refactor code to be called with IXmlContainer. (documentElement:{0})",
                    xmlDoc.DocumentElement.Name);
                return new XmlContainer(xmlDoc);
            }            
			
			if(data is int)
			{
				data = XmlConvert.ToString((int)data);
			}
			else if(data is Guid)
			{
				data = XmlConvert.ToString((Guid)data);
			}
			else if(data is long)
			{
				data = XmlConvert.ToString((long)data);
			}
			else if(data is decimal)
			{
				data = XmlConvert.ToString((decimal)data);
			}
			else if(data is bool)
			{
				data = XmlConvert.ToString((bool)data);
			}
			else if(data is DateTime)
			{
				data = XmlConvert.ToString((DateTime)data);
			}
			else if(data == null)
			{
				return  new XmlContainer("<ROOT/>");
			}
			else if (data is ArrayList)
			{
				doc = new XmlContainer();
				XmlElement root = (XmlElement)doc.Xml.AppendChild(doc.Xml.CreateElement("ROOT"));
				foreach (object item in data as ArrayList)
				{
					root.AppendChild(doc.Xml.CreateElement("value")).InnerText = item.ToString();
				}
				return doc;
			}
			else
			{
				data = data.ToString();
			}

			doc = new XmlContainer();
			doc.Xml.LoadXml("<ROOT><value /></ROOT>");
            doc.Xml.FirstChild.FirstChild.InnerText = (string)data;

			return doc;
		}

		private static string FormatXmlDateTime(DateTime date)
		{
			if(date.Hour == 0 & date.Minute == 0 & date.Second == 0 & date.Millisecond == 0)
			{
				TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(date);
				int daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(date.Year).Delta.Hours;
				int hours = offset.Duration().Hours;
				int finalHours = hours; // + daylight;

				string sign = finalHours >= 0 ? "+" : "-";

				string result = date.ToString("yyyy-MM-dd") + "T00:00:00.0000000" + sign + finalHours.ToString("00") + ":" + offset.Minutes.ToString("00");

				return result;
			}
			else
			{
				return date.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
			}
		}

		private DataSet LoadData(DataStructureQuery query)
		{
			_dataServiceAgent.MethodName = "LoadDataByQuery";
			_dataServiceAgent.Parameters.Clear();
			_dataServiceAgent.Parameters.Add("Query", query);

			_dataServiceAgent.Run();

			return _dataServiceAgent.Result as DataSet;
		}
#endregion

#region Evaluators

		private object Evaluate(SystemFunctionCall functionCall)
		{
			switch(functionCall.Function)
			{
				case SystemFunction.ActiveProfileId:
					return this.ActiveProfileId();

				case SystemFunction.ResourceIdByActiveProfile:
					return this.ResourceIdByActiveProfile();

				default:
					throw new ArgumentOutOfRangeException("Function", functionCall.Function, ResourceUtils.GetString("ErrorUnsupportedFunction"));
			}
		}

		private object Evaluate(DataStructureReference reference)
		{
			return reference;
			//return reference.DataStructure.PrimaryKey;
		}

		private Guid Evaluate(TransformationReference reference)
		{
			return ((XslTransformation)reference.Transformation).Id;
		}

#endregion

#region Rule Evaluators
		private object EvaluateRule(XPathRule rule, IXmlContainer context, XPathNodeIterator contextPosition)
		{
			if(context?.Xml == null)
			{
				throw new NullReferenceException(ResourceUtils.GetString("ErrorEvaluateContextNull"));
			}

			if(log.IsDebugEnabled)
			{
				log.Debug("Evaluating XPath Rule: " + rule?.Name);
				if(contextPosition != null)
				{
					log.Debug("Current Position: " + contextPosition?.Current?.Name);
				}
				log.Debug("  Input data: " + context.Xml?.OuterXml);
			}

			XPathNavigator nav = context.Xml.CreateNavigator();

			return EvaluateXPath(rule.XPath, rule.IsPathRelative, rule.DataType, nav, contextPosition);
		}

		private object EvaluateXPath(string xpath, bool isPathRelative, OrigamDataType returnDataType, XPathNavigator nav, XPathNodeIterator contextPosition)
		{
			XPathExpression expr;

			//#if ORIGAM_CLIENT
			//			if(_xpathRulesCache.Contains(rule.Id))
			//			{
			//				expr = _xpathRulesCache[rule.Id] as XPathExpression;
			//			}
			//			else
			//			{
			//#endif
			expr = nav.Compile(xpath);

			//#if ORIGAM_CLIENT
			//				_xpathRulesCache.Add(rule.Id, expr);
			//			}
			//#endif

			OrigamXsltContext ctx =  new OrigamXsltContext(new NameTable(), this);
			expr.SetContext(ctx);

			object result;

			if(isPathRelative & contextPosition != null)
			{
				result = nav.Evaluate(expr, contextPosition);
			}
			else
			{
				result = nav.Evaluate(expr);
			}

			if(result is XPathNodeIterator)
			{
				XPathNodeIterator iterator = result as XPathNodeIterator;

				if(iterator.Count == 0)
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
				switch(returnDataType)
				{
					case OrigamDataType.Boolean:
						if(result == null)
						{
							return false;
						}
						else if(result is String)
						{
							if((string)result == "" | (string)result == "false" | (string)result == "0")
							{
								return false;
							}
							else
							{
								return true;
							}
						}
						else if(result is double)
						{
							if((double)result == 0)
							{
								return false;
							}
							else
							{
								return true;
							}
						}
						else if(result is bool)
						{
							return result;
						}
						else
						{
							throw new Exception(ResourceUtils.GetString("ErrorConvertToBool"));
						}

					case OrigamDataType.UniqueIdentifier:
						if(result == null || result.ToString() == "")
						{
							return DBNull.Value;
						}
						else
						{
							return new Guid(result.ToString());
						}

					case OrigamDataType.Date:
						if(result == null || result.ToString() == "")
						{
							return DBNull.Value;
						}
						else
						{
							return XmlConvert.ToDateTime(result.ToString());
						}

					case OrigamDataType.Long:
						if(result == null || result.ToString() == "")
						{
							return DBNull.Value;
						}
                        else
                        {
                            return Convert.ToInt64(result, new System.Globalization.NumberFormatInfo());
                        }

					case OrigamDataType.Integer:
                        if (result == null || result.ToString() == "")
                        {
                            return DBNull.Value;
                        }
                        else
                        {
                            return Convert.ToInt32(result, new System.Globalization.NumberFormatInfo());
                        }
				
					case OrigamDataType.Float:
						if(result == null || result.ToString() == "")
						{
							return DBNull.Value;
						}
						else
						{
							return Convert.ToDecimal(result, new System.Globalization.NumberFormatInfo());
						}

					case OrigamDataType.Currency:
						return Convert.ToDecimal(result, new System.Globalization.NumberFormatInfo());

					case OrigamDataType.String:
						if(result == null)
						{
							return DBNull.Value;
						}
						else
						{
							return XmlTools.ConvertToString(result);
						}

					default:
						throw new Exception("Data type not supported by rule evaluation.");
				}
			}
			catch(Exception ex)
			{
				throw new Exception(ResourceUtils.GetString("ErrorConvertToType0") 
					+ Environment.NewLine + ResourceUtils.GetString("ErrorConvertToType1")
					+ returnDataType.ToString() + Environment.NewLine 
					+ ResourceUtils.GetString("ErrorConvertToType2", result.ToString()),
					ex);
			}
			finally
			{
				//expr.SetContext(null);
			}
		}


		private object EvaluateRule(XslRule rule, IXmlContainer context)
		{
			try
			{
			    IXmlContainer result = _transformer.Transform(context, rule.Id, null, this, rule.Structure, false);

				return result;
			}
			catch(OrigamRuleException)
			{
				throw;
			}
			catch(Exception ex)
			{
				throw new Exception(ResourceUtils.GetString("ErrorRuleFailed2"), ex);
			}
		}
#endregion

		private void table_RowChanged(object sender, DataRowChangeEventArgs e)
		{
			ProcessRules(e.Row, _currentRuleDocument, _ruleSet);
		}

		private void table_ColumnChanged(object sender, DataColumnChangeEventArgs e)
		{
			ProcessRules(e.Row, _currentRuleDocument, e.Column, _ruleSet);
		}
	}

#region XPath Functions

	class NullFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public NullFunction(XPathResultType[] argTypes)
		{
			
		}
		public int Minargs    
		{ 
			get { return 0; }
		}
		public int Maxargs    
		{ 
			get { return 0; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.Any; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return null;
		}
	}

	class ActiveProfileBusinessUnitIdFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public ActiveProfileBusinessUnitIdFunction(XPathResultType[] argTypes)
		{
			
		}
		public int Minargs    
		{ 
			get { return 0; }
		}
		public int Maxargs    
		{ 
			get { return 0; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return this.Engine.ActiveProfileBusinessUnitId();
		}
	}

	class ActiveProfileIdFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public ActiveProfileIdFunction(XPathResultType[] argTypes)
		{
			
		}
		public int Minargs    
		{ 
			get { return 0; }
		}
		public int Maxargs    
		{ 
			get { return 0; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return this.Engine.ActiveProfileId();
		}
	}

	class ActiveProfileOrganizationIdFunction : IXsltContextFunction
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public ActiveProfileOrganizationIdFunction(XPathResultType[] argTypes)
		{

		}
		public int Minargs
		{
			get { return 0; }
		}
		public int Maxargs
		{
			get { return 0; }
		}
		public XPathResultType ReturnType
		{
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext,
			object[] args, XPathNavigator docContext)
		{
			return this.Engine.ActiveProfileOrganizationId();
		}
	}

	class NormalRoundFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public NormalRoundFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
			foreach(XPathResultType t in argTypes)
				if (t != XPathResultType.Number)
					throw new Exception(
						ResourceUtils.GetString("ErrorNumberExpected"));
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.Number; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.NormalRound(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]));
		}
	}
	class DecodeSignedOverpunchFunction : IXsltContextFunction 
	{
		public RuleEngine Engine;

		public DecodeSignedOverpunchFunction(XPathResultType[] argTypes)
		{
			ArgTypes = argTypes;
			if (argTypes[0] != XPathResultType.String)
			{
				throw new Exception(
					ResourceUtils.GetString("ErrorStringExpected"));
			}
			if (argTypes[1] != XPathResultType.Number)
			{
				throw new Exception(
					ResourceUtils.GetString("ErrorNumberExpected"));
			}
		}
		
		public int Minargs => 2;
		public int Maxargs => 2;
		public XPathResultType ReturnType => XPathResultType.Number;
		public XPathResultType[] ArgTypes { get; } 

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.DecodeSignedOverpunch(
				XmlTools.XPathArgToString(args[0]),
				int.Parse(XmlTools.XPathArgToString(args[1])));
		}
	}
	class ResizeImageFunction : IXsltContextFunction
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public ResizeImageFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
			foreach (XPathResultType t in argTypes)
				if (t != XPathResultType.String)
					throw new Exception(
						ResourceUtils.GetString("ErrorStringExpected"));
		}
		public int Minargs
		{
			get { return 3; }
		}
		public int Maxargs
		{
			get { return 3; }
		}
		public XPathResultType ReturnType
		{
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext,
			object[] args, XPathNavigator docContext)
		{

			return RuleEngine.ResizeImage(XmlTools.XPathArgToString(args[0]),
				Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
		}
	}




	class OrigamRoundFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public OrigamRoundFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return Engine.OrigamRound(XmlConvert.ToDecimal((string)args[0]), XmlTools.XPathArgToString(args[1]));
		}
	}

	class IifFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public IifFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;

			if(argTypes[0] != XPathResultType.String & argTypes[0] != XPathResultType.Boolean) throw new Exception("Incorrect argument type: String or Boolean expected");
		}
		public int Minargs    
		{ 
			get { return 3; }
		}
		public int Maxargs    
		{ 
			get { return 3; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.iif(args[0], XmlTools.XPathArgToString(args[1]), XmlTools.XPathArgToString(args[2]));
		}
	}

	class IsNullFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public IsNullFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
//			foreach(XPathResultType t in argTypes)
//				if (t != XPathResultType.String)
//					throw new Exception(
//						"incorrect argument type: string expected");
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 4; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			switch(args.Length)
			{
				case 2:
					return RuleEngine.isnull(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]));

				case 3:
					return RuleEngine.isnull(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]), XmlTools.XPathArgToString(args[2]));

				case 4:
					return RuleEngine.isnull(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]), XmlTools.XPathArgToString(args[2]), XmlTools.XPathArgToString(args[3]));

				default:
					throw new Exception(ResourceUtils.GetString("ErrorInvalidNumberArguments"));
			}
		}
	}

	class EncodeDataForUriFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public EncodeDataForUriFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 1; }
		}
		public int Maxargs    
		{ 
			get { return 1; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}
		public object Invoke(XsltContext xsltContext,
			object[] args, XPathNavigator docContext)
		{
			return RuleEngine.EncodeDataForUri(XmlTools.XPathArgToString(args[0]));
		}
	}


	class DecodeDataFromUriFunction : IXsltContextFunction
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public DecodeDataFromUriFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs
		{
			get { return 1; }
		}
		public int Maxargs
		{
			get { return 1; }
		}
		public XPathResultType ReturnType
		{
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{
			get { return _argTypes; }
		}
		public object Invoke(XsltContext xsltContext,
			object[] args, XPathNavigator docContext)
		{
			return RuleEngine.DecodeDataFromUri(XmlTools.XPathArgToString(args[0]));
		}
	}


	class AddDaysFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public AddDaysFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.AddDays(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]));
		}
	}

	class AddYearsFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public AddYearsFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.AddYears(XmlTools.XPathArgToString(
				args[0]), XmlTools.XPathArgToString(args[1]));
		}
	}

    class FormatNumberFunction : IXsltContextFunction
    {
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public FormatNumberFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}
        public object Invoke(
            XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
			return Engine.FormatNumber(XmlTools.XPathArgToString(
				args[0]), XmlTools.XPathArgToString(args[1]));
        }
    }

    class FormatLinkFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public FormatLinkFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.FormatLink(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]));
		}
	}

	class AddMonthsFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public AddMonthsFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.AddMonths(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]));
		}
	}


	class AddHoursFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public AddHoursFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.AddHours(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]));
		}
	}

	class AddMinutesFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public AddMinutesFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.AddMinutes(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]));
		}
	}

	class DifferenceInDaysFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public DifferenceInDaysFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.DifferenceInDays(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]));
		}
	}

	class DifferenceInMinutesFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public DifferenceInMinutesFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.DifferenceInMinutes(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]));
		}
	}

	class DifferenceInSecondsFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public DifferenceInSecondsFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.DifferenceInSeconds(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]));
		}
	}

	class GetConstantFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public GetConstantFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
			foreach(XPathResultType t in argTypes)
				if (t != XPathResultType.String)
					throw new Exception(ResourceUtils.GetString("ErrorStringExpected"));
		}
		public int Minargs    
		{ 
			get { return 1; }
		}
		public int Maxargs    
		{ 
			get { return 1; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return Engine.GetConstant((string)args[0]);
		}
	}

    class GetStringFunction : IXsltContextFunction
    {
        private XPathResultType[] _argTypes = null;
        private bool _throwException;
        public RuleEngine Engine = null;

        public GetStringFunction(XPathResultType[] argTypes, bool throwException)
		{
			_argTypes = argTypes;
            _throwException = throwException;
			foreach(XPathResultType t in argTypes)
				if (t != XPathResultType.String)
					throw new Exception(ResourceUtils.GetString("ErrorStringExpected"));
		}
		public int Minargs    
		{ 
			get { return 1; }
		}
		public int Maxargs    
		{ 
			get { return 9; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
            switch (args.Length)
            {
                case 1:
                    return Engine.GetStringVariadic((string)args[0], _throwException);
                case 2:
                    return Engine.GetStringVariadic((string)args[0], _throwException, XmlTools.XPathArgToString(args[1]));
                case 3:
                    return Engine.GetStringVariadic((string)args[0], _throwException, XmlTools.XPathArgToString(args[2]));
                case 4:
                    return Engine.GetStringVariadic((string)args[0], _throwException, XmlTools.XPathArgToString(args[2]), XmlTools.XPathArgToString(args[3]));
                case 5:
                    return Engine.GetStringVariadic((string)args[0], _throwException, XmlTools.XPathArgToString(args[2]), XmlTools.XPathArgToString(args[3]), XmlTools.XPathArgToString(args[4]));
                case 6:
                    return Engine.GetStringVariadic((string)args[0], _throwException, XmlTools.XPathArgToString(args[2]), XmlTools.XPathArgToString(args[3]), XmlTools.XPathArgToString(args[4]), XmlTools.XPathArgToString(args[5]));
                case 7:
                    return Engine.GetStringVariadic((string)args[0], _throwException, XmlTools.XPathArgToString(args[2]), XmlTools.XPathArgToString(args[3]), XmlTools.XPathArgToString(args[4]), XmlTools.XPathArgToString(args[5]), XmlTools.XPathArgToString(args[6]));
                case 8:
                    return Engine.GetStringVariadic((string)args[0], _throwException, XmlTools.XPathArgToString(args[2]), XmlTools.XPathArgToString(args[3]), XmlTools.XPathArgToString(args[4]), XmlTools.XPathArgToString(args[5]), XmlTools.XPathArgToString(args[6]), XmlTools.XPathArgToString(args[7]));
                case 9:
                    return Engine.GetStringVariadic((string)args[0], _throwException, XmlTools.XPathArgToString(args[2]), XmlTools.XPathArgToString(args[3]), XmlTools.XPathArgToString(args[4]), XmlTools.XPathArgToString(args[5]), XmlTools.XPathArgToString(args[6]), XmlTools.XPathArgToString(args[7]), XmlTools.XPathArgToString(args[8]));
                default:
                    throw new Exception(ResourceUtils.GetString("ErrorInvalidNumberArguments"));
            }        
		}
	}

	class GenerateSerialFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public GenerateSerialFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
			foreach(XPathResultType t in argTypes)
				if (t != XPathResultType.String)
					throw new Exception(ResourceUtils.GetString("ErrorStringExpected"));
		}
		public int Minargs    
		{ 
			get { return 1; }
		}
		public int Maxargs    
		{ 
			get { return 2; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			switch(args.Length)
			{
				case 1:
					return Engine.GenerateSerial((string)args[0]);
				case 2:
					return Engine.GenerateSerial((string)args[0], (string)args[1]);
			}

			throw new InvalidOperationException();
		}
	}

	class IsFeatureOnFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;

		public IsFeatureOnFunction (XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
			foreach(XPathResultType t in argTypes)
				if (t != XPathResultType.String)
					throw new Exception(ResourceUtils.GetString("ErrorStringExpected"));
		}
		public int Minargs    
		{ 
			get { return 1; }
		}
		public int Maxargs    
		{ 
			get { return 1; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.Boolean; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.IsFeatureOn((string)args[0]);
		}
	}

	class IsInRoleFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;

		public IsInRoleFunction (XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
			foreach(XPathResultType t in argTypes)
				if (t != XPathResultType.String)
					throw new Exception(ResourceUtils.GetString("ErrorStringExpected"));
		}
		public int Minargs    
		{ 
			get { return 1; }
		}
		public int Maxargs    
		{ 
			get { return 1; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.Boolean; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.IsInRole((string)args[0]);
		}
	}

    class IsInStateFunction : IXsltContextFunction
    {
        private XPathResultType[] _argTypes = null;

        public IsInStateFunction(XPathResultType[] argTypes)
        {
            _argTypes = argTypes;
            foreach (XPathResultType t in argTypes)
                if (t != XPathResultType.String)
                    throw new Exception(ResourceUtils.GetString("ErrorStringExpected"));
        }
        public int Minargs
        {
            get { return 4; }
        }
        public int Maxargs
        {
            get { return 4; }
        }
        public XPathResultType ReturnType
        {
            get { return XPathResultType.Boolean; }
        }
        public XPathResultType[] ArgTypes
        {
            get { return _argTypes; }
        }

        public object Invoke(XsltContext xsltContext,
            object[] args, XPathNavigator docContext)
        {
            return RuleEngine.IsInState((string)args[0], (string)args[1], (string)args[2], (string)args[3]);
        }
    }

	class IsUserLockedOutFunction : IXsltContextFunction
	{
		private XPathResultType[] _argTypes = null;

		public IsUserLockedOutFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
			foreach (XPathResultType t in argTypes)
				if (t != XPathResultType.String)
					throw new Exception(ResourceUtils.GetString("ErrorStringExpected"));
		}
		public int Minargs
		{
			get { return 1; }
		}
		public int Maxargs
		{
			get { return 1; }
		}
		public XPathResultType ReturnType
		{
			get { return XPathResultType.Boolean; }
		}
		public XPathResultType[] ArgTypes
		{
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext,
			object[] args, XPathNavigator docContext)
		{
			return RuleEngine.IsUserLockedOut((string)args[0]);
		}
	}

	class IsUserEmailConfirmedFunction : IXsltContextFunction
	{
		private XPathResultType[] _argTypes = null;

		public IsUserEmailConfirmedFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
			foreach (XPathResultType t in argTypes)
				if (t != XPathResultType.String)
					throw new Exception(ResourceUtils.GetString("ErrorStringExpected"));
		}
		public int Minargs
		{
			get { return 1; }
		}
		public int Maxargs
		{
			get { return 1; }
		}
		public XPathResultType ReturnType
		{
			get { return XPathResultType.Boolean; }
		}
		public XPathResultType[] ArgTypes
		{
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext,
			object[] args, XPathNavigator docContext)
		{
			return RuleEngine.IsUserEmailConfirmed((string)args[0]);
		}
	}

	class NumberOperandFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;
        public string _operation = null;

		public NumberOperandFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
        public NumberOperandFunction(XPathResultType[] argTypes, string operation)
        {
            _argTypes = argTypes;
            _operation = operation;
        }
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 3; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.Number; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
            if (_operation == null)
            {
                return Engine.NumberOperand(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]), XmlTools.XPathArgToString(args[2]));
            }
            else
            {
                return Engine.NumberOperand(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]), _operation);
            }
		}
	}

	class MaxStringFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public MaxStringFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
			foreach(XPathResultType t in argTypes)
				if (t != XPathResultType.NodeSet)
					throw new Exception(
						ResourceUtils.GetString("ErrorNodeSetExpected"));
		}
		public int Minargs    
		{ 
			get { return 1; }
		}
		public int Maxargs    
		{ 
			get { return 1; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return Engine.MaxString((XPathNodeIterator)args[0]);
		}
	}

	class MinStringFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public MinStringFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
			foreach(XPathResultType t in argTypes)
				if (t != XPathResultType.NodeSet)
					throw new Exception(
						ResourceUtils.GetString("ErrorNodeSetExpected"));
		}
		public int Minargs    
		{ 
			get { return 1; }
		}
		public int Maxargs    
		{ 
			get { return 1; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return Engine.MinString((XPathNodeIterator)args[0]);
		}
	}


	class ToXmlFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;

		public ToXmlFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
			foreach(XPathResultType t in argTypes)
				if (t != XPathResultType.String)
					throw new Exception(
						ResourceUtils.GetString("ErrorStringExpected"));
		}
		public int Minargs    
		{ 
			get { return 1; }
		}
		public int Maxargs    
		{ 
			get { return 1; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.NodeSet; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			return RuleEngine.ToXml((string)args[0]);
		}
	}


	class LookupValueFunction : IXsltContextFunction 
	{
		private XPathResultType[] _argTypes = null;
		public RuleEngine Engine = null;

		public LookupValueFunction(XPathResultType[] argTypes)
		{
			_argTypes = argTypes;
		}
		public int Minargs    
		{ 
			get { return 2; }
		}
		public int Maxargs    
		{ 
			get { return 9; }
		}
		public XPathResultType ReturnType 
		{ 
			get { return XPathResultType.String; }
		}
		public XPathResultType[] ArgTypes
		{ 
			get { return _argTypes; }
		}

		public object Invoke(XsltContext xsltContext, 
			object[] args, XPathNavigator docContext) 
		{
			switch(args.Length)
			{
				case 2:
					return Engine.LookupValue(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]));

				case 5:
					return Engine.LookupValue(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]), XmlTools.XPathArgToString(args[2]), XmlTools.XPathArgToString(args[3]), XmlTools.XPathArgToString(args[4]));

				case 7:
					return Engine.LookupValue(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]), XmlTools.XPathArgToString(args[2]), XmlTools.XPathArgToString(args[3]), XmlTools.XPathArgToString(args[4]), XmlTools.XPathArgToString(args[5]), XmlTools.XPathArgToString(args[6]));

				case 9:
					return Engine.LookupValue(XmlTools.XPathArgToString(args[0]), XmlTools.XPathArgToString(args[1]), XmlTools.XPathArgToString(args[2]), XmlTools.XPathArgToString(args[3]), XmlTools.XPathArgToString(args[4]), XmlTools.XPathArgToString(args[5]), XmlTools.XPathArgToString(args[6]), XmlTools.XPathArgToString(args[7]), XmlTools.XPathArgToString(args[8]));

				default:
					throw new Exception(ResourceUtils.GetString("ErrorInvalidNumberArguments"));
			}
		}
	}

    class Is2FAEnforcedFunction : IXsltContextFunction
    {
        private XPathResultType[] _argTypes = null;

        public Is2FAEnforcedFunction(XPathResultType[] argTypes)
        {
            _argTypes = argTypes;
            foreach (XPathResultType t in argTypes)
                if (t != XPathResultType.String)
                    throw new Exception(ResourceUtils.GetString("ErrorStringExpected"));
        }
        public int Minargs
        {
            get { return 1; }
        }
        public int Maxargs
        {
            get { return 1; }
        }
        public XPathResultType ReturnType
        {
            get { return XPathResultType.Boolean; }
        }
        public XPathResultType[] ArgTypes
        {
            get { return _argTypes; }
        }

        public object Invoke(XsltContext xsltContext,
            object[] args, XPathNavigator docContext)
        {
            return RuleEngine.Is2FAEnforced((string)args[0]);
        }
    }

    class UTCDateTimeFunction : IXsltContextFunction
    {
        private XPathResultType[] _argTypes = null;

        public UTCDateTimeFunction(XPathResultType[] argTypes)
        {
        }
        public int Minargs
        {
            get { return 0; }
        }
        public int Maxargs
        {
            get { return 0; }
        }
        public XPathResultType ReturnType
        {
            get { return XPathResultType.String; }
        }
        public XPathResultType[] ArgTypes
        {
            get { return _argTypes; }
        }

        public object Invoke(XsltContext xsltContext,
            object[] args, XPathNavigator docContext)
        {
            return RuleEngine.UTCDateTime();
        }
    }
    class LocalDateTimeFunction : IXsltContextFunction
    {
        private XPathResultType[] _argTypes = null;

        public LocalDateTimeFunction(XPathResultType[] argTypes)
        {
        }
        public int Minargs
        {
            get { return 0; }
        }
        public int Maxargs
        {
            get { return 0; }
        }
        public XPathResultType ReturnType
        {
            get { return XPathResultType.String; }
        }
        public XPathResultType[] ArgTypes
        {
            get { return _argTypes; }
        }

        public object Invoke(XsltContext xsltContext,
            object[] args, XPathNavigator docContext)
        {
            return RuleEngine.LocalDateTime();
        }
    }

#endregion

#region XPath Helper Classes
    public class RuleFunctionContext : XsltContext
	{
		private ExsltContext _exslt;
		public RuleEngine Engine;

		public RuleFunctionContext(NameTable table) : base (table)
		{
			_exslt = new ExsltContext(table);
		}

		public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
		{
			if (name == "GetConstant")
			{
				GetConstantFunction f = new GetConstantFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
            else if (name == "GetString")
            {
                GetStringFunction f = new GetStringFunction(ArgTypes, true);
                f.Engine = this.Engine;
                return f;
            }
            else if (name == "GetStringOrEmpty")
            {
                GetStringFunction f = new GetStringFunction(ArgTypes, false);
                f.Engine = this.Engine;
                return f;
            }
            else if (name == "NumberOperand")
            {
                NumberOperandFunction f = new NumberOperandFunction(ArgTypes);
                f.Engine = this.Engine;
                return f;
            }
            else if (name == "Plus")
            {
                NumberOperandFunction f = new NumberOperandFunction(ArgTypes, "PLUS");
                f.Engine = this.Engine;
                return f;
            }
            else if (name == "Minus")
            {
                NumberOperandFunction f = new NumberOperandFunction(ArgTypes, "MINUS");
                f.Engine = this.Engine;
                return f;
            }
            else if (name == "Div")
            {
                NumberOperandFunction f = new NumberOperandFunction(ArgTypes, "DIV");
                f.Engine = this.Engine;
                return f;
            }
            else if (name == "Mul")
            {
                NumberOperandFunction f = new NumberOperandFunction(ArgTypes, "MUL");
                f.Engine = this.Engine;
                return f;
            }
            else if (name == "Mod")
            {
                NumberOperandFunction f = new NumberOperandFunction(ArgTypes, "MOD");
                f.Engine = this.Engine;
                return f;
            }
			else if (name == "FormatNumber")
			{
				FormatNumberFunction f = new FormatNumberFunction(ArgTypes);
                f.Engine = this.Engine;
				return f;
			}
            else if (name == "MinString")
            {
                MinStringFunction f = new MinStringFunction(ArgTypes);
                f.Engine = this.Engine;

                return f;
            }
            else if (name == "MaxString")
            {
                MaxStringFunction f = new MaxStringFunction(ArgTypes);
                f.Engine = this.Engine;
                return f;
            }
            else if (name == "LookupValue")
            {
                LookupValueFunction f = new LookupValueFunction(ArgTypes);
                f.Engine = this.Engine;

                return f;
            }
            else if (name == "NormalRound")
            {
                NormalRoundFunction f = new NormalRoundFunction(ArgTypes);
                f.Engine = this.Engine;
                return f;
            }
			else if (name == "DecodeSignedOverpunch")
			{
				DecodeSignedOverpunchFunction f = new DecodeSignedOverpunchFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "ResizeImage")
			{
				ResizeImageFunction f = new ResizeImageFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
            else if (name == "OrigamRound")
            {
                OrigamRoundFunction f = new OrigamRoundFunction(ArgTypes);
                f.Engine = this.Engine;
                return f;
            }
            else if (name == "iif")
            {
                IifFunction f = new IifFunction(ArgTypes);
                f.Engine = this.Engine;
                return f;
            }
            else if (name == "isnull")
            {
                IsNullFunction f = new IsNullFunction(ArgTypes);
                f.Engine = this.Engine;
                return f;
            }
			else if (name == "EncodeDataForUri")
			{
				EncodeDataForUriFunction f = new EncodeDataForUriFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "DecodeDataFromUri")
			{
				DecodeDataFromUriFunction f = new DecodeDataFromUriFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "AddDays")
			{
				AddDaysFunction f = new AddDaysFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "AddMonths")
			{
				AddMonthsFunction f = new AddMonthsFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "AddHours")
			{
				AddHoursFunction f = new AddHoursFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "AddMinutes")
			{
				return new AddMinutesFunction(ArgTypes);
			}
			else if (name == "AddYears")
			{
				AddYearsFunction f = new AddYearsFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "DifferenceInDays")
			{
				DifferenceInDaysFunction f = new DifferenceInDaysFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "DifferenceInMinutes")
			{
				DifferenceInMinutesFunction f = new DifferenceInMinutesFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "DifferenceInSeconds")
			{
				DifferenceInSecondsFunction f = new DifferenceInSecondsFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
            else if (name == "UTCDateTime")
            {
                UTCDateTimeFunction f = new UTCDateTimeFunction(ArgTypes);
                return f;
            }
            else if (name == "LocalDateTime")
            {
                LocalDateTimeFunction f = new LocalDateTimeFunction(ArgTypes);
                return f;
            }
			else if (name == "IsFeatureOn")
			{
				IsFeatureOnFunction f = new IsFeatureOnFunction(ArgTypes);
				return f;
			}
			else if (name == "IsInRole")
			{
				IsInRoleFunction f = new IsInRoleFunction(ArgTypes);
				return f;
			}
			else if (name == "ActiveProfileBusinessUnitId")
			{
				ActiveProfileBusinessUnitIdFunction f = new ActiveProfileBusinessUnitIdFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "ActiveProfileId")
			{
				ActiveProfileIdFunction f = new ActiveProfileIdFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "ActiveProfileOrganizationId")
			{
				ActiveProfileOrganizationIdFunction f = new ActiveProfileOrganizationIdFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "null")
			{
				NullFunction f = new NullFunction(ArgTypes);
				return f;
			}
			else if (name == "ToXml")
			{
				ToXmlFunction f = new ToXmlFunction(ArgTypes);
				return f;
			}
			else if (name == "GenerateSerial")
			{
				GenerateSerialFunction f = new GenerateSerialFunction(ArgTypes);
				f.Engine = this.Engine;
				return f;
			}
			else if (name == "IsInState")
			{
				IsInStateFunction f = new IsInStateFunction(ArgTypes);
				return f;
			}
			else if (name == "FormatLink")
			{
				FormatLinkFunction f = new FormatLinkFunction(ArgTypes);
				return f;
			}
			else if (name == "IsUserLockedOut")
			{
				IsUserLockedOutFunction f = new IsUserLockedOutFunction(ArgTypes);
				return f;
			}
			else if (name == "IsUserEmailConfirmed")
			{
				IsUserEmailConfirmedFunction f = new IsUserEmailConfirmedFunction(ArgTypes);
				return f;
			}
			else if (name == "Is2FAEnforced")
			{
				Is2FAEnforcedFunction f = new Is2FAEnforcedFunction(ArgTypes);
				return f;
			}
			else
			{
                throw new XPathException(string.Format("Xsl Function not found: prefix='{0}', name='{1}'",
                    prefix, name), null);
			}
		}

		public override IXsltContextVariable ResolveVariable(string prefix, string name)
		{
			return null;
		}


		public override bool PreserveWhitespace(XPathNavigator node)
		{
			return false;
		}

		public override int CompareDocument(string baseUri, string nextbaseUri)
		{
			return 0;
		}

		public override bool Whitespace
		{
			get
			{
				return false;
			}
		}
	}

	public class XPathNodeList : XmlNodeList
	{
		// Methods
		static XPathNodeList()
		{
			XPathNodeList.nullparams = new object[0];
		}
		
		public XPathNodeList(XPathNodeIterator iterator)
		{
			this.iterator = iterator;
			this.list = new ArrayList();
			this.done = false;
		}

		public override IEnumerator GetEnumerator()
		{
			return new XmlNodeListEnumerator(this);
		}
		
		private XmlNode GetNode(XPathNavigator n)
		{
			IHasXmlNode node1 = (IHasXmlNode) n;
			return node1.GetNode();
		}

		public override XmlNode Item(int index)
		{
			if (index >= this.list.Count)
			{
				this.ReadUntil(index);
			}
			if ((index < this.list.Count) && (index >= 0))
			{
				return (XmlNode) this.list[index];
			}
			return null;
		}

		internal int ReadUntil(int index)
		{
			int num1 = this.list.Count;
			while (!this.done && (index >= num1))
			{
				if (this.iterator.MoveNext())
				{
					XmlNode node1 = this.GetNode(this.iterator.Current);
					if (node1 != null)
					{
						this.list.Add(node1);
						num1++;
					}
				}
				else
				{
					this.done = true;
					return num1;
				}
			}
			return num1;
		}

		// Properties
		public override int Count 
		{
			get
			{
				if (!this.done)
				{
					this.ReadUntil(0x7fffffff);
				}
				return this.list.Count;
			}
		}

		// Fields
		private bool done;
		private XPathNodeIterator iterator;
		private ArrayList list;
		private static readonly object[] nullparams;
	}

	internal class XmlNodeListEnumerator : IEnumerator
	{
		// Methods
		public XmlNodeListEnumerator(XPathNodeList list)
		{
			this.list = list;
			this.index = -1;
			this.valid = false;
		}

		public bool MoveNext()
		{
			this.index++;
			int num1 = this.list.ReadUntil(this.index + 1);
			if (this.index > (num1 - 1))
			{
				return false;
			}
			this.valid = this.list[this.index] != null;
			return this.valid;
		}

		public void Reset()
		{
			this.index = -1;
		}

		// Properties
		public object Current
		{
			get
			{
				if (this.valid)
				{
					return this.list[this.index];
				}
				return null;
			}
		}
 
		// Fields
		private int index;
		private XPathNodeList list;
		private bool valid;
	}

	#endregion

	#region IComparer Members
	public class ProcessRuleComparer : IComparer
	{
		int IComparer.Compare(Object x, Object y)
		{
			if ((x as DataStructureRule) != null && (y as DataStructureRule) != null)
			{
				return (x as DataStructureRule).Priority.CompareTo((y as DataStructureRule).Priority);
			}
			else
			{
				// rulesets are always an top, so rules are greater
				return 1;
			}
		}
	}
    #endregion
}
