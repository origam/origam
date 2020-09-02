import { getApi } from "model/selectors/getApi";

function getCookie(name: string): string {
  const value = "; " + document.cookie;
  const parts = value.split("; " + name + "=");

  if(parts.length === 2){
    const cookieValue = parts.pop()!.split(";").shift()
    return cookieValue ? cookieValue : ""
  }
  return ""
}

export function getLocaleFromCookie(): string {
  const cookieValue = unescape(getCookie("origamCurrentLocale"));
  const pattern = /c=([a-zA-Z-]+)\|/i;
  const results = cookieValue.match(pattern);
  return results ? results[1] : "cs-CZ";
}

export async function initLocaleCookie(ctx: any) {
  const cookieValue = unescape(getCookie("origamCurrentLocale"));
  if (cookieValue !== undefined && cookieValue !== "") {
    return;
  }
  const api = getApi(ctx);
  const defaultCultureInfo = await api.defaultCulture();
  const expireDate = new Date();
  expireDate.setTime(expireDate.getTime() + (30*24*60*60*1000));
  const expires = "; expires=" + expireDate.toUTCString();
  const cultureInfo = "c=" + defaultCultureInfo.culture
    + "|uic=" + defaultCultureInfo.uiCulture;
  document.cookie = "origamCurrentLocale=" + cultureInfo + expires + "; Path=/";
}
