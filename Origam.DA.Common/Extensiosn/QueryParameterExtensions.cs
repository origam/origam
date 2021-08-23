using System.Collections.Generic;
using System.Linq;

namespace Origam.DA.Common.Extensiosn
{
    public static class QueryParameterExtensions
    {
        public static QueryParameterCollection ToQueryParameterCollection(this IEnumerable<QueryParameter> parameters)
        {
            return new QueryParameterCollection(parameters.ToArray());
        }
    }
}