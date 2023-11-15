const { sleep, openMenuItem, login, afterEachTest,
  beforeEachTest
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
  [browser, page] = await beforeEachTest()
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

async function waitForErrorWindow() {
  await page.waitForXPath(
    `//div[contains(text(),'Error')]`,
    {visible: true}
  );
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

async function waitTillAllDataTypesScreenIsActive() {
  await page.waitForXPath(
    "//div[contains(@class, 'screenHeader')]/h1[contains(text(), 'All Data Types')]",
    {visible: true}
  );
}


// async function waitForWorkflowMessage(message) {
//   await page.waitForXPath(
//     `//div[contains(@class, 'workflowMessage')]/font/p[contains(text(), '${message}')]`,
//     {visible: true}
//   );
// }

async function waitForWorkflowMessage(messageBegging) {
  const messageElement = await page.waitForXPath(
    `//div[contains(@class, 'workflowMessage')]/font/p`,
    {visible: true}
  );
  // await sleep(3000);
  const errorMessage = await page.evaluate(name => name.innerText, messageElement);
  if(!errorMessage || errorMessage.startsWith(messageBegging)){
    throw new Error(`Error message does not start with: \"${messageBegging}\" The actual message is: \"${errorMessage}\".`);
  }
}

describe("Html client", () => {
  it("Should run 'First step fail' workflow", async () => {
    await clickElement("Run Test Workflow")
    await clickElement("First step fail")
    await waitForErrorWindow();
  });
  it("Should run 'Second step fail' workflow", async () => {
    await clickElement("Run Test Workflow")
    await clickElement("Second step fail")
    await waitForErrorWindow();
  });
  it("Should run 'UI third step fail' workflow", async () => {
    await clickElement("Run Test Workflow");
    await clickElement("UI third step fail");
    await clickElement("Next");
    await clickElement("Close","button");
  });
  it("Should run 'UI WFCall step 1' workflow", async () => {
    await clickElement("Run Test Workflow");
    await clickElement("UI WFCall step 1");
    await clickElement("Next");
    await waitForWorkflowMessage('Merge context \'AllDataTypes\', Step \'Basic WF_UI_NextStepFail/0300_StepFail\' failed.');
    await clickElement("Close","button");
    await waitTillAllDataTypesScreenIsActive();
  });
  it("Should run 'UI WFCall step 2' workflow", async () => {
    await clickWorkflowActionButton("UI WFCall step 2");
    await clickElement("Next");
    await waitForWorkflowMessage('Fail Step');
    await clickElement("Close","button");
    await waitTillAllDataTypesScreenIsActive();
  });
});
