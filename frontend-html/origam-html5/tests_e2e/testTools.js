const {userName, password} = require("./additionalConfig");

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

async function waitForRowCountData(page, dataViewId, expectedRowCount) {
  for (let i = 0; i < 200; i++) {
    const countData = await getRowCountData(page, dataViewId)
    if(countData.rowCount === expectedRowCount.toString()){
      return countData;
    }
    await sleep(50);
  }
  throw new Error("Row count did not change before timeout");
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
      console.log(TimeoutError)
      await sleep(30000);
      await args.page.focus(`#${args.inputId}`)
      await args.page.keyboard.type(args.value)
    }
  }
  await sleep(30000);
  throw Error(`${args.selector} did not appear before timeout`);
}

async function clickAndWaitForSelector(args){
  await args.clickable.click();
  for (let i = 0; i < 3; i++) {
    try{
      return await args.page.waitForSelector(
        args.selector,
        {visible: true, timeout: 3000}
      )
    }catch(error){
      console.log(error);
      await args.clickable.click();
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
      console.log(error);
      await args.clickable.click();
    }
  }
  throw Error(`${args.xPath} did not appear before timeout`);
}

module.exports = {sleep, xPathContainsClass, getImage, openMenuItem, login, getRowCountData, waitForRowCountData,
  getTableData, waitForRowCount, catchRequests, waitForRowSelected, clickAndWaitForXPath, clickAndWaitForSelector,
  typeAndWaitForSelector};