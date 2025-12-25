#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using System.ComponentModel;
using Origam.Gui;

namespace Origam.Architect.Server.Controls;

public class BlobControl : ControlBase
{
    [Category("Data Members")]
    public string DateLastModifiedMember { get; set; }

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    [Category("Data Members")]
    public string CompressionStateMember { get; set; }

    [Category("Blob Settings")]
    public bool DisplayStorageTypeSelection { get; set; }

    [Browsable(false)]
    public Guid BlobLookupId { get; set; }

    [Category("(ORIGAM)")]
    public int CaptionLength { get; set; }

    [Category("Data Members")]
    public string OriginalPathMember { get; set; }

    [Category("Data Members")]
    public string BlobMember { get; set; }

    [Category("Data Members")]
    public string AuthorMember { get; set; }

    [Browsable(false)]
    public Guid StorageTypeDefaultConstantId { get; set; }

    public bool HideOnForm { get; set; }

    [Category("Data Members")]
    public string RemarkMember { get; set; }

    [Browsable(false)]
    public Guid ThumbnailWidthConstantId { get; set; }

    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Category("Data Members")]
    public string DateCreatedMember { get; set; }

    [Category("Data Members")]
    public string ThumbnailMember { get; set; }

    public string FileName { get; set; }

    [Browsable(false)]
    public Guid ThumbnailHeightConstantId { get; set; }

    [Category("(ORIGAM)")]
    public CaptionPosition CaptionPosition { get; set; }

    public bool ReadOnly { get; set; }

    [Browsable(false)]
    public Guid DefaultCompressionConstantId { get; set; }

    [Category("Data Members")]
    public string FileSizeMember { get; set; }
}
