module.exports = {
  testMatch: ["./**/*.test.js"],
  testSequencer: './path-sequencer.js',
  verbose: true,
  setupFilesAfterEnv: ["./jest.setup.js"],
  reporters: [
    "default",
    [
      "jest-trx-results-processor",
      {
        outputFile: "frontend-integration-test-results.trx"
      }
    ]
  ]
};


