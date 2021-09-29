/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

import xmlJs from "xml-js";
import { observable } from "mobx";
import { getApi } from "model/selectors/getApi";
import { v4 as uuidv4 } from 'uuid';
import { T } from "utils/translation";

export class Favorites {
  @observable
  public favoriteFolders: FavoriteFolder[] = [];

  private xmlConverter = new XmlToFavoritesConverter();

  public isFavorite(folderId: string, menuId: string) {
    return this.getFolder(folderId)?.has(menuId) === true;
  }

  public isInAnyFavoriteFolder(menuId: string) {
    return this.favoriteFolders.some((folder) => folder.has(menuId));
  }

  public get defaultFavoritesFolderId() {
    return this.favoriteFolders.length > 0 ? this.favoriteFolders[0].id : "Favorites";
  }

  public setXml(xml: string) {
    const foldersFromXml = this.xmlConverter.xmlToFolders(xml);
    if (foldersFromXml.length === 0) {
      foldersFromXml.push(
        new FavoriteFolder(uuidv4(), T("Favorites", "default_group"), true, [], false)
      );
    }
    this.favoriteFolders = foldersFromXml;
  }

  public async add(folderId: string, menuId: string) {
    this.getFolder(folderId)?.add(menuId);
    await this.saveFavorites();
  }

  public async remove(menuId: any) {
    this.favoriteFolders.find((folder) => folder.has(menuId))?.remove(menuId);
    await this.saveFavorites();
  }

  private async saveFavorites() {
    const api = getApi(this);
    const xmlFavorites = this.xmlConverter.favoriteIdsToXml(this.favoriteFolders);
    await api.saveFavorites({ ConfigXml: xmlFavorites });
  }

  public async createFolder(name: string, isPinned: boolean) {
    this.favoriteFolders.push(new FavoriteFolder(uuidv4(), name, false, [], isPinned));
    await this.saveFavorites();
  }

  public async removeFolder(folderId: string) {
    const folderToRemove = this.getFolder(folderId);
    if (folderToRemove) {
      this.favoriteFolders.remove(folderToRemove);
    }
    await this.saveFavorites();
  }

  public getFolderName(folderId: string): string {
    return this.getFolder(folderId)?.name ?? "";
  }

  public async updateFolder(folderId: string, newName: string, isPinned: boolean): Promise<any> {
    const folder = this.getFolder(folderId)!;
    folder.name = newName;
    folder.isPinned = isPinned;
    await this.saveFavorites();
  }

  public getFolder(folderId: string) {
    return this.favoriteFolders.find((folder) => folder.id === folderId);
  }

  public async setPinned(folderId: string, isPinned: boolean): Promise<any> {
    this.getFolder(folderId)!.isPinned = isPinned;
    await this.saveFavorites();
  }

  public async moveItemInFolder(itemIds: string[],  fromIndex: number, toIndex: number) {
    const itemId = itemIds[fromIndex];
    itemIds.splice(fromIndex, 1);
    itemIds.splice(toIndex, 0, itemId);
    await this.saveFavorites();
  }

  async moveItemBetweenFolders(itemId: string, sourceFolder: FavoriteFolder, destinationFolder: FavoriteFolder) {
    sourceFolder.itemIds.remove(itemId);
    destinationFolder.itemIds.push(itemId);
    await this.saveFavorites();
  }

  parent: any;
}

class XmlToFavoritesConverter {
  public xmlToFolders(xml: string): FavoriteFolder[] {
    if (!xml) return [];
    const parsedXml = xmlJs.xml2js(xml);
    if (!parsedXml?.elements?.[0]?.elements) return [];
    return parsedXml.elements[0].elements.map((folderXml: any, i: number) =>
      this.parseToFavoriteFolder(folderXml, i === 0)
    );
  }

  private parseToFavoriteFolder(folderXml: any, isDefault: boolean) {
    const label = folderXml.attributes["label"];
    const id = folderXml.attributes["id"] ?? label;
    const isPinned = folderXml.attributes["isPinned"] === "true";
    const itemIds =
      folderXml.elements
        ?.map((item: xmlJs.Element) => item.attributes?.["menuId"])
        ?.filter((menuId: string) => menuId) ?? [];

    return new FavoriteFolder(id, label, isDefault, itemIds, isPinned);
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
      `  <folder label="${folder.name}" id="${folder.id}" isPinned="${folder.isPinned}">\n` +
      folder.itemIds.map((menuId) => `    <item menuId="${menuId}"/>`).join("\n") +
      "  </folder>"
    );
  }
}

export class FavoriteFolder {
  constructor(
    public id: string,
    name: string,
    public isDefault: boolean,
    items: string[],
    isPinned: boolean
  ) {
    this.name = name;
    this.isPinned = isPinned;
    this.itemIds = items;
  }

  @observable
  itemIds: string[];

  @observable
  isPinned: boolean;

  @observable
  name: string;

  public has(menuId: string) {
    return this.itemIds.includes(menuId);
  }

  public add(menuId: string) {
    return this.itemIds.push(menuId);
  }

  public remove(menuId: string) {
    return this.itemIds.remove(menuId);
  }
}
