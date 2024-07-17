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
using System.Collections.Generic;
using Origam.Schema;
using NUnit.Framework;

namespace Origam.DA.Service_net2Tests;
internal class SchemaItemComparator
{
    public Dictionary<Type, List<ISchemaItem>> ItemsFromDataBaseDict
    {
        private get;
        set;
    }
    public Dictionary<Type, List<ISchemaItem>> ItemsFromXmlDict
    {
        private get;
        set;
    }
    private SchemaItemsToCompare nowComparing;
    public void CompareSchemaItems()
    {
        foreach (var itemsToCompare in FindMatchingIdItems())
        {
            nowComparing = itemsToCompare;
            bool areItemsIdentical = AreItemsIdentical(itemsToCompare);
        }
    }
    private IEnumerable<SchemaItemsToCompare> FindMatchingIdItems()
    {
        foreach (var typeItemsPair in ItemsFromDataBaseDict)
        {
            Type type = typeItemsPair.Key;
            var itemList = typeItemsPair.Value;
            foreach (ISchemaItem itemFromDb in itemList)
            {
                var itemFromXml = ItemsFromXmlDict[type].Find(x =>
                    x.Id == itemFromDb.Id);
                yield return
                    new SchemaItemsToCompare(fromDb: itemFromDb,
                        fromXml: itemFromXml);
            }
        }
    }
    private bool AreItemsIdentical(SchemaItemsToCompare items)
    {
        Dictionary<string, object> dbItemProperties
            = items.FromDb.GetAllProperies();
        Dictionary<string, object> xmlItemProperties 
            = items.FromXml.GetAllProperies();
        
        foreach (var dbItemPropery in dbItemProperties)
        {
            string propertyName = dbItemPropery.Key;
            var dbValue = dbItemPropery.Value;
            var xmlValue = xmlItemProperties[propertyName];
            
            if (!dbValue.IsEqualTo(xmlValue))
            {
                FailTest(propertyName,dbValue.GetType());
                return false;
            } 
        }
        return true;
    }
    private void FailTest(string propertyName, Type propertryType)
    {
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine($"Comparing objects of type: \"{nowComparing.Type}\"");
        Console.WriteLine($"propertyName \"{propertyName}\" of type {propertryType} is not equal in objects retrieved form db and xml.");
        Console.WriteLine($"Before you draw any conclusions make sure that method IsEqualTo for {propertryType} is implemented in EqualityExtensions");
        Console.WriteLine(nowComparing);
        Assert.Fail();
    }
}
