const { expect } = require("@playwright/test");
const { waitForApplicationReady } = require("./testTools");

async function selectFilterOperator(page, propertyId, optionText) {
  const combo = page.locator(`#combo_${propertyId}`);
  const option = page
    .locator(`#dropdown_combo_${propertyId}`)
    .getByText(optionText, { exact: true });

  await expect(async () => {
    await combo.click();
    await expect(option).toBeVisible({ timeout: 2_000 });
  }).toPass({ timeout: 15_000 });
  await option.click();
  await waitForApplicationReady(page);
}

async function removeFocusFromDateInput(page, inputId) {
  const input = page.locator(`#${inputId}`);
  for (let attempt = 0; attempt < 5 && await input.evaluate(element => element === document.activeElement); attempt += 1) {
    await input.press("Tab");
  }
  await expect(input).not.toBeFocused();
}

async function setDateFilter({ page, propertyId, comboOptionText, value }) {
  const inputId = `input_${propertyId}`;
  await selectFilterOperator(page, propertyId, comboOptionText);
  if (value === undefined) return;

  const input = page.locator(`#${inputId}`);
  await input.fill(value);
  await removeFocusFromDateInput(page, inputId);
}

async function setTwoFieldDateFilter({
  page,
  propertyId,
  comboOptionText,
  fromValue,
  toValue,
}) {
  await selectFilterOperator(page, propertyId, comboOptionText);
  const fromInput = page.locator(`#from_input_${propertyId}`);
  const toInput = page.locator(`#to_input_${propertyId}`);

  await fromInput.fill(fromValue);
  await removeFocusFromDateInput(page, `from_input_${propertyId}`);
  await expect(fromInput).toHaveValue(`${fromValue} 00:00:00`);

  await toInput.fill(toValue);
  await removeFocusFromDateInput(page, `to_input_${propertyId}`);
  await expect(toInput).toHaveValue(`${toValue} 00:00:00`);
}

async function setFilter({ page, propertyId, comboOptionText, value }) {
  await selectFilterOperator(page, propertyId, comboOptionText);
  if (value === undefined) return;
  const input = page.locator(`#input_${propertyId}`);
  await input.fill(value);
}

async function setTwoFieldFilter({
  page,
  propertyId,
  comboOptionText,
  fromValue,
  toValue,
}) {
  await selectFilterOperator(page, propertyId, comboOptionText);
  const fromInput = page.locator(`#from_input_${propertyId}`);
  const toInput = page.locator(`#to_input_${propertyId}`);
  await fromInput.fill(fromValue);
  await toInput.fill(toValue);
}

async function setComboFilter({ page, propertyId, comboOptionText, value }) {
  await selectFilterOperator(page, propertyId, comboOptionText);
  if (value === undefined) return;

  const input = page.locator(`#input_${propertyId}`);
  await input.evaluate(element => element.value = "");
  await input.pressSequentially(value);
  await page.locator("div .cell.ord1.withCursor").first().click();
}

async function openFilters({ page, dataViewId, aPropertyId }) {
  await page.locator(`#${dataViewId} [class*="test-filter-button"]`).click();
  await expect(page.locator(`#input_${aPropertyId}`)).toBeVisible();
  await page.waitForLoadState("networkidle");
}

module.exports = {
  openFilters,
  setComboFilter,
  setDateFilter,
  setFilter,
  setTwoFieldDateFilter,
  setTwoFieldFilter,
};
