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
  EditorType,
  IApiEditorNode,
  IApiEditorData,
  IApiEditorProperty,
  ISectionEditorData,
  IScreenEditorData,
} from 'src/API/IArchitectApi.ts';
import { TreeNode } from 'src/components/modelTree/TreeNode.ts';
import { IEditorNode } from 'src/components/editorTabView/EditorTabViewState.ts';

export class EditorNode implements IEditorNode {
  id: string;
  origamId: string;
  nodeText: string;
  editorType: EditorType;
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
  parentNodeId: string | undefined;
  isDirty: boolean;
  node: EditorNode;
  data: IApiEditorProperty[] | ISectionEditorData | IScreenEditorData;
  constructor(data: IApiEditorData, parent: TreeNode | null) {
    this.parentNodeId = data.parentNodeId;
    this.isDirty = data.isDirty;
    this.node = new EditorNode(data.node, parent);
    this.data = data.data;
  }
}
