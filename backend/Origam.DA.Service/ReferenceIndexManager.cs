#region license

/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.RuleModel;

namespace Origam.DA.Service;

public static class ReferenceIndexManager
{
    private static ConcurrentQueue<ISchemaItem> updatesRequestedBeforeFullInitialization = new();

    private static readonly Regex GuidRegEx = new(
        @"([a-z0-9]{8}[-][a-z0-9]{4}[-][a-z0-9]{4}[-][a-z0-9]{4}[-][a-z0-9]{12})"
    );

    public static bool Initialized { get; private set; }

    private static readonly ConcurrentDictionary<Guid, HashSet<ReferenceInfo>> referenceDictionary =
        new();

    internal static HashSet<ReferenceInfo> GetReferences(Guid itemId)
    {
        bool referencesExist = referenceDictionary.TryGetValue(itemId, out var references);
        return referencesExist ? references : new HashSet<ReferenceInfo>();
    }

    public static void Clear(bool fullClear)
    {
        Initialized = false;
        if (fullClear)
        {
            updatesRequestedBeforeFullInitialization = new ConcurrentQueue<ISchemaItem>();
        }
        referenceDictionary.Clear();
    }

    internal static void UpdateNowOrDeffer(ISchemaItem item)
    {
        if (!Initialized)
        {
            updatesRequestedBeforeFullInitialization.Enqueue(item);
        }
        else
        {
            UpdateNow(item);
        }
    }

    private static void UpdateNow(ISchemaItem item)
    {
        RemoveAllReferences(item);
        if (!item.IsDeleted)
        {
            Add(item);
        }
    }

    private static void RemoveAllReferences(ISchemaItem item)
    {
        var referenceInfo = new ReferenceInfo(item.Id, item.GetType());
        var referencesToRemove = referenceDictionary
            .Where(refReference => refReference.Value.Contains(referenceInfo))
            .ToList();
        foreach (var referenceFound in referencesToRemove)
        {
            referenceFound.Value.Remove(referenceInfo);
            if (referenceFound.Value.Count == 0)
            {
                referenceDictionary.TryRemove(referenceFound.Key, out _);
            }
        }
    }

    public static void Add(ISchemaItem item)
    {
        AddReference(item);
        GetReferencesFromText(item);
        GetTypeSpecificReferences(item);
    }

    private static void GetTypeSpecificReferences(ISchemaItem item)
    {
        if (item is EntityUIAction uiAction)
        {
            AddToIndex(uiAction.ConfirmationRuleId, uiAction);
        }
    }

    private static void GetReferencesFromText(ISchemaItem item)
    {
        MatchCollection matchCollection = null;
        if (item is XslTransformation transformation)
        {
            matchCollection = GuidRegEx.Matches(transformation.TextStore);
        }

        if (item is XslRule rule)
        {
            matchCollection = GuidRegEx.Matches(rule.Xsl);
        }

        if (item is XPathRule xPathRule)
        {
            if (xPathRule.XPath == null)
            {
                throw new NullReferenceException(
                    string.Format(Origam.Strings.XPathIsNull, xPathRule.Id)
                );
            }

            matchCollection = GuidRegEx.Matches(xPathRule.XPath);
        }

        if (matchCollection != null)
        {
            foreach (var id in matchCollection)
            {
                AddToIndex(new Guid(id.ToString()), item);
            }
        }
    }

    private static void AddReference(ISchemaItem item)
    {
        GetReferencesFromDependencies(item);
        foreach (ISchemaItem childItem in item.ChildItems)
        {
            GetReferencesFromDependencies(childItem);
        }
    }

    private static void GetReferencesFromDependencies(ISchemaItem item)
    {
        List<ISchemaItem> dependencies = item.GetDependencies(false);
        foreach (ISchemaItem dependency in dependencies)
        {
            if (dependency != null)
            {
                AddToIndex(dependency.Id, item);
            }
        }
    }

    private static void AddToIndex(Guid dependencyItemId, ISchemaItem reference)
    {
        var referenceInfo = new ReferenceInfo(reference.Id, reference.GetType());
        referenceDictionary.AddOrUpdate(
            dependencyItemId,
            new HashSet<ReferenceInfo> { referenceInfo },
            (id, oldSet) =>
            {
                // The HashSet is not thread safe. But it does not look like we
                // need thread safety here based on how this method is called.
                oldSet.Add(referenceInfo);
                return oldSet;
            }
        );
    }

    public static void Initialize()
    {
        while (updatesRequestedBeforeFullInitialization.TryDequeue(out var item))
        {
            UpdateNow(item);
        }
        Initialized = true;
    }
}

record ReferenceInfo(Guid Id, Type Type)
{
    public virtual bool Equals(ReferenceInfo other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
