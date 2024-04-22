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

using Origam.Schema;

namespace Origam.Workbench.Services;

/// <summary>
/// Summary description for IParameterService.
/// </summary>
public interface IParameterService : IWorkbenchService
{
	string GetString(string name, params object[] args);
	string GetString(string name, bool throwException, params object[] args);
	object GetCustomParameterValue(Guid id);
	object GetCustomParameterValue(string parameterName);
	object GetParameterValue(Guid id, Guid? overridenProfileId = null);
	object GetParameterValue(Guid id, OrigamDataType targetType, Guid? overridenProfileId = null);
	object GetParameterValue(string parameterName, Guid? overridenProfileId = null);
	object GetParameterValue(string parameterName, OrigamDataType targetType, Guid? overridenProfileId = null);
	void SetFeatureStatus(string featureCode, bool status);
	bool IsFeatureOn(string featureCode);
	void SetCustomParameterValue(Guid id, object value, Guid guidValue, 
		int intValue, string stringValue, bool boolValue, decimal floatValue, 
		decimal currencyValue, object dateValue);
	void SetCustomParameterValue(Guid id, object value, Guid guidValue, 
		int intValue, string stringValue, bool boolValue, decimal floatValue, 
		decimal currencyValue, object dateValue, bool useIdentity);
	void SetCustomParameterValue(Guid id, object value, Guid guidValue,
		int intValue, string stringValue, bool boolValue, decimal floatValue,
		decimal currencyValue, object dateValue, Guid? overridenProfileId);
	void SetCustomParameterValue(string parameterName, object value, 
		Guid guidValue, int intValue, string stringValue, bool boolValue, 
		decimal floatValue, decimal currencyValue, object dateValue);
	void SetCustomParameterValue(string parameterName, object value, 
		Guid guidValue, int intValue, string stringValue, bool boolValue, 
		decimal floatValue, decimal currencyValue, object dateValue, bool useIdentity);
	void SetCustomParameterValue(string parameterName, object value,
		Guid guidValue, int intValue, string stringValue, bool boolValue,
		decimal floatValue, decimal currencyValue, object dateValue, Guid? overridenProfileId);
	void RefreshParameters();
	Guid ResolveLanguageId(String cultureString);
	void PrepareParameters();
}