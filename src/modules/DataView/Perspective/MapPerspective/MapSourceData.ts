import _ from "lodash";
import { computed } from "mobx";
import { IDataView } from "model/entities/types/IDataView";
import { getDataSourceFieldIndexByName } from "model/selectors/DataSources/getDataSourceFieldIndexByName";
import { MapPerspectiveSetup } from "./MapPerspectiveSetup";
import { parse as wktParse } from "wkt";

export enum IMapObjectType {
  POINT = "Point",
  POLYGON = "Polygon",
  POLYLINE = "Polyline"
}

export interface IMapObjectBase {
  name: string;
  icon: string;
}

export interface IMapPoint extends IMapObjectBase {
  type: IMapObjectType.POINT;
  coordinates: [number, number];
}

export interface IMapPolygon extends IMapObjectBase {
  type: IMapObjectType.POLYGON;
  coordinates: [number, number][];
}

export interface IMapPolyline extends IMapObjectBase {
  type: IMapObjectType.POLYLINE;
  coordinates: [number, number][];
}

export type IMapObject = IMapPoint | IMapPolygon | IMapPolyline;

export class MapSourceData {
  constructor(public dataView: IDataView, public setup: MapPerspectiveSetup) {}

  @computed get fldLocationIndex() {
    return getDataSourceFieldIndexByName(this.dataView, this.setup.mapLocationMember);
  }

  @computed get fldNameIndex() {
    return getDataSourceFieldIndexByName(this.dataView, this.setup.mapTextMember);
  }

  @computed get fldIconIndex() {
    return getDataSourceFieldIndexByName(this.dataView, this.setup.mapIconMember);
  }

  @computed
  get mapObjects() {
    const tableRows = this.dataView.tableRows;
    const result: IMapObject[] = [];
    for (let row of tableRows) {
      if (_.isArray(row)) {
        const objectGeoJson = wktParse(row[this.fldLocationIndex]);
        if (objectGeoJson)
          result.push({
            ...objectGeoJson,
            name: row[this.fldNameIndex],
            icon: row[this.fldIconIndex],
          });
      }
    }
    return result;
  }
}
