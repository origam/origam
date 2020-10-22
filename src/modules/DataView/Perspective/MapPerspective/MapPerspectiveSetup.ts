import { computed } from "mobx";

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
    const match = this.mapCenterRaw.match(/POINT \(([^)]+)\)/);
    const coordsString = match?.[1];
    const coordsSplStr = coordsString?.split(" ");
    const coordLatStr = coordsSplStr?.[1];
    const coordLngStr = coordsSplStr?.[0];
    const lat = coordLatStr ? parseFloat(coordLatStr) : undefined;
    const lng = coordLngStr ? parseFloat(coordLngStr) : undefined;
    return lat !== undefined && lng !== undefined ? { lat, lng } : undefined;
  }
}
