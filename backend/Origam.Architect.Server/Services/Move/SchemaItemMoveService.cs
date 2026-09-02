#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using Origam.Architect.Server.Exceptions;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Move;

public class SchemaItemMoveService(
    SchemaService schemaService,
    MoveNodeResolver nodeResolver,
    MoveRuleEvaluator ruleEvaluator,
    MoveTargetFinder targetFinder,
    SchemaItemMover mover,
    SchemaItemCopier copier,
    TreeNodeFactory treeNodeFactory
)
{
    public ISchemaItemProvider GetRootProviderById(string id)
    {
        return nodeResolver.GetRootProviderById(id);
    }

    public List<MoveVerdictResult> GetMoveVerdicts(
        NodeRefModel sourceReference,
        List<NodeRefModel> targetReferences
    )
    {
        MovePreconditions.RequireActivePackage(schemaService);
        IBrowserNode2 source = nodeResolver.Resolve(sourceReference);
        var results = new List<MoveVerdictResult>();
        if (targetReferences == null)
        {
            return results;
        }

        foreach (NodeRefModel targetReference in targetReferences)
        {
            if (targetReference == null)
            {
                continue;
            }

            IBrowserNode2 target = source == null ? null : nodeResolver.Resolve(targetReference);
            (bool canMove, bool canCopy) = ruleEvaluator.EvaluateBothModes(source, target);
            results.Add(
                new MoveVerdictResult
                {
                    Key = ToNodeKey(targetReference),
                    CanMove = canMove,
                    CanCopy = canCopy,
                }
            );
        }

        return results;
    }

    public MoveTargetsResult GetMoveTargets(NodeRefModel sourceReference)
    {
        MovePreconditions.RequireActivePackage(schemaService);
        return targetFinder.Find(nodeResolver.Resolve(sourceReference));
    }

    public MoveNodeResult Move(
        NodeRefModel sourceReference,
        NodeRefModel targetReference,
        bool isCopy
    )
    {
        MovePreconditions.RequireActivePackage(schemaService);
        IBrowserNode2 source =
            nodeResolver.Resolve(sourceReference)
            ?? throw new UserOrigamException(Strings.Move_SourceNotFound);
        IBrowserNode2 target =
            nodeResolver.Resolve(targetReference)
            ?? throw new UserOrigamException(Strings.Move_TargetNotFound);

        MoveDecision decision = ruleEvaluator.Evaluate(source, target, isCopy);
        if (!decision.IsAllowed)
        {
            throw new UserOrigamException(decision.ErrorMessage);
        }

        var original = (ISchemaItem)source;
        ISchemaItem result = isCopy
            ? copier.Copy(original, decision)
            : mover.Move(original, decision);
        return new MoveNodeResult
        {
            Node = treeNodeFactory.Create(result),
            ParentNodeIds = GetParentNodeIdsOrEmpty(result),
        };
    }

    // The model is already written, a broken parent chain must not fail the move.
    private static List<string> GetParentNodeIdsOrEmpty(ISchemaItem item)
    {
        try
        {
            return SchemaItemTreePath.GetParentNodeIds(item, SchemaItemTreePath.GetRoot(item));
        }
        catch (OrphanedSchemaReferenceException)
        {
            return [];
        }
    }

    private static string ToNodeKey(NodeRefModel reference) => reference.Id + reference.NodeText;
}
