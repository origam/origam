using System;
using System.Collections;
using System.Collections.Generic;
using Origam.Schema.EntityModel;

namespace Origam.DA.Service
{
    public class SelectParameters
    {
        public DataStructure DataStructure { get; set; }
        public DataStructureEntity Entity { get; set; }
        public DataStructureFilterSet Filter { get; set; }
        public DataStructureSortSet SortSet { get; set; }
        public Hashtable Parameters { get; set; }
        public bool Paging { get; set; }
        public string ColumnName { get; set; }
        public string CustomFilters { get; set; } = "";
        public List<Tuple<string, string>> CustomOrdering { get; set; } =
            new List<Tuple<string, string>>();
    }
}