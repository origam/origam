import { createMachine, interpret } from "xstate";

export function preventDoubleclickSelect() {
  const interpreter = interpret(
    createMachine(
      {
        id: "selectionPreventer",
        initial: "IDLE",
        states: {
          IDLE: {
            on: {
              MOUSE_DOWN: {
                target: "DEAD_PERIOD",
                cond: "isNotEditable",
              },
            },
          },
          DEAD_PERIOD: {
            on: {
              MOUSE_DOWN: {
                actions: "preventDefault",
                target: "DEAD_PERIOD",
              },
            },
            after: {
              500: "IDLE",
            },
          },
        },
      },
      {
        actions: {
          preventDefault: (ctx, { payload: { domEvent } }) => {
            domEvent.preventDefault();
          },
        },
        guards: {
          isNotEditable: (ctx, { payload: { domEvent } }) => {
            const targetTag = domEvent.target.tagName.toLowerCase();
            return targetTag !== "textarea" && targetTag !== "input";
          },
        },
      }
    )
  ).start();

  window.addEventListener("mousedown", (e) => {
    interpreter.send({ type: "MOUSE_DOWN", payload: { domEvent: e } });
  });
}
