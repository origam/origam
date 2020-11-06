import xmlJs from "xml-js";
import {observable} from "mobx";
import {getApi} from "model/selectors/getApi";

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

  add(menuId: string) {
    this.favoriteIds.push(menuId);
    this.saveFaforites();
  }

  remove(menuId: any) {
    const index = this.favoriteIds.indexOf(menuId);
    if (index > -1) {
      this.favoriteIds.splice(index, 1);
    }
    this.saveFaforites();
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

  private async saveFaforites() {
    const api = getApi(this);
    const xmlFavorites = this.favoriteIdsToXml();
    await api.saveFavorites({ConfigXml: xmlFavorites});
  }

  parent: any;
}
