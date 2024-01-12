module.exports = {
  //preset: "jest-puppeteer",
  testMatch: ["./**/*.test.js"],
  verbose: true,
  setupFilesAfterEnv: ["./jest.setup.js"],
  reporters: [
    "default",
    [
      "jest-trx-results-processor",
      {
        outputFile: "../../output/resulting.trx"
      }
    ]
  ]
};


