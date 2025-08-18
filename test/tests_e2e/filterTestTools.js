const {sleep, catchRequests, clickAndWaitForXPath, typeAndWaitForSelector, clickAndWaitForSelector} = require("./testTools");

async function removeFocusFromDateInput(page, inputId) {

  await page.keyboard.press("Tab");
  await sleep(300);

  for (let i = 0; i < 5; i++) {
    const activeElement = await page.evaluateHandle(() => document.activeElement);
    const inputIsActive = await page.evaluate(activeElement => activeElement.attributes["id"], activeElement) === inputId;
    if(inputIsActive){
      await page.keyboard.press("Tab");
      await sleep(300);
    }else{
      return;
    }
  }
}

async function setDateFilter(args){
  const inputId = "input_" + args.propertyId;
  const comboId = "combo_" +  args.propertyId;
  const dropdownId = "dropdown_combo_" + args.propertyId;

  await sleep(200);
  let waitForRequests = catchRequests(args.page);

  const text1FilterCombo = await args.page.waitForSelector(
    `#${comboId}`,
    { visible: true }
  );
  await sleep(200);

  const optionDiv = await clickAndWaitForXPath({
    page: args.page,
    clickable: text1FilterCombo,
    xPath: `//div[@id='${dropdownId}']/div[text()='${args.comboOptionText}']`
  });

  await optionDiv.click();

  if(args.value === undefined){
    return;
  }

  await args.page.waitForSelector(
    `#${inputId}`,
    {visible: true});

  await args.page.focus(`#${inputId}`)
  await args.page.keyboard.type(args.value)

  await sleep(500);
  await removeFocusFromDateInput(args.page, inputId);
  await waitForRequests;
}

async function setTwoFieldDateFilter(args){
  const fromInputId = "from_input_" + args.propertyId;
  const toInputId = "to_input_" + args.propertyId;
  const comboId = "combo_" + args.propertyId;
  const dropdownId = "dropdown_combo_" + args.propertyId;

  let waitForRequests = catchRequests(args.page);
  const text1FilterCombo = await args.page.waitForSelector(
    `#${comboId}`,
    { visible: true }
  );

  const optionDiv = await clickAndWaitForXPath({
    page: args.page,
    clickable: text1FilterCombo,
    xPath: `//div[@id='${dropdownId}']/div[text()='${args.comboOptionText}']`,
  });

  await optionDiv.click();

  const fromInput = await args.page.waitForSelector(
    `#${fromInputId}`,
    {visible: true});

  await args.page.focus(`#${fromInputId}`)
  await args.page.keyboard.type(args.fromValue)

  await sleep(500);
  await removeFocusFromDateInput(args.page, fromInputId);
  await sleep(300);
  const finalFromValue = await args.page.evaluate(x => x.value, fromInput);
  expect(finalFromValue).toBe(args.fromValue+" 00:00:00");

  const toInput = await args.page.waitForSelector(
    `#${toInputId}`,
    {visible: true});

  await args.page.focus(`#${toInputId}`)
  await args.page.keyboard.type(args.toValue)

  await sleep(500);
  await removeFocusFromDateInput(args.page, toInputId);
  await sleep(200);
  const finalToValue = await args.page.evaluate(x => x.value, toInput);
  expect(finalToValue).toBe(args.toValue+" 00:00:00");

  await waitForRequests;
}

async function setFilter(args){
  const inputId = "input_" + args.propertyId;
  const comboId = "combo_" +  args.propertyId;
  const dropdownId = "dropdown_combo_" + args.propertyId;

  let waitForRequests = catchRequests(args.page);
  const text1FilterCombo = await args.page.waitForSelector(
    `#${comboId}`,
    { visible: true }
  );

  await sleep(200);

  const optionDiv = await clickAndWaitForXPath({
    page: args.page,
    clickable: text1FilterCombo,
    xPath: `//div[@id='${dropdownId}']/div[text()='${args.comboOptionText}']`
  });

  await optionDiv.click();

  if(args.value === undefined){
    return;
  }
  const input = await args.page.waitForSelector(
    `#${inputId}`,
    {visible: true});

  await args.page.evaluate(x => x.value = "", input);
  const filterValue = await args.page.evaluate(x => x.value, input);
  expect(filterValue).toBe("");

  await args.page.focus(`#${inputId}`)
  await args.page.keyboard.type(args.value)
  await waitForRequests;
}

async function setTwoFieldFilter(args){
  const fromInputId = "from_input_" + args.propertyId;
  const toInputId = "to_input_" + args.propertyId;
  const comboId = "combo_" + args.propertyId;
  const dropdownId = "dropdown_combo_" + args.propertyId;

  let waitForRequests = catchRequests(args.page);
  const text1FilterCombo = await args.page.waitForSelector(
    `#${comboId}`,
    { visible: true }
  );
  await sleep(200);

  const optionDiv = await clickAndWaitForXPath({
    page: args.page,
    clickable: text1FilterCombo,
    xPath: `//div[@id='${dropdownId}']/div[text()='${args.comboOptionText}']`
  });

  await optionDiv.click();

  const fromInput = await args.page.waitForSelector(
    `#${fromInputId}`,
    {visible: true});
  await args.page.evaluate(x => x.value = "", fromInput);
  const fromFilterValue = await args.page.evaluate(x => x.value, fromInput);
  expect(fromFilterValue).toBe("");

  await args.page.focus(`#${fromInputId}`)
  await args.page.keyboard.type(args.fromValue)

  const toInput = await args.page.waitForSelector(
    `#${toInputId}`,
    {visible: true});
  await args.page.evaluate(x => x.value = "", toInput);
  const toFilterValue = await args.page.evaluate(x => x.value, toInput);
  expect(toFilterValue).toBe("");

  await args.page.focus(`#${toInputId}`)
  await args.page.keyboard.type(args.toValue)
  await waitForRequests;
}

async function setComboFilter(args){
  const inputId = "input_" + args.propertyId;
  const comboId = "combo_" +  args.propertyId;
  const dropdownId = "dropdown_combo_" + args.propertyId;

  await sleep(200);

  const text1FilterCombo = await args.page.waitForSelector(
    `#${comboId}`,
    { visible: true }
  );
  const optionDiv = await clickAndWaitForXPath({
    page: args.page,
    clickable: text1FilterCombo,
    xPath: `//div[@id='${dropdownId}']/div[text()='${args.comboOptionText}']`
  });

  await optionDiv.click();

  if(args.value === undefined){
    return;
  }

  await args.page.waitForSelector(
    `#${inputId}`,
    {visible: true});

  const tagOptionDiv = await typeAndWaitForSelector({
    page: args.page,
    inputId: inputId,
    selector: `div .cell.ord1.withCursor`,
    value: args.value
  });

  await tagOptionDiv.click();
}

async function openFilters(args){
  const filterButton = await args.page.waitForSelector(
    `#${args.dataViewId} [class*='test-filter-button']`,
    {visible: true});

  await sleep(300);

  await clickAndWaitForSelector({
    page: args.page,
    clickable: filterButton,
    selector:`#input_${args.aPropertyId}`
  });
}


module.exports = {setDateFilter, setTwoFieldDateFilter, setFilter, setTwoFieldFilter, setComboFilter, openFilters};