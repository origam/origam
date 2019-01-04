#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;
using Origam.Extensions;
using static Origam.DA.ObjectPersistence.ExternalFileExtension;

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for Graphics.
	/// </summary>
	[SchemaItemDescription("Image", 58)]
    [HelpTopic("Images")]
	[XmlModelRoot(ItemTypeConst)]
    public class Graphics : AbstractSchemaItem
	{
		public const string ItemTypeConst = "Graphics";

		public Graphics() : base()
		{
			InitializeProperyContainers();
		}

		public Graphics(Guid schemaExtensionId) : base(schemaExtensionId)
		{
			InitializeProperyContainers();
		}

		public Graphics(Key primaryKey) : base(primaryKey)
		{
			InitializeProperyContainers();
		}
		
		private void InitializeProperyContainers()
		{
			graphicsDataByte = new PropertyContainer<byte[]>(
				containerName: nameof(graphicsDataByte),
				containingObject: this);
		}
		

		#region Overriden AbstractSchemaItem Members
		
		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ItemTypeConst;
			}
		}

		public override string Icon
		{
			get
			{
				return "58";
			}
		}
		#endregion

		#region Properties

		private PropertyContainer<byte[]> graphicsDataByte;
		
		[Browsable(false)]
		[EntityColumn("BLB01")]
		[XmlExternalFileReference(containerName: nameof(graphicsDataByte), extension: Png)]
		public byte[] GraphicsDataByte
		{
			get => graphicsDataByte.Get();
			set => graphicsDataByte.Set(value);
		}

		[Category("Graphics")]
		[Editor(typeof(System.Drawing.Design.BitmapEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public byte[] GraphicsData
		{
			get
			{
				if(GraphicsDataByte == null) return null;

				Bitmap b = new System.Drawing.Bitmap(new System.IO.MemoryStream(GraphicsDataByte));
				if (b.RawFormat == ImageFormat.Bmp)
				{
					b.MakeTransparent(Color.Magenta);
				}
				return b.ToByteArray();
			}
			set
			{
				if (value == null)
				{
					GraphicsDataByte = null;
					return;
				}

			    Bitmap bitmap = value.ToBitmap();
			    System.IO.MemoryStream stream = new System.IO.MemoryStream();

				if (bitmap.RawFormat == ImageFormat.Bmp)
				{
					SetTransparentColor(bitmap);
				    bitmap.Save(stream, ImageFormat.Bmp);
				} 
				else
				{
				    bitmap.Save(stream, ImageFormat.Png);
				}
				GraphicsDataByte = stream.ToArray();
			}
		}

		private static void SetTransparentColor(Bitmap value)
		{
			for (int i = 0; i < value.Height; i++)
			{
				for (int j = 0; j < value.Width; j++)
				{
					if (value.GetPixel(j, i) == Color.Transparent)
					{
						value.SetPixel(j, i, Color.Magenta);
					}
				}
			}
		}

		public override byte[] NodeImage => GraphicsData;

		#endregion
	}
}
