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

import React, { useContext } from "react";
import { action } from "mobx";
import { IDataViewBodyUI, IDataViewToolbarUI } from "modules/DataView/DataViewUI";
import { TypeSymbol } from "dic/Container";
import { SectionViewSwitchers } from "modules/DataView/DataViewTypes";
import { getIdent, IIId } from "utils/common";
import { DataViewHeaderAction } from "gui/Components/DataViewHeader/DataViewHeaderAction";
import { Icon } from "gui/Components/Icon/Icon";

import { MobXProviderContext, observer, Observer } from "mobx-react";
import { IFormPerspective } from "./FormPerspective";
import { IPerspective } from "../Perspective";
import { FormView } from "gui/Workbench/ScreenArea/FormView/FormView";
import { FormBuilder } from "gui/Workbench/ScreenArea/FormView/FormBuilder";
import { T } from "../../../../utils/translation";
import S from "./FormPerspectiveDirector.module.scss";
import cx from "classnames";
import { MobileFormBuilder } from "gui/connections/MobileComponents/Form/MobileFormBuilder";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { IApplication } from "model/entities/types/IApplication";

export class FormPerspectiveDirector implements IIId {
  $iid = getIdent();

  constructor(
    public dataViewToolbarUI = IDataViewToolbarUI(),
    public dataViewBodyUI = IDataViewBodyUI(),
    public formPerspective = IFormPerspective(),
    public perspective = IPerspective()
  ) {
  }

  @action.bound
  setup() {
    this.dataViewBodyUI.contrib.put({
      $iid: this.$iid,
      render: () => (
        <FormPerspectiveDirectorComponent
          key={this.$iid}
          formPerspectiveActive={this.formPerspective.isActive}
        />
      ),
    });

    this.dataViewToolbarUI.contrib.put({
      $iid: this.$iid,
      section: SectionViewSwitchers,
      render: () => (
        <Observer key={this.$iid}>
          {() => (
            <DataViewHeaderAction
              className={"formPerspectiveButton"}
              onMouseDown={()=> this.formPerspective.handleClick({saveNewState: true})}
              isActive={this.formPerspective.isActive}
            >
              <Icon src="./icons/detail-view.svg" tooltip={T("Grid", "form_tool_tip")}/>
            </DataViewHeaderAction>
          )}
        </Observer>
      ),
    });

    this.perspective.contrib.put(this.formPerspective);
  }

  @action.bound
  teardown() {
    this.dataViewBodyUI.contrib.del(this);
    this.dataViewToolbarUI.contrib.del(this);
    this.perspective.contrib.del(this.formPerspective);
  }

  dispose() {
    this.teardown();
  }
}

const FormPerspectiveDirectorComponent: React.FC<{
  formPerspectiveActive: boolean;
}> = observer((props) => {
  const application = useContext(MobXProviderContext).application as IApplication

  return (
    !props.formPerspectiveActive ? (
      <></>
    ) : (
      <div className={cx(S.root, {isActive: props.formPerspectiveActive})}>
        <FormView>
          {isMobileLayoutActive(application)
            ? <MobileFormBuilder mobileState={application.mobileState}/>
            : <FormBuilder/>
          }
        </FormView>
      </div>
    )
  );
});


export const IFormPerspectiveDirector = TypeSymbol<FormPerspectiveDirector>(
  "IFormPerspectiveDirector"
);
