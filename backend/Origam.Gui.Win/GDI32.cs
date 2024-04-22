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
using System.Runtime.InteropServices;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for GDI32.
/// </summary>
/* Author: Perry Lee
 * Submission: Capture Screen (Add Screenshot Capability to Programs)
 * Date of Submission: 12/29/03
 */

// If you have any questions regarding functions (methods) imported from 
// GDI32.dll and User32.dll refer to ‘msdn.microsoft.com’ 
internal class GDI32
{
	[DllImport("GDI32.dll")]
	public static extern bool BitBlt(
		IntPtr hdcDest, // handle to destination DC
		int nXDest, // x-coord of destination upper-left corner
		int nYDest, // y-coord of destination upper-left corner
		int nWidth, // width of destination rectangle
		int nHeight, // height of destination rectangle
		IntPtr hdcSrc, // handle to source DC
		int nXSrc, // x-coordinate of source upper-left corner
		int nYSrc, // y-coordinate of source upper-left corner
		System.Int32 dwRop // raster operation code
	);
	[DllImport("GDI32.dll")]
	public static extern int CreateCompatibleBitmap(int hdc,int nWidth, 
		int nHeight);
	[DllImport("GDI32.dll")]
	public static extern int CreateCompatibleDC(int hdc);
	[DllImport("GDI32.dll")]
	public static extern bool DeleteDC(int hdc);
	[DllImport("GDI32.dll")]
	public static extern bool DeleteObject(int hObject);
	[DllImport("GDI32.dll")]
	public static extern int GetDeviceCaps(int hdc,int nIndex);
	[DllImport("GDI32.dll")]
	public static extern int SelectObject(int hdc,int hgdiobj);
 

	internal class User32
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct RECT 
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		[DllImport("user32.dll")]
		public static extern int GetWindowRect( IntPtr hwnd, ref RECT rc );

		[DllImport("User32.dll")]
		public static extern int GetDesktopWindow();
		[DllImport("User32.dll")]
		public static extern IntPtr GetWindowDC( IntPtr hwnd );
		[DllImport("User32.dll")]
		public static extern int ReleaseDC(IntPtr hWnd,int hDC);
	}
}