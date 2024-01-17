// for jest-trx-results-processor >= 1.0.0
var builder = require("jest-trx-results-processor/dist/testResultsProcessor"); // only this has changed since v 0.x

var processor = builder({
});

module.exports = processor;