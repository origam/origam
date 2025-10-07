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
using System.Data;
using System.Drawing;

namespace Origam.UI;

/// <summary>
/// Summary description for ILookupControl.
/// </summary>
public interface ILookupControl
{
    // properties
    Guid LookupId { get; set; }
    Guid EntityId { get; }
    Guid ValueFieldId { get; }
    Hashtable ParameterMappingsHashtable { get; }
    object LookupValue { get; set; }
    object OriginalLookupValue { get; }
    string LookupDisplayText { get; set; }
    DataView LookupList { get; set; }
    string LookupListValueMember { get; set; }
    string LookupListDisplayMember { get; set; }
    string LookupListTreeParentMember { get; set; }
    bool SuppressEmptyColumns { get; set; }
    bool LookupShowEditButton { get; set; }
    bool LookupCanEditSourceRecord { get; set; }
    bool CacheList { get; set; }
    bool ShowUniqueValues { get; set; }
    string DefaultBindableProperty { get; }
    string ColumnName { get; set; }
    DataRow CurrentRow { get; }
    void CreateMappingItemsCollection();
    string SearchText { get; }
    ScreenLocation ScreenLocation { get; }

    // events
    event EventHandler lookupValueChanged;
    event EventHandler LookupValueChangingByUser;
    event EventHandler LookupListRefreshRequested;
    event EventHandler LookupDisplayTextRequested;
    event EventHandler LookupShowSourceListRequested;
    event EventHandler LookupEditSourceRecordRequested;
}

public class ScreenLocation
{
    public int X { get; }
    public int Y { get; }

    public ScreenLocation(int x, int y)
    {
        X = x;
        Y = y;
    }
}
