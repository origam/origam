import React from "react";
import L from "leaflet";
import "leaflet-draw";
import "leaflet-draw/dist/leaflet.draw-src.css";
import "leaflet/dist/leaflet.css";

import S from "./MapPerspectiveUI.module.scss";
import cx from "classnames";
import { IMapObject, IMapObjectType, MapSourceData } from "./MapSourceData";
import { reaction } from "mobx";
import { MapPerspectiveSetup } from "./MapPerspectiveSetup";

delete (L.Icon.Default.prototype as any)._getIconUrl;

L.Icon.Default.mergeOptions({
  iconRetinaUrl: require("leaflet/dist/images/marker-icon-2x.png"),
  iconUrl: require("leaflet/dist/images/marker-icon.png"),
  shadowUrl: require("leaflet/dist/images/marker-shadow.png"),
});

interface IMapPerspectiveComProps {
  mapCenter: { type: "Point"; coordinates: [number, number] };
  mapSourceData: MapSourceData;
  isReadOnly: boolean;
  isActive: boolean;
  onChange?(geoJson: any): void;
}

export class MapPerspectiveCom extends React.Component<IMapPerspectiveComProps> {
  elmMapDiv: HTMLDivElement | null = null;
  refMapDiv = (elm: any) => (this.elmMapDiv = elm);

  leafletMap?: L.DrawMap;

  leafletMapObjects = new L.FeatureGroup();

  _disposers: any[] = [];

  constructor(props: IMapPerspectiveComProps) {
    super(props);
  }

  panToCenter() {
    this.leafletMap?.panTo([
      this.props.mapCenter.coordinates[1],
      this.props.mapCenter.coordinates[0],
    ]);
  }

  isPropMapCenterDifferent(prevProps: IMapPerspectiveComProps) {
    return (
      this.props.mapCenter.coordinates[0] !== prevProps.mapCenter.coordinates[0] ||
      this.props.mapCenter.coordinates[1] !== prevProps.mapCenter.coordinates[1]
    );
  }

  componentDidUpdate(prevProps: IMapPerspectiveComProps) {
    if (this.isPropMapCenterDifferent(prevProps)) {
      this.panToCenter();
    }
  }

  initLeafletDrawControls() {
    this.leafletMap?.addControl(
      new L.Control.Draw({
        edit: {
          featureGroup: this.leafletMapObjects,
          poly: {
            allowIntersection: true,
          },
        },
        draw: {
          polygon: {
            allowIntersection: false,
            showArea: true,
          },
        },
      } as any) // 🦄
    );

    this.leafletMap?.on(L.Draw.Event.CREATED, (event) => {
      const layer = event.layer;
      this.leafletMapObjects.clearLayers();
      this.leafletMapObjects.addLayer(layer);
      console.log(layer.toGeoJSON());
      const obj = layer.toGeoJSON().geometry;
      this.props.onChange?.(obj);
    });

    this.leafletMap?.on(L.Draw.Event.EDITED, (event) => {
      const layers = (event as any).layers;
      console.log(layers.toGeoJSON());
      const obj = layers.toGeoJSON().features?.[0]?.geometry;
      console.log(obj);
      if (obj) {
        this.props.onChange?.(obj);
      }
    });

    this.leafletMap?.on(L.Draw.Event.DELETED, (event) => {
      this.props.onChange?.(undefined);
    });
  }

  initLeaflet() {
    const osmUrl = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
    const osmAttrib =
      '&copy; <a href="http://openstreetmap.org/copyright">OpenStreetMap</a> contributors';
    const lmap = L.map(this.elmMapDiv!, {});
    //const drawnItems = L.featureGroup().addTo(lmap);
    this.leafletMap = lmap;
    lmap.setZoom(13);
    this.panToCenter();
    L.control
      .layers(
        {},
        {
          osm: L.tileLayer(osmUrl, { maxZoom: 18, attribution: osmAttrib }),
          "osm-mapbox": L.tileLayer(
            "https://api.mapbox.com/styles/v1/{id}/tiles/{z}/{x}/{y}?access_token={accessToken}",
            {
              attribution:
                'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, Imagery © <a href="https://www.mapbox.com/">Mapbox</a>',
              maxZoom: 18,
              id: "mapbox/streets-v11",
              tileSize: 512,
              zoomOffset: -1,
              accessToken:
                "pk.eyJ1IjoicHRvbWFzayIsImEiOiJjazhtNHIyY20wNWF3M2VtZDE4MThia2NzIn0.g3EBH4NfxZzDBQ7DlxUeMQ",
            }
          ),
          /*googlePhoto: L.tileLayer("http://www.google.cz/maps/vt?lyrs=s@189&gl=cn&x={x}&y={y}&z={z}", {
            attribution: "google",
          }),*/
          mapycz: L.tileLayer("https://mapserver.mapy.cz/base-m/{z}-{x}-{y}", {
            attribution: "Mapy.cz",
            maxZoom: 38,
            id: "mapycz-default",
            tileSize: 256,
            zoomOffset: 0,
            //accessToken: 'your.mapbox.access.token'
          }),
          lpisPole: L.tileLayer.wms(`http://eagri.cz/public/app/wms/public_DPB_PB_OPV.fcgi?`, {
            layers: "PB_UCINNE,DPB_UCINNE,DPB_UCINNE_KOD,DPB_VYM,DPB_KUL",
            tiled: "false",
            format: "image/png",
            transparent: "true",
            crs: L.CRS.EPSG4326,
          } as any),
          nexrad: L.tileLayer.wms("http://mesonet.agron.iastate.edu/cgi-bin/wms/nexrad/n0r.cgi", {
            layers: "nexrad-n0r-900913",
            format: "image/png",
            transparent: true,
            attribution: "Weather data © 2012 IEM Nexrad",
          }),
          Heigit: L.tileLayer.wms("https://maps.heigit.org/osm-wms/service?", {
            layers: "europe_wms:hs_srtm_europa",
            format: "image/png",
            transparent: true,
            attribution: "Heigit",
          }),
          drawlayer: this.leafletMapObjects,
        },
        { position: "topleft", collapsed: true }
      )
      .addTo(lmap);

    lmap.addLayer(this.leafletMapObjects);

    /*const drawControl = new L.Control.Draw();
    lmap.addControl(drawControl);*/
    this._disposers.push(
      reaction(
        () => this.props.mapSourceData.mapObjects,
        (objects) => {
          console.log("Drawing layers", objects);
          this.leafletMapObjects.clearLayers();
          for (let obj of objects) {
            console.log("Drawing object of layer", obj);
            switch (obj.type) {
              case IMapObjectType.POINT:
                {
                  const point = L.marker([obj.coordinates[1], obj.coordinates[0]])
                    .bindTooltip(obj.name)
                    .addTo(this.leafletMapObjects);
                }
                break;
              case IMapObjectType.POLYGON:
                console.log(
                  "Drawing polygon",
                  obj.coordinates.length,
                  obj.coordinates.map((coords) => coords.map((coord) => [coord[1], coord[0]]))
                );
                {
                  const polygon = L.polygon(
                    obj.coordinates[0].map((coords) => [coords[1], coords[0]]),
                    { color: "blue" }
                  )
                    .bindTooltip(obj.name)
                    .addTo(this.leafletMapObjects);
                }
                break;
              case IMapObjectType.LINESTRING:
                {
                  const polygon = L.polyline(
                    obj.coordinates.map((coords) => [coords[1], coords[0]]),
                    { color: "blue" }
                  )
                    .bindTooltip(obj.name)
                    .addTo(this.leafletMapObjects);
                }
                break;
            }
          }
        },
        {
          delay: 100,
          fireImmediately: true,
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
