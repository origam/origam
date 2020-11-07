import cx from "classnames";
import L from "leaflet";
import "leaflet-draw/dist/leaflet.draw-src.js";
import "leaflet-draw/dist/leaflet.draw-src.css";
import "leaflet/dist/leaflet.css";
import _ from "lodash";
import { action, computed, reaction, runInAction } from "mobx";
import qs from "querystring";
import React from "react";
import S from "./MapPerspectiveUI.module.scss";
import { IMapObject, IMapObjectType } from "./stores/MapObjectsStore";
import { MapLayer } from "./stores/MapSetupStore";

delete (L.Icon.Default.prototype as any)._getIconUrl;

L.Icon.Default.mergeOptions({
  iconRetinaUrl: require("leaflet/dist/images/marker-icon-2x.png"),
  iconUrl: require("leaflet/dist/images/marker-icon.png"),
  shadowUrl: require("leaflet/dist/images/marker-shadow.png"),
});

interface IMapPerspectiveComProps {
  mapCenter: { type: "Point"; coordinates: [number, number] };
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

export class MapPerspectiveCom extends React.Component<IMapPerspectiveComProps> {
  elmMapDiv: HTMLDivElement | null = null;
  refMapDiv = (elm: any) => (this.elmMapDiv = elm);

  leafletMap?: L.DrawMap;

  leafletMapObjects = new L.FeatureGroup();
  leafletMapRoute = new L.FeatureGroup();

  editingObjectsRepaintDisabled = false;

  _disposers: any[] = [];

  constructor(props: IMapPerspectiveComProps) {
    super(props);
  }

  panToCenter() {
    this.leafletMap?.panTo(
      [this.props.mapCenter.coordinates[1], this.props.mapCenter.coordinates[0]],
      { ...MAP_ANIMATE_SETTING }
    );
  }

  panToLoc(loc: [number, number]) {
    this.leafletMap?.panTo(loc, { ...MAP_ANIMATE_SETTING });
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
      this.leafletMap?.fitBounds(bounds.pad(0.1), { ...MAP_ANIMATE_SETTING });
    } else if ((lLayer as any).getLatLng) {
      const latLng = (lLayer as any).getLatLng();
      this.leafletMap?.panTo(latLng, { ...MAP_ANIMATE_SETTING });
    }
  }

  panToFirstObject() {
    if (this.mapDrawnObjectLayers.length > 0) {
      this.panToLayer(this.mapDrawnObjectLayers[0][1]);
    }
  }

  isPropMapCenterDifferent(prevProps: IMapPerspectiveComProps) {
    return (
      this.props.mapCenter.coordinates[0] !== prevProps.mapCenter.coordinates[0] ||
      this.props.mapCenter.coordinates[1] !== prevProps.mapCenter.coordinates[1]
    );
  }

  componentDidUpdate(prevProps: IMapPerspectiveComProps) {
    runInAction(() => {
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

  highlightLayer(lLayer: L.Layer) {
    if ((lLayer as any).setStyle) {
      (lLayer as any).setStyle({ color: "yellow" });
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

  unHighlightLayer(lLayer: L.Layer) {
    if ((lLayer as any).setStyle) {
      (lLayer as any).setStyle({ color: "blue" });
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
        this.highlightLayer(lLayer);
      } else {
        this.unHighlightLayer(lLayer);
      }
    }
  }

  @computed get layerList() {
    return this.props.mapLayers
      .map((rawLayer, index) => {
        if (rawLayer.type === "OSM") {
          return [
            rawLayer,
            L.tileLayer(rawLayer.getUrl(), { ...rawLayer.getOptions(), zIndex: index }),
          ];
        } else if (rawLayer.type === "WMS") {
          return [
            rawLayer,
            L.tileLayer.wms(rawLayer.getUrl(), { ...rawLayer.getOptions(), zIndex: index }),
          ];
        }
      })
      .filter((layer) => layer) as [MapLayer, L.TileLayer][];
  }

  @computed get leafletlayersDescriptor() {
    console.log(this.layerList);
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
          case IMapObjectType.POINT:
            {
              const iconUrl = obj.icon || "img/map/marker-icon.png#anchor=[12,41]";
              const pq = iconUrl ? qs.parse(iconUrl.split("#")[1] || "") : null;
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
            {
              result = [
                obj,
                L.polygon(
                  obj.coordinates[0].map((coords) => [coords[1], coords[0]]),
                  { color: "blue" }
                ).bindTooltip(obj.name),
              ];
            }
            break;
          case IMapObjectType.LINESTRING:
            {
              result = [
                obj,
                L.polyline(
                  obj.coordinates.map((coords) => [coords[1], coords[0]]),
                  { color: "blue" }
                ).bindTooltip(obj.name),
              ];
            }
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
        console.log(obj);
        switch (obj.type) {
          case "LineString":
            {
              return L.polyline(
                obj.coordinates.map((coords: any) => [coords[1], coords[0]]),
                { color: "blue" }
              );
            }
            break;
        }
      })
      .filter((obj) => obj);
  }

  @computed get mapRoutefinderEditables() {
    return this.props
      .getRoutefinderEditables()
      .map((obj) => {
        console.log(obj);
        switch (obj.type) {
          case "LineString":
            {
              return L.polyline(
                obj.coordinates.map((coords: any) => [coords[1], coords[0]]),
                { color: "green", dashArray: "10 5 3 5", opacity: 1.0, weight: 1.5 }
              );
            }
            break;
        }
      })
      .filter((obj) => obj);
  }

  @action.bound handleOjectCreated(event: any) {
    const layer = event.layer;
    this.leafletMapObjects.clearLayers();
    this.leafletMapObjects.addLayer(layer);
    const obj = layer.toGeoJSON().geometry;
    this.props.onChange?.(obj);
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
    console.log("Object created");
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
    console.log("Vertex drawn");
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
          },
          circle: false,
          circlemarker: false,
        },
        draw: {
          polygon: {
            allowIntersection: false,
            showArea: true,
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

  initLeaflet() {
    const lmap = L.map(this.elmMapDiv!, {
      layers: this.layerList
        .filter(([rawLayer, leaLayer]) => rawLayer.defaultEnabled)
        .map(([rawLayer, leaLayer]) => leaLayer),
    });
    this.leafletMap = lmap;
    lmap.setZoom(15);
    this.panToCenter();
    L.control
      .layers({}, this.leafletlayersDescriptor, { position: "topleft", collapsed: true })
      .addTo(lmap);
    L.control.scale().addTo(lmap);

    lmap.addLayer(this.leafletMapObjects);
    lmap.addLayer(this.leafletMapRoute);

    this._disposers.push(
      reaction(
        () => this.mapDrawnObjectLayers,
        (layers) => {
          console.log("Drawing layers", layers);
          this.leafletMapObjects.clearLayers();
          for (let layer of layers) {
            this.leafletMapObjects.addLayer(layer[1]);
            layer[1].on("click", () => this.props.onLayerClick?.(layer[0].id));
          }
          this.highlightSelectedLayer();
        },
        {
          delay: 100,
          fireImmediately: true,
        }
      ),
      reaction(
        () => this.mapRoutefinderRoute,
        (layers) => {
          console.log(layers);
          this.leafletMapRoute.clearLayers();
          for (let layer of layers) {
            this.leafletMapRoute.addLayer(layer!);
          }
        }
      ),
      reaction(
        () => this.mapRoutefinderEditables,
        (layers) => {
          console.log(layers);
          if (this.editingObjectsRepaintDisabled) return;
          this.leafletMapObjects.clearLayers();
          for (let layer of layers) {
            this.leafletMapObjects.addLayer(layer!);
          }
        }
      )
    );
  }

  componentDidMount() {
    this.initLeaflet();
    if (!this.props.isReadOnly) {
      this.initLeafletDrawControls();
    }
  }

  componentWillUnmount() {
    this._disposers.forEach((d) => d());
  }

  render() {
    return (
      <div className={cx(S.mapDiv, { isHidden: !this.props.isActive })} ref={this.refMapDiv}></div>
    );
  }
}
