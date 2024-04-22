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

using Origam.DA.Common;
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;


namespace Origam.Schema.GuiModel;

[SchemaItemDescription("File Mapping", "Parameter Mappings", 29)]
[HelpTopic("File+Mapping")]
[ClassMetaVersion("6.0.0")]
public class PageParameterFileMapping : PageParameterMapping
{
	public PageParameterFileMapping() : base() {Init();}
	public PageParameterFileMapping(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public PageParameterFileMapping(Key primaryKey) : base(primaryKey) {Init();}

	private void Init()
	{
		}

	#region Properties
	private PageParameterFileInfo _fileInfoType = PageParameterFileInfo.FileContent;
	[Category("File Info")]
	[XmlAttribute("fileInfoType")]
	public PageParameterFileInfo FileInfoType
	{
		get
		{
				return _fileInfoType;
			}
		set
		{
				_fileInfoType = value;
			}
	}

	private int _thumbnailWidth = 0;

	[Category("File Info")]
	[DefaultValue(0)]
	[XmlAttribute("thumbnailWidth")]
	public int ThumbnailWidth
	{
		get
		{
				return _thumbnailWidth;
			}
		set
		{
				_thumbnailWidth = value;
			}
	}

	private int _thumbnailHeight = 0;

	[Category("File Info")]
	[DefaultValue(0)]
	[XmlAttribute("thumbnailHeight")]
	public int ThumbnailHeight
	{
		get
		{
				return _thumbnailHeight;
			}
		set
		{
				_thumbnailHeight = value;
			}
	}
	#endregion			
}