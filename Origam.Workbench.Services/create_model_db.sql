--Copyright 2005 - 2017 Advantage Solutions, s. r. o.
--
--This file is part of ORIGAM (http://www.origam.org).
--
--ORIGAM is free software: you can redistribute it and/or modify
--it under the terms of the GNU General Public License as published by
--the Free Software Foundation, either version 3 of the License, or
--(at your option) any later version.

--ORIGAM is distributed in the hope that it will be useful,
--but WITHOUT ANY WARRANTY; without even the implied warranty of
--MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
--GNU General Public License for more details.

--You should have received a copy of the GNU General Public License
--along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchemaItemGroup](
	[Id] [uniqueidentifier] NOT NULL,
	[refParentGroupId] [uniqueidentifier] NULL,
	[refSchemaExtensionId] [uniqueidentifier] NOT NULL,
	[refParentItemId] [uniqueidentifier] NULL,
	[RootItemType] [nvarchar](255) NULL,
	[Name] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_SchemaItemGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Extension] ON [dbo].[SchemaItemGroup] 
(
	[refSchemaExtensionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE TABLE [dbo].[SchemaItemAncestor](
	[Id] [uniqueidentifier] NOT NULL,
	[refSchemaItemId] [uniqueidentifier] NOT NULL,
	[refAncestorId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_SchemaItemAncestor] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[SchemaItem](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[refParentItemId] [uniqueidentifier] NULL,
	[refSchemaItemGroupId] [uniqueidentifier] NULL,
	[TargetType] [nvarchar](255) NOT NULL,
	[refSchemaExtensionId] [uniqueidentifier] NOT NULL,
	[ItemType] [nvarchar](255) NOT NULL,
	[IsAbstract] [bit] NOT NULL,
	[L01] [bigint] NULL,
	[L02] [bigint] NULL,
	[BLB01] [image] NULL,
	[LS01] [nvarchar](2000) NULL,
	[SS01] [nvarchar](300) NULL,
	[SS02] [nvarchar](300) NULL,
	[SS03] [nvarchar](300) NULL,
	[SS04] [nvarchar](300) NULL,
	[SS05] [nvarchar](300) NULL,
	[M01] [nvarchar](max) NULL,
	[M02] [nvarchar](max) NULL,
	[M03] [nvarchar](max) NULL,
	[M04] [nvarchar](max) NULL,
	[M05] [nvarchar](max) NULL,
	[I01] [int] NULL,
	[I02] [int] NULL,
	[I03] [int] NULL,
	[I04] [int] NULL,
	[I05] [int] NULL,
	[I06] [int] NULL,
	[I07] [int] NULL,
	[I08] [int] NULL,
	[I09] [int] NULL,
	[G01] [uniqueidentifier] NULL,
	[G02] [uniqueidentifier] NULL,
	[G03] [uniqueidentifier] NULL,
	[G04] [uniqueidentifier] NULL,
	[G05] [uniqueidentifier] NULL,
	[G06] [uniqueidentifier] NULL,
	[G07] [uniqueidentifier] NULL,
	[G08] [uniqueidentifier] NULL,
	[G09] [uniqueidentifier] NULL,
	[G10] [uniqueidentifier] NULL,
	[G11] [uniqueidentifier] NULL,
	[G12] [uniqueidentifier] NULL,
	[G13] [uniqueidentifier] NULL,
	[G14] [uniqueidentifier] NULL,
	[G15] [uniqueidentifier] NULL,
	[C01] [money] NULL,
	[C02] [money] NULL,
	[F01] [decimal](18, 10) NULL,
	[F02] [decimal](18, 10) NULL,
	[B01] [bit] NULL,
	[B02] [bit] NULL,
	[B03] [bit] NULL,
	[B04] [bit] NULL,
	[B05] [bit] NULL,
	[B06] [bit] NULL,
	[B07] [bit] NULL,
	[D01] [datetime] NULL,
	[D02] [datetime] NULL,
	[D03] [datetime] NULL,
	[D04] [datetime] NULL,
	[G16] [uniqueidentifier] NULL,
	[G17] [uniqueidentifier] NULL,
	[G18] [uniqueidentifier] NULL,
	[G19] [uniqueidentifier] NULL,
	[G20] [uniqueidentifier] NULL,
 CONSTRAINT [PK_SchemaItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Extension] ON [dbo].[SchemaItem] 
(
	[refSchemaExtensionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE TABLE [dbo].[SchemaExtension](
	[SchemaExtensionGuid] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Version] [nvarchar](20) NULL,
	[Copyright] [nvarchar](250) NULL,
	[Description] [ntext] NULL,
 CONSTRAINT [PK_SchemaExtension] PRIMARY KEY CLUSTERED 
(
	[SchemaExtensionGuid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE TABLE [dbo].[PackageReferenceElement](
	[Id] [uniqueidentifier] NOT NULL,
	[refElementId] [uniqueidentifier] NOT NULL,
	[refPackageReferenceId] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [ix_refPackageReferenceId] ON [dbo].[PackageReferenceElement] 
(
	[refPackageReferenceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE TABLE [dbo].[PackageReference](
	[Id] [uniqueidentifier] NOT NULL,
	[refPackageId] [uniqueidentifier] NOT NULL,
	[refReferencedPackageId] [uniqueidentifier] NOT NULL,
	[IncludeAllElements] [bit] NOT NULL,
	[ReferenceType] [int] NOT NULL,
PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [ix_refPackageId] ON [dbo].[PackageReference] 
(
	[refPackageId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE FUNCTION [dbo].[extract_xml] 
	(@xmlpart varchar(30),
	@value varchar(2000))
RETURNS nvarchar(2000) 
AS
BEGIN
	RETURN(
	select
		substring(@value, 
		CHARINDEX('<' + @xmlpart + '>', @value) + len(@xmlpart) + 2, 
		CHARINDEX('</' + @xmlpart + '>', @value) - CHARINDEX('<' + @xmlpart + '>', @value) - len(@xmlpart) - 2) 
)
END
GO
CREATE TABLE [dbo].[Documentation](
	[Id] [uniqueidentifier] NOT NULL,
	[Data] [ntext] NOT NULL,
	[refSchemaItemId] [uniqueidentifier] NOT NULL,
	[Category] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Documentation] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE CLUSTERED INDEX [ix_refSchemaItemId] ON [dbo].[Documentation] 
(
	[refSchemaItemId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE PROCEDURE [dbo].[DeletePackage]
	@packageId uniqueidentifier
AS
	DECLARE @errMessage nvarchar(max)
	SET @errMessage = 'Cannot delete package, it is referenced by other items.'
	
	DECLARE @referingPackage uniqueidentifier
	
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G01 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G02 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G03 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G04 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G05 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G06 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G07 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G08 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G09 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G10 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G11 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G12 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G13 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G14 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G15 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G16 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G17 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G18 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G19 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItem si INNER JOIN SchemaItem dependentSi ON dependentSi.G20 = si.Id WHERE si.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError

	-- groups
	SET @referingPackage = (SELECT TOP 1 dependentGroup.refSchemaExtensionId from SchemaItemGroup gr INNER JOIN SchemaItemGroup dependentGroup ON dependentGroup.refParentGroupId = gr.Id WHERE gr.refSchemaExtensionId = @packageId and dependentGroup.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError
	SET @referingPackage = (SELECT TOP 1 dependentSi.refSchemaExtensionId from SchemaItemGroup gr INNER JOIN SchemaItem dependentSi ON dependentSi.refSchemaItemGroupId = gr.Id WHERE gr.refSchemaExtensionId = @packageId and dependentSi.refSchemaExtensionId != @packageId)
	IF (@referingPackage IS NOT NULL) GOTO ReferringPackageError

	DELETE FROM Documentation WHERE EXISTS (SELECT * FROM SchemaItem si WHERE si.Id = Documentation.refSchemaItemId AND si.refSchemaExtensionId = @packageId)
	DELETE FROM SchemaItemAncestor WHERE EXISTS (SELECT * FROM SchemaItem si WHERE si.Id = SchemaItemAncestor.refSchemaItemId AND si.refSchemaExtensionId = @packageId)
	DELETE FROM SchemaItemGroup WHERE refSchemaExtensionId = @packageId
	DELETE FROM SchemaItem WHERE refSchemaExtensionId = @packageId
	DELETE FROM PackageReference WHERE PackageReference.refPackageId = @packageId
	DELETE FROM SchemaExtension WHERE SchemaExtensionGuid = @packageId
	return

ReferringPackageError:
	DECLARE @packageName nvarchar(200)
	SET @packageName = (SELECT Name FROM SchemaExtension WHERE SchemaExtensionGuid = @referingPackage)
	
	RAISERROR (@packageName, 10, 1)
GO
CREATE FUNCTION [dbo].[getRootItemId] 
	(@id varchar(100),
	@parentId varchar(100))
RETURNS varchar(100)
AS
BEGIN
	declare @foundId varchar(100)

	IF @parentId IS NULL
	BEGIN
		RETURN @id
	END

	SET @foundId = (SELECT refParentItemId FROM SchemaItem WHERE Id = @parentId)
	
	IF @foundId IS NULL
	BEGIN
		RETURN @parentId
	END

	RETURN dbo.getRootItemId(@parentId, @foundId)
END
GO
ALTER TABLE [dbo].[SchemaItem] ADD  CONSTRAINT [DF_SchemaItem_IsAbstract]  DEFAULT ((0)) FOR [IsAbstract]
GO
ALTER TABLE [dbo].[SchemaItemAncestor] ADD  CONSTRAINT [DF_SchemaItemAncestor_Id]  DEFAULT (newid()) FOR [Id]
GO
CREATE  PROCEDURE [dbo].[OrigamDatabaseSchemaVersion] AS SELECT '5.0'
GO