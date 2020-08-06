# User authorization and authentication in external Origam applications

Origam system uses OpenID Connect to log a user in and out. The protocol is implemented using IdentityServer on server side and it is recommended to use its provided counterpart [`oidc-client-js`](https://github.com/IdentityModel/oidc-client-js) which is also a [certified OpenID client](https://openid.net/developers/certified/).

You may like a thorough tutorial section about adding the JavaScript client in the [IdentityServer documentation](https://identityserver4.readthedocs.io/en/latest/quickstarts/4_javascript_client.html)

To understand details of following sections, see also the example code.

## How to rub the example

To run the app you need:

- nodejs 11.12.0 (build process will be very probably successful on most of the more recent versions)
- yarn (any recent version should be fine)

Execute following commands:

```
git clone https://bitbucket.org/origamsource/origam-html5.git
cd origam-html5/examples/app-login
yarn
yarn start
```

The default port the example is running at is `5599`.

The example uses self-signed HTTPS certificate. For development purposes it is convenient to use Chrome browser with `allow-insecure-localhost` option `Enabled`:

- Go to `chrome://flags` location
- Search for `localhost` and switch *Allow invalid certificates for resources loaded from localhost* to `Enabled`

## Initializing `oidc-client-js`

Create `Oidc.UserManager` instance, giving it appropriate options.

Also make sure, that the server side is configured to allow redirect uris stated in the options.

## Determining whether a user is logged in

In your application, call `userManager.getUser()` (where `userManager` is a manager instance created above) to get a `Promise` of the instance of currently logged in user record when it is currently authenticated or `undefined` if it is not.

## Logging user in

Signing in is initiated by calling `userManager.signinRedirect()` which changes current location to the IdentityServer log in endpoint, giving it control over the authentication process. 

After successfull authorization the server sets the authentication cookie and redirects the browser to a URL given by `redirect_uri` option. Once again: This has to be present in `RedirectUris` list in server options.

In the `redirect_uri` target page you need to process server response by calling `userManager.signinRedirectCallback()` returning a `Promise`. After the `Promise` is resolved, you can redirect the browser back to your application and start using the API.

## Logging user out

The process is analogous to logging in. You need to call `userManager.signoutRedirect()`, which will pass the control to the IdentityServer, signing the user out. 

After successfull signout the browser will be redirected to `post_logout_redirect_uri` given in the `UserManager`s options. This URI has to be also stated in `PostLogoutRedirectUris` server setting.

You need to call `userManager.signoutRedirectCallback()` on the redirected location and after its completion you can go back to your application.

## Accessing an external Origam API endpoint

Accessing an Origam API consumed by an external application is authorized by a cookie set by the IdentityServer during login process so there is no need to do any additional steps after being authenticated by the server. 

