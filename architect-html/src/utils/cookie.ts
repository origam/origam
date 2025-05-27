/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
  const localeRegex = new RegExp(`^${localizationCookieName}=c=([a-zA-Z-]+)|`);
  return localeRegex.test(cookie);
}


