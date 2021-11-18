const chrome = require("selenium-webdriver/chrome");
const { Builder, By, until } = require("selenium-webdriver");

chrome.setDefaultService(
  new chrome.ServiceBuilder("C:\\selenium-webdriver\\chromedriver.exe").build()
);

const driver = new Builder().forBrowser("chrome").build();

async function clickByXpath(xpath) {
  await driver.wait(until.elementLocated(By.xpath(xpath)), 5000);
  const element = await driver.findElement(By.xpath(xpath));
  await element.click();
}

async function sendKeysByXpath(xpath, keys) {
  await driver.wait(until.elementLocated(By.xpath(xpath)), 5000);
  const element = await driver.findElement(By.xpath(xpath));
  await element.sendKeys(keys);
}

const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

async function login() {
  await sendKeysByXpath('//input[@name="userName"]', "admin");
  await sendKeysByXpath('//input[@name="password"]', "adminADMIN123..");
  await clickByXpath('//button[contains(text(), "Login")]');
}

async function logout() {
  await clickByXpath(
    "//div[@class='ScreenToolbar_root__2_PdE']//div[contains(text(), 'User')]"
  );
}

async function main() {
  await driver.get("http://localhost:3000/");
  await login();

  await clickByXpath(
    "//a[@class='MainMenuItem_anchor__35Zz6']//*[contains(text(), 'Shared Inventories')]"
  );
  await clickByXpath(
    "//a[@class='MainMenuItem_anchor__35Zz6']//*[contains(text(), 'All Active Properties')]"
  );
  await delay(3000);
  await logout();
  await delay(5000);
  await driver.close();
  // await clickByText("Show Detail");
}

main();
