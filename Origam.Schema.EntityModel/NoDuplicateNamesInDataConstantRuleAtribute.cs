using System;
using System.Data;
using System.Linq;
using Origam.DA.ObjectPersistence;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel
{   
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public class NoDuplicateNamesInDataConstantRuleAtribute : AbstractModelElementRuleAttribute 
    {
        public NoDuplicateNamesInDataConstantRuleAtribute()
        {
        }

        public override Exception CheckRule(object instance)
        {
            return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
        }

        public override Exception CheckRule(object instance, string memberName)
        {
            if(string.IsNullOrEmpty(memberName)) CheckRule(instance);
            if(memberName != "Name") throw new Exception(nameof(NoDuplicateNamesInDataConstantRuleAtribute) +" can be only applied to Name properties");  
            if (!(instance is DataConstant dataconstant)) return null;
            if (dataconstant.RootProvider == null)
            {
                
            }
            string instanceName = (string)Reflector.GetValue(instance.GetType(), instance, memberName);

            var itemWithDuplicateName = dataconstant
                .RootProvider.ChildItems
                .ToEnumerable()
                .Where(item => item is DataConstant)
                .Where(item => item.Name == instanceName)
                .FirstOrDefault(item => item.Id != dataconstant.Id);

            if (itemWithDuplicateName != null)
            {
                return new DataException(dataconstant.Name+" contains duplicate  names ");
            }
            return null;
        }
    }
}