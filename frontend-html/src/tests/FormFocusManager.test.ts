/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

import { FormFocusManager } from "model/entities/FormFocusManager";
import { TabIndex } from "model/entities/TabIndexOwner";

jest.mock("model/actions-ui/ScreenToolbar/openSearchWindow", () => ({
  isGlobalAutoFocusDisabled: () => false,
}));

function createField({
  visible,
  disabled,
}: {
  visible: boolean;
  disabled: boolean;
}) {
  return {
    focus: jest.fn(),
    checkVisibility: jest.fn(() => visible),
    disabled,
  };
}

function createManager(fields: ReturnType<typeof createField>[] = []) {
  const manager = new FormFocusManager({
    $type_IDataView: 1,
    isFormViewActive: () => true,
  });
  fields.forEach((field, index) => {
    manager.subscribe(field, String(index), TabIndex.create(String(index)));
  });
  return manager;
}

beforeEach(() => {
  jest.useFakeTimers();
});

afterEach(() => {
  jest.clearAllTimers();
  jest.useRealTimers();
});

describe.each(["focusNext", "focusPrevious"] as const)("%s", (navigate) => {
  test("returns safely when all fields are hidden", () => {
    const fields = [
      createField({ visible: false, disabled: false }),
      createField({ visible: false, disabled: false }),
    ];
    const manager = createManager(fields);

    manager[navigate](fields[0]);
    jest.runAllTimers();

    fields.forEach((field) => expect(field.focus).not.toHaveBeenCalled());
    expect(manager.lastFocused).toBeUndefined();
  });

  test("returns safely when all fields are disabled", () => {
    const fields = [
      createField({ visible: true, disabled: true }),
      createField({ visible: true, disabled: true }),
    ];
    const manager = createManager(fields);

    manager[navigate](fields[0]);
    jest.runAllTimers();

    fields.forEach((field) => expect(field.focus).not.toHaveBeenCalled());
    expect(manager.lastFocused).toBeUndefined();
  });

  test("preserves the last focus when no field is eligible", () => {
    const fields = [
      createField({ visible: false, disabled: false }),
      createField({ visible: true, disabled: true }),
    ];
    const manager = createManager(fields);
    manager.setLastFocused(fields[0]);

    manager[navigate](fields[0]);
    jest.runAllTimers();

    fields.forEach((field) => expect(field.focus).not.toHaveBeenCalled());
    expect(manager.lastFocused).toBe(fields[0]);
  });

  test("returns safely when there are no registered fields", () => {
    const manager = createManager();

    manager[navigate](document.body);
    jest.runAllTimers();

    expect(manager.lastFocused).toBeUndefined();
  });

  test("skips hidden and disabled fields in tab order", () => {
    const fields = [
      createField({ visible: true, disabled: false }),
      createField({ visible: false, disabled: false }),
      createField({ visible: true, disabled: true }),
      createField({ visible: true, disabled: false }),
    ];
    const manager = createManager(fields);
    const start = navigate === "focusNext" ? fields[0] : fields[3];
    const target = navigate === "focusNext" ? fields[3] : fields[0];

    manager[navigate](start);
    jest.runAllTimers();

    expect(target.focus).toHaveBeenCalledTimes(1);
    expect(start.focus).not.toHaveBeenCalled();
    expect(fields[1].focus).not.toHaveBeenCalled();
    expect(fields[2].focus).not.toHaveBeenCalled();
    expect(manager.lastFocused).toBe(target);
  });

  test("wraps around the ends of the form", () => {
    const fields = [
      createField({ visible: true, disabled: false }),
      createField({ visible: true, disabled: false }),
      createField({ visible: true, disabled: false }),
    ];
    const manager = createManager(fields);
    const start = navigate === "focusNext" ? fields[2] : fields[0];
    const target = navigate === "focusNext" ? fields[0] : fields[2];

    manager[navigate](start);
    jest.runAllTimers();

    expect(target.focus).toHaveBeenCalledTimes(1);
    expect(manager.lastFocused).toBe(target);
  });

  test("starts at the appropriate end when the active element is unregistered", () => {
    const fields = [
      createField({ visible: true, disabled: false }),
      createField({ visible: true, disabled: false }),
    ];
    const manager = createManager(fields);
    const target = navigate === "focusNext" ? fields[0] : fields[1];

    manager[navigate](document.body);
    jest.runAllTimers();

    expect(target.focus).toHaveBeenCalledTimes(1);
  });

  test("finds an eligible field beyond twenty disabled fields", () => {
    const target = createField({ visible: true, disabled: false });
    const fields = [
      target,
      ...Array.from({ length: 25 }, () =>
        createField({ visible: true, disabled: true }),
      ),
    ];
    const manager = createManager(fields);

    manager[navigate](fields[navigate === "focusNext" ? 1 : 25]);
    jest.runAllTimers();

    expect(target.focus).toHaveBeenCalledTimes(1);
    expect(manager.lastFocused).toBe(target);
  });

  test("can return to the only eligible field", () => {
    const target = createField({ visible: true, disabled: false });
    const manager = createManager([
      target,
      createField({ visible: false, disabled: false }),
      createField({ visible: true, disabled: true }),
    ]);

    manager[navigate](target);
    jest.runAllTimers();

    expect(target.focus).toHaveBeenCalledTimes(1);
  });
});

describe("forceAutoFocus", () => {
  test("does nothing when all fields are hidden", () => {
    const fields = [
      createField({ visible: false, disabled: true }),
      createField({ visible: false, disabled: false }),
    ];
    const manager = createManager(fields);

    manager.forceAutoFocus();
    jest.runAllTimers();

    fields.forEach((field) => expect(field.focus).not.toHaveBeenCalled());
    expect(manager.lastFocused).toBeUndefined();
  });

  test("focuses the first visible enabled field in tab order", () => {
    const target = createField({ visible: true, disabled: false });
    const fields = [
      createField({ visible: false, disabled: false }),
      createField({ visible: true, disabled: true }),
      target,
    ];
    const manager = createManager(fields);

    manager.forceAutoFocus();
    jest.runAllTimers();

    expect(target.focus).toHaveBeenCalledTimes(1);
    expect(fields[0].focus).not.toHaveBeenCalled();
    expect(fields[1].focus).not.toHaveBeenCalled();
    expect(manager.lastFocused).toBe(target);
  });

  test("returns safely for an empty form without a placeholder", () => {
    const manager = createManager();

    manager.forceAutoFocus();
    jest.runAllTimers();

    expect(manager.lastFocused).toBeUndefined();
  });

  test("focuses the backup placeholder for an empty form", () => {
    const manager = createManager();
    const placeholder = document.createElement("input");
    const focus = jest.spyOn(placeholder, "focus");
    manager.setBackupFocusPlaceHolder(placeholder);

    manager.forceAutoFocus();
    jest.runAllTimers();

    expect(focus).toHaveBeenCalledTimes(1);
  });
});
