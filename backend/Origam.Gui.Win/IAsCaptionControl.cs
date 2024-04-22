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

public static class CaptionDoc
{
	public const string GridColumnWidthDescription =
		"Column Width (in pixels) to be used in grid-view. "
		+ "If the value is less than then zero, then the column is hidden by"
		+ " default. However, when it's enabled, the abs(configured value) "
		+ "is used.";
}

/// <summary>
/// Summary description for IAsCaptionControl.
/// </summary>
public interface IAsCaptionControl
{
	string Caption{get; set;}
	string GridColumnCaption {get; set;}
	int CaptionLength {get; set;}
	CaptionPosition CaptionPosition{get; set;}
	int GridColumnWidth {get; set;}
	bool HideOnForm {get; set;}
}