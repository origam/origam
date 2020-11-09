import xmlJs from "xml-js";
import { observable } from "mobx";
import { getApi } from "model/selectors/getApi";

export class Favorites {
  favoritesNodeLabel: string = "";

  @observable
  private favoriteIds: string[] = [];

  isFavorite(menuId: any) {
    return this.favoriteIds.includes(menuId);
  }

  setXml(xml: string) {
    const favoritesObject = xmlJs.xml2js(xml);
    this.favoriteIds = favoritesObject.elements[0].elements[0].elements
      .map((item: xmlJs.Element) => item.attributes?.["menuId"])
      .filter((menuId: string) => menuId);

    this.favoritesNodeLabel = favoritesObject.elements[0].elements[0].attributes["label"];
  }

  async add(menuId: string) {
    this.favoriteIds.push(menuId);
    await this.saveFavorites();
  }

  async remove(menuId: any) {
    this.favoriteIds.remove(menuId);
    await this.saveFavorites();
  }

  private favoriteIdsToXml() {
    return (
      "<favourites>\n" +
      `  <folder label=\"${this.favoritesNodeLabel}\">\n` +
      this.favoriteIds.map((menuId) => `    <item menuId="${menuId}"/>`).join("\n") +
      "  </folder>\n" +
      "</favourites>"
    );
  }

  private async saveFavorites() {
    const api = getApi(this);
    const xmlFavorites = this.favoriteIdsToXml();
    await api.saveFavorites({ ConfigXml: xmlFavorites });
  }

  async createFolder(name: string) {
    throw new Error("Method not implemented.");
  }

  parent: any;
}
