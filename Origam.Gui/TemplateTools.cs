using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Origam.DA;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;

namespace Origam.Gui
{
    public static class TemplateTools
    {
        public static object[] AddTemplateRecord(DataRow parentRow, DataStructureTemplate template, string dataMember, Guid dataStructureId, DataSet formData)
        {
            object[] templatePosition = null;
            IXmlContainer doc;

            if (parentRow == null)
            {
                doc = DataDocumentFactory.New(new DataSet("ROOT"));
            }
            else
            {
                DataSet slice = formData.Clone();
                DatasetTools.GetDataSlice(
                    slice, new List<DataRow> { parentRow },
                    null, false, new ArrayList());

                try
                {
                    doc = DataDocumentFactory.New(slice);
                }
                catch
                {
                    return new object[] { };
                }
            }

            try
            {
                templatePosition = AddTemplateRecord(dataMember, template, doc, dataStructureId, formData);
            }
            catch (Exception ex)
            {
                throw new Exception(ResourceUtils.GetString("ErrorProcessTemplate"), ex);
            }

            return templatePosition;
        }

        private static object[] AddTemplateRecord(string dataMember, DataStructureTemplate template, IXmlContainer dataSource, Guid dataStructureId, DataSet formData)
        {
            if (template == null) throw new NullReferenceException(ResourceUtils.GetString("ErrorNoTemplate"));
            if (dataMember != template.Entity.Name)
            {
                if (dataMember != GetDataMember(template.Entity)) return null;
            }
            DataSet newData = NewRecord(template, dataSource, dataStructureId);
            if (newData == null) return null;
            UserProfile profile = SecurityManager.CurrentUserProfile();
            DatasetTools.MergeDataSet(formData, newData, null, new MergeParams(profile.Id));
            return DatasetTools.PrimaryKey(newData.Tables[template.Entity.Name].Rows[0]);
        }

        public static DataSet NewRecord(DataStructureTemplate template, IXmlContainer dataSource, Guid dataStructureId)
        {
            XslTransformation xslt = (template as DataStructureTransformationTemplate).Transformation as XslTransformation;
            IDataStructure outputStructure = xslt.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(dataStructureId)) as IDataStructure;

            IXsltEngine transform = AsTransform.GetXsltEngine(
                xslt.XsltEngineType, template.PersistenceProvider);
            IXmlContainer result = transform.Transform(dataSource, xslt.TextStore, null, new RuleEngine(null, null), outputStructure, false);

            if (result is IDataDocument)
            {
                return (result as IDataDocument).DataSet;
            }

            throw new Exception(ResourceUtils.GetString("ErrorResultNotSupported", result.GetType().ToString()));
        }

        private static string GetDataMember(DataStructureEntity entity)
        {
            DataStructureEntity parentEntity = entity;
            string result = "";

            while (parentEntity != null)
            {
                if (result != "") result = "." + result;

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
}
