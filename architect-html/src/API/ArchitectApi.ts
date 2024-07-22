import axios from "axios";
import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import { EditorProperty } from "src/components/gridEditor/GridEditor.tsx";
import { IArchitectApi } from "src/API/IArchitectApi.ts";

export class ArchitectApi implements IArchitectApi {
  async getTopModelNodes(): Promise<TreeNode[]> {
    return (await axios.get(`/Model/GetTopNodes`)).data;
  }

  async getNodeChildren(node: TreeNode): Promise<TreeNode[]> {
    return (await axios.get(
      `/Model/GetChildren`,
      {
        params: {
          id: node.id,
          nodeText: node.nodeText,
          isNonPersistentItem: node.isNonPersistentItem
        }
      })).data;
  }

  async getProperties(nodeId: string): Promise<EditorProperty[]> {
    return (await (axios.get("/Editor/EditableProperties", {
      params: {schemaItemId: nodeId}
    }))).data;
  }
}

