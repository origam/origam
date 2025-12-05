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

import { IArchitectApi, IPackagesInfo } from '@api/IArchitectApi';
import { TreeNode } from '@components/modelTree/TreeNode';
import { RootStore } from '@stores/RootStore';
import { computed, observable } from 'mobx';

export class ModelTreeState {
  @observable accessor modelNodes: TreeNode[] = [];
  @observable accessor packagesInfo: IPackagesInfo | null = null;
  private architectApi: IArchitectApi;

  constructor(private rootStore: RootStore) {
    this.architectApi = this.rootStore.architectApi;
  }

  @computed
  get activePackageName(): string | null {
    if (!this.packagesInfo) {
      return null;
    }
    const activePackage = this.packagesInfo.packages.find(
      pkg => pkg.id === this.packagesInfo!.activePackageId,
    );
    return activePackage?.name ?? null;
  }

  *loadPackageNodes(): Generator<Promise<any>, void, any> {
    const packagesInfo = yield this.architectApi.getPackages();
    this.packagesInfo = packagesInfo;

    const apiNodes = yield this.architectApi.getTopModelNodes();
    this.modelNodes = apiNodes.map(node => new TreeNode(node, this.rootStore));
  }

  findNodeById(nodeId: string | undefined): TreeNode | null {
    return this.findNodeByIdRecursively(nodeId, this.modelNodes);
  }

  private findNodeByIdRecursively(nodeId: string | undefined, nodes: TreeNode[]): TreeNode | null {
    if (!nodeId) {
      return null;
    }
    for (const node of nodes) {
      if (node.id === nodeId) {
        return node;
      }
      const foundNode = this.findNodeByIdRecursively(nodeId, node.children);
      if (foundNode) {
        return foundNode;
      }
    }
    return null;
  }
}
