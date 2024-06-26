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

namespace Origam.DA;
public class MergeParams
{
    public bool TrueDelete { get; set; } 
    public bool PreserveChanges { get; set; }
    public bool SourceIsFragment { get; set; }
    public bool PreserveNewRowState { get; set; }
    public object ProfileId {get; set; }
    public MergeParams()
    {
        TrueDelete = false;
        PreserveChanges = false;
        SourceIsFragment = false;
        PreserveNewRowState = false;
        ProfileId = DBNull.Value;
    }
    public MergeParams(object ProfileId) : this()
    {
        this.ProfileId = ProfileId;
    }
}
