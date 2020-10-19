using System;

namespace Origam.ServerCore
{
    public class SearchResult
    {
        public string Group { get; set; }
        public Guid DataSourceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid DataSourceLookupId { get; set; }
        public string ReferenceId { get; set; }
    }
}