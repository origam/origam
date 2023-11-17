const { sleep, openMenuItem, login, afterEachTest,
  beforeEachTest, catchRequests, waitForRowCount
} = require('./testTools');
const {widgetsMenuItemId, topMenuHeader, allDataTypesMenuId
} = require("./modelIds");
const { restoreWidgetSectionTestMaster, clearScreenConfiguration} = require("./dbTools");
const {installMouseHelper} = require("./instalMouseHelper_");

let browser;
let page;

beforeAll(async() => {
  await restoreWidgetSectionTestMaster();
  await clearScreenConfiguration();
  [browser, page] = await beforeEachTest(); // all tests run in the same browser instance unlike the others. That is why the beforeEachTest is called here.
  await login(page);
  await openMenuItem(
    page,
    [
      topMenuHeader,
      widgetsMenuItemId,
      allDataTypesMenuId
    ]);
});

afterAll(async() => {
  await sleep(1000); // let the last workflow screen close
  await afterEachTest(browser);
  browser = undefined;
});

async function clickElement(elementText, elementType){
  console.log("Waiting for: " + elementText)
  const tag = elementType ? elementType : "div";
  const element = await page.waitForXPath(
    `//${tag}[contains(text(),'${elementText}')]`,
    {visible: true}
  );
  await sleep(200);
  console.log("Clicking: " + elementText)
  await element.click();
}

async function waitForErrorWindowWithMessageAndClose(messageBegging) {
  await page.waitForXPath(
    `//div[contains(text(),'Error')]`,
    {visible: true}
  );

  const messageElement = await page.waitForXPath(
    `//div[contains(@class,'dialogMessage')]`,
    {visible: true}
  );

  const errorMessage = await page.evaluate(name => name.innerText, messageElement);
  if(!errorMessage || !errorMessage.startsWith(messageBegging)){
    throw new Error(`Error message does not start with: \"${messageBegging}\" The actual message is: \"${errorMessage}\".`);
  }

  const okButton = await page.waitForXPath(
    `//button[contains(text(),'OK')]`,
    {visible: true}
  );
  await sleep(200);
  await okButton.click();
}

async function clickWorkflowActionButton(buttonText) {
  await clickElement("Run Test Workflow");

  const buttonElement = await page.waitForXPath(
    `//div[contains(@class,'dropdownItem')]/div/div[contains(text(),'${buttonText}')]`,
    {visible: true}
  );
  await sleep(200);
  await buttonElement.click();
}

async function waitForWorkflowMessage(messageBegging) {
  const messageElement = await page.waitForXPath(
    `//div[contains(@class, 'workflowMessage')]/font/p`,
    {visible: true}
  );
  const errorMessage = await page.evaluate(name => name.innerText, messageElement);
  if(!errorMessage || !errorMessage.startsWith(messageBegging)){
    throw new Error(`Error message does not start with: \"${messageBegging}\" The actual message is: \"${errorMessage}\".`);
  }
}

const dataViewId = "dataView_e67865b0-ce91-413c-bed7-1da486399633";

describe("Html client", () => {
  it("Should run 'First step fail' workflow", async () => {
    await clickElement("Run Test Workflow")
    await clickElement("First step fail")
    await waitForErrorWindowWithMessageAndClose(
      "Server error occurred. Please check server log for more details:\n" +
      "Merge context 'AllDataTypes', Step 'Basic WF_1stepfail/0100_StepFail' failed.");
  });
  it("Should run 'Second step fail' workflow", async () => {
    await clickElement("Run Test Workflow")
    await clickElement("Second step fail")
    await waitForErrorWindowWithMessageAndClose("Fail Step");
  });
  it("Should run 'UI third step fail' workflow", async () => {
    await clickElement("Run Test Workflow");
    await clickElement("UI third step fail");
    await clickElement("Next");
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_UI_NextStepFail/0300_StepFail\' failed.');
    await clickElement("Close","button");
  });
  it("Should run 'UI WFCall step 1' workflow", async () => {
    await clickElement("Run Test Workflow");
    await clickElement("UI WFCall step 1");
    await clickElement("Next");
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_1stepfail/0100_StepFail\' failed.');
    await clickElement("Close","button");
  });
  it("Should run 'UI WFCall step 2' workflow", async () => {
    await clickWorkflowActionButton("UI WFCall step 2");
    await clickElement("Next");
    await waitForWorkflowMessage('Fail Step');
    await clickElement("Close","button");
  });
  it("Should run 'UI WFCall step UI fail' workflow", async () => {
    await clickElement("Run Test Workflow");
    await clickElement("UI WFCall step UI fail");
    await clickElement("Next");
    await waitForWorkflowMessage('Item has already been added. Key in dictionary:');
    await clickElement("Close","button");
  });
  it("Should run 'UI WFService Sub 1 step' workflow", async () => {
    await clickElement("Run Test Workflow");
    await clickElement("UI WFService Sub 1 step");
    await clickElement("Next");
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_1stepfail/0100_StepFail\' failed.');
    await clickElement("Close","button");
  });
  it("Should run 'UI WFService Sub 2 steps' workflow", async () => {
    await clickElement("Run Test Workflow");
    await clickElement("UI WFService Sub 2 steps");
    await clickElement("Next");
    await waitForWorkflowMessage('Fail Step');
    await clickElement("Close","button");
  });
  it("Should run 'UI WFService Sub UI' workflow", async () => {
    await clickElement("Run Test Workflow");
    await clickElement("UI WFService Sub UI");
    await clickElement("Next");
    await waitForRowCount(page, dataViewId, 2);
    await clickElement("Next");
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_UI_NextStepFail/0300_StepFail\' failed. ');
    await clickElement("Close","button");
  });
  it("Should run 'CallServiceWorkflow Sub 1 step' workflow", async () => {
    await clickElement("Run Test Workflow")
    await clickElement("CallServiceWorkflow Sub 1 step")
    await waitForErrorWindowWithMessageAndClose(
      "Server error occurred. Please check server log for more details:\n" +
      "Merge context 'AllDataTypes', Step 'Basic WF_1stepfail/0100_StepFail' failed.");
  });
  it("Should run 'CallServiceWorkflow Sub 2 step' workflow", async () => {
    await clickElement("Run Test Workflow");
    await clickElement("CallServiceWorkflow Sub 2 step");
    await waitForErrorWindowWithMessageAndClose("Fail Step");
  });
  it("Should run 'CallServiceWorkflow Sub UI step' workflow", async () => {
    await clickElement("Run Test Workflow");
    await clickElement("CallServiceWorkflow Sub UI step");
    await waitForWorkflowMessage('Fail Step');
    await clickElement("Close","button");
  });
  it("Should run 'CallSubWorkflow 1 step' workflow", async () => {
    await clickElement("Run Test Workflow")
    await clickElement("CallSubWorkflow 1 step")
    await waitForErrorWindowWithMessageAndClose(
      "Server error occurred. Please check server log for more details:\n" +
      "Merge context 'AllDataTypes', Step 'Basic WF_1stepfail/0100_StepFail' failed.");
  });
  it("Should run 'CallSubWorkflow 2 step' workflow", async () => {
    await clickElement("Run Test Workflow");
    await clickElement("CallSubWorkflow 2 step");
    await waitForErrorWindowWithMessageAndClose("Fail Step");
  });
  it("Should run 'CallSubWorkflow UI step' workflow", async () => {
    await clickElement("Run Test Workflow");
    await clickElement("CallSubWorkflow UI step");
    await clickElement("Next");
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_UI_NextStepFail/0300_StepFail\' failed.');
    await clickElement("Close","button");
  });
});
