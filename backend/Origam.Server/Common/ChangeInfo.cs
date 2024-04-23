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
using Origam.Rule;

namespace Origam.Server;

/// <summary>
/// Wrapper class to be used as return object to inform client about changes in data.
/// </summary>
public class ChangeInfo
{
    #region Private Fields
    private string entity = string.Empty;
    private string requestingGrid;
    private Operation operation;
    private object objectId = string.Empty;
    private object wrappedObject = null;
    private RowSecurityState _state;
    #endregion
    #region Public Properties

    /// <summary>
    /// Entity id.
    /// In Flex client it will be used as a part of data source name (in form of entityName_rawDataHolder).
    /// </summary>
    public string Entity
    {
        get { return entity; }
        set { entity = value; }
    }

    /// <summary>
    /// Grid id. It is necessary to have this info to be able to proper react on ADD operation.
    /// </summary>
    public string RequestingGrid
    {
        get { return requestingGrid; }
        set { requestingGrid = value; }
    }
	

    /// <summary>
    /// Operation. Possible values:
    /// -2 - DELETE ALL DATA
    /// -1 - DELETE
    ///  0 - UPDATE
    ///  1 - CREATE
    ///  2 - FORM SAVED
    ///  3 - FORM NEEDS REFRESH
    ///  4 - CURRENT RECORD NEEDS RELOAD
    /// </summary>
    public Operation Operation
    {
        get { return operation; }
        set { operation = value; }
    }

    /// <summary>
    /// Id of the object. Used only in case of DELETE operation.
    /// </summary>
    public object ObjectId
    {
        get { return objectId; }
        set { objectId = value; }
    }

    /// <summary>
    /// New object/new state of the object. Used in cases of ADD or UPDATE operation.
    /// </summary>
    public object WrappedObject
    {
        get { return wrappedObject; }
        set { wrappedObject = value; }
    }

    public RowSecurityState State
    {
        get { return _state; }
        set { _state = value; }
    }

    #endregion

    public static ChangeInfo SavedChangeInfo()
    {
            ChangeInfo ci = new ChangeInfo();
            ci.Operation = Operation.FormSaved;
            return ci;
        }

    public static ChangeInfo RefreshFormChangeInfo()
    {
            ChangeInfo ci = new ChangeInfo();
            ci.Operation = Operation.FormNeedsRefresh;
            return ci;
        }

    public static ChangeInfo ReloadCurrentRecordChangeInfo()
    {
            ChangeInfo ci = new ChangeInfo();
            ci.Operation = Operation.CurrentRecordNeedsUpdate;
            return ci;
        }

    public static ChangeInfo RefreshPortalInfo()
    {
            ChangeInfo ci = new ChangeInfo();
            ci.Operation = Operation.RefreshPortal;
            return ci;
        }

    public static ChangeInfo CleanDataChangeInfo()
    {
            ChangeInfo ci = new ChangeInfo();
            ci.Operation = Operation.DeleteAllData;
            return ci;
        }

    protected bool Equals(ChangeInfo other)
    {
            return entity == other.entity && operation == other.operation && Equals(objectId, other.objectId);
        }

    public override bool Equals(object obj)
    {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((ChangeInfo)obj);
        }

    public override int GetHashCode()
    {
            return HashCode.Combine(entity, (int)operation, objectId);
        }
}