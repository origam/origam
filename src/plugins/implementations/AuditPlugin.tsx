import React from "react";
import {IPluginTableRow} from "../types/IPluginRow";
import S from './AuditView.module.scss';
import {IPlugin} from "./IPlugin";
import {IPluginData} from "../types/IPluginData";

export class AuditPlugin implements IPlugin{
  name = "AuditPlugin";

  getComponent(data: IPluginData): JSX.Element {
    return <AuditComponent pluginData={data}/>;
  }
}

class AuditComponent extends React.Component<{ pluginData: IPluginData }> {

  renderHeader(row: IPluginTableRow){
    return Object.keys(row).map(property =><div className={S.column}>{property}</div>)
  }

  renderRow(row: IPluginTableRow){
    return Object.values(row).map(value =><div className={S.column}>{value}</div>)
  }

  render(){
    if(this.props.pluginData.dataView.tableRows.length === 0){
      return <div>Empty</div>;
    }
    return(
      <div className={S.table}>
        <div className={S.row}>
          {this.renderHeader(this.props.pluginData.dataView.tableRows[0])}
        </div>
        {this.props.pluginData.dataView.tableRows.map(row => <div className={S.row}>{this.renderRow(row)}</div>)}
      </div>
    );
  }
}

