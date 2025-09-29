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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Origam.DA;
using Origam.Extensions;
using Origam.Server;
using Origam.Server.Attributes;

namespace Origam.Server.Model.UIService;

public class GetGroupsInput : IEntityIdentification
{
    [RequiredNonDefault]
    public Guid MenuId { get; set; }

    [RequiredNonDefault]
    public Guid DataStructureEntityId { get; set; }
    public string Filter { get; set; }
    public List<InputRowOrdering> Ordering { get; set; }

    [Required]
    public int RowLimit { get; set; }

    [Required]
    public string GroupBy { get; set; }
    public string GroupingUnit { get; set; }
    public Guid MasterRowId { get; set; }
    public Guid GroupByLookupId { get; set; }
    public Guid SessionFormIdentifier { get; set; }
    public List<Aggregation> AggregatedColumns { get; set; }
    public List<IRowOrdering> OrderingList => Ordering.ToList<IRowOrdering>();
    public Dictionary<string, Guid> FilterLookups { get; set; }
}
