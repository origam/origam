const { readFile, writeFile } = require("fs/promises");
const TrxReporter = require("playwright-trx-reporter").default;

module.exports = class extends TrxReporter {
  constructor(options) {
    super(options);
    this.reportPath = options.outputFile;
  }

  async onEnd(result) {
    await super.onEnd(result);
    const report = await readFile(this.reportPath, "utf8");
    // The upstream reporter writes creation/finish, but dorny/test-reporter also
    // requires the run's start timestamp to calculate its duration.
    const compatibleReport = report.replace(/<Times\b([^>]*)>/, (element, attributes) => {
      if (/\bstart=/.test(attributes)) return element;
      const creation = attributes.match(/\bcreation="([^"]+)"/)[1];
      return element.replace("<Times", `<Times start="${creation}"`);
    });
    await writeFile(this.reportPath, compatibleReport);
  }
};
