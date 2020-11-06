import { action, computed, observable } from "mobx";
import { MapRootStore } from "./MapRootStore";

export class MapRoutefinderStore {
  constructor(private root: MapRootStore) {}

  get navigation() {
    return this.root.mapNavigationStore;
  }

  @observable isActive = false;

  @observable.shallow routeLatLngs: any[] = [];
  @observable.shallow driveThruPoints: any[] = [];

  @computed get mapObjectsRoute() {
    return [{ type: "LineString", coordinates: this.routeLatLngs.map((latLng) => latLng) }];
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
    console.log(results);
    this.routeLatLngs = results.geometry.map((coord: any) => [coord.x, coord.y]);
  }

  @action.bound handleResultsError() {
    console.log("Route lookup failed.");
  }

  @action.bound
  handleGeometryChange(obj: any) {
    console.log("Route object:", obj);
    if (obj) {
      const gjsCoords = obj.coordinates;
      this.driveThruPoints = gjsCoords;
      if (gjsCoords.length > 1) {
        const smapCoords = gjsCoords.map((ll: any) =>
          (window as any).SMap.Coords.fromWGS84(ll[0], ll[1])
        );
        const routeInstance = new (window as any).SMap.Route(smapCoords, (route: any) => {
          const results = route.getResults();
          if (results.error) {
            this.handleResultsError();
          } else {
            this.handleResultsReceived(results);
          }
          //polyline.setLatLngs(coords);
        });
      }
    }
  }
}
