/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

import S from "./FilterSwitch.module.scss";
import { T } from "utils/translation";
import { Checkbox } from "gui/Components/CheckBox/Checkbox";
import React from "react";
import {
  runGeneratorInFlowWithHandler,
} from "utils/runInFlowWithHandler";
import {
  IConfigurationManager
} from "model/entities/TablePanelView/types/IConfigurationManager";
import {
  saveColumnConfigurations
} from "model/actions/DataView/TableView/saveColumnConfigurations";
import { observer } from "mobx-react-lite";
import {
  getFilterConfiguration
} from "model/selectors/DataView/getFilterConfiguration";


export interface IFilterSwitchContainer {
  alwaysShowFilters: boolean;
}

export const FilterSwitch = observer((props: {
  container: IFilterSwitchContainer
}) => {
  function onClick() {
    runGeneratorInFlowWithHandler({
      ctx: props.container,
      generator: function* () {
        const ctx = props.container;
        const filterConfiguration = getFilterConfiguration(ctx);
        props.container.alwaysShowFilters = !props.container.alwaysShowFilters ;
        filterConfiguration.isFilterControlsDisplayed = props.container.alwaysShowFilters ;
        yield*saveColumnConfigurations(ctx)();
      }()
    });
  }

  return (
    <div className={S.root} onClick={onClick}>
      {T("Always Show Filter", "filter_always_menu")}
      <Checkbox
        checked={props.container.alwaysShowFilters}
        id={"filter_always_menu"}/>
    </div>
  );
});
