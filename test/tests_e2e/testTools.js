const {userName, password, backEndUrl} = require("./additionalConfig");
const fs = require('fs');
const puppeteer = require("puppeteer");

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

async function login(page) {
    const userNameInput = await page.waitForXPath(
    `//input[@id='userNameInput']`,
    { visible: true }
  );
  await page.$eval(`#userNameInput`, (element, value) => element.value = value, userName);
  await sleep(200);

  const passwordInput = await page.waitForXPath(`//input[@id='passInput']`, {
    visible: true,
  });
  await page.$eval(`#passInput`, (element, value) => element.value = value, password);
  await sleep(200);

  const loginButton = await page.waitForXPath(`//a[@id='loginButton']`, {
    visible: true,
  });
  await sleep(500); // give the translations some time to load
  await loginButton.click();
}


async function afterEachTest(browser){
  const pages = await browser.pages();
  try{
    await Promise.all(pages.map(async page => await page.close()));
  }catch(e){
    console.warn(e);
  }
  await sleep(200);
  if(browser) {
    await browser.close();
  }
}

async function beforeEachTest(){
  const browser = await puppeteer.launch({
    ignoreHTTPSErrors: true,
    //devtools: true,
    // slowMo: 50,
    headless: false,
    defaultViewport: {
      width: 1800,
      height: 2000, // to make all 30 lines visible and avoid the need for scrolling
    },
    args: [
      "--disable-gpu",
      "--disable-dev-shm-usage",
      "--disable-setuid-sandbox",
      "--no-sandbox",
    ]
  });
  const page = await browser.newPage();
  // await installMouseHelper(page); // uncomment to see the mouse movement
  await page.goto(backEndUrl);
  await sleep(500);
  await page.evaluate(() => {
    localStorage.setItem("debugCloseAllForms", "1");
  });
  return [browser, page]
}


async function openMenuItem(page, menuItemIdList) {
  for (const menuItemId of menuItemIdList) {
    const menuItem = await page.waitForXPath(
      `//*[@id='${menuItemId}']`,
      {visible: true}
    );
    await sleep(100);
    await menuItem.click();
  }
}

async function getImage(page, element){
  const elementBounds = await element.boundingBox();
  return page.screenshot({
    clip: elementBounds,
  });
}

function xPathContainsClass(className){
  return `contains(concat(' ',normalize-space(@class),' '),' ${className} ')`
}

async function waitForRowCount(page, dataViewId, expectedRowCount){
  const modelInstanceId = dataViewId.substring(9);
  const timeoutMs = 10_000;
  const evalDelayMs = 50;
  let tableData
  for (let i = 0; i < timeoutMs / evalDelayMs; i++) {
    tableData = await page.evaluate(
      modelInstanceId => window.tableDebugMonitor && window.tableDebugMonitor[modelInstanceId],
      modelInstanceId);
    if(tableData && tableData.rendered && tableData.data.length === expectedRowCount){
      await page.evaluate(() => window.tableDebugMonitor = undefined);
      return tableData;
    }
    await sleep(evalDelayMs);
  }
  await page.evaluate(() => window.tableDebugMonitor = undefined);
  const rowCount = tableData ?  tableData.data.length : 0;
  expect(rowCount).toBe(expectedRowCount);
}

async function takeScreenShot(page, name){
  const screenDir = process.platform === "win32"
    ? "screenshots"
    : "/home/origam/server_bin/screenshots";
  if(!fs.existsSync(screenDir)){
    fs.mkdirSync(screenDir)
  }
  const dateTime =new Date().toISOString()
    .replace(/T/, ' ')
    .replace(/Z/, '')
    .replace(/\./, '_')
    .replace(/:/, '_')
    .replace(/:/, '_')
  await page.screenshot({path: `${screenDir}/${name}_${dateTime}.png`});
}

async function waitForRowCountData(page, dataViewId, expectedRowCount) {
  let countData;
  for (let i = 0; i < 200; i++) {
    countData = await getRowCountData(page, dataViewId)
    if(countData.rowCount === expectedRowCount.toString()){
      return countData;
    }
    await sleep(50);
  }
  await takeScreenShot(page, `Error expectedRowCount ${expectedRowCount} `);
  throw new Error(`Row count did not change before timeout, expectedRowCount: ${expectedRowCount},  countData.rowCount: ${countData.rowCount}`);
}

async function getTableData(page, dataViewId){
  const modelInstanceId = dataViewId.substring(9);
  const timeoutMs = 10_000;
  const evalDelayMs = 50;
  let tableData
  for (let i = 0; i < timeoutMs / evalDelayMs; i++) {
    tableData = await page.evaluate(
      modelInstanceId => window.tableDebugMonitor && window.tableDebugMonitor[modelInstanceId],
      modelInstanceId);
    if(tableData && tableData.rendered){
      await page.evaluate(() => window.tableDebugMonitor = undefined);
      return tableData;
    }
    await sleep(evalDelayMs);
  }
}

async function getRowCountData(page, dataViewId) {
  const rowCountElement =  await page.waitForSelector(`#${dataViewId} .rowCount`,{visible: true});
  let rowCountText = await page.evaluate(x => x.textContent, rowCountElement);
  const rowCountData = rowCountText
    .split("/")
    .map(x => x.trim())
    .filter(x=> x !== "");
  return {
    rowCount: rowCountData[1],
    selectedRow: rowCountData[0]};
}

async function waitForRowSelected(page, dataViewId, rowNumber){
  const timeoutMs = 10_000;
  const testDelayMs = 50;
  let rowCountData;
  for (let i = 0; i < timeoutMs / testDelayMs ; i++) {
    rowCountData = await getRowCountData(page, dataViewId);
    if(rowCountData.selectedRow === rowNumber.toString()){
      return rowCountData;
    }
    await sleep(testDelayMs);
  }
  expect(rowCountData && rowCountData.selectedRow).toBe(rowNumber.toString());
}

function catchRequests(page, reqs = 0) {
  const started = () => (reqs = reqs + 1);
  const ended = () => (reqs = reqs - 1);
  page.on('request', started);
  page.on('requestfailed', ended);
  page.on('requestfinished', ended);
  return async (timeout = 5000, success = false) => {
    while (true) {
      if (reqs < 1) break;
      await new Promise((yay) => setTimeout(yay, 100));
      if ((timeout = timeout - 100) < 0) {
        throw new Error('Timeout');
      }
    }
    page.off('request', started);
    page.off('requestfailed', ended);
    page.off('requestfinished', ended);
  };
}

async function typeAndWaitForSelector(args){

  await args.page.focus(`#${args.inputId}`)
  await args.page.keyboard.type(args.value)

  for (let i = 0; i < 3; i++) {
    try{
     return await args.page.waitForSelector(
        args.selector,
       {visible: true, timeout: 3000}
      );

    }catch(TimeoutError){
      await sleep(30000);
      await args.page.focus(`#${args.inputId}`)
      await args.page.keyboard.type(args.value)
    }
  }
  await sleep(30000);
  throw Error(`${args.selector} did not appear before timeout`);
}

async function clickAndWaitForSelector(args){
  try{
    await args.clickable.click();
  }catch(error){
    console.error(error);
    await sleep(200);
  }
  for (let i = 0; i < 3; i++) {
    try{
      return await args.page.waitForSelector(
        args.selector,
        {visible: true, timeout: 5000}
      )
    }catch(error){
      if(error.name !== "TimeoutError"){
        console.error(error);
      }
      try {
        await args.clickable.click();
      }catch(error){
        console.error(error)
        await args.page.evaluate(x => x.click(), args.clickable);
      }
    }
  }
  throw Error(`${args.selector} did not appear before timeout`);
}

async function clickAndWaitForXPath(args){
  await args.clickable.click();
  for (let i = 0; i < 3; i++) {
    try{
      return await args.page.waitForXPath(
        args.xPath,
        { visible: true, timeout: 3000 }
      );
    }catch(error){
      if(error.name !== "TimeoutError"){
        console.error(error);
      }
      await args.clickable.click();
    }
  }
  throw Error(`${args.xPath} did not appear before timeout`);
}

async function switchToFormPerspective(args){
  const switchButton = await args.page.waitForSelector(
    `.formPerspectiveButton`,
    {visible: true});
  await sleep(300);

  await clickAndWaitForSelector({
    page: args.page,
    clickable: switchButton,
    selector:`#editor_${args.aPropertyId}`
  });
}

async function switchToTablePerspective(args){
  const switchButton = await args.page.waitForSelector(
    `.tablePerspectiveButton`,
    {visible: true});
  await sleep(300);

  await clickAndWaitForSelector({
    page: args.page,
    clickable: switchButton,
    selector:`.tablePerspectiveDirector`
  });
}

async function inputByPressingKeys(args){
  for (const key of args.value) {
    await args.page.keyboard.press(key);
    await sleep(100);
  }
}

async function switchLanguageTo(args){

  const languageLinkContainer = await args.page.waitForSelector(
    "#languageLinkContainer",
    {visible: true});
  await sleep(300);

  await args.page.evaluate((container, locale) =>
      Array.from(container.children)
        .filter(element=> element.tagName === "A" &&  element.attributes["value"].value === locale)
        .forEach(element => element.click())
    ,languageLinkContainer, args.locale);
}

async function waitForFocus(args){

  const element = await args.page.waitForSelector(
    "#" + args.elementId,
    {visible: true});

  await sleep(300);

  for (let i = 0; i < 10 ; i++) {
    await args.page.evaluate(x => x.focus(), element);
    await sleep(100);
    const focusedElementId = await args.page.evaluate(x => document.activeElement.attributes["id"].nodeValue);
    if(focusedElementId === args.elementId){
      return;
    }
  }
  throw new Error("Could not set focus to:" + args.elementId);
}

module.exports = {sleep, xPathContainsClass, getImage, openMenuItem, login, getRowCountData, waitForRowCountData,
  getTableData, waitForRowCount, catchRequests, waitForRowSelected, clickAndWaitForXPath, clickAndWaitForSelector,
  typeAndWaitForSelector, switchToFormPerspective, inputByPressingKeys, switchLanguageTo, waitForFocus,
  switchToTablePerspective, afterEachTest, beforeEachTest};