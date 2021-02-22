import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { action, observable, runInAction } from "mobx";
import { observer } from "mobx-react";
import React from "react";
import S from "./ColorEditor.module.scss";
import { ColorResult, SketchPicker } from "react-color";
import { createMachine, interpret } from "xstate";
import { identity } from "lodash";

@observer
export default class ColorEditor extends React.Component<{
  value: string | null;
  isReadOnly?: boolean;
  onChange?: (value: string | null) => void;
  onFocus?: () => void;
  onBlur?: () => void;
}> {
  refContainer = (elm: any) => (this.elmContainer = elm);
  elmContainer: any;
  refInput = (elm: any) => (this.elmInput = elm);
  elmInput: any;
  refDropdowner = (elm: any) => (this.elmDropdowner = elm);
  elmDropdowner: Dropdowner | null = null;
  refDroppedPanelContainer = (elm: any) => (this.elmDroppedPanelContainer = elm);
  elmDroppedPanelContainer: any;

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

  machine = interpret(
    createMachine(
      {
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
                /*after: {
                  100: {
                    actions: "focusInputField",
                  },
                },*/
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
            this.elmDroppedPanelContainer?.focus();
          },
          focusInputField: (ctx, event) => {
            this.elmInput?.select();
          },
          signalComponentBlur: (ctx, event) => {
            this.props.onBlur?.();
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
      console.log(state.value, state);
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
    this.props.onChange?.(this.appliedValue);
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

  render() {
    return (
      <Dropdowner
        ref={this.refDropdowner}
        onContainerMouseDown={undefined /*this.handleContainerMouseDown*/}
        onDroppedUp={() => this.send({ type: "PICKER_DROPPED_UP" })}
        onDroppedDown={() => this.send({ type: "PICKER_DROPPED_DOWN" })}
        onOutsideInteraction={() => this.send({ type: "PICKER_OUTSIDE_INTERACTION" })}
        trigger={({ refTrigger, setDropped }) => (
          <div
            className={S.editorContainer}
            ref={this.refContainer}
            style={{
              zIndex: this.isDroppedDown ? 1000 : undefined,
            }}
          >
            <input
              style={{}}
              className={S.input}
              type="text"
              readOnly={this.props.isReadOnly}
              onBlur={() => this.send({ type: "INPUT_FIELD_BLUR" })}
              onFocus={() => this.send({ type: "INPUT_FIELD_FOCUS" })}
              ref={this.refInput}
              // value={this.textfieldValue}
              // readOnly={this.props.isReadOnly}
              // onChange={this.handleTextfieldChange}
              //onClick={this.props.onClick}
              //onDoubleClick={this.props.onDoubleClick}
              onKeyDown={(event) => this.send({ type: "INPUT_FIELD_KEY_DOWN", payload: { event } })}
              value={(this.isDroppedDown ? this.pickedColor : this.appliedValue) || "#000000"}
            />
            {!this.props.isReadOnly && (
              <div
                className={S.dropdownSymbol}
                onMouseDown={() =>
                  !this.props.isReadOnly &&
                  this.machine.send({ type: "DROPDOWN_SYMBOL_MOUSE_DOWN" })
                }
                ref={refTrigger}
              >
                <i className="fas fa-palette" />
              </div>
            )}
          </div>
        )}
        content={({ setDropped }) => (
          <div
            tabIndex={0}
            ref={this.refDroppedPanelContainer}
            className={S.droppedPanelContainer}
            onKeyDown={(event: any) => this.send({ type: "PICKER_KEY_DOWN", payload: { event } })}
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
