/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { UserManager } from "oidc-client-ts";

const windowLocation = window.location.origin;

const config = {
  authority: `${windowLocation}`,
  client_id: "origamWebClient",
  redirect_uri: `${windowLocation}/origamClientCallback/`,
  response_type: "code",
  scope: "openid offline_access internal_api",
  post_logout_redirect_uri: `${windowLocation}`,
  automaticSilentRenew: true,
  silent_redirect_uri: `${windowLocation}/origamClientCallbackRenew/`,
};

export const userManager = new UserManager(config);

export async function ensureLogin() {
  console.log("ensureLogin");
  const authOvr = sessionStorage.getItem("origamAuthTokenOverride");
  if (authOvr) {
    sessionStorage.setItem("origamAuthTokenOverride", authOvr);
    return { access_token: authOvr };
  }

  const path = window.location.pathname;
  debugger;
  // Handle the OIDC redirect callback coming back from the server
  if (path.startsWith("/origamClientCallback")) {
    try {
      const user = await userManager.signinRedirectCallback();
      // Clean up the URL so code/state are not visible anymore
      window.history.replaceState(null, "", "/");
      if (!user || !user.access_token) {
        await userManager.signinRedirect();
        return;
      }
      return user;
    } catch (err) {
      console.warn("signinRedirectCallback error", err);
      await userManager.signinRedirect();
      return;
    }
  }

  // Normal case: either we already have a user or we need to start login
  const user = await userManager.getUser();
  if (user && user.access_token) {
    return user;
  } else {
    console.log("windowLocation: " + windowLocation);
    console.log("userManager.signinRedirect");
    await userManager.signinRedirect();
  }
}

export async function logoff() {
  await userManager.signoutRedirect();
}
