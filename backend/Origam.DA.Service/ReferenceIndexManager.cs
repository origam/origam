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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.RuleModel;

namespace Origam.DA.Service;

public static class ReferenceIndexManager
{
    private static ConcurrentQueue<AbstractSchemaItem> updatesRequestedBeforeFullInitialization = new ();

    private static readonly Regex GuidRegEx =
       new (@"([a-z0-9]{8}[-][a-z0-9]{4}[-][a-z0-9]{4}[-][a-z0-9]{4}[-][a-z0-9]{12})");
    
    public static bool Initialized { get; private set; }

    private static readonly ConcurrentDictionary<Guid, HashSet<ReferenceInfo>>
        referenceDictionary = new ();

    internal static HashSet<ReferenceInfo> GetReferences(Guid itemId)
    {
        bool referencesExist = referenceDictionary.TryGetValue(
            itemId, out var references);
        return referencesExist
            ? references 
            : new HashSet<ReferenceInfo>();
    }

    public static void Clear(bool fullClear)
    {
        Initialized = false;
        if (fullClear)
        {
            updatesRequestedBeforeFullInitialization = new ConcurrentQueue<AbstractSchemaItem>();
        }
        referenceDictionary.Clear();
    }
    
    internal static void UpdateIndex(AbstractSchemaItem item)
    {
        if (!Initialized)
        { 
            updatesRequestedBeforeFullInitialization.Enqueue(item);
        }
        else
        {
            Update(item);
        }
    }

    private static void Update(AbstractSchemaItem item)
    {
        referenceDictionary.TryRemove(item.Id, out _);
        if (!item.IsDeleted)
        {
            Add(item);
        }
    }

    public static void Add(AbstractSchemaItem retrievedObj)
    {
        GetReferencesFromDependencies(retrievedObj);
        GetReferencesFromText(retrievedObj);
        GetTypeSpecificReferences(retrievedObj);
        foreach (AbstractSchemaItem item in retrievedObj.ChildItems)
        {
            GetReferencesFromDependencies(item);
            GetReferencesFromText(item);
            GetTypeSpecificReferences(item);
        }
    }

    private static void GetTypeSpecificReferences(AbstractSchemaItem retrievedObj)
    {
        if (retrievedObj is EntityUIAction uiAction)
        {
            AddToIndex(uiAction.ConfirmationRuleId, uiAction);
            ArrayList screenConditions = uiAction.ChildItemsByType(
                ScreenCondition.CategoryConst);
            foreach (ScreenCondition screenCondition in screenConditions)
            {
                AddToIndex(uiAction.Id, screenCondition.Screen);
            }

            ArrayList sectionConditions = uiAction.ChildItemsByType(
                ScreenSectionCondition.CategoryConst);
            foreach (ScreenSectionCondition sectionCondition in
                     sectionConditions)
            {
                AddToIndex(uiAction.Id, sectionCondition.ScreenSection);
            }
        }
    }

    private static void GetReferencesFromText(AbstractSchemaItem retrievedObj)
    {
        MatchCollection matchCollection = null;
        if (retrievedObj is XslTransformation transformation)
        {
            matchCollection = GuidRegEx.Matches(transformation.TextStore);
        }

        if (retrievedObj is XslRule rule)
        {
            matchCollection = GuidRegEx.Matches(rule.Xsl);
        }

        if (retrievedObj is XPathRule xPathRule)
        {
            if (xPathRule.XPath == null)
            {
                throw new NullReferenceException(
                    string.Format(Origam.Strings.XPathIsNull, xPathRule.Id));
            }

            matchCollection = GuidRegEx.Matches(xPathRule.XPath);
        }

        if (matchCollection != null)
        {
            foreach (var id in matchCollection)
            {
                AddToIndex(new Guid(id.ToString()), retrievedObj);
            }
        }
    }

    private static void GetReferencesFromDependencies(AbstractSchemaItem item)
    {
        ArrayList dependencies = item.GetDependencies(false);
        foreach (AbstractSchemaItem dependency in dependencies)
        {
            if (dependency != null)
            {
                AddToIndex(dependency.Id, item);
            }
        }
    }

    private static void AddToIndex(Guid guid, AbstractSchemaItem item)
    {
        var referenceInfo = new ReferenceInfo(item.Id, item.GetType());
        referenceDictionary.AddOrUpdate(guid,
            new HashSet<ReferenceInfo> { referenceInfo },
            (id, oldSet) =>
            {
                oldSet.Add(referenceInfo);
                return oldSet;
            });
    }

    public static void Initialize()
    {
        while (updatesRequestedBeforeFullInitialization.TryDequeue(out var item))
        {
            Update(item);
        }
        Initialized = true;
    }
}

record ReferenceInfo(Guid Id, Type Type)
{
    public virtual bool Equals(ReferenceInfo other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}