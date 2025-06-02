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

const [windowLocation] = window.location.href.split("#");

const config = {
  authority: `${windowLocation}`,
  client_id: "origamWebClient",
  redirect_uri: `${windowLocation}#origamClientCallback/`,
  response_type: "code",
  scope: "openid api offline_access",
  post_logout_redirect_uri: `${windowLocation}`,
  automaticSilentRenew: true,
  silent_redirect_uri: `${windowLocation}#origamClientCallbackRenew/`,
};

export const userManager = new UserManager(config);

export async function ensureLogin() {
  const authOvr = sessionStorage.getItem("origamAuthTokenOverride");
  if (authOvr) {
    sessionStorage.setItem("origamAuthTokenOverride", authOvr);
    return {access_token: authOvr};
  }
  if (window.location.hash.startsWith("#origamClientCallback/")) {
    try {
      const user = await userManager.signinRedirectCallback(
        window.location.hash.replace("#origamClientCallback/", "")
      );
      const [urlpart] = window.location.href.split("#");
      window.history.replaceState(null, "", urlpart);
      if (!user.access_token) {
        await userManager.signinRedirect();
      } else {
        return user;
      }
    } catch (err) {
      console.warn(err);
      await userManager.signinRedirect();
    }
  } else {
    const user = await userManager.getUser();
    if (user && user.access_token) {
      return user;
    } else {
      await userManager.signinRedirect();
    }
  }
}

export async function logoff() {
  await userManager.signoutRedirect();
}
