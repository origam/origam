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
import { FilterSettings } from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettings";
import { getTableViewProperties } from "model/selectors/TablePanelView/getTableViewProperties";
import { IDataView } from "model/entities/types/IDataView";
import { observer, Provider } from "mobx-react";
import S from "./FilterEditor.module.scss";
import { getTotalRowCount } from "model/selectors/DataView/getTotalGroupRowCount";
import { T } from "utils/translation";

export const FilterEditor: React.FC<{
  dataView: IDataView
}> = observer((props) => {

  const propertiesToDisplay = getTableViewProperties(props.dataView)
    .filter(prop => prop.column !== "Image");
  const totalRowCount = getTotalRowCount(props.dataView);

  return (

    <div className={S.root}>
      <div className={S.rowContainer}>
        {
          propertiesToDisplay.map(property =>
            <div className={S.filterRow}>
              <Provider property={property}>
                <div className={S.label}>
                  {property.name}
                </div>
                <div className={S.filterSettingContainer}>
                  <FilterSettings
                    key={`filter-settings-${property.id}`}
                    autoFocus={false} ctx={props.dataView}/>
                </div>
              </Provider>
            </div>
          )
        }
      </div>
      <div className={S.rowCount}>
        {T("Rows", "row_count") + ": " + totalRowCount}
      </div>
    </div>
  );
});
