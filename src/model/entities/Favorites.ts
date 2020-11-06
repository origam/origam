import xmlJs from "xml-js";

export class Favorites {
  // favoritesObject: xmlJs.Element | xmlJs.ElementCompact | undefined;
  favoriteIds: string[] = [];

  isFavorite(menuId: any) {
    return this.favoriteIds.includes(menuId);
  }

  setXml(xml: string) {
    const favoritesObject = xmlJs.xml2js(xml);
    this.favoriteIds = favoritesObject.elements[0].elements[0]
      .elements
      .map((item: xmlJs.Element) => item.attributes?.["menuId"])
      .filter((menuId: string) => menuId)
  }
}
