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
﻿using System;
using System.IO;
using NUnit.Framework;
using Origam;
using Origam.OrigamEngine;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace OrigamArchitectTests
{
    [TestFixture]
    public class ArchitectTests
    {
        [Test,  Order(1)]
        public void ShouldCreateNewEntity()
        {
            OrigamEngine.ConnectRuntime(
                customServiceFactory: new TestRuntimeServiceFactory(),
                runRestartTimer: false);

            SchemaItemGroup parentGroup = 
                GetItemById<SchemaItemGroup>(new Guid("d86679c6-3cee-419e-afc0-98011fad460e"));

            TableMappingItem newItem = 
                EntityHelper.CreateTable("Test1", parentGroup, true);

            TableMappingItem persistedItem = GetItemById<TableMappingItem>(newItem.Id);
            Assert.That(persistedItem, Is.Not.Null);
            Assert.That(persistedItem.Id, Is.EqualTo(newItem.Id));

            string xmlFilePath = Path.Combine(ProjectTopDirectory, newItem.RelativeFilePath);
            FileAssert.Exists(xmlFilePath);
        }

        [Test]
        public void Test()
        {
            Console.WriteLine(ProjectDir.FullName);
        }

        private DirectoryInfo ProjectDir =>
            new DirectoryInfo(TestContext.CurrentContext.TestDirectory)
                .Parent
                .Parent;

        private string ProjectTopDirectory
        {
            get
            {
                OrigamSettings settings =
                    ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
                return settings.ModelSourceControlLocation;
            }
        }


        private static T GetItemById<T>(Guid id) 
        {
            return (T)ServiceManager
                .Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(T),  new Key {{"Id" , id}});
        }
    }
}