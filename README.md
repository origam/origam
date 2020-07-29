## Origam client web app

### Requirements for building

To build the app you need:

- nodejs 11.12.0 (build process will be very probably successful on most of the more recent versions)
- yarn (any recent version should be fine)

### How to build

```
git clone https://bitbucket.org/origamsource/origam-html5.git
cd origam-html5
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
git clone https://bitbucket.org/origamsource/origam-html5.git
cd origam-html5
set HTTPS=true
yarn start
```

By default https://localhost:44356 is used as a backend proxy target. If you need to customize this 
for your setup, create a file `.env.development.local` and add this line to it:

```
WDS_PROXY_TARGET=your-protocol://your-backend-proxy-target
```
