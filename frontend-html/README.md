## Origam client web app

### Requirements for building

To build the app you need:

- nodejs 14.*
- yarn 4.5.0 (newer will probably be fine too)

### How to build

```
git clone https://github.com/origam/origam-html.git
cd origam-html
yarn
yarn build
```

### How to run in the development mode

You need to disable browser security for localhost. In Chrome you can find it here:

```
chrome://flags/#allow-insecure-localhost
```

Then in the command line

```
git clone https://github.com/origam/origam-html.git
cd origam-html
set HTTPS=true
yarn start
```

By default https://localhost:44356 is used as a backend proxy target. If you need to customize this 
for your setup, create a file `.env.development.local` and add this line to it:

```
WDS_PROXY_TARGET=your-protocol://your-backend-proxy-target
```
### Notes
If you are on linux and `yarn start` fails with:
```
Error: spawn chrome ENOENT
    at Process.ChildProcess._handle.onexit (internal/child_process.js:269:19)
    at onErrorNT (internal/child_process.js:467:16)
    at processTicksAndRejections (internal/process/task_queues.js:82:21)
Emitted 'error' event on ChildProcess instance at:
    at Process.ChildProcess._handle.onexit (internal/child_process.js:275:12)
    at onErrorNT (internal/child_process.js:467:16)
    at processTicksAndRejections (internal/process/task_queues.js:82:21) {
  errno: -2,
  code: 'ENOENT',
  syscall: 'spawn chrome',
  path: 'chrome',
  spawnargs: [ 'https://localhost:3000' ]
}

```
Try running it with the browser path specified:

```BROWSER=/usr/bin/firefox yarn start```

#### Debug Constants
You can set these constants in the browser's local storage:

`debugNoPolling` will prevent work queue refresh from running

`debugNoPolling_notificationBox` will prevent notifications refresh from running

`debugNoPolling_chatrooms` will prevent chat refresh from running



