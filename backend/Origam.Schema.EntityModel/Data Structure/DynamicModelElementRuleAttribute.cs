using Origam.DA.ObjectPersistence;
using System;
using System.Collections;

namespace Origam.Schema.EntityModel
{
    internal class DynamicModelElementRuleAttribute : AbstractModelElementRuleAttribute
    {
        public override Exception CheckRule(object instance)
        {
            return new NotSupportedException(
                ResourceUtils.GetString("MemberNameRequired"));
        }

        public override Exception CheckRule(object instance, string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                CheckRule(instance);
            }
            var filterSet = (DataStructureFilterSet)instance;
            ArrayList filters = filterSet.ChildItemsRecursive;
            foreach (DataStructureFilterSetFilter filter in filters)
            {
                if (((filter.IgnoreFilterConstantId != Guid.Empty)
                     || !string.IsNullOrEmpty(filter.IgnoreFilterParameterName) 
                     || filter.PassWhenParameterMatch) 
                    && !filterSet.IsDynamic)
                {
                    return new Exception( ResourceUtils.GetString(
                        "ErrorDynamicParameter", memberName));
                }
            }
            return null;
        }
    }
}