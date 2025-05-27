const localizationCookieName = '.AspNetCore.Culture';

export function getLocaleFromCookie(): string {
  const cookieValue = decodeURIComponent(getCookie(localizationCookieName));
  const pattern = /c=([a-zA-Z-]+)\|/;
  const results = cookieValue.match(pattern);
  if (results) {
    return results[1];
  } else {
    throw new Error("Locale cookie was not found. Was the function \"initLocaleCookie\" called?");
  }
}

export function setLocaleToCookie(locale: string): void {
  document.cookie = `${localizationCookieName}=c=${locale}|uic=${locale}; path=/`;
}

function getCookie(name: string): string {
  const value = "; " + document.cookie;
  const parts = value.split("; " + name + "=");

  if (parts.length === 2) {
    const cookieValue = parts.pop()!.split(";").shift();
    return cookieValue ? cookieValue : "";
  }
  return "";
}

export function initLocaleCookie() {
  const cookieValue = decodeURIComponent(getCookie(localizationCookieName));
  if (isValidLocalizationCookie(cookieValue)) {
    return;
  }
  setLocaleToCookie("en-US");
}

function isValidLocalizationCookie(cookie: string) {
  if (!cookie) {
    return false;
  }
  const localeRegex = new RegExp(`^${localizationCookieName}=c=([a-zA-Z-]+)\|`);
  return localeRegex.test(cookie);
}


