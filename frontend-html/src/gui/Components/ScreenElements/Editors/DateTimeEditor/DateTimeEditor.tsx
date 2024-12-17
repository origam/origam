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

import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { action, computed, flow, observable, runInAction } from "mobx";
import { observer, Observer } from "mobx-react";
import moment from "moment";
import * as React from "react";
import { IFocusable } from "model/entities/FormFocusManager";
import S from "gui/Components/ScreenElements/Editors/DateTimeEditor/DateTimeEditor.module.scss";
import { createPortal } from "react-dom";
import { CalendarWidget } from "gui/Components/ScreenElements/Editors/DateTimeEditor/CalendarWidget";
import {
  DateEditorModel,
  IEditorState
} from "gui/Components/ScreenElements/Editors/DateTimeEditor/DateEditorModel";
import { requestFocus } from "utils/focus";


class DesktopEditorState implements IEditorState{
  constructor(value: string | null) {
    this.initialValue = value;
  }

  @observable
  initialValue : string | null;
}

@observer
export class DateTimeEditor extends React.Component<{
  id?: string;
  value: string | null;
  outputFormat: string;
  outputFormatToShow: string;
  isReadOnly?: boolean;
  autoFocus?: boolean;
  foregroundColor?: string;
  backgroundColor?: string;
  onChange?: (event: any, isoDay: string | undefined | null) => Promise<void>;
  onChangeByCalendar?: (event: any, isoDay: string) => void;
  onClick?: (event: any) => void;
  onDoubleClick?: (event: any) => void;
  onKeyDown?: (event: any) => void;
  onMount?(onChange?: (value: any) => void): void;
  onEditorBlur?: (event: any) => Promise<void>;
  subscribeToFocusManager?: (obj: IFocusable, onBlur: ()=> Promise<void>) => void;
  className?: string;
}> {
  @observable isDroppedDown = false;

  @observable isShowFormatHintTooltip = false;

  refDropdowner = (elm: Dropdowner | null) => (this.elmDropdowner = elm);
  elmDropdowner: Dropdowner | null = null;

  editorState = new DesktopEditorState(this.props.value);

  editorModel = new DateEditorModel(
    this.editorState,
    this.props.outputFormat,
    this.props.onChange,
    this.props.onClick,
    this.props.onKeyDown,
    this.props.onEditorBlur,
    this.props.onChangeByCalendar);

  @action.bound handleDropperClick(event: any) {
    event.stopPropagation();
    this.makeDroppedDown();
  }

  @action.bound makeDroppedDown() {
    if (!this.isDroppedDown) {
      this.isDroppedDown = true;
      window.addEventListener("click", this.handleWindowClick);
    }
  }

  @action.bound makeDroppedUp() {
    if (this.isDroppedDown) {
      this.isDroppedDown = false;
      window.removeEventListener("click", this.handleWindowClick);
    }
  }

  @action.bound handleWindowClick(event: any) {
    if (this.elmContainer && !this.elmContainer.contains(event.target)) {
      this.makeDroppedUp();
    }
  }

  disposers: any[] = [];

  componentDidMount() {
    this.makeFocusedIfNeeded();
    if (this.elmInput && this.props.subscribeToFocusManager) {
      this.props.subscribeToFocusManager(
        this.elmInput,
        async ()=> {
          const event = {} as any;
          event.type = "click";
          await this.handleInputBlur(event)();
        });
    }
    this.props.onMount?.(value => {
      if (this.elmInput) {
        this.elmInput.value = value;
        const event = {target: this.elmInput};
        this.handleTextFieldChange(event);
      }
    });
  }

  componentWillUnmount() {
    this.props.onEditorBlur?.({target: this.elmInput});
    this.disposers.forEach((d) => d());
  }

  componentDidUpdate(prevProps: { value: string | null }) {
    runInAction(() => {
      if (prevProps.value !== null && this.props.value === null) {
        this.editorModel.dirtyTextualValue = "";
      }
    });
    this.editorState.initialValue = this.props.value;
  }

  @action.bound
  makeFocusedIfNeeded() {
    setTimeout(() => {
      if ((this.props.autoFocus) && this.elmInput) {
        this.elmInput.select();
        requestFocus(this.elmInput);
        this.elmInput.scrollLeft = 0;
      }
    });
  }

  _hLocationInterval: any;

  @action.bound
  handleWindowMouseWheel(e: any) {
    this.setShowFormatHint(false);
  }

  @action.bound
  setShowFormatHint(state: boolean) {
    if (state && !this.isShowFormatHintTooltip) {
      this.measureInputElement();
      this._hLocationInterval = setInterval(() => this.measureInputElement(), 500);
      window.addEventListener("mousewheel", this.handleWindowMouseWheel);
    } else if (!state && this.isShowFormatHintTooltip) {
      clearInterval(this._hLocationInterval);
      window.removeEventListener("mousewheel", this.handleWindowMouseWheel);
    }
    this.isShowFormatHintTooltip = state;
  }

  @action.bound
  handleInputBlur(event: any) {
    const self = this;
    return flow(function*() {
      self.setShowFormatHint(false);
      yield self.editorModel.handleInputBlur(event);
    });
  }

  @action.bound handleKeyDown(event: any) {
    if (event.key === "Escape" || event.key === "Tab") {
      this.setShowFormatHint(false)
      if(this.elmDropdowner?.isDropped){
        event.closedADropdown = true;
        this.elmDropdowner.setDropped(false);
      }
    }
    this.editorModel.handleKeyDown(event);
  }

  @action.bound handleContainerMouseDown(event: any) {
    setTimeout(() => {
      requestFocus(this.elmInput);
    }, 30);
  }

  refContainer = (elm: HTMLDivElement | null) => (this.elmContainer = elm);
  elmContainer: HTMLDivElement | null = null;
  refInput = (elm: HTMLInputElement | null) => {
    this.elmInput = elm;
  };
  @observable inputRect: any;
  elmInput: HTMLInputElement | null = null;

  @action.bound
  measureInputElement() {
    if (this.elmInput) {
      this.inputRect = this.elmInput.getBoundingClientRect();
    } else {
      this.inputRect = undefined;
    }
  }

  @computed get isTooltipShown() {
    return (
      this.editorModel.textFieldValue !== undefined &&
      (!moment(this.editorModel.textFieldValue, this.props.outputFormat) ||
        this.editorModel.formattedMomentValue !== this.editorModel.textFieldValue)
    );
  }

  @action.bound handleTextFieldChange(event: any) {
    this.setShowFormatHint(true);
    this.editorModel.handleTextFieldChange(event);
  }

  @action.bound handleDayClick(event: any, day: moment.Moment) {
    this.elmDropdowner && this.elmDropdowner.setDropped(false);
    this.editorModel.handleDayClick(event, day);
  }

  @action.bound
  handleFocus(event: any) {
    if (this.elmInput) {
      this.elmInput.select();
      this.elmInput.scrollLeft = 0;
    }
  }

  customFormatContainsDate() {
    return (
      this.props.outputFormat.includes("D") ||
      this.props.outputFormat.includes("M") ||
      this.props.outputFormat.includes("Y")
    );
  }

  renderWithCalendarWidget() {
    return (
      <Dropdowner
        ref={this.refDropdowner}
        onContainerMouseDown={this.handleContainerMouseDown}
        trigger={({refTrigger, setDropped}) => (
          <div
            className={S.editorContainer}
            ref={this.refContainer}
            style={{
              zIndex: this.isDroppedDown ? 1000 : undefined,
            }}
          >
            <Observer>
              {() => (
                <>
                  {this.isShowFormatHintTooltip && (
                    <FormatHintTooltip
                      boundingRect={this.inputRect}
                      line1={this.editorModel.autocompletedText}
                      line2={this.props.outputFormatToShow}
                    />
                  )}
                  <input
                    id={this.props.id}
                    title={this.editorModel.autocompletedText + '\n' + this.props.outputFormatToShow}
                    style={{
                      color: this.props.foregroundColor,
                      backgroundColor: this.props.backgroundColor,
                    }}
                    autoComplete={"off"}
                    className={S.input +" "+ this.props.className + " " + (this.props.isReadOnly ? S.readOnlyInput : "")}
                    type="text"
                    onBlur={event => this.handleInputBlur(event)()}
                    onFocus={this.handleFocus}
                    ref={(elm) => {
                      this.refInput(elm);
                    }}
                    value={this.editorModel.textFieldValue}
                    readOnly={this.props.isReadOnly}
                    onChange={this.handleTextFieldChange}
                    onClick={this.props.onClick}
                    onDoubleClick={this.props.onDoubleClick}
                    onKeyDown={this.handleKeyDown}
                    onDragStart={(e: any) =>  e.preventDefault()}
                  />
                </>
              )}
            </Observer>

            {!this.props.isReadOnly && (
              <div
                className={S.dropdownSymbol}
                onMouseDown={() => setDropped(true)}
                ref={refTrigger}
              >
                <i className="far fa-calendar-alt"/>
              </div>
            )}
          </div>
        )}
        content={() => (
          <div className={S.droppedPanelContainer}>
            <CalendarWidget
              onDayClick={this.handleDayClick}
              initialDisplayDate={this.editorModel.momentValue?.isValid() ? this.editorModel.momentValue : moment()}
              selectedDay={this.editorModel.momentValue?.isValid() ? this.editorModel.momentValue : moment()}
            />
          </div>
        )}
      />
    );
  }

  renderInputFieldOnly() {
    return (
      <div
        className={S.editorContainer}
        ref={this.refContainer}
        style={{
          zIndex: this.isDroppedDown ? 1000 : undefined,
        }}
      >
        <input
          id={this.props.id}
          style={{
            color: this.props.foregroundColor,
            backgroundColor: this.props.backgroundColor,
          }}
          autoComplete={"off"}
          title={this.editorModel.autocompletedText + '\n' + this.props.outputFormat}
          className={S.input + " " + (this.props.isReadOnly ? S.readOnlyInput : "")}
          type="text"
          onBlur={event => this.handleInputBlur(event)()}
          onFocus={this.handleFocus}
          ref={this.refInput}
          value={this.editorModel.textFieldValue}
          readOnly={this.props.isReadOnly}
          onChange={this.handleTextFieldChange}
          onClick={this.props.onClick}
          onDoubleClick={this.props.onDoubleClick}
          onKeyDown={this.handleKeyDown}
          onDragStart={(e: any) =>  e.preventDefault()}
        />
      </div>
    );
  }

  render() {
    if (!this.props.outputFormat || this.customFormatContainsDate()) {
      return this.renderWithCalendarWidget();
    } else {
      return this.renderInputFieldOnly();
    }
  }
}

export function FormatHintTooltip(props: { boundingRect?: any; line1?: string; line2?: string }) {
  const [tooltipHeight, setTooltipHeight] = React.useState(0);

  function refTooltip(elm: any) {
    if (elm) {
      setTooltipHeight(elm.getBoundingClientRect().height);
    }
  }

  if (!props.boundingRect || (!props.line1 && !props.line2)) return null;
  const bounds = props.boundingRect;

  return createPortal(
    <div
      ref={refTooltip}
      className={S.formatHintTooltip}
      style={{top: bounds.top - 1 - tooltipHeight, left: bounds.left}}
    >
      {props.line1}
      {props.line2 && <br/>}
      {props.line2}
    </div>,
    document.getElementById("tooltip-portal")!
  );
}
