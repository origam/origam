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

namespace Origam.Gui.Win;
public class DataGridColumnStylePadding 
{
	
	int m_left;
	int m_right;
	int m_top;
	int m_bottom;
	public int Left 
	{
		get { return m_left; }
		set { m_left = value; }
	}
	public int Right 
	{
		get { return m_right; }
		set { m_right = value; }
	}
	public int Top 
	{
		get { return m_top; }
		set { m_top = value; }
	}
	public int Bottom 
	{
		get { return m_bottom; }
		set { m_bottom = value; }
	}
	public void SetPadding( int padValue ) 
	{
		
		m_left = padValue;
		m_right = padValue;
		m_top = padValue;
		m_bottom = padValue;
	}
	public void SetPadding( int top, int right, int bottom, int left ) 
	{
		UpdatePaddingValues( top, right, bottom, left );
	}
	public DataGridColumnStylePadding( int padValue ) 
	{
		this.SetPadding( padValue );
	}
	public DataGridColumnStylePadding( int top, int right, int bottom, int left ) 
	{
		UpdatePaddingValues( top, right, bottom, left );
	}
	private void UpdatePaddingValues( int top, int right, int bottom, int left ) 
	{
		m_top = top;
		m_right = right;
		m_bottom = bottom;
		m_left = left;
	
	}
}
