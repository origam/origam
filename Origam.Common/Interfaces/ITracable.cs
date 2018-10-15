using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origam
{
    /// <summary>
    /// Services e.g. IReportService can use it in order to pass trace info into them.
    /// </summary>
    public interface ITracable
    {
        void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo);
    }
}
