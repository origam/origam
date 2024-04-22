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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Origam.Extensions;

public static class ControlExtensions
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

    public static void RunWithInvoke(this Control control, Action action)
    {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
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