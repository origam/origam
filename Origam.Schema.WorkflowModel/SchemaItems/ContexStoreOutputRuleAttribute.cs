using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using System;
using System.Collections;
using System.Data;

namespace Origam.DA.ObjectPersistence.Attributes
{
    public class ContexStoreOutputRuleAttribute : AbstractModelElementRuleAttribute
    {
        public ContexStoreOutputRuleAttribute()
        {
        }

        public override Exception CheckRule(object instance)
        {
            return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
        }

        public override Exception CheckRule(object instance, string memberName)
        {
            if (memberName == String.Empty | memberName == null) CheckRule(instance);

            ContextStore context = (ContextStore)instance;
            var arrayContex = context.RootItem.ChildItemsByType(ContextStore.ItemTypeConst);
            int countIsTrue = 0;
            foreach (ContextStore contextStore in arrayContex)
            {
                if (contextStore.IsReturnValue)
                {
                    if (context.Id != contextStore.Id)
                    {
                        countIsTrue++;
                    }
                }
            }
            if (context.IsReturnValue)
            {
                    countIsTrue++;
            }

            if (countIsTrue>1)
            {
                return new DataException("Workflow has more that one ContexStore with IsReturnValue=True");
            }
             return null;
        }
    }
}
