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


import { observable } from "mobx";
import React from "react";
import S from './RadarChartPlugin.module.scss';
import { Radar } from 'react-chartjs-2';
import moment from "moment";
import { ISectionPlugin } from "plugins/types/ISectionPlugin";
import { IPluginData } from "plugins/types/IPluginData";
import { IPluginTableRow } from "plugins/types/IPluginRow";
import { ILocalization } from "plugins/types/ILocalization";
import { ILocalizer } from "plugins/types/ILocalizer";

const seriesLabelFieldName = "SeriesLabelField";
const seriesValueFieldsName = "SeriesValueFields";
const filterFieldName = "FilterField";
const noDataMessageName = "NoDataMessage";

export class RadarChartPlugin implements ISectionPlugin {
  $type_ISectionPlugin: 1 = 1;
  id: string = ""
  seriesValueFields: string | undefined;
  seriesLabelField: string | undefined;
  noDataMessage: string | undefined;
  filterField: string | undefined;


  @observable
  initialized = false;

  initialize(xmlAttributes: { [key: string]: string }): void {
    this.seriesValueFields = this.getXmlParameter(xmlAttributes, seriesValueFieldsName);
    this.seriesLabelField = this.getXmlParameter(xmlAttributes, seriesLabelFieldName);
    this.noDataMessage = this.getXmlParameter(xmlAttributes, noDataMessageName);
    this.filterField = xmlAttributes[filterFieldName];
    this.initialized = true;
  }

  getXmlParameter(xmlAttributes: { [key: string]: string }, parameterName: string){
    if(!xmlAttributes[parameterName]){
      throw new Error(`Parameter ${parameterName} was not found. Cannot plot anything.`)
    }
    return xmlAttributes[parameterName];
  }

  getLabel(data: IPluginData, row: IPluginTableRow){
    const property = this.getProperty(data, this.seriesLabelField!)

    return property.type === "Date"
      ? moment(data.dataView.getCellValue(row, property.id)).format(property.momentFormatterPattern)
      : data.dataView.getCellValue(row, property.id);
  }

  getProperty(data: IPluginData, propertyId: string){
    const property = data.dataView.properties.find(prop => prop.id === propertyId)
    if(!property){
      throw new Error(`Property ${propertyId} was not found`)
    }
    return property;
  }

  getComponent(data: IPluginData, createLocalizer: (localizations: ILocalization[]) => ILocalizer): JSX.Element {
    const localizer = createLocalizer([]);
    moment.locale(localizer.locale)

    if(!this.initialized) {
      return <></>;
    }

    const properties = this.seriesValueFields!
      .split(";")
      .map(propertyId => this.getProperty(data, propertyId.trim()))

    const dataSets = data.dataView.tableRows
      .filter(row => !this.filterField || data.dataView.getCellValue(row, this.filterField))
      .map((row, i) => {
        const color = SeriesColor.getBySeriesNumber(i);
        return {
          label: this.getLabel(data, row),
          data: properties.map(prop => data.dataView.getCellValue(row, prop.id)),
          backgroundColor: color.background,
          borderColor: color.border,
          borderWidth: 1,
        }
      })
    if(dataSets.length === 0){
      return <div className={S.noDatMessageContainer}>{this.noDataMessage}</div>
    }
    return(
      <div className={S.chartContainer}>
        <Radar
          data={{
            labels: properties.map(prop => prop.name),
            datasets: dataSets,
          }}
          options={
            {maintainAspectRatio: false}
          }
        />
      </div>
    );
  }

  @observable
  getScreenParameters: (() => { [parameter: string]: string }) | undefined;
}


class SeriesColor{

  static seriesColorsRGB = [
    new SeriesColor(255, 99, 132),
    new SeriesColor(234, 99, 255),
    new SeriesColor(99, 112, 255),
    new SeriesColor(99, 252, 255),
    new SeriesColor(99, 255, 102),
  ]

  static getBySeriesNumber(seriesNumber: number){
    return SeriesColor.seriesColorsRGB[seriesNumber % 5];
  }

  constructor(
    private red: number,
    private green: number,
    private blue: number) {

  }

  get background(){
    return `rgba(${this.red}, ${this.green}, ${this.blue}, 0.2)`
  }

  get border(){
    return `rgba(${this.red}, ${this.green}, ${this.blue}, 1)`
  }
}
