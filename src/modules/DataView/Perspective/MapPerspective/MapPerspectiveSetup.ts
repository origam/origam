import { computed } from "mobx";
import { parse as wktParse } from "wkt";

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
    };
  }
}

export class MapPerspectiveSetup {
  mapLocationMember: string = "";
  mapAzimuthMember: string = "";
  mapColorMember: string = "";
  mapIconMember: string = "";
  mapTextMember: string = "";
  textColorMember: string = "";
  textLocationMember: string = "";
  textRotationMember: string = "";
  mapCenterRaw: string = "";
  layers: MapLayer[] = [];
  isReadOnlyView: boolean = false;

  @computed
  get mapCenter() {
    return wktParse(this.mapCenterRaw);
  }
}
