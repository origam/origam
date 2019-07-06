import S from "./ScreenArea.module.css";
import FSS from "./FormScreen.module.css";

import React from "react";
import { observer, inject, Observer } from "mobx-react";
import { action, observable } from "mobx";
import {
  ModalWindowOverlay,
  ModalWindow
} from "../../Components/Dialog/Dialog";
import { FormScreen } from "./FormScreen";
import { VBox } from "../../Components/ScreenElements/VBox";
import {
  TabbedPanel,
  TabBody,
  TabHandle
} from "../../Components/ScreenElements/TabbedPanel";
import { VSplit, VSplitPanel } from "../../Components/ScreenElements/VSplit";
import { HSplit, HSplitPanel } from "../../Components/ScreenElements/HSplit";
import { FormScreenBuilder } from "./FormScreenBuilder";
import axios from "axios";
import xmlJs from "xml-js";
import { ColumnsDialog } from "../../Components/Dialogs/ColumnsDialog";
import { getOpenedScreens } from "../../../model/selectors/getOpenedScreens";
import { IOpenedScreen } from "../../../model/types/IOpenedScreen";
import { getApplicationLifecycle } from "../../../model/selectors/getApplicationLifecycle";
import { ScreenBuilder } from "./ScreenBuilder";
import { getOpenedScreenItems } from '../../../model/selectors/getOpenedScreenItems';

@observer
class MainViewHandle extends React.Component<{
  order: number;
  label: string;
  isActive: boolean;
  onClick?: (event: any) => void;
  onCloseClick?: (event: any) => void;
}> {
  render() {
    return (
      <div
        className={S.TabHandle + (this.props.isActive ? ` ${S.active}` : "")}
        onClick={this.props.onClick}
      >
        {this.props.label}
        {this.props.order > 0 ? ` [${this.props.order}] ` : ""}
        <button
          className={S.TabHandleCloseBtn}
          onClick={this.props.onCloseClick}
        >
          <i className="fas fa-times" />
        </button>
      </div>
    );
  }
}

@inject(({ workbench }) => {
  return {
    openedScreenItems: getOpenedScreenItems(workbench),
    onScreenTabHandleClick: getApplicationLifecycle(workbench)
      .onScreenTabHandleClick,
    onScreenTabCloseClick: getApplicationLifecycle(workbench)
      .onScreenTabCloseClick
  };
})
@observer
export class ScreenArea extends React.Component<{
  openedScreenItems?: IOpenedScreen[];
  onScreenTabHandleClick?: (event: any, openedScreen: IOpenedScreen) => void;
  onScreenTabCloseClick?: (event: any, openedScreen: IOpenedScreen) => void;
}> {
  render() {
    return (
      <div className={S.Root}>
        {/*<ColumnsDialog />*/}
        {/*<ModalWindowOverlay>
          <ModalWindow
            title="Columns"
            buttonsCenter={
              <>
                <button>OK</button>
                <button>Save As...</button>
                <button>Cancel</button>
              </>
            }
            buttonsLeft={null}
            buttonsRight={null}
          >
            <div className
          </ModalWindow>
          </ModalWindowOverlay>*/}

        <div className={S.TabHandles}>
          <Observer>
            {() => (
              <>
                {this.props.openedScreenItems!.map(item => (
                  <MainViewHandle
                    key={`${item.menuItemId}@${item.order}`}
                    label={item.title}
                    order={item.order}
                    isActive={item.isActive}
                    onClick={(event: any) =>
                      this.props.onScreenTabHandleClick!(event, item)
                    }
                    onCloseClick={(event: any) =>
                      this.props.onScreenTabCloseClick!(event, item)
                    }
                  />
                ))}
              </>
            )}
          </Observer>
        </div>
        <Observer>
          {() => (
            <>
              {this.props.openedScreenItems!.map(item => (
                <ScreenBuilder
                  key={`${item.menuItemId}@${item.order}`}
                  openedScreen={item}
                />
              ))}
            </>
          )}
        </Observer>
      </div>
    );
  }
}
