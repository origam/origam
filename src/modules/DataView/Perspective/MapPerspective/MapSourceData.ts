import _ from "lodash";
import { computed } from "mobx";
import { IDataView } from "model/entities/types/IDataView";
import { getDataSourceFieldIndexByName } from "model/selectors/DataSources/getDataSourceFieldIndexByName";
import { getCellValueByIdx } from "model/selectors/TablePanelView/getCellValue";
import { parseGeoString } from "./helpers/geoStrings";
import { MapPerspectiveSetup } from "./MapPerspectiveSetup";

export enum IMapObjectType {
  POINT = "POINT",
  POLYGON = "POLYGON",
}

export interface IMapObjectBase {
  name: string;
  icon: string;
}

export interface IMapPoint extends IMapObjectBase {
  type: IMapObjectType.POINT;
  lat: number;
  lng: number;
}

export interface IMapPolygon extends IMapObjectBase {
  type: IMapObjectType.POLYGON;
  coords: { lat: number; lng: number }[];
}

export type IMapObject = IMapPoint | IMapPolygon;

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
        const parsedString = parseGeoString(row[this.fldLocationIndex]);
        if (parsedString)
          result.push({
            ...parsedString,
            name: row[this.fldNameIndex],
            icon: row[this.fldIconIndex],
          });
      }
    }
    return result;
  }
}
