import xmlJs from "xml-js";
import {observable} from "mobx";
import {getApi} from "model/selectors/getApi";
import {uuidv4} from "utils/uuid";

export class Favorites {
  @observable
  public favoriteFolders: FavoriteFolder[] = [];

  private xmlConverter = new XmlToFavoritesConverter();

  public isFavorite(folderId: string, menuId: string) {
    return this.favoriteFolders.find((folder) => folderId === folder.id)?.has(menuId) === true;
  }

  public isInAnyFavoriteFolder(menuId: string) {
    return this.favoriteFolders.some((folder) => folder.has(menuId));
  }

  public get dafaultFavoritesFolderId() {
    return this.favoriteFolders.length > 0 ? this.favoriteFolders[0].id : "Favorites";
  }

  public get customFolders() {
    return this.favoriteFolders.slice(1);
  }

  public setXml(xml: string) {
    this.favoriteFolders = this.xmlConverter.xmlToFolders(xml);
  }

  public async add(folderId: string, menuId: string) {
    return this.favoriteFolders.find((folder) => folderId === folder.id)?.add(menuId);
    await this.saveFavorites();
  }

  public async remove(menuId: any) {
    return this.favoriteFolders.find((folder) => folder.has(menuId))?.remove(menuId);
    await this.saveFavorites();
  }

  private async saveFavorites() {
    const api = getApi(this);
    const xmlFavorites = this.xmlConverter.favoriteIdsToXml(this.favoriteFolders);
    await api.saveFavorites({ConfigXml: xmlFavorites});
  }

  public async createFolder(name: string) {
    this.favoriteFolders.push(new FavoriteFolder(uuidv4(), name, false, []));
    await this.saveFavorites();
  }

  public async removeFolder(id: string) {
    const folderToRemove = this.favoriteFolders.find((folder) => folder.id === id);
    if (folderToRemove) {
      this.favoriteFolders.remove(folderToRemove);
    }
    await this.saveFavorites();
  }

  public getFolderName(folderId: string): string {
    return this.favoriteFolders.find((folder) => folder.id === folderId)?.name ?? "";
  }

  public async renameFolder(folderId: string, newName: string): Promise<any> {
    this.favoriteFolders.find((folder) => folder.id === folderId)!.name = newName;
    await this.saveFavorites();
  }

  parent: any;
}

class XmlToFavoritesConverter {
  public xmlToFolders(xml: string) {
    return xmlJs
      .xml2js(xml)
      .elements[0].elements
      .map((folderXml: any, i: number) => this.parseToFavoriteFolder(folderXml, i === 0)
      );
  }

  private parseToFavoriteFolder(foldeXml: any, isDefault: boolean) {
    const label = foldeXml.attributes["label"];
    const id = foldeXml.attributes["id"] ?? label;
    const itemIds =
      foldeXml.elements
        ?.map((item: xmlJs.Element) => item.attributes?.["menuId"])
        ?.filter((menuId: string) => menuId) ?? [];

    return new FavoriteFolder(id, label, isDefault, itemIds);
  }

  public favoriteIdsToXml(favoriteFolders: FavoriteFolder[]) {
    return (
      "<favourites>\n" +
      favoriteFolders.map((folder) => this.folderToXml(folder)).join("\n") +
      "\n" +
      "</favourites>"
    );
  }

  private folderToXml(folder: FavoriteFolder) {
    return (
      `  <folder label=\"${folder.name}\" id=\"${folder.id}\">\n` +
      folder.items.map((menuId) => `    <item menuId="${menuId}"/>`).join("\n") +
      "  </folder>"
    );
  }
}

export class FavoriteFolder {
  constructor(
    public id: string,
    name: string,
    public isDefault: boolean,
    public items: string[]
  ) {
    this.name = name;
  }

  @observable
  name: string;

  public has(menuId: string) {
    return this.items.includes(menuId);
  }

  public add(menuId: string) {
    return this.items.push(menuId);
  }

  public remove(menuId: string) {
    return this.items.remove(menuId);
  }
}
