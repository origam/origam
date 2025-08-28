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
using System.Collections.Generic;
using System.Data;
using Origam.DA;
using Origam.Schema;
using Origam.Schema.EntityModel;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Workbench.Services;

public class NullParameterService : IParameterService
{
    public void InitializeService() { }

    public void UnloadService() { }

    public string GetString(string name, params object[] args)
    {
        return "";
    }

    public string GetString(string name, bool throwException, params object[] args)
    {
        return "";
    }

    public object GetCustomParameterValue(Guid id)
    {
        return null;
    }

    public object GetCustomParameterValue(string parameterName)
    {
        return null;
    }

    public object GetParameterValue(Guid id, Guid? overridenProfileId = null)
    {
        return null;
    }

    public object GetParameterValue(
        Guid id,
        OrigamDataType targetType,
        Guid? overridenProfileId = null
    )
    {
        return null;
    }

    public object GetParameterValue(string parameterName, Guid? overridenProfileId = null)
    {
        return null;
    }

    public object GetParameterValue(
        string parameterName,
        OrigamDataType targetType,
        Guid? overridenProfileId = null
    )
    {
        return null;
    }

    public void SetFeatureStatus(string featureCode, bool status) { }

    public bool IsFeatureOn(string featureCode)
    {
        return true;
    }

    public void SetCustomParameterValue(
        Guid id,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue
    ) { }

    public void SetCustomParameterValue(
        Guid id,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue,
        bool useIdentity
    ) { }

    public void SetCustomParameterValue(
        Guid id,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue,
        Guid? overridenProfileId
    ) { }

    public void SetCustomParameterValue(
        string parameterName,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue
    ) { }

    public void SetCustomParameterValue(
        string parameterName,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue,
        bool useIdentity
    ) { }

    public void SetCustomParameterValue(
        string parameterName,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue,
        Guid? overridenProfileId
    ) { }

    public void RefreshParameters() { }

    public void PrepareParameters() { }

    public Guid ResolveLanguageId(string cultureString)
    {
        return Guid.Empty;
    }
}

/// <summary>
/// Summary description for ParameterService.
/// </summary>
public class ParameterService : IParameterService
{
    SchemaService _schemaService =
        ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
    IDictionary<Guid, object> _constantsById = new Dictionary<Guid, object>();
    IDictionary<string, Guid> _constantIdByName = new Dictionary<string, Guid>();
    SortedList _stringsByName = new SortedList();
    IDictionary<Guid, DataConstant> _userDefinableConstants = new Dictionary<Guid, DataConstant>();
    OrigamParametersData _parameterData = null;
    private object _parameterLock = new object();
    private static Hashtable languageResolveDict = null;

    #region IParameterService Members
    public string GetString(string name, params object[] args)
    {
        return GetString(name, true, args);
    }

    public string GetString(string name, bool throwException, params object[] args)
    {
        if (_stringsByName.Contains(name))
        {
            IPersistenceService ps =
                ServiceManager.Services.GetService(typeof(IPersistenceService))
                as IPersistenceService;
            StringItem s = (StringItem)
                ps.SchemaProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey((Guid)_stringsByName[name])
                );
            string rawString = s.String;
            return string.Format(rawString, args);
        }
        if (throwException)
        {
            throw new ArgumentOutOfRangeException(
                "name",
                name,
                ResourceUtils.GetString("ErrorStringNotFound")
            );
        }

        return "";
    }

    public object GetParameterValue(Guid id, Guid? overridenProfileId = null)
    {
        if (_constantsById.ContainsKey(id))
        {
            if (_userDefinableConstants.ContainsKey(id))
            {
                DataConstant constant = _userDefinableConstants[id];
                OrigamParametersData.OrigamParametersRow row =
                    _parameterData.OrigamParameters.FindByProfileIdId(
                        (
                            (overridenProfileId != null)
                                ? overridenProfileId.Value
                                : SecurityManager.CurrentUserProfile().Id
                        ),
                        id
                    );
                if (row == null)
                {
                    if (constant.UserDefinableDefaultConstant == null)
                    {
                        return constant.Value;
                    }

                    return GetParameterValue(constant.UserDefinableDefaultConstantId);
                }

                return ValueFromRow(constant.DataType, row);
            }

            return _constantsById[id];
        }
        throw new ArgumentOutOfRangeException(
            "id",
            id,
            ResourceUtils.GetString("ErrorParamNotFound")
        );
    }

    public object GetParameterValue(
        Guid id,
        OrigamDataType targetType,
        Guid? overridenProfileId = null
    )
    {
        return Convert(targetType, GetParameterValue(id, overridenProfileId));
    }

    public object GetParameterValue(string parameterName, Guid? overridenProfileId = null)
    {
        if (_constantIdByName.ContainsKey(parameterName))
        {
            return GetParameterValue(_constantIdByName[parameterName], overridenProfileId);
        }

        throw new ArgumentOutOfRangeException(
            "parameterName",
            parameterName,
            ResourceUtils.GetString("ConstantNotFound")
        );
    }

    public object GetParameterValue(
        string parameterName,
        OrigamDataType targetType,
        Guid? overridenProfileId = null
    )
    {
        return Convert(targetType, GetParameterValue(parameterName, overridenProfileId));
    }

    public object GetCustomParameterValue(string parameterName)
    {
        if (_constantIdByName.ContainsKey(parameterName))
        {
            return GetCustomParameterValue(_constantIdByName[parameterName]);
        }

        throw new ArgumentOutOfRangeException(
            "parameterName",
            parameterName,
            ResourceUtils.GetString("ConstantNotFound")
        );
    }

    public object GetCustomParameterValue(Guid id)
    {
        IDataLookupService ls =
            ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
        IPersistenceService ps =
            ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        DataConstant constant =
            ps.SchemaProvider.RetrieveInstance(typeof(DataConstant), new ModelElementKey(id))
            as DataConstant;
        Guid lookupId;
        switch (constant.DataType)
        {
            case OrigamDataType.Date:
            {
                lookupId = new Guid("394142f3-c07f-4d7a-a05a-d83fa5f8625e");
                break;
            }

            case OrigamDataType.Boolean:
            {
                lookupId = new Guid("f04ad389-c4c9-4af5-b69f-ef9dbe88378f");
                break;
            }

            case OrigamDataType.Integer:
            {
                lookupId = new Guid("852438ec-2c48-4ea7-b9b3-c554555bc93b");
                break;
            }

            case OrigamDataType.UniqueIdentifier:
            {
                lookupId = new Guid("cbcbd053-a374-4661-a1ae-e6c9004edffe");
                break;
            }

            case OrigamDataType.Currency:
            {
                lookupId = new Guid("d592a7f5-1e95-4d49-97d4-e0133f2b8154");
                break;
            }

            case OrigamDataType.Float:
            {
                lookupId = new Guid("3f08e25d-bbea-4026-b84f-956524d2b763");
                break;
            }

            case OrigamDataType.String:
            {
                lookupId = new Guid("2e14ba59-e415-4098-9414-2cca6e702f5b");
                break;
            }

            default:
                throw new ArgumentOutOfRangeException(
                    "dataType",
                    constant.DataType,
                    "Type not supported for custom parameters. Cannot retrieve value."
                );
        }
        return ls.GetDisplayText(lookupId, id, false, false, null);
    }

    public void SetCustomParameterValue(
        string parameterName,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue
    )
    {
        SetCustomParameterValue(
            parameterName,
            value,
            guidValue,
            intValue,
            stringValue,
            boolValue,
            floatValue,
            currencyValue,
            dateValue,
            true
        );
    }

    public void SetCustomParameterValue(
        string parameterName,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue,
        bool useIdentity
    )
    {
        SetCustomParameterValue(
            parameterName,
            value,
            guidValue,
            intValue,
            stringValue,
            boolValue,
            floatValue,
            currencyValue,
            dateValue,
            useIdentity,
            null
        );
    }

    private void SetCustomParameterValue(
        string parameterName,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue,
        bool useIdentity,
        Guid? overridenProfileId
    )
    {
        if (_constantIdByName.ContainsKey(parameterName))
        {
            SetCustomParameterValue(
                _constantIdByName[parameterName],
                value,
                guidValue,
                intValue,
                stringValue,
                boolValue,
                floatValue,
                currencyValue,
                dateValue,
                useIdentity,
                overridenProfileId
            );
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                "parameterName",
                parameterName,
                ResourceUtils.GetString("ConstantNotFound")
            );
        }
    }

    public void SetCustomParameterValue(
        Guid id,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue
    )
    {
        SetCustomParameterValue(
            id,
            value,
            guidValue,
            intValue,
            stringValue,
            boolValue,
            floatValue,
            currencyValue,
            dateValue,
            true
        );
    }

    public void SetCustomParameterValue(
        Guid id,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue,
        bool useIdentity
    )
    {
        SetCustomParameterValue(
            id,
            value,
            guidValue,
            intValue,
            stringValue,
            boolValue,
            floatValue,
            currencyValue,
            dateValue,
            useIdentity,
            null
        );
    }

    private void SetCustomParameterValue(
        Guid id,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue,
        bool useIdentity,
        Guid? overridenProfileId
    )
    {
        // test if the value exists, otherwise error will be thrown
        GetParameterValue(id);
        bool isUserDefinable = _userDefinableConstants.ContainsKey(id);
        UserProfile profile = null;
        if (useIdentity)
        {
            profile = SecurityManager.CurrentUserProfile();
        }
        Guid profileId;
        if (overridenProfileId.HasValue)
        {
            profileId = overridenProfileId.Value;
        }
        else
        {
            if (!useIdentity && isUserDefinable)
            {
                throw new Exception(
                    string.Format(
                        "Constant {0} is set to be user definable. Cannot set its value at this time.",
                        id
                    )
                );
            }
            profileId = (isUserDefinable ? profile.Id : Guid.Empty);
        }
        // save the value into the custom parameter database
        OrigamParametersData.OrigamParametersRow row =
            _parameterData.OrigamParameters.FindByProfileIdId(profileId, id);
        if (row == null)
        {
            row = _parameterData.OrigamParameters.NewOrigamParametersRow();
            row.RecordCreated = DateTime.Now;
            if (useIdentity)
            {
                row.RecordCreatedBy = profile.Id;
            }
            row.Id = id;
            row.ProfileId = profileId;
            _parameterData.OrigamParameters.AddOrigamParametersRow(row);
        }
        else
        {
            row.RecordUpdated = DateTime.Now;
            if (useIdentity)
            {
                row.RecordUpdatedBy = profile.Id;
            }
        }
        row.GuidValue = guidValue;
        row.IntValue = intValue;
        row.StringValue = stringValue;
        row.BooleanValue = boolValue;
        row.CurrencyValue = currencyValue;
        if (dateValue == null)
        {
            row.SetDateValueNull();
        }
        else
        {
            row.DateValue = System.Convert.ToDateTime(dateValue);
        }
        SaveParameters(useIdentity);
        if (!isUserDefinable)
        {
            // set the value for our session
            _constantsById[id] = value;
        }
        // reset sql/dataset caches so the new default value
        // is used - only for the current user, everybody else
        // will have to log-off/log-in
        OrigamUserContext.Reset();
    }

    public void RefreshParameters()
    {
        PrepareParameters();
        LoadParameters();
        LoadFeatures();
        LoadStrings();
        DataConstantSchemaItemProvider constants =
            _schemaService.GetProvider(typeof(DataConstantSchemaItemProvider))
            as DataConstantSchemaItemProvider;
        if (constants == null)
        {
            throw new InvalidOperationException(ResourceUtils.GetString("ErrorModelNotLoaded"));
        }

        foreach (DataConstant constant in constants.ChildItems)
        {
            if (constant.Name == "null")
            {
                _constantsById[constant.Id] = null;
            }
            else
            {
                object val = constant.Value;
                // load user constants redefined by the customer
                OrigamParametersData.OrigamParametersRow row =
                    _parameterData.OrigamParameters.FindByProfileIdId(Guid.Empty, constant.Id);
                if (row != null)
                {
                    _constantsById[constant.Id] = ValueFromRow(constant.DataType, row);
                }
            }
        }
    }

    private static object ValueFromRow(
        OrigamDataType dataType,
        OrigamParametersData.OrigamParametersRow row
    )
    {
        string stringValue = null;
        int intValue = 0;
        decimal currencyValue = 0;
        decimal floatValue = 0;
        bool booleanValue = row.BooleanValue;
        Guid guidValue = Guid.Empty;
        object dateValue = null;
        if (!row.IsStringValueNull())
        {
            stringValue = row.StringValue;
        }

        if (!row.IsIntValueNull())
        {
            intValue = row.IntValue;
        }

        if (!row.IsCurrencyValueNull())
        {
            currencyValue = row.CurrencyValue;
        }

        if (!row.IsFloatValueNull())
        {
            floatValue = row.FloatValue;
        }

        if (!row.IsGuidValueNull())
        {
            guidValue = row.GuidValue;
        }

        if (!row.IsDateValueNull())
        {
            dateValue = row.DateValue;
        }

        return DataConstant.ConvertValue(
            dataType,
            stringValue,
            intValue,
            guidValue,
            currencyValue,
            floatValue,
            booleanValue,
            dateValue
        );
    }

    public bool IsFeatureOn(string features)
    {
        if (features == null | features == "")
        {
            return true;
        }

        foreach (string feature in features.Split(";".ToCharArray()))
        {
            bool negation = false;
            string featureName = feature;
            if (featureName.StartsWith("!"))
            {
                negation = true;
                featureName = feature.Substring(1);
            }
            else
            {
                featureName = feature;
            }
            if (!FeatureList.Contains(featureName))
            {
                throw new ArgumentOutOfRangeException(
                    "featureCode",
                    featureName,
                    ResourceUtils.GetString("ErrorFeatureNotFound")
                );
            }
            if ((bool)FeatureList[featureName])
            {
                if (negation)
                {
                    return false;
                }

                return true;
            }
            else if (negation)
            {
                return true;
            }
        }
        return false;
    }

    private Hashtable LanguageResolveDict
    {
        get
        {
            if (languageResolveDict != null)
            {
                return languageResolveDict;
            }
            // lazily initialize resolver dict from db
            languageResolveDict = new Hashtable();

            DataSet result = core.DataService.Instance.LoadData(
                new Guid("a406c361-3f02-4947-90ef-f9d73228ed60"),
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                null
            );
            DataTable table = result.Tables["Language"];
            if (table == null)
            {
                throw new NullReferenceException(ResourceUtils.GetString("ErrorRoleListNotLoaded"));
            }

            String[] array = new String[table.Rows.Count];
            for (int i = 0; i < array.Length; i++)
            {
                DataRow row = table.Rows[i];
                languageResolveDict[((string)row["TagIETF"]).ToLower()] = row["Id"];
            }
            return languageResolveDict;
        }
    }
    #endregion

    #region IWorkbenchService Members

    public void UnloadService()
    {
        if (_schemaService != null)
        {
            _schemaService.SchemaChanged -= new EventHandler(schemaService_SchemaChanged);
            _schemaService = null;
        }
        _constantsById.Clear();
        _constantIdByName.Clear();
        //_featureList.Clear();
        if (_parameterData != null)
        {
            _parameterData.Dispose();
            _parameterData = null;
        }
    }

    public void InitializeService()
    {
        _schemaService.SchemaChanged += new EventHandler(schemaService_SchemaChanged);
    }
    #endregion
    #region Private Methods
    private Hashtable FeatureList
    {
        get
        {
            if (!OrigamUserContext.Context.Contains("Features"))
            {
                OrigamUserContext.Context.Add("Features", new Hashtable());
                LoadFeatures();
            }
            return OrigamUserContext.Context["Features"] as Hashtable;
        }
    }

    public void PrepareParameters()
    {
        lock (_constantsById)
        {
            _constantsById.Clear();
            _constantIdByName.Clear();
            _userDefinableConstants.Clear();
            // get constants from the model
            DataConstantSchemaItemProvider constants =
                _schemaService.GetProvider(typeof(DataConstantSchemaItemProvider))
                as DataConstantSchemaItemProvider;
            if (constants == null)
            {
                throw new InvalidOperationException(ResourceUtils.GetString("ErrorModelNotLoaded"));
            }

            foreach (DataConstant constant in constants.ChildItems)
            {
                if (_constantIdByName.ContainsKey(constant.Name))
                {
                    throw new Exception(
                        ResourceUtils.GetString("MultipleConstantsWithSameName", constant.Name)
                    );
                }
                _constantIdByName.Add(constant.Name, constant.Id);
                object val = constant.Value;
                _constantsById.Add(constant.Id, val);
                if (constant.IsUserDefinable)
                {
                    _userDefinableConstants.Add(constant.Id, constant);
                }
            }
        }
    }

    private void LoadParameters()
    {
        lock (_parameterLock)
        {
            IServiceAgent dataServiceAgent = (
                ServiceManager.Services.GetService(typeof(IBusinessServicesService))
                as IBusinessServicesService
            ).GetAgent("DataService", null, null);
            if (_parameterData != null)
            {
                _parameterData.Clear();
                _parameterData.Dispose();
            }
            DataStructureQuery query = new DataStructureQuery(
                new Guid("996ec515-47a5-4e88-a94b-7bb9afca1a0d")
            );
            query.LoadByIdentity = false;
            dataServiceAgent.MethodName = "LoadDataByQuery";
            dataServiceAgent.Parameters.Clear();
            dataServiceAgent.Parameters.Add("Query", query);
            dataServiceAgent.Run();
            _parameterData = dataServiceAgent.Result as OrigamParametersData;
            if (_parameterData == null)
            {
                throw new Exception(
                    "Old version of Root model detected. Please upgrade to Root version 5.0 or higher."
                );
            }
        }
    }

    private void LoadStrings()
    {
        lock (_stringsByName)
        {
            StringSchemaItemProvider provider =
                _schemaService.GetProvider(typeof(StringSchemaItemProvider))
                as StringSchemaItemProvider;
            _stringsByName.Clear();
            foreach (StringItem s in provider.ChildItems)
            {
                _stringsByName[s.Name] = s.Id;
            }
        }
    }

    public void SetFeatureStatus(string featureCode, bool status)
    {
        // ignore features not contained in the current model
        if (this.FeatureList.Contains(featureCode))
        {
            this.FeatureList[featureCode] = status;
        }
    }

    private void LoadFeatures()
    {
        lock (FeatureList)
        {
            // prepare features defined by the model
            FeatureSchemaItemProvider provider =
                _schemaService.GetProvider(typeof(FeatureSchemaItemProvider))
                as FeatureSchemaItemProvider;
            Hashtable featureList = this.FeatureList;
            featureList.Clear();
            foreach (Feature f in provider.ChildItems)
            {
                // all features are turned off by default (e.g. if not found in the customer's configuration
                featureList[f.Name] = false;
            }
            // load custom feature settings from the database
            IServiceAgent dataServiceAgent = (
                ServiceManager.Services.GetService(typeof(IBusinessServicesService))
                as IBusinessServicesService
            ).GetAgent("DataService", null, null);
            DataStructureQuery query = new DataStructureQuery(
                new Guid("278cce8c-f0d2-4738-89f4-d85b1c9ce870")
            );
            query.LoadByIdentity = false;

            dataServiceAgent.MethodName = "LoadDataByQuery";
            dataServiceAgent.Parameters.Clear();
            dataServiceAgent.Parameters.Add("Query", query);
            try
            {
                dataServiceAgent.Run();
                DataSet data = dataServiceAgent.Result as DataSet;
                foreach (DataRow row in data.Tables["OrigamFeatureList"].Rows)
                {
                    string featureCode = (string)row["ReferenceCode"];

                    // ignore features not contained in the current model
                    if (featureList.Contains(featureCode))
                    {
                        SetFeatureStatus(featureCode, (bool)row["IsFeatureOn"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ResourceUtils.GetString("ErrorFeatureListFailed"), ex);
            }
        }
    }

    private void SaveParameters(bool useIdentity)
    {
        IServiceAgent dataServiceAgent = (
            ServiceManager.Services.GetService(typeof(IBusinessServicesService))
            as IBusinessServicesService
        ).GetAgent("DataService", null, null);
        DataStructureQuery query = new DataStructureQuery(
            new Guid("996ec515-47a5-4e88-a94b-7bb9afca1a0d")
        );
        query.LoadByIdentity = useIdentity;
        query.FireStateMachineEvents = useIdentity;
        dataServiceAgent.MethodName = "StoreDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add("Query", query);
        dataServiceAgent.Parameters.Add("Data", _parameterData);
        dataServiceAgent.Run();
    }

    private object Convert(OrigamDataType targetType, object value)
    {
        switch (targetType)
        {
            case OrigamDataType.String:
            {
                if (value == null)
                {
                    return null;
                }
                if (value is decimal)
                {
                    return System.Xml.XmlConvert.ToString((decimal)value);
                }
                else if (value is float)
                {
                    return System.Xml.XmlConvert.ToString((float)value);
                }
                else if (value is bool)
                {
                    return System.Xml.XmlConvert.ToString((bool)value);
                }
                else if (value is DateTime)
                {
                    return System.Xml.XmlConvert.ToString(
                        (DateTime)value,
                        System.Xml.XmlDateTimeSerializationMode.RoundtripKind
                    );
                }
                else
                {
                    return value.ToString();
                }
            }

            default:
                return value;
        }
    }

    private void schemaService_SchemaChanged(object sender, EventArgs e)
    {
        if ((sender is DataConstant) || (sender is Feature) || (sender is StringItem))
        {
            RefreshParameters();
        }
        OrigamUserContext.ResetAll();
    }
    #endregion
    public Guid ResolveLanguageId(string cultureString)
    {
        if (LanguageResolveDict.ContainsKey(cultureString.ToLower()))
        {
            return (Guid)LanguageResolveDict[cultureString.ToLower()];
        }

        return Guid.Empty;
    }

    public void SetCustomParameterValue(
        Guid id,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue,
        Guid? overridenProfileId
    )
    {
        SetCustomParameterValue(
            id,
            value,
            guidValue,
            intValue,
            stringValue,
            boolValue,
            floatValue,
            currencyValue,
            dateValue,
            true,
            overridenProfileId
        );
    }

    public void SetCustomParameterValue(
        string parameterName,
        object value,
        Guid guidValue,
        int intValue,
        string stringValue,
        bool boolValue,
        decimal floatValue,
        decimal currencyValue,
        object dateValue,
        Guid? overridenProfileId
    )
    {
        SetCustomParameterValue(
            parameterName,
            value,
            guidValue,
            intValue,
            stringValue,
            boolValue,
            floatValue,
            currencyValue,
            dateValue,
            true,
            overridenProfileId
        );
    }
}
