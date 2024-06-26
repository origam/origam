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

namespace Origam;
public class LanguageSwitcher : IDisposable
{
    private System.Globalization.CultureInfo originalUICulture;
    private System.Globalization.CultureInfo originalCulture;
    public LanguageSwitcher(string langIETF = "")
    {
        originalCulture = null;
        originalUICulture = null;
        if (!string.IsNullOrEmpty(langIETF))
        {
            originalUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            originalCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(langIETF);
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(langIETF);
        }
    }
    /*
    ~LanguageSwitcher()
    {
        if (originalUICulture != null)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = originalUICulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
    */
    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (originalUICulture != null)
                {
                    System.Threading.Thread.CurrentThread.CurrentUICulture = originalUICulture;
                    System.Threading.Thread.CurrentThread.CurrentCulture = originalCulture;
                }
                // TODO: dispose managed state (managed objects).
            }
            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            disposedValue = true;
        }
    }
    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~LanguageSwitcher() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }
    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }
    #endregion
}
