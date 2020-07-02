import Oidc from "oidc-client";

const [windowLocation] = window.location.href.split("#");

const config = {
  authority: `${windowLocation}`,
  client_id: "origamWebClient",
  redirect_uri: `${windowLocation}#origamClientCallback/`,
  response_type: "code",
  scope: "openid IdentityServerApi offline_access",
  post_logout_redirect_uri: `${windowLocation}`,
  response_mode: "query",
  automaticSilentRenew: true,
  silent_redirect_uri: `${windowLocation}#origamClientCallbackRenew/`,
};

export const userManager = new Oidc.UserManager(config);

export async function ensureLogin() {
  const authOvr = sessionStorage.getItem("origamAuthTokenOverride");
  if (authOvr) {
    sessionStorage.setItem("origamAuthTokenOverride", authOvr);
    return { access_token: authOvr };
  }
  if (window.location.hash.startsWith("#origamClientCallback/")) {
    const user = await userManager.signinRedirectCallback(
      window.location.hash.replace("#origamClientCallback/", "")
    );
    const [urlpart] = window.location.href.split("#");
    window.history.replaceState(null, "", urlpart);
    return user;
  } else {
    const user = await userManager.getUser();
    if (user) {
      return user;
    } else {
      userManager.signinRedirect();
    }
  }
}
