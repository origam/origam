using System;
using System.Linq;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;

namespace Origam.DA.EntityModel
{   
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public class NoDuplicateNamesInParentRule : AbstractModelElementRuleAttribute 
    {
        public NoDuplicateNamesInParentRule()
        {
        }

        public override Exception CheckRule(object instance)
        {
            return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
        }

        public override Exception CheckRule(object instance, string memberName)
        {
            if(string.IsNullOrEmpty(memberName)) CheckRule(instance);
            if(memberName != "Name") throw new Exception(nameof(NoDuplicateNamesInParentRule)+" can be only applied to Name properties");  
            if (!(instance is AbstractSchemaItem abstractSchemaItem)) return null;
            if (abstractSchemaItem.ParentItem == null) return null;

            string instanceName = (string)Reflector.GetValue(instance.GetType(), instance, memberName);

            AbstractSchemaItem itemWithDuplicateName = abstractSchemaItem
                .ParentItem.ChildItems
                .ToEnumerable()
                .Where(item => item is AbstractDataEntityColumn)
                .Where(item => item.Name == instanceName)
                .FirstOrDefault(item => item.Id != abstractSchemaItem.Id);

            if (itemWithDuplicateName != null)
            {
                return new Exception(abstractSchemaItem.ParentItem.Name+" contains duplicate child names: "+instanceName);
            }
            return null;
        }
    }
}