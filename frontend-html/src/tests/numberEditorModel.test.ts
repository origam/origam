import { createNumberEditorModel } from "gui/Components/ScreenElements/Editors/NumberEditorModel";

test("loads number editor model decorators without redecorating overrides", () => {
  expect(createNumberEditorModel).toBeDefined();
});
