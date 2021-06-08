import React from "react";
import S from './AuditPlugin.module.scss';
import {IPluginData} from "../types/IPluginData";
import {IPluginProperty} from "../types/IPluginProperty";
import {observer} from "mobx-react";
import {ISectionPlugin} from "../types/ISectionPlugin";
import {observable} from "mobx";
import moment from "moment";
import {IPluginTableRow} from "../types/IPluginRow";
import {IPluginDataView} from "../types/IPluginDataView";
import {Localizer} from "../tools/Localizer";
import {localizations} from "./AuditPluginLocalization";

export class AuditPlugin implements ISectionPlugin{
  $type_ISectionPlugin: 1 = 1;
  name = "AuditPlugin";
  
  getComponent(data: IPluginData): JSX.Element {
    return <AuditComponent
      pluginData={data}
      getFormParameters={this.getFormParameters}
      localizer={new Localizer(localizations, "en-US")}/>;
  }

  @observable
  getFormParameters: (() => { [parameter: string]: string }) | undefined;
}

@observer
class AuditComponent extends React.Component<{
  pluginData: IPluginData,
  getFormParameters: (() => { [parameter: string]: string }) | undefined;
  localizer: Localizer
}> {

  translate = (key: string, parameters?: {[key: string]: any}) => this.props.localizer.translate(key, parameters);
  dataView = this.props.pluginData.dataView;
  propertiesToRender: IPluginProperty[] = [];

  constructor(props: any) {
    super(props);
    this.propertiesToRender = ["RecordCreated", "refColumnId", "OldValue", "NewValue", "RecordCreatedBy"]
      .map(propId => this.props.pluginData.dataView.properties.find(prop=> prop.id === propId)!);
  }

  renderHeader(properties: IPluginProperty[]){
    return properties.map(property =>
      <>
        <div className={S.header}>{property.name}</div>
        <div className={S.headerSeparator}></div>
      </>
    );
  }

  renderRow(row: any[]){
    return this.propertiesToRender.map(property =><div className={S.column}>{this.dataView.getCellText(row, property.id)}</div>)
  }

  getGroupContainer(){
    const parameters = this.props.getFormParameters?.();
    if(!parameters){
      return undefined;
    }
    const dateFrom = parameters["OrigamDataAuditLog_DateFrom"];
    if(dateFrom.endsWith("01T00:00:00")){
      return new MonthTimeGroupContainer(this.dataView);
    }
    if(dateFrom.endsWith("00:00:00")){
      return new DayTimeGroupContainer(this.dataView);
    }
    if(dateFrom.endsWith("00:00")){
      return new HourTimeGroupContainer(this.dataView);
    }
  }

  render(){
    const groupContainer = this.getGroupContainer();
    if(this.dataView.tableRows.length === 0 || !groupContainer){
      return <div>{this.translate("empty")}</div>;
    }

    return(
      <div>
        <div className={S.summary}>
          {this.renderSummary()}
        </div>
        {Array.from(groupContainer.groups.keys())
          .sort((a, b) => a-b)
          .map(subTimeunitValue => this.renderGroup(subTimeunitValue, groupContainer))
        }
      </div>
    );
  }

  renderGroup(subTimeunitValue: number, groupContainer: ITimeGroupContainer){
    return(
      <div className={S.table}>
        <div className={S.groupHeader}>{groupContainer.makeGroupHeaderText(subTimeunitValue)}</div>
        <div className={S.row}>
          {this.renderHeader(this.propertiesToRender)}
        </div>
        <div className={S.rows}>
          {groupContainer.groups
            .get(subTimeunitValue)!
            .map(row => <div className={S.row}>{this.renderRow(row as any[])}</div>)}
        </div>
      </div>
    );
  }

  private renderSummary() {
    const userCount = new Set(
      this.dataView.tableRows
        .map(row => this.dataView.getCellText(row, "RecordCreatedBy"))
    ).size

    return (
      <>{this.translate(
        "recordSummary",
        {recordCount: this.dataView.tableRows.length, userCount: userCount})}</>
    );
  }
}

interface ITimeGroupContainer{
  groups: Map<number,IPluginTableRow[]>
  makeGroupHeaderText(timeUnitValue: number): string;
}

class MonthTimeGroupContainer implements ITimeGroupContainer{
  groups: Map<number,IPluginTableRow[]>;
  dataView: IPluginDataView;

  constructor(dataView: IPluginDataView) {
    this.dataView = dataView;
    this.groups = dataView.tableRows
      .groupBy(row => moment(dataView.getCellText(row, "RecordCreated")).day());
  }

  makeGroupHeaderText(dayOfMonth: number): string {
    const firstRow = this.groups.get(dayOfMonth)![0];
    const firstRecordCreated = moment(this.dataView.getCellText(firstRow, "RecordCreated"))
    return firstRecordCreated.format('MMMM Do');
  }
}

class DayTimeGroupContainer implements ITimeGroupContainer{
  groups: Map<number,IPluginTableRow[]>;
  dataView: IPluginDataView;

  constructor(dataView: IPluginDataView) {
    this.dataView = dataView;
    this.groups = dataView.tableRows
      .groupBy(row => moment(dataView.getCellText(row, "RecordCreated")).hour());
  }

  makeGroupHeaderText(hour: number): string {
    const firstRow = this.groups.get(hour)![0];
    const firstRecordCreated = moment(this.dataView.getCellText(firstRow, "RecordCreated"))
    const endHourMoment = firstRecordCreated.clone().add(-1, 'hours')
    return firstRecordCreated.format('HH:00') + " - " + endHourMoment.format('HH:00');
  }
}

class HourTimeGroupContainer implements ITimeGroupContainer{
  groups: Map<number,IPluginTableRow[]>;
  dataView: IPluginDataView;

  constructor(dataView: IPluginDataView) {
    this.dataView = dataView;
    this.groups = dataView.tableRows
      .groupBy(row => moment(dataView.getCellText(row, "RecordCreated")).minute());
  }

  makeGroupHeaderText(minute: number): string {
    const firstRow = this.groups.get(minute)![0];
    const firstRecordCreated = moment(this.dataView.getCellText(firstRow, "RecordCreated"))
    const endMinuteMoment = firstRecordCreated.clone().add(-1, 'minutes')
    return firstRecordCreated.format('HH:mm') + " - " + endMinuteMoment.format('HH:mm');
  }
}

