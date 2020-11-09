import xmlJs from "xml-js";
import { observable } from "mobx";
import { getApi } from "model/selectors/getApi";

export class Favorites {
  @observable
  private favoriteFolders: FavoriteFolder[] = [];

  private xmlConverter =  new XmlToFavoritesConverter();

  isFavorite(folderId: string, menuId: string) {
    return this.favoriteFolders.find((folder) => folderId === folder.id)?.has(menuId) === true;
  }

  isInAnyFavoriteFolder( menuId: string){
    return this.favoriteFolders.some(folder => folder.has(menuId));
  }

  get dafaultFavoritesFolderId() {
    return this.favoriteFolders.length > 0 ? this.favoriteFolders[0].id : undefined;
  }

  get customFolderIds(){
    return this.favoriteFolders.slice(1).map(folder => folder.id);
  }

  setXml(xml: string) {
    this.favoriteFolders = this.xmlConverter.xmlToFolders(xml);
  }

  async add(folderId: string, menuId: string) {
    return this.favoriteFolders
      .find((folder) => folderId === folder.id)
      ?.add(menuId);
    await this.saveFavorites();
  }

  async remove(folderId: string, menuId: any) {
    return this.favoriteFolders
      .find((folder) => folderId === folder.id)
      ?.remove(menuId);
    await this.saveFavorites();
  }

  private async saveFavorites() {
    const api = getApi(this);
    const xmlFavorites = this.xmlConverter.favoriteIdsToXml(this.favoriteFolders);
    await api.saveFavorites({ ConfigXml: xmlFavorites });
  }

  async createFolder(name: string) {
    throw new Error("Method not implemented.");
  }

  parent: any;
}

class XmlToFavoritesConverter {
  
  public xmlToFolders(xml: string){
    return xmlJs
      .xml2js(xml)
      .elements[0].elements
      .map((folderXml: any, i: number) => this.parseToFavoriteFolder(folderXml, i === 0));
  }

  private parseToFavoriteFolder(foldeXml: any, isDefault: boolean){
    const label = foldeXml.attributes["label"];
    const id = label;
    const itemIds = foldeXml.elements
      ?.map((item: xmlJs.Element) => item.attributes?.["menuId"])
      ?.filter((menuId: string) => menuId)
      ?? [];

    return new FavoriteFolder(id, label, isDefault, itemIds);
  }
  
  public favoriteIdsToXml(favoriteFolders: FavoriteFolder[]) {
    return (
      "<favourites>\n" +
      favoriteFolders.map(folder => this.folderToXml(folder)).join("\n")+"\n"+
      "</favourites>"
    );
  }

  private folderToXml(folder: FavoriteFolder){
    return  `  <folder label=\"${folder.name}\">\n` +
      folder.items.map((menuId) => `    <item menuId="${menuId}"/>`).join("\n") +
      "  </folder>";
  }
}

class FavoriteFolder {
  constructor(public id: string, public name: string, isDefault: boolean, public items: string[]) {}

  public has(menuId: string){
    return this.items.includes(menuId);
  }

  public add(menuId: string){
    return this.items.push(menuId);
  }

  public remove(menuId: string){
    return this.items.remove(menuId);
  }
}
