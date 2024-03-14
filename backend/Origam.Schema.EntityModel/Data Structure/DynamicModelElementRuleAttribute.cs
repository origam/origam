using Origam.DA.ObjectPersistence;
using System;
using System.Collections;

namespace Origam.Schema.EntityModel
{
    internal class DynamicModelElementRuleAttribute : AbstractModelElementRuleAttribute
    {
        public override Exception CheckRule(object instance)
        {
            throw new NotImplementedException();
        }

        public override Exception CheckRule(object instance, string memberName)
        {
            if (memberName == string.Empty | memberName == null) CheckRule(instance);

            var filterset = (DataStructureFilterSet)instance;
            ArrayList filters = filterset.ChildItemsRecursive;
            foreach (DataStructureFilterSetFilter filter in filters)
            {
                if ((filter.IgnoreFilterConstantId != Guid.Empty || 
                        !string.IsNullOrEmpty(filter.IgnoreFilterParameterName) ||
                        filter.PassWhenParameterMatch) && 
                        !filterset.IsDynamic)
                {
                    return new Exception(ResourceUtils.GetString("ErrorDynamicParameter", memberName));
                }
            }
            return null;
        }
    }
}