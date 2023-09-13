using Origam.Extensions;
using Origam.Schema.EntityModel;
using System;
using System.Collections.Generic;
using System.Data;

namespace Origam.Rule
{
    public class RuleEvaluationCache
    {
        Dictionary<Tuple<Guid, CredentialValueType, Guid>, bool> ruleCacheDict;
        Dictionary<Tuple<Guid, CredentialType>, bool>
            rulelessFieldSecurityRuleResultDict;


        public RuleEvaluationCache() {
            ruleCacheDict =
                new Dictionary<Tuple<Guid, CredentialValueType, Guid>, bool>();
            rulelessFieldSecurityRuleResultDict =
                new Dictionary<Tuple<Guid, CredentialType>, bool>();
        }
        public Boolean? Get(AbstractEntitySecurityRule rule, Guid entityId)
        {
            bool result;
            if (ruleCacheDict.TryGetValue(new Tuple<Guid, CredentialValueType,
                Guid>(rule.Id, rule.ValueType, entityId), out result))
            {
                return result;
            }
            return null;
        }
        public void Put(AbstractEntitySecurityRule rule, Guid entityId,
            bool value)
        {
            ruleCacheDict.Add(new Tuple<Guid, CredentialValueType, Guid>
                (rule.Id, rule.ValueType, entityId), value);
        }

        public Boolean? GetRulelessFieldResult(Guid entityId,
            CredentialType type)
        {
            bool result;
            if (rulelessFieldSecurityRuleResultDict.TryGetValue(
                new Tuple<Guid, CredentialType>(entityId, type), out result))
            {
                return result;
            }
            return null;
        }

        public void PutRulelessFieldResult(Guid entityId, CredentialType type,
            bool value)
        {
            rulelessFieldSecurityRuleResultDict.Add(
                new Tuple<Guid, CredentialType>(entityId, type), value);
        }
    }
}