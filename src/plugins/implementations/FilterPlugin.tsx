import React from "react";
import S from './FilterPlugin.module.scss';
import {IFormPlugin} from "../types/IFormPlugin";
import {IPluginData} from "../types/IPluginData";
import {toOrigamServerString} from "../../utils/moment";
import moment from "moment";
import {Moment} from "moment/moment";
import {observer} from "mobx-react";
import {observable} from "mobx";

export class FilterPlugin implements IFormPlugin {
  $type_IFormPlugin: 1 = 1;
  name = "FilterPlugin";

  @observable
  timeUnit = "day";

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
    switch(this.timeUnit){
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
        throw new Error("time unit \"" + this.timeUnit + "\" not implemented" )
    }
  }

  subtractTime(args:{end: Moment}){
    switch(this.timeUnit){
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
        throw new Error("time unit \"" + this.timeUnit + "\" not implemented" )
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

  async setTimeunit(value: string){
    this.timeUnit = value;
    this.addTime({start: moment()});
    await this.refresh();
  }
}

// enum TimeUnit {Hour, Day, Month}
//
// function timeUnitFromString(strValue : string){
//   switch (strValue){
//     case "month": return TimeUnit.Month;
//     case "day": return TimeUnit.Day;
//     case "hour": return TimeUnit.Hour;
//   }
//   throw new Error("Cannot parse" + strValue + " to TimeUnit")
// }

@observer
class FilterComponent extends  React.Component<{filterPlugin: FilterPlugin}> {
  plugin = this.props.filterPlugin;

  async onTimeUnitChanged(event: any){
    await this.plugin.setTimeunit(event.target.value);
  }

  render() {
    return (
      <div>
        <div className={S.row}>
          <button onClick={() => this.plugin.previousIntervalClick()}>Prev</button>
          <select id="timeSpan" name="timeSpan" value={this.plugin.timeUnit} onChange={(event: any) => this.onTimeUnitChanged(event)}>
            <option value="month">Month</option>
            <option value="day">Day</option>
            <option value="hour">Hour</option>
          </select>
          <button onClick={() => this.plugin.nextIntervalClick()}>Next</button>
        </div>
        <div>
          {"from: " + toOrigamServerString(this.plugin.dateFrom)}
        </div>
        <div>
          {"to: " + toOrigamServerString(this.plugin.dateTo)}
        </div>
      </div>
    );
  }
}