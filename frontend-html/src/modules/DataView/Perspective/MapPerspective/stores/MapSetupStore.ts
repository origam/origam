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

import { computed } from "mobx";
import { parse as wktParse } from "wkt";
import { MapRootStore } from "./MapRootStore";

export class MapLayer {
  id: string = "";
  title: string = "";
  defaultEnabled: boolean = false;
  type: string = "";
  mapLayerParameters = new Map<string, any>();

  getUrl() {
    return this.mapLayerParameters.get("url");
  }

  getTitle() {
    return this.title;
  }

  getOptions() {
    const rawOptions = Object.fromEntries(this.mapLayerParameters);
    delete rawOptions.url;
    delete rawOptions.title;
    return {
      ...rawOptions,
      id: this.id,
      minZoom: rawOptions.minZoom !== undefined ? parseInt(rawOptions.minZoom) : undefined,
      maxZoom: rawOptions.maxZoom !== undefined ? parseInt(rawOptions.maxZoom) : undefined,
      maxNativeZoom: rawOptions.maxZoom !== undefined ? parseInt(rawOptions.maxZoom) : undefined,
    };
  }
}

export class MapSetupStore {
  constructor(private rootStore: MapRootStore) {
  }

  mapLocationMember: string = "";
  mapAzimuthMember: string = "";
  mapColorMember: string = "";
  mapIconMember: string = "";
  mapTextMember: string = "";
  mapResolutionRaw: string = "";
  textColorMember: string = "";
  textLocationMember: string = "";
  textRotationMember: string = "";
  mapCenterRaw: string = "";
  isReadOnlyView: boolean = false;

  layers: MapLayer[] = [];

  get mapZoom() {
    let zoom = this.mapResolutionRaw ? parseInt(this.mapResolutionRaw) : 0;
    if (zoom < 0 || zoom > 15) {
      throw new Error("Map zoom must be between 0 and 15. The value is: " + zoom);
    }
    return zoom;
  }

  @computed
  get mapCenter() {
    try {
      return this.mapCenterRaw ? wktParse(this.mapCenterRaw) : undefined;
    } catch (e) {
      console.error(e); // eslint-disable-line no-console
      return undefined;
    }
  }
}
