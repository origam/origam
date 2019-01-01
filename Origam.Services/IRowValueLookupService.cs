using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origam.Services
{
    public interface IRowValueLookupService
    {
        string ValueFromRow(DataRow row, string[] columns);
        DataView GetList(Guid lookupId, string transactionId);
    }
}
