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

namespace Origam.Architect.Server.ReturnModels;

public class DropTargetResult
{
    public string Id { get; set; }
    public bool CanMove { get; set; }
    public bool CanCopy { get; set; }
}

public class MoveNodeResult
{
    public TreeNode Node { get; set; }
    public List<string> ParentNodeIds { get; set; }
}

public enum MoveTargetKind
{
    Provider,
    Group,
    Item,
}

public class MoveTargetResult
{
    // Id and NodeText are what MoveNode expects back as the target reference.
    public string Id { get; set; }
    public string NodeText { get; set; }
    public string Key { get; set; }
    public string Path { get; set; }
    public string PackageName { get; set; }
    public MoveTargetKind Kind { get; set; }
    public bool IsInActivePackage { get; set; }
    public bool IsCurrentLocation { get; set; }
    public bool CanMove { get; set; }
    public bool CanCopy { get; set; }
}

public class MoveTargetsResult
{
    public List<MoveTargetResult> Targets { get; set; }
    public bool IsTruncated { get; set; }
}
