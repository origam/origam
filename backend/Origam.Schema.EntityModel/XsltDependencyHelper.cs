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
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;
/// <summary>
/// Summary description for XsltDependencyHelper.
/// </summary>
public class XsltDependencyHelper
{
	public static void GetDependencies(ISchemaItem item, List<ISchemaItem> dependencies, string text)
	{
		if(text == null) return;
        IPersistenceProvider persistenceprovider = item.PersistenceProvider;
		// references
		int found = 0;
		for (int i = 0; i < text.Length; i++) 
		{
			found = text.IndexOf("model://", i);
			if (found > 0) 
			{
				string id = text.Substring(found + 8, 36);
                try
                {
                    ISchemaItem reference = persistenceprovider.RetrieveInstance(typeof(ISchemaItem), new ModelElementKey(new Guid(id))) as ISchemaItem;
                    dependencies.Add(reference);
                }
                catch (System.FormatException)
                {
                    // don't follow invalid (non-guid) references (e.g. in comment)
                }
				i = found;
			}
			else
				break;
		}
		// constants
		found = 0;
		var constants = new List<string>();
		for (int i = 0; i < text.Length; i++) 
		{
			found = text.IndexOf(":GetConstant('", i);
			if (found > 0) 
			{
				constants.Add(text.Substring(found + 14, text.IndexOf("'", found + 14) - found - 14));
				i = found;
			}
			else
				break;
		}
        List<DataConstant> listDataconstant = persistenceprovider.RetrieveListByCategory<DataConstant>(DataConstant.CategoryConst);
        foreach (string c in constants)
		{
            foreach (DataConstant child in listDataconstant)
            {
                if (child.Name == c)
                {
                    dependencies.Add(child);
                    break;
                }
            }
            if (dependencies.Count == 0) throw new ArgumentOutOfRangeException(ResourceUtils.GetString("ErrorConstantNotFound", c, item.ItemType, item.Name));
        }
        
        // strings
        found = 0;
		var strings = new List<string>();
		for (int i = 0; i < text.Length; i++) 
		{
			found = text.IndexOf(":GetString('", i);
			if (found > 0) 
			{
				strings.Add(text.Substring(found + 12, text.IndexOf("'", found + 12) - found - 12));
				i = found;
			}
			else
				break;
		}
        List<StringItem> listStringItem = persistenceprovider.RetrieveListByCategory<StringItem>(StringItem.CategoryConst);
        foreach (string s in strings)
		{
            foreach (StringItem child in listStringItem)
            {
                if (child.Name == s)
                {
                    dependencies.Add(child);
                    break;
                }
			}
			if(dependencies.Count == 0) throw new ArgumentOutOfRangeException(ResourceUtils.GetString("ErrorStringNotFound", s, item.ItemType, item.Name));
		}
		// lookups
		found = 0;
		var lookups = new List<Guid>();
		for (int i = 0; i < text.Length; i++) 
		{
			found = text.IndexOf(":LookupValue('", i);
			if (found > 0) 
			{
                try {
                    lookups.Add(new Guid(text.Substring(found + 14, text.IndexOf("'", found + 14) - found - 14)));
                } catch (System.FormatException)
                {
                    // we shouldn't bother when someone adds something
                    // invalid after LookupValue() - e.g. could be in a comment 
                }
				i = found;
			}
			else
				break;
		}
		for (int i = 0; i < text.Length; i++) 
		{
			found = text.IndexOf(":LookupValueEx('", i);
			if (found > 0) 
			{
                try
                {
                    lookups.Add(new Guid(text.Substring(found + 16, text.IndexOf("'", found + 16) - found - 16)));
                } catch (System.FormatException)
                { }
				i = found;
			}
			else
				break;
		}
        foreach (Guid l in lookups)
		{
            if (persistenceprovider.RetrieveInstance(typeof(ISchemaItem), new ModelElementKey(l)) is ISchemaItem lookup)
            {
                dependencies.Add(lookup);
            }
            if (dependencies.Count == 0) throw new ArgumentOutOfRangeException(ResourceUtils.GetString("ErrorLookupNotFound", l, item.ItemType, item.Name));
		}
	}
}
