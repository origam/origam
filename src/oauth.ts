import Oidc from "oidc-client";

const config = {
  authority: "https://localhost:3000/",
  client_id: "origamWebClient",
  redirect_uri: "https://localhost:3000/#origamClientCallback/",
  response_type: "code",
  scope: "openid IdentityServerApi offline_access",
  post_logout_redirect_uri: "https://localhost:3000/",
  response_mode: "query",
  automaticSilentRenew: true,
  silent_redirect_uri: "https://localhost:3000/#origamClientCallbackRenew/"
};

export const userManager = new Oidc.UserManager(config);



export async function ensureLogin() {
  if (window.location.hash.startsWith("#origamClientCallback/")) {
    const user = await userManager.signinRedirectCallback(
      window.location.hash.replace("#origamClientCallback/", "")
    );
    const [urlpart, hashpart] = window.location.href.split("#");
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
