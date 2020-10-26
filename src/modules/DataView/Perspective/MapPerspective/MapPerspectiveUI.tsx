import React from "react";
import L from "leaflet";
import "leaflet-draw";
import "leaflet-draw/dist/leaflet.draw-src.css";
import "leaflet/dist/leaflet.css";

import S from "./MapPerspectiveUI.module.scss";
import { IMapObject, IMapObjectType, MapSourceData } from "./MapSourceData";
import { reaction } from "mobx";

delete (L.Icon.Default.prototype as any)._getIconUrl;

L.Icon.Default.mergeOptions({
  iconRetinaUrl: require("leaflet/dist/images/marker-icon-2x.png"),
  iconUrl: require("leaflet/dist/images/marker-icon.png"),
  shadowUrl: require("leaflet/dist/images/marker-shadow.png"),
});

interface IMapPerspectiveComProps {
  mapCenter: { lat: number; lng: number };
  mapSourceData: MapSourceData;
}

export class MapPerspectiveCom extends React.Component<IMapPerspectiveComProps> {
  elmMapDiv: HTMLDivElement | null = null;
  refMapDiv = (elm: any) => (this.elmMapDiv = elm);

  leafletMap?: L.DrawMap;

  leafletMapObjects = new L.FeatureGroup();

  _disposers: any[] = [];

  panToCenter() {
    this.leafletMap?.panTo([this.props.mapCenter.lat, this.props.mapCenter.lng]);
  }

  isPropMapCenterDifferent(prevProps: IMapPerspectiveComProps) {
    return (
      this.props.mapCenter.lat !== prevProps.mapCenter.lat ||
      this.props.mapCenter.lng !== prevProps.mapCenter.lng
    );
  }

  componentDidUpdate(prevProps: IMapPerspectiveComProps) {
    if (this.isPropMapCenterDifferent(prevProps)) {
      this.panToCenter();
    }
  }

  componentDidMount() {
    const osmUrl = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
    const osmAttrib =
      '&copy; <a href="http://openstreetmap.org/copyright">OpenStreetMap</a> contributors';
    const lmap = L.map(this.elmMapDiv!, {});
    const drawnItems = L.featureGroup().addTo(lmap);
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
          drawlayer: drawnItems,
        },
        { position: "topleft", collapsed: true }
      )
      .addTo(lmap);
    lmap.addControl(
      new L.Control.Draw({
        edit: {
          featureGroup: drawnItems,
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

    lmap.addLayer(this.leafletMapObjects);

    lmap.on(L.Draw.Event.CREATED, function (event) {
      const layer = event.layer;
      console.log(drawnItems.toGeoJSON());
      drawnItems.addLayer(layer);
    });

    lmap.on(L.Draw.Event.EDITED, function (event) {
      console.log(drawnItems.toGeoJSON());
    });

    /*const drawControl = new L.Control.Draw();
    lmap.addControl(drawControl);*/
    this._disposers.push(
      reaction(
        () => this.props.mapSourceData.mapObjects,
        (objects) => {
          console.log("Drawing layers");
          console.log(objects);
          this.leafletMapObjects.clearLayers();
          for (let obj of objects) {
            switch (obj.type) {
              case IMapObjectType.POINT:
                {
                  const point = L.marker([obj.lat, obj.lng])
                    .bindTooltip(obj.name)
                    .addTo(this.leafletMapObjects);
                }
                break;
              case IMapObjectType.POLYGON:
                {
                  const polygon = L.polygon(
                    obj.coords.map((loc) => [loc.lat, loc.lng]),
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

  componentWillUnmount() {
    this._disposers.forEach((d) => d());
  }

  render() {
    return <div className={S.mapDiv} ref={this.refMapDiv}></div>;
  }
}
