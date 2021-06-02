import React from "react";
import S from './AuditPlugin.module.scss';
import {IPluginData} from "../types/IPluginData";
import {IPluginProperty} from "../types/IPluginProperty";
import {observer} from "mobx-react";
import {ISectionPlugin} from "../types/ISectionPlugin";

export class AuditPlugin implements ISectionPlugin{
  name = "AuditPlugin";

  getComponent(data: IPluginData): JSX.Element {
    return <AuditComponent pluginData={data}/>;
  }
}

@observer
class AuditComponent extends React.Component<{ pluginData: IPluginData }> {

  dataView = this.props.pluginData.dataView;
  propertiesToRender: IPluginProperty[] = [];

  constructor(props: any) {
    super(props);
    this.propertiesToRender = ["RecordCreated", "refColumnId", "OldValue", "NewValue", "RecordCreatedBy"]
      .map(propId => this.props.pluginData.dataView.properties.find(prop=> prop.id === propId)!);
  }

  renderHeader(properties: IPluginProperty[]){
    return properties.map(property =><div className={S.column}>{property.name}</div>)
  }

  renderRow(row: any[]){
    return this.propertiesToRender.map(property =><div className={S.column}>{this.dataView.getCellText(row, property.id)}</div>)
  }

  render(){
    if(this.dataView.tableRows.length === 0){
      return <div>Empty</div>;
    }
    return(
      <div className={S.table}>
        <div className={S.row}>
          {this.renderHeader(this.propertiesToRender)}
        </div>
        {this.dataView.tableRows.map(row => <div className={S.row}>{this.renderRow(row as any[])}</div>)}
      </div>
    );
  }
}

