const {
  sleep, openMenuItem, login, afterEachTest,
  beforeEachTest, switchToFormPerspective, inputByPressingKeys, waitForFocus, catchRequests
} = require('./testTools');
const {
  topMenuHeader
} = require("./modelIds");

let browser;
let page;

beforeEach(async () => {
  [browser, page] = await beforeEachTest();
});

afterEach(async () => {
  await afterEachTest(browser);
  browser = undefined;
});

async function clickElement(elementText, elementType) {
  const tag = elementType ? elementType : "div";
  const element = await page.waitForXPath(
    `//${tag}[contains(text(),'${elementText}')]`,
    {visible: true}
  );
  await sleep(200);
  await element.click();
}

async function waitForErrorMessage(messageBegging) {
  const messageElement = await page.waitForXPath(
    `//div[contains(@class, 'dialogMessage')]`,
    {visible: true}
  );
  let errorMessage=''
  for (let i = 0; i < 5; i++) {
    errorMessage = await page.evaluate(name => name.innerText, messageElement);
    if (!errorMessage || !errorMessage.includes(messageBegging)) {
      await sleep(500);
    }
    return;
  }
  if (!errorMessage || !errorMessage.includes(messageBegging)) {
    throw new Error(`Error message does not contain: \"${messageBegging}\" The actual message is: \"${errorMessage}\".`);
  }
}

const stateTransitionMenuId = "menu_f69ef86f-7fad-44c0-9ecb-78164a656a38"
const statePropertyId = "908c313c-fc0e-44d6-8c1a-c4c1659f08fd"

describe("Html client", () => {
  it("Should show descriptive message after invalid state change", async () => {
    await login(page);
    const waitForRequests = catchRequests(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        stateTransitionMenuId
      ]);

    await switchToFormPerspective({
      page: page,
      aPropertyId: statePropertyId
    });

    await sleep(300);
    const stateEditor = await page.waitForSelector(
      `#editor_${statePropertyId}`,
      {visible: true});
    await page.evaluate(x => {
      x.focus()
      x.value = ""
    }, stateEditor);
    await sleep(500);

    await waitForRequests;
    await inputByPressingKeys({page: page, value: `end`})

    await sleep(200);

    await waitForFocus({
      elementId: `editor_${statePropertyId}`,
      page: page
    })
    await sleep(200);
    await page.keyboard.press("Tab");

    await page.waitForSelector('#saveButton .isRed');
    await page.$eval("#saveButton", elem => elem.click())

    await waitForErrorMessage("State of 'State Transition' cannot enter the state 'Error'. Current state: 'end'");
  });
});
