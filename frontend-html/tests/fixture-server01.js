async function clickByXpath(xpath, index) {
  await driver.wait(until.elementLocated(By.xpath(xpath)), 5000);
  const element = (await driver.findElements(By.xpath(xpath)))[index || 0];
  await element.click();
}

async function clickByCss(css, index) {
  await driver.wait(until.elementLocated(By.css(css)), 5000);
  const element = (await driver.findElements(By.css(css)))[index || 0];
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

const chrome = require("selenium-webdriver/chrome");
const { Builder, By, until } = require("selenium-webdriver");

chrome.setDefaultService(
  new chrome.ServiceBuilder("C:\\selenium-webdriver\\chromedriver.exe").build()
);

const driver = new Builder().forBrowser("chrome").build();

const express = require("express");
const app = express();
const port = 3500;
const cors = require("cors");
app.use(cors());

async function run() {
  await delay(5000);
  await driver.get("http://localhost:3000/");
  //await login();

  app.post("/app-reload", async (req, res) => {
    console.log("App reloaded.");
    if (
      (
        await driver.findElements(
          By.xpath("//div[@class='LoginPage_loginPage__3KpHO']")
        )
      ).length > 0
    ) {
      await login();
    }
    await clickByXpath(
      "//a[@class='MainMenuItem_anchor__35Zz6']//*[contains(text(), 'Suppliers (Admin View)')]"
    );
    await clickByCss(".test-filter-button", 0);

    res.send();
  });

  app.listen(port, "127.0.0.1", () =>
    console.log(`Example app listening on port ${port}!`)
  );
}

run();
