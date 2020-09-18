import React from "react";
import { action, flow } from "mobx";
import { IDataViewBodyUI, IDataViewToolbarUI } from "modules/DataView/DataViewUI";
import { TypeSymbol } from "dic/Container";
import { SectionViewSwitchers } from "modules/DataView/DataViewTypes";
import { getIdent, IIId } from "utils/common";
import { DataViewHeaderAction } from "gui02/components/DataViewHeader/DataViewHeaderAction";
import { Icon } from "gui02/components/Icon/Icon";

import { Observer } from "mobx-react";
import { IFormPerspective } from "./FormPerspective";
import { IPerspective } from "../Perspective";
import { FormView } from "gui/Workbench/ScreenArea/FormView/FormView";
import { FormBuilder } from "gui/Workbench/ScreenArea/FormView/FormBuilder";
import { T } from "../../../../utils/translation";
import S from "./FormPerspectiveDirector.module.scss";
import cx from "classnames";

export class FormPerspectiveDirector implements IIId {
  $iid = getIdent();

  constructor(
    public dataViewToolbarUI = IDataViewToolbarUI(),
    public dataViewBodyUI = IDataViewBodyUI(),
    public formPerspective = IFormPerspective(),
    public perspective = IPerspective()
  ) {}

  @action.bound
  setup() {
    this.dataViewBodyUI.contrib.put({
      $iid: this.$iid,
      render: () => (
        <Observer key={this.$iid}>
          {() =>
            !this.formPerspective.isActive ? (
              <></>
            ) : (
              <div className={cx(S.root, { isActive: this.formPerspective.isActive })}>
                <FormView>
                  <FormBuilder />
                </FormView>
              </div>
            )
          }
        </Observer>
      ),
    });

    this.dataViewToolbarUI.contrib.put({
      $iid: this.$iid,
      section: SectionViewSwitchers,
      render: () => (
        <Observer key={this.$iid}>
          {() => (
            <DataViewHeaderAction
              onClick={flow(this.formPerspective.handleToolbarBtnClick)}
              isActive={this.formPerspective.isActive}
            >
              <Icon src="./icons/detail-view.svg" tooltip={T("Grid", "form_tool_tip")} />
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

export const IFormPerspectiveDirector = TypeSymbol<FormPerspectiveDirector>(
  "IFormPerspectiveDirector"
);
