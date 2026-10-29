const { test, expect } = require("@playwright/test");
const { openMenuItem, login, beforeEachTest, waitForRowCount
} = require('./testTools');
const {widgetsMenuItemId, topMenuHeader, workflowTestItemId
} = require("./modelIds");
const { clearScreenConfiguration, restoreAllDataTypesTable} = require("./dbTools");

let page;

test.beforeAll(async() => {
  await restoreAllDataTypesTable();
  await clearScreenConfiguration();
});

test.beforeEach(async ({ page: testPage }) => {
  page = testPage;
  await beforeEachTest(page);
  await login(page);
});



async function clickElement(elementText, elementType){
  await page.getByText(elementText, {
    exact: false,
  }).and(page.locator(elementType || "div")).first().click();
}

async function waitForWorkflowMessage(messageBegging) {
  const message = page.locator("div[class*='workflowMessage'] font div");
  const normalizedBeginning = messageBegging.replaceAll("\\", "/");
  await expect.poll(async () =>
    (await message.innerText()).replaceAll("\\", "/").startsWith(normalizedBeginning)
  , { timeout: 60_000 }).toBe(true);
}

const dataViewId = "dataView_e67865b0-ce91-413c-bed7-1da486399633";
const firstStepFailItemId ="menu_1119a229-eafa-4afa-8986-39487eb78cca";
const secondStepFailItemId ="menu_3e5a6435-b4cb-4338-ad16-04ad24d1402c";
const uiThirdStepFail ="menu_6d3f26b6-1dbc-44f3-851b-5a119f22b925";
const callServiceWorkflowSub1StepItemId ="menu_e70afa1f-c511-4e5c-bcdf-a8041717d2c3";
const callServiceWorkflowSub2StepItemId ="menu_d948a606-9ff4-46eb-b259-7fcb420eb59c";
const callServiceWorkflowSubUiStepItemId ="menu_88fed471-a2ed-4fd0-9ae7-d57ffbbe5f91";
const callSubWorkflow1StepItemId ="menu_44e1b7cc-0e83-4379-8227-c78982eaf71d";
const callSubWorkflow2StepItemId ="menu_f907f6c6-13f3-4c58-913c-a0e8dfc52f0a";
const callSubWorkflowUiStepItemId ="menu_f066f5e3-320a-42b4-b2ec-39f8670921e9";
const uiWFCallServiceStep1ItemId ="menu_3c857341-72f3-4835-83ea-43b320579bec";
const uiWFCallServiceStep2ItemId ="menu_486315ce-e6d0-4a92-b6bd-451d3eff5fa7";
const uiWFServiceSub1stepItemId ="menu_f68209ca-75c4-499d-859a-1ff8c3ac22cf";
const uiWFServiceSub2stepItemId ="menu_931a64cd-b3d2-412b-a888-be4203c62292";
const uiWFCallServiceSubUiStepItemId ="menu_e01a1eff-43e0-4dee-97db-d616e6926a10";
const uiWFServiceSubUiItemId ="menu_22ca65ba-a587-4cfc-a1cc-aeda9d3e588e";

test.describe("Html client", () => {
  test("Should run 'First step fail' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        firstStepFailItemId
      ]);
    await waitForWorkflowMessage("Merge context 'AllDataTypes', Step 'Basic WF_1stepfail/0100_StepFail' failed.");
    await clickElement("Close","button");
  });
  test("Should run 'Second step fail' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        secondStepFailItemId
      ]);
    await waitForWorkflowMessage("Fail Step");
    await clickElement("Close","button");
  });
  test("Should run 'UI third step fail' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        uiThirdStepFail
      ]);
    await clickElement("Next");
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_UI_NextStepFail/0300_StepFail\' failed.');
    await clickElement("Close","button");
  });
  test("Should run 'CallServiceWorkflow Sub 1 step' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        callServiceWorkflowSub1StepItemId
      ]);
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_1stepfail/0100_StepFail\' failed.');
    await clickElement("Close","button");
  });
  test("Should run 'CallServiceWorkflow Sub 2 step' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        callServiceWorkflowSub2StepItemId
      ]);
    await waitForWorkflowMessage('Fail Step');
    await clickElement("Close","button");
  });
  test("Should run 'CallServiceWorkflow Sub UI step' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        callServiceWorkflowSubUiStepItemId
      ]);
    await clickElement("Next");
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_UI_NextStepFail/0300_StepFail\' failed.');
    await clickElement("Close","button");
  });
  test("Should run 'CallSubWorkflow 1 step' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        callSubWorkflow1StepItemId
      ]);
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_1stepfail/0100_StepFail\' failed.');
    await clickElement("Close","button");
  });
  test("Should run 'CallSubWorkflow 2 step' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        callSubWorkflow2StepItemId
      ]);
    await waitForWorkflowMessage('Fail Step');
    await clickElement("Close","button");
  });
  test("Should run 'CallSubWorkflow UI step' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        callSubWorkflowUiStepItemId
      ]);
    await clickElement("Next");
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_UI_NextStepFail/0300_StepFail\' failed.');
    await clickElement("Close","button");
  });
  test("Should run 'UI WFCallService step 1' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        uiWFCallServiceStep1ItemId
      ]);
    await clickElement("Next");
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_1stepfail/0100_StepFail\' failed.');
    await clickElement("Close","button");
  });
  test("Should run 'UI WFCallService step 2' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        uiWFCallServiceStep2ItemId
      ]);
    await clickElement("Next");
    await waitForWorkflowMessage('Fail Step');
    await clickElement("Close","button");
  });
  test("Should run 'UI WFCallService Sub UI step' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        uiWFCallServiceSubUiStepItemId
      ]);
    await clickElement("Next");
    await waitForRowCount(page, dataViewId, 2);
    await clickElement("Next");
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_UI_NextStepFail/0300_StepFail\' failed. ');
    await clickElement("Close","button");
  });
  test("Should run 'UI WFService Sub 1 step' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        uiWFServiceSub1stepItemId
      ]);
    await clickElement("Next");
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_1stepfail/0100_StepFail\' failed.');
    await clickElement("Close","button");
  });
  test("Should run 'UI WFService Sub 2 steps' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        uiWFServiceSub2stepItemId
      ]);
    await clickElement("Next");
    await waitForWorkflowMessage('Fail Step');
    await clickElement("Close","button");
  });
  test("Should run 'UI WFService Sub UI' workflow", async () => {
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        workflowTestItemId,
        uiWFServiceSubUiItemId
      ]);
    await clickElement("Next");
    await waitForWorkflowMessage('Item has already been added. Key in dictionary:');
    await clickElement("Close","button");
  });
});
