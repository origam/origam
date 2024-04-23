#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using Origam;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public class PortalSessionStore
{
    private DateTime? sessionStart;
    private object _profileId;
    private IList<SessionStore> _formSessions = new List<SessionStore>();
    private bool _isExclusiveScreenOpen = false;

    public PortalSessionStore(object profileId)
    {
            _profileId = profileId;
            sessionStart = DateTime.Now;
        }

    public object ProfileId
    {
        get { return _profileId; }
        set { _profileId = value; }
    }

    public IList<SessionStore> FormSessions
    {
        get { return _formSessions; }
    }

    public bool IsExclusiveScreenOpen
    {
        get
        {
                return _isExclusiveScreenOpen;
            }
        set
        {
                _isExclusiveScreenOpen = value;
            }
    }

    public SessionStore ExclusiveSession
    {
        get
        {
                if (IsExclusiveScreenOpen)
                {
                    foreach (var item in FormSessions)
                    {
                        if (item.IsExclusive)
                        {
                            return item;
                        }
                    }
                    throw new System.Exception("Exclusive screen not found.");
                }
                else
                {
                    return null;
                }
            }
    }

    public bool ShouldBeCleared()
    {
            DataSet data = core.DataService.Instance.LoadData(
                new Guid("aa4c9df9-d6da-408e-a095-fd377ffcc319"),
                new Guid("ece8b03a-f378-4026-b3b3-588cb58317b6"), 
                Guid.Empty, 
                Guid.Empty, 
                null,
                "OrigamOnlineUser_par_UserName",
                SecurityManager.CurrentPrincipal.Identity.Name);
            if (data.Tables[0].Rows.Count == 0)
            {
                return false;
            }
            else
            {
                DataRow row = data.Tables[0].Rows[0];
                return Nullable.Compare<DateTime>(
                    row["ClearSessionRequestTimestamp"] as DateTime?, sessionStart) 
                    > 0;
            }
        }

    public void ResetSessionStart()
    {
            sessionStart = DateTime.Now;
        }
}