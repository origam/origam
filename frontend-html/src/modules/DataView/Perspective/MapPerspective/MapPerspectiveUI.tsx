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

import cx from "classnames";
import L from "leaflet";
import "leaflet-draw";
import "leaflet-draw/dist/leaflet.draw-src.js";
import "leaflet-draw/dist/leaflet.draw-src.css";
import "leaflet/dist/leaflet.css";
import { action, autorun, computed, observable, reaction, runInAction } from "mobx";
import React from "react";
import S from "./MapPerspectiveUI.module.scss";
import { IMapObject, IMapObjectType } from "./stores/MapObjectsStore";
import { MapLayer } from "./stores/MapSetupStore";
import Measure, { ContentRect } from "react-measure";
import { ring as area } from "@mapbox/geojson-area";
import marker2xIcon from "leaflet/dist/images/marker-icon-2x.png";
import markerIcon from "leaflet/dist/images/marker-icon.png";
import markerShadow from "leaflet/dist/images/marker-shadow.png";
import { flashColor2htmlColor } from "utils/flashColorFormat";

delete (L.Icon.Default.prototype as any)._getIconUrl;

L.Icon.Default.mergeOptions({
  iconRetinaUrl: marker2xIcon,
  iconUrl: markerIcon,
  shadowUrl: markerShadow,
});

interface IMapPerspectiveComProps {
  mapCenter: { type: "Point"; coordinates: [number, number] } | undefined;
  initialZoom: number | undefined;
  getMapObjects: () => IMapObject[];
  getRoutefinderRoute: () => any[];
  getRoutefinderEditables: () => any[];
  lastDetailedObject?: IMapObject;
  mapLayers: MapLayer[];
  isReadOnly: boolean;
  isActive: boolean;

  onChange?(geoJson: any): void;

  onLayerClick?(id: string): void;

  onRoutefinderGeometryChange?(obj: any): void;

  onRoutefinderGeometryEditStart?(): void;

  onRoutefinderGeometryEditSave?(): void;

  onRoutefinderGeometryEditCancel?(): void;
}

const MAP_ANIMATE_SETTING = {
  animate: true,
  duration: 1.1,
};

function getOptionsEPSG(options: any) {
  switch (options.crs?.toUpperCase()) {
    case "EPSG4326":
      return L.CRS.EPSG4326;
  }
  return undefined;
}

export class MapPerspectiveCom extends React.Component<IMapPerspectiveComProps> {
  elmMapDiv: HTMLDivElement | null = null;
  refMapDiv = (elm: any) => (this.elmMapDiv = elm);

  leafletMap?: L.DrawMap;

  leafletMapObjects = new L.FeatureGroup();
  leafletMapRoute = new L.FeatureGroup();

  editingObjectsRepaintDisabled = false;

  _disposers: any[] = [];

  panToCenter() {
    if (this.props.mapCenter) {
      this.leafletMap?.panTo(
        [this.props.mapCenter.coordinates[1], this.props.mapCenter.coordinates[0]],
        {...MAP_ANIMATE_SETTING}
      );
    } else {
      this.leafletMap?.panTo([0, 0], {...MAP_ANIMATE_SETTING});
    }
  }

  panToLoc(loc: [number, number]) {
    this.leafletMap?.panTo(loc, {...MAP_ANIMATE_SETTING});
  }

  panToSelectedObject() {
    for (let [obj, lLayer] of this.mapDrawnObjectLayers) {
      if (obj.id === this.props.lastDetailedObject?.id) {
        this.panToLayer(lLayer);
        return;
      }
    }
  }

  panToLayer(lLayer: L.Layer) {
    if ((lLayer as any).getBounds) {
      const bounds = (lLayer as any).getBounds() as L.LatLngBounds;
      this.leafletMap?.fitBounds(bounds.pad(0.1), {...MAP_ANIMATE_SETTING});
    } else if ((lLayer as any).getLatLng) {
      const latLng = (lLayer as any).getLatLng();
      this.leafletMap?.panTo(latLng, {...MAP_ANIMATE_SETTING});
    }
  }

  panToFirstObject() {
    if (this.mapDrawnObjectLayers.length > 0) {
      this.panToLayer(this.mapDrawnObjectLayers[0][1]);
    }
  }

  isPropMapCenterDifferent(prevProps: IMapPerspectiveComProps) {
    return (
      this.props.mapCenter?.coordinates[0] !== prevProps.mapCenter?.coordinates[0] ||
      this.props.mapCenter?.coordinates[1] !== prevProps.mapCenter?.coordinates[1]
    );
  }

  isPropActiveDifferent(prevProps: IMapPerspectiveComProps) {
    return this.props.isActive !== prevProps.isActive;
  }

  componentDidUpdate(prevProps: IMapPerspectiveComProps) {
    runInAction(() => {
      if (this.isPropActiveDifferent(prevProps)) {
        if (this.props.isActive) {
          this.mountLeaflet();
        } else {
          //this.unmountLeaflet();
        }
      }
      if (this.isPropMapCenterDifferent(prevProps)) {
        this.panToCenter();
      }
      /*const { lastDetailedObject } = this.props;
      if (
        lastDetailedObject &&
        (!prevProps.lastDetailedObject ||
          !_.isEqual(lastDetailedObject, prevProps.lastDetailedObject))
      ) {
        this.panToSelectedObject();
        this.highlightSelectedLayer();
      }
      if (!lastDetailedObject && prevProps.lastDetailedObject) {
        this.highlightSelectedLayer();
      }*/
    });
  }

  highlightLayer(obj: IMapObject, lLayer: L.Layer) {
    if ((lLayer as any).setStyle) {
      (lLayer as any).setStyle({color: "yellow"});
    }
    if ((lLayer as any).setIcon) {
      (lLayer as any).setIcon(
        L.divIcon({
          ...(lLayer as any).getIcon().options,
          className: "markerHighlighted",
        })
      );
    }
  }

  unHighlightLayer(obj: IMapObject, lLayer: L.Layer) {
    if ((lLayer as any).setStyle) {
      (lLayer as any).setStyle({
        color:
          obj.color !== undefined && obj.color !== 0 && obj.color !== null
            ? flashColor2htmlColor(obj.color)
            : "blue",
      });
    }
    if ((lLayer as any).setIcon) {
      (lLayer as any).setIcon(
        L.divIcon({
          ...(lLayer as any).getIcon().options,
          className: "",
        })
      );
    }
  }

  highlightSelectedLayer() {
    for (let [obj, lLayer] of this.mapDrawnObjectLayers) {
      if (obj.id === this.props.lastDetailedObject?.id) {
        this.highlightLayer(obj, lLayer);
      } else {
        this.unHighlightLayer(obj, lLayer);
      }
    }
  }

  @computed get layerList() {
    return this.props.mapLayers
      .map((rawLayer, index) => {
        if (rawLayer.type === "OSM") {
          return [
            rawLayer,
            L.tileLayer(rawLayer.getUrl(), {...rawLayer.getOptions(), zIndex: index}),
          ];
        } else if (rawLayer.type === "WMS") {
          return [
            rawLayer,
            L.tileLayer.wms(rawLayer.getUrl(), {
              ...rawLayer.getOptions(),
              zIndex: index,
              crs: getOptionsEPSG(rawLayer.getOptions()),
            }),
          ];
        }
        return undefined;
      })
      .filter((layer) => layer) as [MapLayer, L.TileLayer][];
  }

  @computed get leafletlayersDescriptor() {
    return Object.fromEntries(
      this.layerList.map((layer) => {
        return [layer[0].getTitle(), layer[1]];
      })
    );
  }

  @computed get mapDrawnObjectLayers() {
    return this.props
      .getMapObjects()
      .map((obj) => {
        let result: [IMapObject, L.Layer];
        switch (obj.type) {
          case IMapObjectType.POINT: {
            const iconUrl = obj.icon || "img/map/marker-icon.png#anchor=[12,41]";
            const urlParams = new URLSearchParams(iconUrl.split("#")[1] || "");
            const urlQuery: {[key: string]: any} = {};
            for (let key of urlParams.keys()){
              urlQuery[key] = urlParams.get(key);
            }
            const pq = iconUrl ? urlQuery : null;
            const anchor = pq?.anchor ? JSON.parse(pq.anchor as string) : [0, 0];
            const iconAnchor: [number, number] = anchor;
            const iconRotation = obj.azimuth || 0;
            const myIcon = L.divIcon({
              html: `<img src="${iconUrl}" style="
                transform: rotate3d(0,0,1,${iconRotation}deg);
                transform-origin: ${iconAnchor[0]}px ${iconAnchor[1]}px;" />`,
              // iconSize: [38, 95],
              iconAnchor,
              className: "",
              //popupAnchor: [-3, -76],
            });
            result = [
              obj,
              L.marker([obj.coordinates[1], obj.coordinates[0]], {
                icon: myIcon,
              }).bindTooltip(obj.name),
            ];
          }
            break;
          case IMapObjectType.POLYGON:
            result = [
              obj,
              L.polygon(
                obj.coordinates[0].map((coords) => [coords[1], coords[0]]),
                {
                  color:
                    obj.color !== undefined && obj.color !== 0 && obj.color !== null
                      ? flashColor2htmlColor(obj.color)
                      : "blue",
                  weight: 2
                }
              ).bindTooltip(obj.name),
            ];
            break;
          case IMapObjectType.LINESTRING:
            result = [
              obj,
              L.polyline(
                obj.coordinates.map((coords) => [coords[1], coords[0]]),
                {
                  color:
                    obj.color !== undefined && obj.color !== 0 && obj.color !== null
                      ? flashColor2htmlColor(obj.color)
                      : "blue",
                  weight: 2
                }
              ).bindTooltip(obj.name),
            ];
            break;
        }
        return result;
      })
      .filter((layer) => layer) as [IMapObject, L.Layer][];
  }

  @computed get mapRoutefinderRoute() {
    return this.props
      .getRoutefinderRoute()
      .map((obj) => {
        switch (obj.type) {
          case "LineString":
            return L.polyline(
              obj.coordinates.map((coords: any) => [coords[1], coords[0]]),
              {color: "blue"}
            );
        }
        return undefined;
      })
      .filter((obj) => obj);
  }

  @computed get mapRoutefinderEditables() {
    return this.props
      .getRoutefinderEditables()
      .map((obj) => {
        switch (obj.type) {
          case "LineString":
            return L.polyline(
              obj.coordinates.map((coords: any) => [coords[1], coords[0]]),
              {color: "green", dashArray: "10 5 3 5", opacity: 1.0, weight: 1.5}
            );
        }
        return undefined;
      })
      .filter((obj) => obj);
  }

  @action.bound handleOjectCreated(event: any) {
    const layer = event.layer;
    this.leafletMapObjects.clearLayers();
    this.leafletMapObjects.addLayer(layer);
    const geoJSON = layer.toGeoJSON();
    const geometry = geoJSON.geometry.type === "Polygon"
      ? this.getCounterClockWisePolygonPoints(geoJSON.geometry)
      : geoJSON.geometry;
    this.props.onChange?.(geometry);
  }

  getCounterClockWisePolygonPoints(polygonGeometry: any){
    if(!polygonGeometry || polygonGeometry.coordinates?.length !== 1){
      // eslint-disable-next-line no-console
      console.warn(`Failed to check polygonGeometry:`);
      // eslint-disable-next-line no-console
      console.warn(polygonGeometry);
      return polygonGeometry;
    }
    const polygonArea = area(polygonGeometry.coordinates[0]);
    if(polygonArea > 0) {
      return {
        type: "Polygon",
        coordinates: [polygonGeometry.coordinates[0].reverse()]
      };
    }
    return polygonGeometry
  }

  @action.bound handleObjectEdited(event: any) {
    const layers = (event as any).layers;
    const obj = layers.toGeoJSON().features?.[0]?.geometry;
    if (obj) {
      this.props.onChange?.(obj);
    }
  }

  @action.bound handleObjectDeleted(event: any) {
    this.props.onChange?.(undefined);
  }

  @action.bound handleRoutefinderOjectCreated(event: any) {
    const layer = event.layer;
    this.leafletMapObjects.clearLayers();
    this.leafletMapObjects.addLayer(layer);
    const obj = (this.leafletMapObjects as any).toGeoJSON().features?.[0]?.geometry;
    this.routefinderUpdate(obj);
  }

  @action.bound handleRoutefinderObjectEdited(event: any) {
    const obj = (this.leafletMapObjects as any).toGeoJSON().features?.[0]?.geometry;
    this.routefinderUpdate(obj);
  }

  @action.bound handleRoutefinderObjectDeleted(event: any) {
    this.routefinderUpdate(undefined);
  }

  @action.bound handleRoutefinderVertexDrawn(event: any) {
    const obj = {
      type: "LineString",
      coordinates: event.layers
        .getLayers()
        .map((layer: any) => [layer.getLatLng().lng, layer.getLatLng().lat]),
    };
    this.routefinderUpdate(obj);
  }

  @action.bound handleRoutefinderVertexEdited(event: any) {
    const obj = (this.leafletMapObjects as any).toGeoJSON().features?.[0]?.geometry;
    this.routefinderUpdate(obj);
  }

  @action.bound routefinderUpdate(obj: any) {
    this.props.onRoutefinderGeometryChange?.(obj);
  }

  @action.bound handleRoutefinderEditStart() {
    this.editingObjectsRepaintDisabled = true;
    this.props.onRoutefinderGeometryEditStart?.();
  }

  @action.bound handleRoutefinderDrawStart() {
    this.handleRoutefinderEditStart();
  }

  _isCancelAction = false;

  @action.bound handleRoutefinderEditStop() {
    this.editingObjectsRepaintDisabled = false;
    if (this._isCancelAction) {
      this.props.onRoutefinderGeometryEditCancel?.();
    } else {
      this.props.onRoutefinderGeometryEditSave?.();
    }
  }

  @action.bound handleRoutefinderDrawStop() {
    this.handleRoutefinderEditStop();
  }

  setMapControlCancelHack() {
    /*
      This is need to distinguish between Finish and Cancel button click.
    */
    const self = this;

    const edit = (this.mapControl! as any)._toolbars.edit;
    const oldEditDisable = edit.disable;
    (this.mapControl! as any)._toolbars.edit.disable = function () {
      self._isCancelAction = true;
      oldEditDisable.apply(edit, arguments);
      self._isCancelAction = false;
    };

    const draw = (this.mapControl! as any)._toolbars.draw;
    const oldDrawDisable = draw.disable;
    (this.mapControl! as any)._toolbars.draw.disable = function () {
      self._isCancelAction = true;
      oldDrawDisable.apply(draw, arguments);
      self._isCancelAction = false;
    };
  }

  initLeafletDrawControls() {
    this.activateNormalControls();
  }

  mapControl: L.Control.Draw | undefined;
  mapControlDisposers: Array<() => void> = [];

  activateRoutingControls() {
    for (let h of this.mapControlDisposers) h();
    this.mapControlDisposers = [];
    this.leafletMapObjects.clearLayers();
    this.leafletMapRoute.clearLayers();
    this.leafletMap?.addControl(
      (this.mapControl = new L.Control.Draw({
        edit: {
          featureGroup: this.leafletMapObjects,
          poly: {
            allowIntersection: true,
          },
          circle: false,
          circlemarker: false,
        },
        draw: {
          polygon: false,
          point: false,
          marker: false,
          circle: false,
          circlemarker: false,
          rectangle: false,
        },
      } as any)) // 🦄
    );

    this.setMapControlCancelHack();

    this.leafletMap?.on(L.Draw.Event.CREATED, this.handleRoutefinderOjectCreated);
    this.leafletMap?.on(L.Draw.Event.EDITED, this.handleRoutefinderObjectEdited);
    this.leafletMap?.on(L.Draw.Event.DELETED, this.handleRoutefinderObjectDeleted);
    this.leafletMap?.on(L.Draw.Event.DRAWVERTEX, this.handleRoutefinderVertexDrawn);
    this.leafletMap?.on(L.Draw.Event.EDITVERTEX, this.handleRoutefinderVertexEdited);

    this.leafletMap?.on(L.Draw.Event.EDITSTART, this.handleRoutefinderEditStart);
    this.leafletMap?.on(L.Draw.Event.EDITSTOP, this.handleRoutefinderEditStop);
    this.leafletMap?.on(L.Draw.Event.DRAWSTART, this.handleRoutefinderDrawStart);
    this.leafletMap?.on(L.Draw.Event.DRAWSTOP, this.handleRoutefinderDrawStop);

    this.mapControlDisposers.push(() => {
      this.leafletMap?.off(L.Draw.Event.CREATED, this.handleRoutefinderOjectCreated);
      this.leafletMap?.off(L.Draw.Event.EDITED, this.handleRoutefinderObjectEdited);
      this.leafletMap?.off(L.Draw.Event.DELETED, this.handleRoutefinderObjectDeleted);
      this.leafletMap?.off(L.Draw.Event.DRAWVERTEX, this.handleRoutefinderVertexDrawn);
      this.leafletMap?.off(L.Draw.Event.EDITVERTEX, this.handleRoutefinderVertexEdited);

      this.leafletMap?.off(L.Draw.Event.EDITSTART, this.handleRoutefinderEditStart);
      this.leafletMap?.off(L.Draw.Event.EDITSTOP, this.handleRoutefinderEditStop);
      this.leafletMap?.off(L.Draw.Event.DRAWSTART, this.handleRoutefinderDrawStart);
      this.leafletMap?.off(L.Draw.Event.DRAWSTOP, this.handleRoutefinderDrawStop);

      if (this.mapControl) this.leafletMap?.removeControl(this.mapControl);
      this.mapControl = undefined;
    });
  }

  activateNormalControls() {
    for (let h of this.mapControlDisposers) h();
    this.mapControlDisposers = [];
    this.leafletMapObjects.clearLayers();
    this.leafletMapRoute.clearLayers();
    this.leafletMap?.addControl(
      (this.mapControl = new L.Control.Draw({
        edit: {
          featureGroup: this.leafletMapObjects,
          poly: {
            allowIntersection: true,
            icon: new L.DivIcon({
              iconSize: new L.Point(8, 8),
              className: 'leaflet-div-icon leaflet-editing-icon'
            }),
            touchIcon: new L.DivIcon({
              iconSize: new L.Point(8, 8),
              className: 'leaflet-div-icon leaflet-editing-icon leaflet-touch-icon'
            }), 
          },
          circle: false,
          circlemarker: false,
        },
        draw: {
          polygon: {
            allowIntersection: false,
            showArea: true,
            icon: new L.DivIcon({
              iconSize: new L.Point(8, 8),
              className: 'leaflet-div-icon leaflet-editing-icon'
            }),
            touchIcon: new L.DivIcon({
              iconSize: new L.Point(8, 8),
              className: 'leaflet-div-icon leaflet-editing-icon leaflet-touch-icon'
            }), 
            shapeOptions: {
              weight: 2
            }
          },
          circle: false,
          circlemarker: false,
        },
      } as any)) // 🦄
    );
    this.leafletMap?.on(L.Draw.Event.CREATED, this.handleOjectCreated);
    this.leafletMap?.on(L.Draw.Event.EDITED, this.handleObjectEdited);
    this.leafletMap?.on(L.Draw.Event.DELETED, this.handleObjectDeleted);
    this.mapControlDisposers.push(() => {
      this.leafletMap?.off(L.Draw.Event.CREATED, this.handleOjectCreated);
      this.leafletMap?.off(L.Draw.Event.EDITED, this.handleObjectEdited);
      this.leafletMap?.off(L.Draw.Event.DELETED, this.handleObjectDeleted);
      if (this.mapControl) this.leafletMap?.removeControl(this.mapControl);
      this.mapControl = undefined;
    });
  }

  mapFittedToLayers = false;

  lmap: L.DrawMap | undefined;

  initLeaflet() {
    this.lmap = L.map(this.elmMapDiv!, {
      layers: this.layerList
        .filter(([rawLayer, leaLayer]) => rawLayer.defaultEnabled)
        .map(([rawLayer, leaLayer]) => leaLayer),
    });
    this.leafletMap = this.lmap;
    this.lmap.setZoom(this.props.initialZoom || 0);

    const minZooms = this.layerList
      .map(([rawLayer, tileLayer]) => tileLayer.options.minZoom ?? 0);
    const leastZoom = Math.min(...minZooms);
    this.lmap.setMinZoom(leastZoom);

    const maxZooms = this.layerList
      .map(([rawLayer, tileLayer]) => tileLayer.options.maxNativeZoom ?? 18);
    const maxZoom = Math.max(...maxZooms);
    this.lmap.setMaxZoom(maxZoom);

    this.panToCenter();
    L.control
      .layers({}, this.leafletlayersDescriptor, {position: "topleft", collapsed: true})
      .addTo(this.lmap);
    L.control.scale().addTo(this.lmap);

    this.lmap.addLayer(this.leafletMapObjects);
    this.lmap.addLayer(this.leafletMapRoute);

    this._disposers.push(
      reaction(
        () => this.mapDrawnObjectLayers,
        (layers) => {
          this.leafletMapObjects.clearLayers();
          let allLayerBounds = L.latLngBounds([]);
          for (let layer of layers) {
            this.leafletMapObjects.addLayer(layer[1]);
            if ((layer[1] as any).getBounds) {
              allLayerBounds.extend((layer[1] as any).getBounds());
            } else if ((layer[1] as any).getLatLng) {
              allLayerBounds.extend((layer[1] as any).getLatLng());
            }
            layer[1].on("click", () => this.props.onLayerClick?.(layer[0].id));
          }
          if (!this.props.mapCenter && allLayerBounds.isValid() && !this.mapFittedToLayers) {
            allLayerBounds = allLayerBounds.pad(0.1);
            const mapCenter = allLayerBounds.getCenter();
            this.lmap!.panTo(mapCenter);
            this.mapFittedToLayers = true;
          }
          this.highlightSelectedLayer();
          if(layers.length === 1){
            this.panToFirstObject();
          }
        },
        {
          delay: 100,
          fireImmediately: true,
        }
      ),
      reaction(
        () => this.mapRoutefinderRoute,
        (layers) => {
          this.leafletMapRoute.clearLayers();
          for (let layer of layers) {
            this.leafletMapRoute.addLayer(layer!);
          }
        }
      ),
      reaction(
        () => this.mapRoutefinderEditables,
        (layers) => {
          if (this.editingObjectsRepaintDisabled) return;
          this.leafletMapObjects.clearLayers();
          for (let layer of layers) {
            this.leafletMapObjects.addLayer(layer!);
          }
        }
      )
    );
  }

  _isMounted = false;

  mountLeaflet() {
    if (!this._isMounted) {
      this.initLeaflet();
      if (!this.props.isReadOnly) {
        this.initLeafletDrawControls();
      }
      this._isMounted = true;
    }
  }

  componentDidMount() {
    autorun(
      () => {
        if (
          (this.contentRect?.bounds?.width || 0) > 40 &&
          (this.contentRect?.bounds?.height || 0) > 40 &&
          this.props.isActive
        ) {
          this.mountLeaflet();
        }
      },
      {delay: 500}
    );
  }

  componentWillUnmount() {
    this._disposers.forEach((d) => d());
  }

  @observable.ref contentRect?: ContentRect;

  @action.bound
  handleResize(rect: ContentRect) {
    this.contentRect = rect;
    this.lmap?.invalidateSize();
  }

  render() {
    return (
      <Measure bounds={true} onResize={this.handleResize}>
        {({measureRef}) => (
          <div ref={measureRef} style={{width: "100%", height: "100%"}}>
            <div
              className={cx(S.mapDiv, {isHidden: !this.props.isActive})}
              ref={(elm) => {
                this.refMapDiv(elm);
              }}
            ></div>
          </div>
        )}
      </Measure>
    );
  }
}
