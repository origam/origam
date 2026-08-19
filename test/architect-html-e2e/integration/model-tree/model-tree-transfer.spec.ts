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

import { expect, test, type Locator, type Page } from "@playwright/test";
import fs from "node:fs";
import { activatePackage } from "@support/activatePackage";
import { dragTreeNode } from "@support/dragTreeNode";
import { modelFilePath, resetBackend } from "@support/resetBackend";

// A move is only allowed inside the active package.
const PACKAGE = "AutomaticTests";
const CONSTANTS_DIR = "AutomaticTests/DataConstant";

test.describe("Model tree move and copy (real backend)", () => {
  test.describe.configure({ mode: "default", timeout: 45_000 });
  test.use({ actionTimeout: 10_000, navigationTimeout: 20_000 });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, PACKAGE);
  });

  test("Drag and drop moves a constant out of its group", async ({ page }) => {
    await openConstants(page);
    await page.getByTestId("tree-toggle-AutomaticTests").click();

    await expectMoveRequest(page, () =>
      dragTreeNode(
        page,
        page.getByTestId("tree-node-GroupedConstant"),
        page.getByTestId("tree-node-Constants"),
      ),
    );

    await expectTreeSettled(page, "GroupedConstant");
    await page.getByTestId("tree-toggle-AutomaticTests").click();
    await expect(page.getByTestId("tree-node-GroupedConstant")).toBeVisible();

    await expectModelFile(`${CONSTANTS_DIR}/GroupedConstant.origam`, true);
    await expectModelFile(
      `${CONSTANTS_DIR}/AutomaticTests/GroupedConstant.origam`,
      false,
    );
  });

  test("Cut and paste moves a constant into a group", async ({ page }) => {
    await openConstants(page);

    const constant = page.getByTestId("tree-node-UngroupedConstant");
    await constant.click();
    await expectDropTargets(page, () => page.keyboard.press("Control+x"));
    await expect(constant).toHaveClass(/cutNode/);

    await page.getByTestId("tree-node-AutomaticTests").click();
    await expectMoveRequest(page, () => page.keyboard.press("Control+v"));

    const group = page.getByTestId("tree-toggle-AutomaticTests");
    await expectTreeSettled(page, "UngroupedConstant");
    // The refresh expands the target group, collapsing it earlier is undone.
    await expect(group).toHaveText("▼");

    await group.click();
    await expect(page.getByTestId("tree-node-UngroupedConstant")).toHaveCount(
      0,
    );

    await expectModelFile(
      `${CONSTANTS_DIR}/AutomaticTests/UngroupedConstant.origam`,
      true,
    );
    await expectModelFile(`${CONSTANTS_DIR}/UngroupedConstant.origam`, false);
  });

  test("Ctrl and drag copies a constant out of its group", async ({ page }) => {
    await openConstants(page);
    await page.getByTestId("tree-toggle-AutomaticTests").click();

    await expectMoveRequest(page, () =>
      dragTreeNode(
        page,
        page.getByTestId("tree-node-GroupedConstant"),
        page.getByTestId("tree-node-Constants"),
        { copy: true },
      ),
    );

    await expect(
      page.getByTestId("tree-node-Copy of GroupedConstant"),
    ).toBeVisible();
    await page.getByTestId("tree-toggle-AutomaticTests").click();
    await expect(page.getByTestId("tree-node-GroupedConstant")).toHaveCount(0);
    await expect(
      page.getByTestId("tree-node-Copy of GroupedConstant"),
    ).toBeVisible();

    await expectModelFile(
      `${CONSTANTS_DIR}/Copy of GroupedConstant.origam`,
      true,
    );
    await expectModelFile(
      `${CONSTANTS_DIR}/AutomaticTests/GroupedConstant.origam`,
      true,
    );
  });

  test("Copy and paste copies a constant into a group", async ({ page }) => {
    await openConstants(page);

    await openContextMenu(page, "UngroupedConstant");
    await expectDropTargets(page, () =>
      menuItem(page, "UngroupedConstant", "tree-menu-copy").click(),
    );

    await openContextMenu(page, "AutomaticTests");
    await expectMoveRequest(page, () =>
      menuItem(page, "AutomaticTests", "tree-menu-paste").click(),
    );

    await expect(
      page.getByTestId("tree-node-Copy of UngroupedConstant"),
    ).toBeVisible();
    await expect(page.getByTestId("tree-node-UngroupedConstant")).toBeVisible();

    await expectModelFile(
      `${CONSTANTS_DIR}/AutomaticTests/Copy of UngroupedConstant.origam`,
      true,
    );
    await expectModelFile(`${CONSTANTS_DIR}/UngroupedConstant.origam`, true);
  });

  test("Cut from the context menu moves a constant into a group", async ({
    page,
  }) => {
    await openConstants(page);

    await openContextMenu(page, "UngroupedConstant");
    await expectDropTargets(page, () =>
      menuItem(page, "UngroupedConstant", "tree-menu-cut").click(),
    );
    await expect(page.getByTestId("tree-node-UngroupedConstant")).toHaveClass(
      /cutNode/,
    );

    await openContextMenu(page, "AutomaticTests");
    await expectMoveRequest(page, () =>
      menuItem(page, "AutomaticTests", "tree-menu-paste").click(),
    );

    await expectModelFile(
      `${CONSTANTS_DIR}/AutomaticTests/UngroupedConstant.origam`,
      true,
    );
    await expectModelFile(`${CONSTANTS_DIR}/UngroupedConstant.origam`, false);
  });

  test("Ctrl C copies an item of another package into the active one", async ({
    page,
  }) => {
    await openConstants(page);

    await page.getByTestId("tree-node-InitialUserCreated").click();
    await expectDropTargets(page, () => page.keyboard.press("Control+c"));

    await page.getByTestId("tree-node-AutomaticTests").click();
    await expectMoveRequest(page, () => page.keyboard.press("Control+v"));

    await expect(
      page.getByTestId("tree-node-Copy of InitialUserCreated"),
    ).toBeVisible();

    await expectModelFile(
      `${CONSTANTS_DIR}/AutomaticTests/Copy of InitialUserCreated.origam`,
      true,
    );
    await expectModelFile("Root/DataConstant/InitialUserCreated.origam", true);
  });

  test("Escape cancels a pending cut", async ({ page }) => {
    await openConstants(page);

    const constant = page.getByTestId("tree-node-UngroupedConstant");
    await constant.click();
    await expectDropTargets(page, () => page.keyboard.press("Control+x"));
    await expect(constant).toHaveClass(/cutNode/);

    await page.keyboard.press("Escape");
    await expect(constant).not.toHaveClass(/cutNode/);

    await openContextMenu(page, "AutomaticTests");
    await expect(
      menuItem(page, "AutomaticTests", "tree-menu-paste"),
    ).toHaveAttribute("aria-disabled", "true");
  });

  test("A group itself cannot be cut or copied", async ({ page }) => {
    await openConstants(page);

    await openContextMenu(page, "AutomaticTestsGroup");
    await expect(
      menuItem(page, "AutomaticTestsGroup", "tree-menu-cut"),
    ).toHaveAttribute("aria-disabled", "true");
    await expect(
      menuItem(page, "AutomaticTestsGroup", "tree-menu-copy"),
    ).toHaveAttribute("aria-disabled", "true");
  });

  test("Holding a dragged constant over a collapsed group expands it", async ({
    page,
  }) => {
    await openConstants(page);
    await page.getByTestId("tree-toggle-AutomaticTests").click();

    await expectMoveRequest(page, () =>
      dragTreeNode(
        page,
        page.getByTestId("tree-node-GroupedConstant"),
        page.getByTestId("tree-node-AutomaticTestsGroup"),
        {
          whileOverTarget: async () => {
            await expect(
              page.getByTestId("tree-node-TargetConstant"),
            ).toBeVisible();
          },
        },
      ),
    );

    await expectModelFile(
      `${CONSTANTS_DIR}/AutomaticTestsGroup/GroupedConstant.origam`,
      true,
    );
    await expectModelFile(
      `${CONSTANTS_DIR}/AutomaticTests/GroupedConstant.origam`,
      false,
    );
  });
});

async function openConstants(page: Page): Promise<void> {
  await page.goto("/");
  await page.getByTestId("tree-toggle-Data").click();
  await page.getByTestId("tree-toggle-Constants").click();
}

// Both parents reload after the move, so the node exists twice for a moment.
async function expectTreeSettled(page: Page, nodeText: string): Promise<void> {
  await expect(page.getByTestId(`tree-node-${nodeText}`)).toHaveCount(1);
}

// Every node renders its own menu and earlier ones stay around.
function menuItem(page: Page, nodeText: string, testId: string): Locator {
  return page
    .getByTestId(`tree-node-${nodeText}`)
    .locator("xpath=..")
    .getByTestId(testId);
}

// The menu opens before its items arrive from the server.
async function openContextMenu(page: Page, nodeText: string): Promise<void> {
  const pendingResponse = page.waitForResponse(
    (response) => response.url().includes("/Model/GetMenuItems"),
    { timeout: 10_000 },
  );
  await page.getByTestId(`tree-node-${nodeText}`).click({ button: "right" });
  await pendingResponse;
  await expectStable(menuItem(page, nodeText, "tree-menu-paste"));
}

// The arriving items change the height of the menu, which moves it.
async function expectStable(locator: Locator): Promise<void> {
  let previous: string | null = null;
  await expect
    .poll(
      async () => {
        const box = await locator.boundingBox();
        const current = box && `${box.x} ${box.y} ${box.width} ${box.height}`;
        const hasSettled = !!current && current === previous;
        previous = current;
        return hasSettled;
      },
      { message: "the context menu never stopped moving", timeout: 10_000 },
    )
    .toBe(true);
}

// The verdicts arrive for the whole tree and re-render every node.
async function expectDropTargets(
  page: Page,
  action: () => Promise<void>,
): Promise<void> {
  const pendingResponse = page.waitForResponse(
    (response) => response.url().includes("/Model/GetDropTargets"),
    { timeout: 10_000 },
  );
  await action();
  await pendingResponse;
}

// Reports a server side rejection instead of a later missing node.
async function expectMoveRequest(
  page: Page,
  action: () => Promise<void>,
): Promise<void> {
  const pendingResponse = page.waitForResponse(
    (response) => response.url().includes("/Model/MoveNode"),
    { timeout: 10_000 },
  );
  await action();
  const response = await pendingResponse;
  expect(response.ok(), await response.text()).toBeTruthy();
}

async function expectModelFile(
  relativePath: string,
  exists: boolean,
): Promise<void> {
  await expect
    .poll(() => fs.existsSync(modelFilePath(relativePath)), {
      message: relativePath,
      timeout: 5_000,
    })
    .toBe(exists);
}
