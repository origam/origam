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
using System.Data;
using Origam.DA;
using Origam.Rule.Xslt;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Service.Core;

namespace Origam.Gui;

public static class TemplateTools
{
    public static object[] AddTemplateRecord(
        DataRow parentRow,
        DataStructureTemplate template,
        string dataMember,
        Guid dataStructureId,
        DataSet formData
    )
    {
        object[] templatePosition = null;
        IXmlContainer doc;
        if (parentRow == null)
        {
            doc = DataDocumentFactory.New(dataSet: new DataSet(dataSetName: "ROOT"));
        }
        else
        {
            DataSet slice = formData.Clone();
            DatasetTools.GetDataSlice(
                target: slice,
                rows: new List<DataRow> { parentRow },
                profileId: null,
                copy: false,
                tablesToSkip: new List<string>()
            );
            try
            {
                doc = DataDocumentFactory.New(dataSet: slice);
            }
            catch
            {
                return new object[] { };
            }
        }
        try
        {
            templatePosition = AddTemplateRecord(
                dataMember: dataMember,
                template: template,
                dataSource: doc,
                dataStructureId: dataStructureId,
                formData: formData
            );
        }
        catch (Exception ex)
        {
            throw new Exception(
                message: ResourceUtils.GetString(key: "ErrorProcessTemplate"),
                innerException: ex
            );
        }
        return templatePosition;
    }

    private static object[] AddTemplateRecord(
        string dataMember,
        DataStructureTemplate template,
        IXmlContainer dataSource,
        Guid dataStructureId,
        DataSet formData
    )
    {
        if (template == null)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(key: "ErrorNoTemplate")
            );
        }

        if (dataMember != template.Entity.Name)
        {
            if (dataMember != GetDataMember(entity: template.Entity))
            {
                return null;
            }
        }
        DataSet newData = NewRecord(
            template: template,
            dataSource: dataSource,
            dataStructureId: dataStructureId
        );
        if (newData == null)
        {
            return null;
        }

        UserProfile profile = SecurityManager.CurrentUserProfile();
        DatasetTools.MergeDataSet(
            inout_dsTarget: formData,
            in_dsSource: newData,
            changeList: null,
            mergeParams: new MergeParams(ProfileId: profile.Id)
        );
        return DatasetTools.PrimaryKey(
            row: newData.Tables[name: template.Entity.Name].Rows[index: 0]
        );
    }

    public static DataSet NewRecord(
        DataStructureTemplate template,
        IXmlContainer dataSource,
        Guid dataStructureId
    )
    {
        XslTransformation xslt =
            (template as DataStructureTransformationTemplate).Transformation as XslTransformation;
        IDataStructure outputStructure =
            xslt.PersistenceProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(id: dataStructureId)
            ) as IDataStructure;
        IXsltEngine transform = new CompiledXsltEngine(persistence: template.PersistenceProvider);
        IXmlContainer result = transform.Transform(
            data: dataSource,
            xsl: xslt.TextStore,
            parameters: null,
            transactionId: null,
            outputStructure: outputStructure,
            validateOnly: false
        );
        if (result is IDataDocument)
        {
            return (result as IDataDocument).DataSet;
        }
        throw new Exception(
            message: ResourceUtils.GetString(
                key: "ErrorResultNotSupported",
                args: result.GetType().ToString()
            )
        );
    }

    private static string GetDataMember(DataStructureEntity entity)
    {
        DataStructureEntity parentEntity = entity;
        string result = "";
        while (parentEntity != null)
        {
            if (result != "")
            {
                result = "." + result;
            }

            result = parentEntity.Name + result;
            if (parentEntity.ParentItem is DataStructureEntity)
            {
                parentEntity = parentEntity.ParentItem as DataStructureEntity;
            }
            else
            {
                parentEntity = null;
            }
        }
        return result;
    }
}
