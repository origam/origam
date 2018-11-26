using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Origam.Extensions
{
    public static class ControlExtrensions
    {
        public static T RunWithInvoke<T>(this Control control, Func<T> func)
        {
            T result = default(T);
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => result = func()));
            }
            else
            {
                result = func();
            }
            return result;
        }

        public static void RunWithInvokeAsync(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                Task.Run(() => {
                    control.Invoke(action);
                });
            }
            else
            {
                action();
            }
        }
    }
}
