import React from "react";
import S from './AuditView.module.scss';
import {IPlugin} from "./IPlugin";
import {IPluginData} from "../types/IPluginData";
import {IPluginProperty} from "../types/IPluginProperty";
import {observer} from "mobx-react";

export class AuditPlugin implements IPlugin{
  name = "AuditPlugin";

  getComponent(data: IPluginData): JSX.Element {
    return <AuditComponent pluginData={data}/>;
  }
}

@observer
class AuditComponent extends React.Component<{ pluginData: IPluginData }> {

  properties = this.props.pluginData.dataView.properties;
  dataView = this.props.pluginData.dataView;

  renderHeader(properties: IPluginProperty[]){
    return properties.map(property =><div className={S.column}>{property.id}</div>)
  }

  renderRow(row: any[]){
    return this.properties.map(property =><div className={S.column}>{this.dataView.getValue(row, property.id)}</div>)
  }

  render(){
    if(this.dataView.tableRows.length === 0){
      return <div>Empty</div>;
    }
    return(
      <div className={S.table}>
        <div className={S.row}>
          {this.renderHeader(this.properties)}
        </div>
        {this.dataView.tableRows.map(row => <div className={S.row}>{this.renderRow(row as any[])}</div>)}
      </div>
    );
  }
}

