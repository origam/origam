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
import { action, observable, runInAction } from "mobx";
import { observer } from "mobx-react";
import React from "react";
import S from "./ColorEditor.module.scss";
import { ColorResult, SketchPicker } from "react-color";
import { createMachine, interpret } from "xstate";

import { IFocusable } from "model/entities/FormFocusManager";
import { requestFocus } from "utils/focus";

@observer
export default class ColorEditor extends React.Component<{
  value: string | null;
  isReadOnly?: boolean;
  onChange?: (value: string | null) => void;
  onFocus?: () => void;
  onBlur?: (event: any) => void;
  onKeyDown?(event: any): void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
}> {
  refContainer = (elm: any) => (this.elmContainer = elm);
  elmContainer: any;

  refDropdowner = (elm: any) => (this.elmDropdowner = elm);
  elmDropdowner: Dropdowner | null = null;
  refDroppedPanelContainer = (elm: any) => (this.elmDroppedPanelContainer = elm);
  elmDroppedPanelContainer: any;

  elmInput: any = null;
  refInput = (elm: any) => {
    this.elmInput = elm;
    if (this.elmInput && this.props.subscribeToFocusManager) {
      this.props.subscribeToFocusManager(this.elmInput);
    }
  };

  constructor(props: any) {
    super(props);
    this.revertAppliedValue();
  }

  componentDidUpdate(prevProps: any) {
    runInAction(() => {
      if (prevProps.value !== this.props.value) {
        this.revertAppliedValue();
      }
    });
  }

  componentWillUnmount() {
    this.props.onBlur?.({target: this.elmInput});
  }

  machine = interpret(
    createMachine(
      {
        predictableActionArguments: true,
        id: "colorEditor",
        type: "parallel",
        states: {
          UI: {
            initial: "CLOSED_INACTIVE",
            states: {
              CLOSED_INACTIVE: {
                on: {
                  DROPDOWN_SYMBOL_MOUSE_DOWN: {
                    actions: "setDroppedDown",
                  },
                  PICKER_DROPPED_DOWN: "OPEN",
                  INPUT_FIELD_FOCUS: "CLOSED_ACTIVE",
                },
              },
              CLOSED_ACTIVE: {
                on: {
                  DROPDOWN_SYMBOL_MOUSE_DOWN: {
                    actions: "setDroppedDown",
                  },
                  PICKER_DROPPED_DOWN: "OPEN",
                  INPUT_FIELD_BLUR: {
                    actions: ["signalComponentBlur", "commitAppliedValue"],
                    target: "CLOSED_INACTIVE",
                  },
                  INPUT_FIELD_KEY_DOWN: [
                    {
                      actions: "commitAppliedValue",
                      cond: "eventKeyIsEnter",
                    },
                    {
                      actions: "revertAppliedValue",
                      cond: "eventKeyIsEscape",
                    },
                  ],
                },
              },
              OPEN: {
                entry: "pickValueFromApplied",
                on: {
                  PICKER_KEY_DOWN: [
                    {
                      actions: ["applyPickedValue", "commitAppliedValue", "setDroppedUp"],
                      cond: "eventKeyIsEnter",
                    },
                    {
                      cond: "eventKeyIsEscape",
                      actions: ["setDroppedUp"],
                    },
                  ],
                  PICKER_OUTSIDE_INTERACTION: {
                    target: "CLOSED_INACTIVE",
                    actions: [
                      "applyPickedValue",
                      "commitAppliedValue",
                      "signalComponentBlur",
                      "setDroppedUp",
                    ],
                  },
                  PICKER_DROPPED_UP: {
                    target: "CLOSED_ACTIVE",
                  },
                },
                after: {
                  100: {
                    actions: "focusPickerContainer",
                  },
                },
              },
            },
          },
        },
      },
      {
        actions: {
          applyPickedValue: (ctx, event) => {
            this.applyPickedValue();
          },
          commitAppliedValue: (ctx, event) => {
            this.commitAppliedValue();
          },
          revertAppliedValue: (ctx, event) => {
            this.revertAppliedValue();
          },
          pickValueFromApplied: (ctx, event) => {
            this.pickValueFromApplied();
          },
          setDroppedDown: (ctx, event) => {
            this.setDropped(true);
          },
          setDroppedUp: (ctx, event) => {
            this.setDropped(false);
          },
          focusPickerContainer: (ctx, event) => {
            requestFocus(this.elmDroppedPanelContainer);
          },
          focusInputField: (ctx, event) => {
            this.elmInput?.select();
          },
          signalComponentBlur: (ctx, event) => {
            this.props.onBlur?.({target: this.elmInput});
          },
        },
        guards: {
          eventKeyIsEnter: (ctx, event) => {
            return event.payload.event.key === "Enter";
          },
          eventKeyIsEscape: (ctx, event) => {
            return event.payload.event.key === "Escape";
          },
        },
      }
    )
  )
    .onTransition((state, event) => {
      if (state.changed) {
        this.machineState = state;
      }
    })
    .start();

  @observable machineState = this.machine.state;

  @action.bound setDropped(state: boolean) {
    this.elmDropdowner?.setDropped(state);
  }

  get isDroppedDown() {
    return this.machineState.matches("UI.OPEN");
  }

  @observable pickedColor: string | null = "#000000";
  @observable appliedValue: string | null = "#000000";

  @action.bound
  handleColorChange(colorResult: ColorResult, event: any) {
    this.pickedColor = colorResult.hex.toUpperCase();
  }

  @action.bound applyPickedValue() {
    this.appliedValue = this.pickedColor;
  }

  @action.bound commitAppliedValue() {
    if (this.appliedValue !== this.props.value) {
      this.props.onChange?.(this.appliedValue);
    }
  }

  @action.bound revertAppliedValue() {
    this.appliedValue = this.props.value;
  }

  @action.bound pickValueFromApplied() {
    this.pickedColor = this.appliedValue;
  }

  @action.bound send(evt: any) {
    this.machine.send(evt);
  }

  refTrigger: any;
  @action.bound setRefTrigger(elm: any) {
    this.refTrigger = elm;
  }

  @action.bound refColorDiv(elm: any) {
    this.refTrigger?.(elm);
    this.refInput(elm);
  }

  render() {
    return (
      <Dropdowner
        ref={this.refDropdowner}
        onContainerMouseDown={undefined /*this.handleContainerMouseDown*/}
        onDroppedUp={() => this.send({type: "PICKER_DROPPED_UP"})}
        onDroppedDown={() => this.send({type: "PICKER_DROPPED_DOWN"})}
        onOutsideInteraction={() => this.send({type: "PICKER_OUTSIDE_INTERACTION"})}
        trigger={({ refTrigger, setDropped }) => {
          this.setRefTrigger(refTrigger);
          return (
            <div
              className={S.editorContainer}
              ref={this.refContainer}
              style={{
                zIndex: this.isDroppedDown ? 1000 : undefined,
              }}
            >
              <div
                className={S.colorDiv}
                tabIndex={0}
                ref={this.refColorDiv}
                onMouseDown={() => this.send({type: "DROPDOWN_SYMBOL_MOUSE_DOWN"})}
                onFocus={() => this.send({type: "INPUT_FIELD_FOCUS"})}
                onBlur={() => this.send({type: "INPUT_FIELD_BLUR"})}
                onKeyDown={(event) => this.props.onKeyDown?.(event)}
              >
                <div
                  className={S.colorRect}
                  style={{
                    backgroundColor:
                      (this.isDroppedDown ? this.pickedColor : this.appliedValue) || "#000000",
                  }}
                />
              </div>
            </div>
          );
        }}
        content={({setDropped}) => (
            <div
              tabIndex={0}
              ref={this.refDroppedPanelContainer}
              className={S.droppedPanelContainer}
              onKeyDown={(event: any) => {
                this.send({type: "PICKER_KEY_DOWN", payload: {event}});
              }}
            >
              <SketchPicker
                color={this.pickedColor || "#000000"}
                onChange={this.handleColorChange}
                disableAlpha={true}
              />
            </div>
        )}
      />
    );
  }
}
