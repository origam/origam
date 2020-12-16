using System;
using System.Collections.Generic;
using System.Text;

namespace Origam.DA.Service.Generators
{
    public class MsSqlFilterRenderer : AbstractFilterRenderer
    {
        public override string GetSqlInCaseSensitiveLike()
        {
            return "LIKE";
        }
    }
}
