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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Origam.Schema;
using Origam.Schema.EntityModel;

namespace Origam.DA.Service_net2Tests;

internal static class EqualityExtensions
{
    public static Dictionary<string, object> GetAllProperies(this object atype)
    {
        if (atype == null)
        {
            return new Dictionary<string, object>();
        }

        Type t = atype.GetType();
        PropertyInfo[] props = t.GetProperties();
        Dictionary<string, object> dict = new Dictionary<string, object>();
        foreach (PropertyInfo prp in props)
        {
            object value = prp.GetValue(atype, new object[] { });
            dict.Add(prp.Name, value);
        }
        return dict;
    }

    private static bool ContainsEqualObject(this ICollection thisCollection, object testObject)
    {
        return thisCollection.Cast<object>().Any(item => IsEqualTo(item, testObject));
    }

    private static bool IsEqualTo(this ICollection collection, object testObject)
    {
        if (!(testObject is ICollection testCollection))
        {
            return false;
        }

        return collection
            .Cast<object>()
            .All(itemFromDb => testCollection.ContainsEqualObject(itemFromDb));
    }

    private static bool IsEqualTo(this ISchemaItem item, object testObject)
    {
        if (!(testObject is ISchemaItem testItem))
        {
            return false;
        }

        return item.Id == testItem.Id;
    }

    private static bool IsEqualTo(this DataEntityConstraint dataConstraint, object testObject)
    {
        if (!(testObject is DataEntityConstraint))
        {
            return false;
        }

        var testEntityConstraint = (DataEntityConstraint)testObject;
        return testEntityConstraint.Fields.Any(field =>
            dataConstraint.Fields.ContainsEqualObject(field)
        );
    }

    [DllImport("msvcrt.dll")]
    private static extern int memcmp(IntPtr b1, IntPtr b2, long count);

    private static bool IsEqualTo(this Bitmap b1, object obj)
    {
        Bitmap b2 = obj as Bitmap;
        if ((b1 == null) != (b2 == null))
        {
            return false;
        }

        if (b1.Size != b2.Size)
        {
            return false;
        }

        BitmapData bd1 = b1.LockBits(
            new Rectangle(new Point(0, 0), b1.Size),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb
        );
        BitmapData bd2 = b2.LockBits(
            new Rectangle(new Point(0, 0), b2.Size),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb
        );
        try
        {
            IntPtr bd1scan0 = bd1.Scan0;
            IntPtr bd2scan0 = bd2.Scan0;
            int stride = bd1.Stride;
            int len = stride * b1.Height;
            return memcmp(bd1scan0, bd2scan0, len) == 0;
        }
        finally
        {
            b1.UnlockBits(bd1);
            b2.UnlockBits(bd2);
        }
    }

    private static bool IsEqualTo(this SchemaItemAncestor ancestor, object testObject)
    {
        if (!(testObject is SchemaItemAncestor testAncestor))
        {
            return false;
        }

        return ancestor.SchemaItem.Id == testAncestor.SchemaItem.Id;
    }

    private static bool IsEqualTo(this DictionaryEntry entry, object testObject)
    {
        if (!(testObject is DictionaryEntry testEntry))
        {
            return false;
        }

        return IsEqualTo(entry.Key, testEntry.Key) && IsEqualTo(entry.Value, testEntry.Value);
    }

    public static bool IsEqualTo(this object item, object testObject)
    {
        switch (item)
        {
            case ISchemaItem schemaItem:
            {
                return schemaItem.IsEqualTo(testObject);
            }
            case DataEntityConstraint dataConstraint:
            {
                return dataConstraint.IsEqualTo(testObject);
            }
            case SchemaItemAncestor dbSchemaItemAncestor:
            {
                return dbSchemaItemAncestor.IsEqualTo(testObject);
            }
            case DictionaryEntry dictEntry:
            {
                return dictEntry.IsEqualTo(testObject);
            }
            case ICollection collectionFromDb:
            {
                return collectionFromDb.IsEqualTo(testObject);
            }
            case Bitmap dbBitmap:
            {
                return dbBitmap.IsEqualTo(testObject);
            }
            case ISchemaItemFactory _:
            {
                return true;
            }
        }
        return Equals(testObject, item);
    }
}
