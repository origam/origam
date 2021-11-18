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

import { action, computed, observable } from "mobx";
import { MapRootStore } from "./MapRootStore";

export class MapRoutefinderStore {
  constructor(private root: MapRootStore) {
  }

  get navigation() {
    return this.root.mapNavigationStore;
  }

  get objects() {
    return this.root.mapObjectsStore;
  }

  @observable isActive = false;

  @observable.shallow routeLatLngs: any[] = [];
  @observable.shallow driveThruPointsEditing: any[] = [];
  @observable.shallow driveThruPoints: any[] = [];

  @computed get mapObjectsRoute() {
    return [{type: "LineString", coordinates: this.routeLatLngs.map((latLng) => latLng)}];
  }

  @computed get mapObjectsEditable() {
    return [
      {
        type: "LineString",
        coordinates: this.driveThruPoints.map((latLng) => latLng),
      },
    ];
  }

  @action.bound
  handleRoutefinderButtonClick(event: any) {
    if (!this.isActive) {
      this.isActive = true;
      this.navigation.activateRoutingControls();
    } else {
      this.isActive = false;
      this.navigation.activateNormalControls();
    }
  }

  @action.bound handleResultsReceived(results: any) {
    this.routeLatLngs = results.geometry.map((coord: any) => [coord.x, coord.y]);
  }

  @action.bound handleEditingFinished() {
    this.driveThruPoints = this.driveThruPointsEditing;
    if (this.routeLatLngs.length > 0) {
      this.objects.handleGeometryChange(this.mapObjectsRoute[0]);
    }
  }

  @action.bound handleEditingCancelled() {
    this.driveThruPointsEditing = [];
    if (this.driveThruPoints.length > 1) {
      this.calculateRoute(this.driveThruPoints);
    } else {
      this.routeLatLngs = [];
    }
  }

  @action.bound handleEditingStarted() {
    this.driveThruPointsEditing = this.driveThruPoints;
  }

  @action.bound handleResultsError() {
    console.error("Route lookup failed."); // eslint-disable-line no-console
  }

  @action.bound
  handleGeometryChange(obj: any) {
    if (obj) {
      const gjsCoords = obj.coordinates;
      this.driveThruPointsEditing = gjsCoords;
      this.calculateRoute(gjsCoords);
    }
  }

  @action.bound calculateRoute(gjsCoords: any) {
    if (gjsCoords.length > 1) {
      const smapCoords = gjsCoords.map((ll: any) =>
        (window as any).SMap.Coords.fromWGS84(ll[0], ll[1])
      );
      new (window as any).SMap.Route(smapCoords, (route: any) => {
        const results = route.getResults();
        if (results.error) {
          this.handleResultsError();
        } else {
          this.handleResultsReceived(results);
        }
      });
    }
  }
}
