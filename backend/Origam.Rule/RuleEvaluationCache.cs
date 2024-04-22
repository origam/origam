#region license
/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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

using Origam.Schema.EntityModel;
using System;
using System.Collections.Generic;

namespace Origam.Rule;

public class RuleEvaluationCache
{
    private readonly Dictionary<Tuple<Guid, CredentialValueType, Guid>, bool> 
        rules = new();
    private readonly Dictionary<Tuple<Guid, CredentialType>, bool>
        rulelessFieldSecurityRuleResults = new();

    public bool? Get(AbstractEntitySecurityRule rule, Guid entityId)
    {
            if (rules.TryGetValue(new Tuple<Guid, CredentialValueType,
                    Guid>(rule.Id, rule.ValueType, entityId), out var result))
            {
                return result;
            }
            return null;
        }
    public void Put(AbstractEntitySecurityRule rule, Guid entityId,
        bool value)
    {
            rules.Add(new Tuple<Guid, CredentialValueType, Guid>
                (rule.Id, rule.ValueType, entityId), value);
        }

    public bool? GetRulelessFieldResult(Guid entityId, CredentialType type)
    {
            if (rulelessFieldSecurityRuleResults.TryGetValue(
                    new Tuple<Guid, CredentialType>(entityId, type), 
                    out var result))
            {
                return result;
            }
            return null;
        }

    public void PutRulelessFieldResult(Guid entityId, CredentialType type,
        bool value)
    {
            rulelessFieldSecurityRuleResults.Add(
                new Tuple<Guid, CredentialType>(entityId, type), value);
        }
}