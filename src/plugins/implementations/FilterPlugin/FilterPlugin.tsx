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

import React from "react";
import S from './FilterPlugin.module.scss';
import {IFormPlugin} from "../../types/IFormPlugin";
import {IPluginData} from "../../types/IPluginData";
import {toOrigamServerString} from "../../../utils/moment";
import moment from "moment";
import {Moment} from "moment/moment";
import {observer} from "mobx-react";
import {observable} from "mobx";
import {IOption, SimpleDropdown} from "../../../gui/Components/PublicComponents/SimpleDropdown";
import {Button} from "../../../gui/Components/PublicComponents/Button";
import {Localizer} from "../../tools/Localizer";
import {localizations} from "./FilterPluginLocalization";

export default class FilterPlugin implements IFormPlugin {
  $type_IFormPlugin: 1 = 1;
  id: string = "";

  timeUnits = [{value: "month", label: "Month"}, {value: "day", label: "Day"}, {value: "hour", label: "Hour"}]

  @observable
  selectedTimeUnit = this.timeUnits[1];

  private async refresh() {
    this.setParameters();
    await this.requestSessionRefresh?.();
  }

  @observable
  dateFrom: Moment = moment();
  @observable
  dateTo: Moment = moment();

  requestSessionRefresh:  (() => Promise<any>) | undefined;
  setFormParameters:  ((parameters: { [key: string]: string }) => void) | undefined;
  fromParameterName: string | undefined;
  toParameterName: string | undefined;

  initialize(xmlAttributes: {[key: string]: string}) {
    this.addTime({start: moment()});
    this.fromParameterName = xmlAttributes["FromParameterName"];
    this.toParameterName = xmlAttributes["ToParameterName"];
    if(!this.toParameterName){
      throw new Error("ToParameterName attribute of the FilterPlugin is not set")
    }
    if(!this.fromParameterName){
      throw new Error("FromParameterName attribute of the FilterPlugin is not set")
    }
    this.setParameters();
  }

  private setParameters() {
    const parameters: { [key: string]: string } = {};
    parameters[this.fromParameterName!] = toOrigamServerString(this.dateFrom);
    parameters[this.toParameterName!] = toOrigamServerString(this.dateTo);
    this.setFormParameters?.(parameters);
  }

  getComponent(data: IPluginData): JSX.Element {
    let localizer = new Localizer(localizations, "en-US");
    for (let timeUnit of this.timeUnits) {
      timeUnit.label = localizer.translate(timeUnit.value)
    }
    return <FilterComponent
      filterPlugin={this}
      localizer={localizer}/>;
  }

  addTime(args:{start: Moment}){
    switch(this.selectedTimeUnit.value){
      case "month":
        this.dateFrom = moment([ args.start.year(), args.start.month()])
        this.dateTo =  this.dateFrom.clone().add(1, 'months');
        break;
      case "day":
        this.dateFrom = moment([ args.start.year(), args.start.month(), args.start.date()])
        this.dateTo = this.dateFrom.clone().add(1, 'days');
        break;
      case "hour":
        this.dateFrom = moment([ args.start.year(), args.start.month(), args.start.date(), args.start.hour()])
        this.dateTo = this.dateFrom.clone().add(1, 'hours');
        break;
      default:
        throw new Error("time unit \"" + this.selectedTimeUnit.value + "\" not implemented" )
    }
  }

  subtractTime(args:{end: Moment}){
    switch(this.selectedTimeUnit.value){
      case "month":
        this.dateTo = moment([ args.end.year(), args.end.month() ])
        this.dateFrom = this.dateTo.clone().add(-1, 'months')
        break;
      case "day":
        this.dateTo = moment([ args.end.year(), args.end.month(), args.end.date() ])
        this.dateFrom = this.dateTo.clone().add(-1, 'days')
        break;
      case "hour":
        this.dateTo = moment([ args.end.year(), args.end.month(), args.end.date(), args.end.hour()])
        this.dateFrom = this.dateTo.clone().add(-1, 'hours')
        break;
      default:
        throw new Error("time unit \"" + this.selectedTimeUnit.value + "\" not implemented" )
    }
  }

  async nextIntervalClick() {
    this.addTime({start: this.dateTo});
    await this.refresh();
  }

  async previousIntervalClick() {
    this.subtractTime({end: this.dateFrom});
    await this.refresh();
  }

  async setTimeunit(timeUnit: IOption<string>){
    this.selectedTimeUnit = timeUnit;
    this.addTime({start: this.dateFrom});
    await this.refresh();
  }
}

@observer
class FilterComponent extends React.Component<{
  filterPlugin: FilterPlugin,
  localizer: Localizer
}> {

  plugin = this.props.filterPlugin;
  translate = (key: string, parameters?: {[key: string]: any}) => this.props.localizer.translate(key, parameters);

  render() {
    return (
      <div className={S.mainRow}>
        <h1>
          {this.plugin.dateFrom.format('MMMM Do YYYY, HH:mm') + " - " + this.plugin.dateTo.format('MMMM Do YYYY, HH:mm')}
        </h1>
        <div className={S.controlsColumn}>
          <SimpleDropdown
            width={"170px"}
            options={this.plugin.timeUnits}
            selectedOption={this.plugin.selectedTimeUnit}
            onOptionClick={(timeUnit) => this.plugin.setTimeunit(timeUnit)}
          />
          <div className={S.buttonRow}>
            <Button
              onClick={() => this.plugin.previousIntervalClick()}
              label={this.translate("prev")}/>
            <Button
              onClick={() => this.plugin.nextIntervalClick()}
              label={this.translate("next")}/>
          </div>
        </div>
      </div>
    );
  }
}