#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for XsltDependencyHelper.
	/// </summary>
	public class XsltDependencyHelper
	{
		public static void GetDependencies(AbstractSchemaItem item, ArrayList dependencies, string text)
		{
			if(text == null) return;

			ISchemaService sch = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
			IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;

			// references
			int found = 0;

			ArrayList references = new ArrayList();

			for (int i = 0; i < text.Length; i++) 
			{
				found = text.IndexOf("model://", i);

				if (found > 0) 
				{
					string id = text.Substring(found + 8, 36);
                    try
                    {
                        AbstractSchemaItem reference = persistence.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(new Guid(id))) as AbstractSchemaItem;
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
			ArrayList constants = new ArrayList();

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

			DataConstantSchemaItemProvider constantProvider = sch.GetProvider(typeof(DataConstantSchemaItemProvider)) as DataConstantSchemaItemProvider;

			foreach(string c in constants)
			{
				foreach(DataConstant child in constantProvider.LoadChildItems())
				{
					if(child.Name == c)
					{
						dependencies.Add(child);
						break;
					}
				}

				if(dependencies.Count == 0) throw new ArgumentOutOfRangeException(ResourceUtils.GetString("ErrorConstantNotFound", c, item.ItemType, item.Name));
			}

			// strings
			found = 0;
			ArrayList strings = new ArrayList();

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

			StringSchemaItemProvider stringProvider = sch.GetProvider(typeof(StringSchemaItemProvider)) as StringSchemaItemProvider;

			foreach(string s in strings)
			{
				foreach(StringItem child in stringProvider.LoadChildItems())
				{
					if(child.Name == s)
					{
						dependencies.Add(child);
						break;
					}
				}

				if(dependencies.Count == 0) throw new ArgumentOutOfRangeException(ResourceUtils.GetString("ErrorStringNotFound", s, item.ItemType, item.Name));
			}

			// lookups
			found = 0;
			ArrayList lookups = new ArrayList();

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

			IDataLookupSchemaItemProvider lookupProvider = sch.GetProvider(typeof(IDataLookupSchemaItemProvider)) as IDataLookupSchemaItemProvider;

			foreach(Guid l in lookups)
			{
				foreach(AbstractSchemaItem child in lookupProvider.LoadChildItems())
				{
					if(child.Id == l)
					{
						dependencies.Add(child);
						break;
					}
				}

				if(dependencies.Count == 0) throw new ArgumentOutOfRangeException(ResourceUtils.GetString("ErrorLookupNotFound", l, item.ItemType, item.Name));
			}
		}
	}
}
