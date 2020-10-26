import { IMapObjectType, IMapPoint, IMapPolygon } from "../MapSourceData";

export function parseGeoString(strIn: string) {
  return parseGeoPoint(strIn) || parseGeoPolygon(strIn);
}

export function parseGeoPoint(strIn: string) {
  const match = strIn.match(/POINT \(([^)]+)\)/);
  const coordsString = match?.[1];
  const coordsSplStr = coordsString?.split(" ");
  const coordLatStr = coordsSplStr?.[1];
  const coordLngStr = coordsSplStr?.[0];
  const lat = coordLatStr ? parseFloat(coordLatStr) : undefined;
  const lng = coordLngStr ? parseFloat(coordLngStr) : undefined;
  return lat !== undefined && lng !== undefined
    ? ({ type: "POINT", lat, lng } as IMapPoint)
    : undefined;
}

export function parseGeoPolygon(strIn: string) {
  if (!strIn.startsWith("POLYGON")) return;
  const match = strIn.match(/([\d\.]+ [\d\.]+)/g);
  const coordsSplStrs = match?.map((str) => str.split(" "));
  const coordsLatLng = coordsSplStrs?.map(([lngStr, latStr]) => ({
    lat: parseFloat(latStr),
    lng: parseFloat(lngStr),
  }));
  return coordsLatLng ? ({ type: "POLYGON", coords: coordsLatLng } as IMapPolygon) : undefined;
}
