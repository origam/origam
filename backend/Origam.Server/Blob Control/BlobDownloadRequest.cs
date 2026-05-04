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

#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections;
using System.Data;

namespace Origam.Server;

public class BlobDownloadRequest
{
    private DataRow _row;
    private IDictionary _parameters;
    private string _property;
    private bool _isPreview;

    public BlobDownloadRequest(DataRow row, IDictionary parameters, string property, bool isPreview)
    {
        _row = row;
        _parameters = parameters;
        _property = property;
        _isPreview = isPreview;
    }

    public DataRow Row
    {
        get { return _row; }
        set { _row = value; }
    }
    public IDictionary Parameters
    {
        get { return _parameters; }
        set { _parameters = value; }
    }
    public string Property
    {
        get { return _property; }
        set { _property = value; }
    }
    public bool IsPreview
    {
        get { return _isPreview; }
        set { _isPreview = value; }
    }

    public string BlobMember
    {
        get { return (string)this.Parameters[key: "BlobMember"]; }
    }
    public string FileSizeMember
    {
        get { return (string)this.Parameters[key: "FileSizeMember"]; }
    }
    public Guid ThumbnailHeightConstantId
    {
        get { return new Guid(g: (string)this.Parameters[key: "ThumbnailHeightConstantId"]); }
    }
    public Guid DefaultCompressionConstantId
    {
        get { return new Guid(g: (string)this.Parameters[key: "DefaultCompressionConstantId"]); }
    }
    public Guid BlobLookupId
    {
        get { return new Guid(g: (string)this.Parameters[key: "BlobLookupId"]); }
    }
    public string GridColumnCaption
    {
        get { return (string)this.Parameters[key: "GridColumnCaption"]; }
    }
    public string AuthorMember
    {
        get { return (string)this.Parameters[key: "AuthorMember"]; }
    }
    public Guid StorageTypeDefaultConstantId
    {
        get { return new Guid(g: (string)this.Parameters[key: "StorageTypeDefaultConstantId"]); }
    }
    public string ThumbnailMember
    {
        get { return (string)this.Parameters[key: "ThumbnailMember"]; }
    }
    public string DateCreatedMember
    {
        get { return (string)this.Parameters[key: "DateCreatedMember"]; }
    }
    public string RemarkMember
    {
        get { return (string)this.Parameters[key: "RemarkMember"]; }
    }
    public string DisplayStorageTypeSelection
    {
        get { return (string)this.Parameters[key: "DisplayStorageTypeSelection"]; }
    }
    public Guid ThumbnailWidthConstantId
    {
        get { return new Guid(g: (string)this.Parameters[key: "ThumbnailWidthConstantId"]); }
    }
    public string DateLastModifiedMember
    {
        get { return (string)this.Parameters[key: "DateLastModifiedMember"]; }
    }
    public string OriginalPathMember
    {
        get { return (string)this.Parameters[key: "OriginalPathMember"]; }
    }
    public string FileName
    {
        get { return (string)this.Parameters[key: "FileName"]; }
    }
    public string CompressionStateMember
    {
        get { return (string)this.Parameters[key: "CompressionStateMember"]; }
    }
    public bool IsCompressed
    {
        get
        {
            bool compressed = false;
            if (this.CompressionStateMember != "" && this.CompressionStateMember != null)
            {
                compressed = (bool)Row[columnName: this.CompressionStateMember];
            }
            return compressed;
        }
    }
}
