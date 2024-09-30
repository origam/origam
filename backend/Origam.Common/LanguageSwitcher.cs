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
using System.Globalization;
using System.Threading;

namespace Origam;
public class LanguageSwitcher : IDisposable
{
    private readonly CultureInfo? originalUICulture;
    private readonly CultureInfo? originalCulture;
    public LanguageSwitcher(string langIetf = "")
    {
        originalCulture = null;
        originalUICulture = null;
        if (!string.IsNullOrEmpty(langIetf))
        {
            originalUICulture = Thread.CurrentThread.CurrentUICulture;
            originalCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(langIetf);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(langIetf);
        }
    }
    
    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls
    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue)
        {
            return;
        }
        if (disposing)
        {
            if (originalUICulture != null)
            {
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
                Thread.CurrentThread.CurrentCulture = originalCulture!;
            }
        }
        disposedValue = true;
    }
    public void Dispose()
    {
        Dispose(true);
    }
    #endregion
}
