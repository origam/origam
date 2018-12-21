using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origam.DA
{
    public static class RuleTools
    {
        public static IEnumerable<Exception> CheckRules(object instance)
        {
            IList members = Reflector.FindMembers(instance.GetType(), typeof(IModelElementRule), new Type[0]);
            foreach (MemberAttributeInfo mi in members)
            {
                IModelElementRule rule = mi.Attribute as IModelElementRule;

                yield return rule.CheckRule(instance, mi.MemberInfo.Name);
            }
        }

        public static void DoOnFirstViolation(object objectToCheck, Action<Exception> action)
        {
            Exception firstException = CheckRules(objectToCheck).FirstOrDefault();
            if (firstException != null)
            {
                action(firstException);
            }
        }
    }
}
