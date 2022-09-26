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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Origam;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Providers;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;

namespace TestSqlCommandGenerator
{
    /// <summary>
    ///This is a test class for AbstractSqlCommandGeneratorTest and is intended
    ///to contain all AbstractSqlCommandGeneratorTest Unit Tests
    ///</summary>
	[TestClass()]
	public class AbstractSqlCommandGeneratorTest
	{
		private TestContext testContextInstance;
        private static Guid _testPackageId = new Guid("cd63a482-a300-4aa0-ac29-9659be191647");

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
            Reflector.ClassCache = new MockReflectorCache();
            DatabasePersistenceProvider schemaProvider = new DatabasePersistenceProvider(null as LocalizationCache);
            PersistenceService persistenceService = new PersistenceService(
                new DatabasePersistenceProvider(null as LocalizationCache),
                schemaProvider,
                new DatabasePersistenceProvider(null as LocalizationCache),
                false);

            ServiceManager.Services.AddService(persistenceService);
            SchemaService schemaService = new SchemaService(_testPackageId);
            ServiceManager.Services.AddService(schemaService);
            OrigamMetadataDataService metadataService = new OrigamMetadataDataService();
            IDataService architectService = new MsSqlDataService("", 0, 0);
            // AbstractDataService needs its own persistence provider, we use OrigamPersistenceProvider
            DatabasePersistenceProvider metadataPersistence = new DatabasePersistenceProvider(null as LocalizationCache);
            // The persistence provider needs its own data service - we use the simplest possible one
            metadataPersistence.DataService = new OrigamMetadataDataService();
            // There is no really query, we read the whole xml file as it is
            metadataPersistence.DataStructureQuery = new DataStructureQuery();
            // We load up our metadata persistence provider
            metadataPersistence.Refresh(false, null);
            (architectService as AbstractDataService).PersistenceProvider = metadataPersistence;
            schemaProvider.DataService = architectService;
            schemaProvider.DataStructureId = new Guid(PersistenceService.SchemaDataStructureId);
            schemaProvider.Init();

            // add basic schema item providers
            EntityModelSchemaItemProvider entityProvider = new EntityModelSchemaItemProvider();
            schemaService.AddProvider(entityProvider);
            schemaService.AddProvider(new StateMachineSchemaItemProvider());
            schemaService.AddProvider(new DataStructureSchemaItemProvider());

            // create a test package
            Package package = new Package(new ModelElementKey(_testPackageId));
            package.Name = "TestPackage";
            package.PersistenceProvider = persistenceService.SchemaProvider;
            package.Persist();

            // add IOrigamEntity2 because creating new table entities adds it as an ancestor by default
            DetachedEntity entity = entityProvider.NewItem<DetachedEntity>(
	            _testPackageId, null);
            entity.Name = EntityHelper.DefaultAncestorName;
            entity.Persist();
            // add primary key to IOrigamEntity2
            FieldMappingItem field = EntityHelper.CreateColumn(entity, "Id", false, OrigamDataType.UniqueIdentifier, 0, "Id", null, null, false);
            field.IsPrimaryKey = true;
            field.Persist();

			// create Language entity, becouse it is is necessary for creating language translation entities
			TableMappingItem langEntity = EntityHelper.CreateTable("Language", null, false);
			langEntity.PrimaryKey = new ModelElementKey(EntityHelper.LanguageEntityId);
			langEntity.Persist();
			EntityHelper.CreateColumn(langEntity, "Name", true, OrigamDataType.String, 200, "Language", null, null, true);
			EntityHelper.CreateColumn(langEntity, "TagIETF", false, OrigamDataType.String, 20, "IETF Tag", null, null, true);
        }
		
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		internal virtual AbstractSqlCommandGenerator CreateAbstractSqlCommandGenerator()
		{
			return new MsSqlCommandGenerator(new DetachedFieldPackerMs());
		}

		/// <summary>
		///A test for SelectSql
		///</summary>
        //[TestMethod()]
        public void SelectSqlTest()
        {
            // create an entity
            TableMappingItem table1 = EntityHelper.CreateTable("table1", null, true);
            // with one field
            IDataEntityColumn col1 = EntityHelper.CreateColumn(table1, "col1", true, OrigamDataType.String, 100, "col1 caption", null, null, true);

            ArrayList cols = new ArrayList
            {
                col1
            };
            // create child entity with language translations
            TableMappingItem languageEntity = EntityHelper.CreateLanguageTranslationChildEntity(table1, cols);

            // create a default data structure for the entity
            DataStructure ds = EntityHelper.CreateDataStructure(table1, "table1Ds", false);
			ds.IsLocalized = true;
			ds.Persist();
            // input parameters
            AbstractSqlCommandGenerator target = CreateAbstractSqlCommandGenerator();
            DataStructureEntity entity = ds.Entities[0] as DataStructureEntity;

			DataStructureEntity languageDSEntity = entity
				.NewItem<DataStructureEntity>(_testPackageId, null);
			languageDSEntity.RelationType = RelationType.LeftJoin;
			languageDSEntity.Entity = table1.LocalizationRelation;
			languageDSEntity.Persist();
 
            // test
            //string expected = "SELECT [table1].[Id] AS [Id],  [table1].[col1] AS [col1] FROM [table1] AS [table1]";
			string expected = "SELECT [table1].[Id] AS [Id], ISNULL([table1_l10n].[col1],[table1].[col1]) FROM [table1] AS [table1] LEFT JOIN [table1_l10n] AS [table1_l10n] ON [table1_l10n].[refTable1Id] = [table1].[Id]";
	

            string actual = target.SelectSql(ds, entity, null, null, ColumnsInfo.Empty, null, null, false);
            Assert.AreEqual(expected, actual);
        }
	}
}
