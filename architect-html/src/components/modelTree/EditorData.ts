/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o. 

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

import {
  DocumentationEditorData,
  EditorSubType,
  EditorType,
  IApiEditorData,
  IApiEditorNode,
  IApiEditorProperty,
  IDeploymentScriptsGeneratorEditorData,
  ISearchResultsEditorData,
  IScreenEditorData,
  ISectionEditorData,
} from '@api/IArchitectApi';
import { IEditorNode } from '@components/editorTabView/EditorTabViewState';
import { TreeNode } from '@components/modelTree/TreeNode';

export class EditorNode implements IEditorNode {
  id: string;
  origamId: string;
  nodeText: string;
  editorType: EditorSubType;
  parent: TreeNode | null = null;

  constructor(node: IApiEditorNode, parent: TreeNode | null) {
    this.id = node.id;
    this.origamId = node.origamId;
    this.nodeText = node.nodeText;
    this.editorType = node.editorType;
    this.parent = parent;
  }
}

export class EditorData implements IApiEditorData {
  editorId: string;
  editorType: EditorType;
  parentNodeId: string | undefined;
  isDirty: boolean;
  node: EditorNode;
  data:
    | IApiEditorProperty[]
    | ISectionEditorData
    | IScreenEditorData
    | DocumentationEditorData
    | IDeploymentScriptsGeneratorEditorData
    | ISearchResultsEditorData;

  constructor(data: IApiEditorData, parent: TreeNode | null) {
    this.editorId = data.editorId;
    this.editorType = data.editorType;
    this.parentNodeId = data.parentNodeId;
    this.isDirty = data.isDirty;
    this.node = new EditorNode(data.node, parent);
    this.data = data.data;
  }
}
