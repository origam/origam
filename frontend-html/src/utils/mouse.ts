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

import { createMachine, createActor } from "xstate";

interface MouseContext {}

interface MouseDownEvent {
  type: "MOUSE_DOWN";
  payload: { domEvent: globalThis.MouseEvent };
}

export function preventDoubleclickSelect() {
  const interpreter = createActor(
    createMachine<MouseContext, MouseDownEvent>(
      {
        id: "selectionPreventer",
        initial: "IDLE",
        states: {
          IDLE: {
            on: {
              MOUSE_DOWN: {
                target: "DEAD_PERIOD",
                guard: "isNotEditable",
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
          preventDefault: (ctx, {payload: {domEvent}}) => {
            domEvent.preventDefault();
          },
        },
        guards: {
          isNotEditable: (ctx, {payload: {domEvent}}) => {
            const targetTag = domEvent.target.tagName.toLowerCase();
            return targetTag !== "textarea" && targetTag !== "input";
          },
        },
      }
    )
  ).start();

  window.addEventListener("mousedown", (e) => {
    interpreter.send({type: "MOUSE_DOWN", payload: {domEvent: e}});
  });
}
