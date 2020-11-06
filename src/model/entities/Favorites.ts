import xmlJs from "xml-js";
import {observable} from "mobx";

export class Favorites {

  @observable
  favoriteIds: string[] = [];

  isFavorite(menuId: any) {
    return this.favoriteIds.includes(menuId);
  }

  setXml(xml: string) {
    const favoritesObject = xmlJs.xml2js(xml);
    this.favoriteIds = favoritesObject.elements[0].elements[0].elements
      .map((item: xmlJs.Element) => item.attributes?.["menuId"])
      .filter((menuId: string) => menuId);
  }

  add(menuId: string) {
    this.favoriteIds.push(menuId);
  }

  remove(menuId: any) {
    const index = this.favoriteIds.indexOf(menuId);
    if(index > -1){
      this.favoriteIds.splice(index, 1);
    }
  }
}
