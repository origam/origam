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
using System.IO;
using System.Linq;
using NUnit.Framework;
using Origam.Git;
using Origam.TestCommon;

namespace Origam.GitTests;
[TestFixture]
public class GitFileComparerTests: AbstractFileTestClass
{
    protected override TestContext TestContext =>
        TestContext.CurrentContext;
    [Test]
    public void ShouldFindDiferences()
    {
        string  expectedDiffBody = "     <SchemaItem Id=\"79a66d46-5c51-4502-8fc1-fc65259dd2ff\" ItemType=\"ParameterReference\" Name=\"parUserEmail\" TargetType=\"Origam.Schema.ParameterReference\" IsAbstract=\"true\" G01=\"118a16a2-f31e-40f4-9eff-8deaf1dd4a89\" refSchemaExtensionId=\"951f2cda-2867-4b99-8824-071fa8749ead\" refParentItemId=\"acc82953-468e-4d00-a77d-f31cb76addb9\" />"+ Environment.NewLine +
                   "     <SchemaItem Id=\"bd05e4af-804f-4307-99eb-6bbcee6bfa6a\" ItemType=\"EntityRelation\" Name=\"BusinessPartnerOrigamRole\" TargetType=\"Origam.Schema.EntityModel.EntityRelationItem\" IsAbstract=\"true\" B01=\"true\" G01=\"ad2aebfe-e684-4e2e-8843-775c468357d5\" refSchemaExtensionId=\"951f2cda-2867-4b99-8824-071fa8749ead\" refParentItemId=\"457a4391-e94f-424c-89d0-dc3804121ce6\" />" + Environment.NewLine +
                   "     <SchemaItem Id=\"336dd1f6-1cfc-4121-8df8-8b7504861f5f\" ItemType=\"EntityRelationColumnPair\" Name=\"BusinessPartnerOrigamRoleKey1\" TargetType=\"Origam.Schema.EntityModel.EntityRelationColumnPairItem\" IsAbstract=\"true\" G01=\"2fdc34b8-fe8b-4353-aa84-92bb5d768370\" G02=\"bd07e079-7328-4959-97ed-2575e2db3f02\" refSchemaExtensionId=\"951f2cda-2867-4b99-8824-071fa8749ead\" refParentItemId=\"bd05e4af-804f-4307-99eb-6bbcee6bfa6a\" />" + Environment.NewLine +
                   "-    <SchemaItem Id=\"d0e0cb68-c308-4511-be2d-112014b919ef\" ItemType=\"EntityUIAction\" Name=\"Identity_ChangePassword\" TargetType=\"Origam.Schema.MenuModel.EntityWorkflowAction\" IsAbstract=\"true\" SS01=\"Change Password\" SS02=\"IDENTITY_USER_MANAGEMENT\" LS01=\"*\" I01=\"10\" I02=\"2\" F01=\"490.0000000000\" F02=\"130.0000000000\" B02=\"true\" B04=\"true\" G01=\"e5cfa7bb-1eb4-429a-a0c0-8840359de4a1\" G02=\"7856accc-c1bf-43c0-a38e-15625ea14b47\" G05=\"f11a9a03-29d6-4b07-b6d0-71bcea36da86\" refSchemaExtensionId=\"951f2cda-2867-4b99-8824-071fa8749ead\" refParentItemId=\"457a4391-e94f-424c-89d0-dc3804121ce6\" />" + Environment.NewLine +
                   "+    <SchemaItem Id=\"d0e0cb68-c308-4511-be2d-112014b919ef\" ItemType=\"EntityUIAction\" Name=\"Identity_ChangePassword\" TargetType=\"Origam.Schema.MenuModel.EntityWorkflowAction\" IsAbstract=\"true\" SS01=\"Change Password\" SS02=\"IDENTITY_USER_MANAGEMENT\" LS01=\"*\" I01=\"10\" I02=\"2\" F01=\"490\" F02=\"130\" B02=\"true\" B04=\"true\" G01=\"e5cfa7bb-1eb4-429a-a0c0-8840359de4a1\" G02=\"7856accc-c1bf-43c0-a38e-15625ea14b47\" G05=\"f11a9a03-29d6-4b07-b6d0-71bcea36da86\" refSchemaExtensionId=\"951f2cda-2867-4b99-8824-071fa8749ead\" refParentItemId=\"457a4391-e94f-424c-89d0-dc3804121ce6\" />" + Environment.NewLine +
                   "     <SchemaItem Id=\"95553560-c50c-4b4f-973d-cde2a0cdf9c4\" ItemType=\"EntityUIActionParameterMapping\" Name=\"UserProfile_Screen\" TargetType=\"Origam.Schema.GuiModel.EntityUIActionParameterMapping\" IsAbstract=\"true\" SS01=\".\" refSchemaExtensionId=\"951f2cda-2867-4b99-8824-071fa8749ead\" refParentItemId=\"d0e0cb68-c308-4511-be2d-112014b919ef\" />" + Environment.NewLine +
                   "     <SchemaItem Id=\"17071b0b-2910-425a-8268-2d07ed497c09\" ItemType=\"EntityUIAction\" Name=\"Identity_UnlockUser\" TargetType=\"Origam.Schema.MenuModel.EntityWorkflowAction\" IsAbstract=\"true\" SS01=\"Unlock\" SS02=\"IDENTITY_USER_MANAGEMENT\" LS01=\"*\" I01=\"10\" I02=\"2\" G01=\"65c96bc7-2fdf-4867-b9f9-17b5b0026a75\" G02=\"9b147754-8bc8-48e7-becb-75d9eaa66d01\" G03=\"61f2c354-9d74-41bf-a1bf-ac19c3d29521\" G05=\"dfe07009-c933-44c7-8df8-b5c204e0d856\" refSchemaExtensionId=\"951f2cda-2867-4b99-8824-071fa8749ead\" refParentItemId=\"457a4391-e94f-424c-89d0-dc3804121ce6\" />" + Environment.NewLine +
                   "     <SchemaItem Id=\"7e595fe7-9e86-412c-8e70-310cdcf931ce\" ItemType=\"EntityUIActionParameterMapping\" Name=\"UserName\" TargetType=\"Origam.Schema.GuiModel.EntityUIActionParameterMapping\" IsAbstract=\"true\" SS01=\"UserName\" refSchemaExtensionId=\"951f2cda-2867-4b99-8824-071fa8749ead\" refParentItemId=\"17071b0b-2910-425a-8268-2d07ed497c09\" />" + Environment.NewLine;
        
        FileInfo pathToOld = new FileInfo(Path.Combine(TestFilesDir.FullName, "old.xml"));
        FileInfo pathToNew = new FileInfo(Path.Combine(TestFilesDir.FullName, "new.xml"));
        var gitFileComparer = new GitFileComparer();
        GitDiff gitDiff = gitFileComparer.GetGitDiff(pathToOld, pathToNew);
        string actualDiffBody = string.Join(Environment.NewLine, gitDiff.Text.Split('\n').Skip(5).
            Select(line => { return line.Replace("\r", ""); }).ToList());
        
        
        StringAssert.AreEqualIgnoringCase(actualDiffBody, expectedDiffBody);
    }
    [Test]
    public void ShouldReturnEmptyDiffIfNoDifferenecesExist()
    {
      
        FileInfo pathToOld = new FileInfo(Path.Combine(TestFilesDir.FullName, "old.xml"));
        FileInfo pathToNew = new FileInfo(Path.Combine(TestFilesDir.FullName, "old.xml"));
        var gitFileComparer = new GitFileComparer();
        GitDiff gitDiff = gitFileComparer.GetGitDiff(pathToOld, pathToNew);
        Assert.That(gitDiff.IsEmpty);
    }
}
