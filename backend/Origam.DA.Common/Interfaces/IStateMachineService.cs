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
using System.Data;
using Origam.Service.Core;

namespace Origam.DA;

public enum StateMachineServiceStatelessEventType
{
    RecordCreated = 0,
    RecordUpdated = 1,
    RecordDeleted = 2,
    BeforeRecordDeleted = 3,
}

/// <summary>
/// Summary description for IStateMachineService.
/// </summary>
public interface IStateMachineService
{
    object[] AllowedStateValues(
        Guid entityId,
        Guid fieldId,
        object currentStateValue,
        DataRow dataRow,
        string transactionId
    );
    object[] AllowedStateValues(
        Guid entityId,
        Guid fieldId,
        object currentStateValue,
        IXmlContainer dataRow,
        string transactionId
    );
    bool IsStateAllowed(
        Guid entityId,
        Guid fieldId,
        object currentStateValue,
        object newStateValue,
        DataRow dataRow,
        string transactionId
    );
    void OnDataChanging(DataTable changedTable, string transactionId);
    void OnDataChanged(DataSet data, List<string> changedTables, string transactionId);
    bool IsInState(Guid entityId, Guid fieldId, object currentStateValue, Guid targetStateId);
}
