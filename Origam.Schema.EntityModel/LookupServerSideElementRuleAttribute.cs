using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel.Interfaces;
using System;
using System.Data;

namespace Origam.Schema.EntityModel
{
    public class LookupServerSideElementRuleAttribute : AbstractModelElementRuleAttribute
    {
        public override Exception CheckRule(object instance)
        {
            return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
        }

        public override Exception CheckRule(object instance, string memberName)
        {
            if (memberName == String.Empty | memberName == null) CheckRule(instance);
            var iDataLookup = ((ILookupReference)instance).Lookup;
            
            if(iDataLookup.IsFilteredServerside)
            {
                return null;
            }
            return new DataException("Lookup have to have property isFilteredServerSide on true."); ;
        }
    }
}
