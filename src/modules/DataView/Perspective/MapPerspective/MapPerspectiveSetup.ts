import { computed } from "mobx";
import { parse as wktParse } from "wkt";

export class MapLayer {
  id: string = "";
  title: string = "";
  defaultEnabled: boolean = false;
  type: string = "";
  mapLayerParameters = new Map<string, any>();
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

  @computed
  get mapCenter() {
    return wktParse(this.mapCenterRaw);
  }
}
