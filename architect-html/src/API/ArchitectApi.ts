import axios from "axios";
import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import { IArchitectApi } from "src/API/IArchitectApi.ts";
import { EditorProperty } from "src/components/editors/gridEditor/GrirEditorSlice.ts";
import { Package } from "src/components/packages/Packages";

export class ArchitectApi implements IArchitectApi {

  async setActivePackage(packageId: string): Promise<void> {
      await axios.post("/Package/SetActive", {id: packageId})
  }

  async getPackages(): Promise<Package[]> {
     return (await axios.get("/Package/GetAll")).data
  }

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

  async getProperties(schemaItemId: string): Promise<EditorProperty[]> {
    return (await (axios.get("/Editor/EditableProperties", {
      params: {schemaItemId: schemaItemId}
    }))).data;
  }

  async persistChanges(schemaItemId: string, changedProperties: EditorProperty[]): Promise<void> {
    const changes = changedProperties
      .filter(x => !x.readOnly)
      .map(x => {
        return {
          name: x.name,
          value: x.value === undefined || x.value === null ? null : x.value.toString(),
        }
      });
    await axios.post(`/Editor/PersistChanges`, {schemaItemId, changes});
  }
}

