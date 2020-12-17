using System;
using System.Collections.Generic;
using System.Text;

namespace Origam.DA.Service.Generators
{
    public class PgSqlFilterRenderer : AbstractFilterRenderer
    {
        protected override string LikeOperator()
        {
            //for support case insensitive in PostgreSQL
            return "ILIKE";
        }
    }
}
