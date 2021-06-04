import React from "react";
import S from './FilterPlugin.module.scss';
import {IFormPlugin} from "../types/IFormPlugin";
import {IPluginData} from "../types/IPluginData";
import {toOrigamServerString} from "../../utils/moment";
import moment from "moment";
import {Moment} from "moment/moment";
import {observer} from "mobx-react";
import {observable} from "mobx";
import {IOption, SimpleDropdown} from "../../gui/Components/PublicComponenets/SimpleDropdown";
import {Button} from "../../gui/Components/PublicComponenets/Button";

export class FilterPlugin implements IFormPlugin {
  $type_IFormPlugin: 1 = 1;
  name = "FilterPlugin";

  timeUnits = [{value: "month", label: "Month"}, {value: "day", label: "Day"}, {value: "hour", label: "Hour"}]

  @observable
  selectedTimeUnit = this.timeUnits[1];

  private async refresh() {
    this.setFormParameters?.({
      OrigamDataAuditLog_DateFrom: toOrigamServerString(this.dateFrom),
      OrigamDataAuditLog_DateTo: toOrigamServerString(this.dateTo)
    });
    await this.requestSessionRefresh?.();
  }

  @observable
  dateFrom: Moment = moment();
  @observable
  dateTo: Moment = moment();

  requestSessionRefresh:  (() => Promise<any>) | undefined;
  setFormParameters:  ((parameters: { [key: string]: string }) => void) | undefined;

  initialize() {
    this.addTime({start: moment()});
    this.setFormParameters?.({
      OrigamDataAuditLog_DateFrom: toOrigamServerString(this.dateFrom),
      OrigamDataAuditLog_DateTo: toOrigamServerString(this.dateTo)
    });
  }

  getComponent(data: IPluginData): JSX.Element {
    return <FilterComponent filterPlugin={this}/>;
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
    this.addTime({start: moment()});
    await this.refresh();
  }
}

@observer
class FilterComponent extends React.Component<{filterPlugin: FilterPlugin}> {
  plugin = this.props.filterPlugin;

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
              label={"Prev"}/>
            <Button
              onClick={() => this.plugin.nextIntervalClick()}
              label={"Next"}/>
          </div>

        </div>
      </div>
    );
  }
}