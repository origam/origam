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

import React, { useContext, useState } from "react";
import S from "gui/connections/MobileComponents/Grid/ColumnConfiguration.module.scss";
import { IDataView } from "model/entities/types/IDataView";
import { getColumnConfigurationModel } from "model/selectors/getColumnConfigurationModel";
import { IColumnConfiguration } from "model/entities/TablePanelView/types/IConfigurationManager";
import { T } from "utils/translation";
import {
  aggregationOptions,
  ColumnConfigurationModel,
  timeunitOptions
} from "model/entities/TablePanelView/ColumnConfigurationModel";
import { MobXProviderContext, observer } from "mobx-react";
import { MobileState } from "model/entities/MobileState/MobileState";
import { EditLayoutState, ScreenLayoutState } from "model/entities/MobileState/MobileLayoutState";
import { MobileBooleanInput } from "gui/connections/MobileComponents/Form/MobileBooleanInput";
import { ColumnOrderEditor } from "gui/connections/MobileComponents/Grid/ColumnOrderEditor";
import { BottomIcon } from "gui/connections/MobileComponents/BottomToolBar/BottomIcon";
import { NavigationButton } from "gui/connections/MobileComponents/Navigation/NavigationButton";
import { Button } from "gui/Components/Button/Button";
import { SimpleDropdown } from "gui/Components/Dialogs/SimpleDropdown";

export const ColumnConfiguration: React.FC<{
  dataView: IDataView
}> = observer((props) => {

  const configModel = getColumnConfigurationModel(props.dataView);

  const mobileState = useContext(MobXProviderContext).application.mobileState as MobileState;

  return (
    <div className={S.root}>
      <div className={S.content}>
        <div className={S.columnConfigs}>
          {configModel.sortedColumnConfigs.map((config, i) =>
            <ColumnConfig
              index={i}
              config={config}
              model={configModel}
            />
          )}
        </div>
      </div>
      <div className={S.lockedColumnsRow}>
        {T("Locked columns count", "column_config_locked_columns_count")}
        <input
          className={S.input}
          type="number"
          autoComplete={"new-password"}
          min={0}
          value={"" + configModel.columnsConfiguration.fixedColumnCount}
          onChange={configModel.handleFixedColumnsCountChange}
        />
      </div>
      <div className={S.buttonRow}>
        <Button
          label={T("OK", "button_ok")}
          onClick={() => {
            configModel.onColumnConfigurationSubmit();
            mobileState.layoutState = new ScreenLayoutState();
          }}
        />
        <Button
          label={T("Save As...", "column_config_save_as")}
          onClick={() => configModel.onSaveAsClick()}
        />
        <Button
          label={T("Order", "order_columns_button")}
          onClick={() => {
            const previousLayout = mobileState.layoutState;
            mobileState.layoutState = new EditLayoutState(
              <ColumnOrderEditor dataView={props.dataView}/>,
              T("Order Columns" , "order_columns_title"),
              previousLayout,
              true,
            );
          }}
        />
        <BottomIcon
          key={"close"}
          iconPath={"./icons/noun-close-25798.svg"}
          onClick={async () => {
            configModel.onColumnConfCancel();
            mobileState.layoutState = new ScreenLayoutState();
          }}
        />
      </div>
    </div>
  );
});

export const ColumnConfig: React.FC<{
  config: IColumnConfiguration,
  index: number;
  model: ColumnConfigurationModel;
}> = observer((props) => {


  const selectedAggregationOption = aggregationOptions.find(option => option.value === props.config.aggregationType)!;

  const selectedTimeUnitOption = timeunitOptions.find(option => option.value === props.config.timeGroupingUnit)!;

  const {
    gridCaption,
    entity,
    canGroup,
    canAggregate,
    modelInstanceId // eslint-disable-line
  } = props.model.columnOptions.get(props.config.propertyId)!;

  const isDefault = props.config.groupingIndex === 0 && props.config.isVisible && !props.config.aggregationType

  const [isExpanded, setIsExpanded] = useState<boolean>(!isDefault);

  function renderContent() {
    return (
      <>
        <div
          key={"Visible"}
          className={S.row}>
          <div className={S.label}>
            {T("Visible", "column_config_visible")}
          </div>
          <MobileBooleanInput
            onChange={(event: any) => props.model.setVisible(props.index, event.target.checked)}
            checked={props.config.isVisible}
          />
        </div>
        {canGroup &&
          <div
            key={"GroupBy"}
            className={S.row}>
            <div className={S.label}>
              {T("GroupBy", "column_config_group_by")}
            </div>
            <MobileBooleanInput
              checked={props.config.groupingIndex > 0}
              onChange={(event: any) => props.model.setGrouping(props.index, event.target.checked, entity)}
              disabled={!canGroup}
            />
          </div>
        }
        {props.config.groupingIndex > 0 && entity === "Date" &&
          <div
            key={"Grouping Unit"}
            className={S.row}>
            <div className={S.label}>
              {T("Grouping Unit", "column_config_time_grouping_unit")}
            </div>
            <SimpleDropdown
              className={S.dropdown}
              options={timeunitOptions}
              selectedOption={selectedTimeUnitOption}
              onOptionClick={option => props.model.setTimeGroupingUnit(props.index, option.value)}
            />
          </div>
        }
        {(entity === "Currency" ||
            entity === "Integer" ||
            entity === "Float" ||
            entity === "Long") &&
          canAggregate &&
          <div
            key={"Aggregation"}
            className={S.row}>
            <div className={S.label}>
              {T("Aggregation", "column_config_aggregation")}
            </div>
            <SimpleDropdown
              className={S.dropdown}
              options={aggregationOptions}
              selectedOption={selectedAggregationOption}
              onOptionClick={option => props.model.setAggregation(props.index, option.value)}
            />
          </div>
        }
        <div
          key={"Width"}
          className={S.row}>
          <div className={S.label}>
            {T("Width", "column_width")}
          </div>
          <input
            className={S.input}
            type="number"
            autoComplete={"new-password"}
            onChange={(event: any) => props.model.setWidth(props.index, event.target.value)}
            value={props.config.width}
          />
        </div>
      </>
    );
  }

  return (
    <NavigationButton
      label={gridCaption}
      onClick={() => setIsExpanded(!isExpanded)}
      isOpen={isExpanded}
    >
      {isExpanded && renderContent()}
    </NavigationButton>
  );
});
