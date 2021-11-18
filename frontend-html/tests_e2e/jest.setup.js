const { toMatchImageSnapshot } = require('jest-image-snapshot');

jest.setTimeout(60*1000);
expect.extend({ toMatchImageSnapshot });
