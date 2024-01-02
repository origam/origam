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

import { getApi } from "model/selectors/getApi";

function getCookie(name: string): string {
  const value = "; " + document.cookie;
  const parts = value.split("; " + name + "=");

  if (parts.length === 2) {
    const cookieValue = parts.pop()!.split(";").shift();
    return cookieValue ? cookieValue : "";
  }
  return "";
}

let _locale: any;

export function getLocaleFromCookie(): string {
  if (!_locale) {
    const cookieValue = decodeURIComponent(getCookie("origamCurrentLocale"));
    const pattern = /c=([a-zA-Z-]+)\|/i;
    const results = cookieValue.match(pattern);
    if (results) {
      _locale = results[1];
    } else {
      throw new Error("Locale cookie was not found. Was the function \"initLocaleCookie\" called?");
    }
  }
  return _locale;
}

export interface IDefaultDateFormats {
  defaultDateSeparator: string;
  defaultTimeSeparator: string;
  defaultDateTimeSeparator: string;
  defaultDateSequence: DateSequence;
  defaultLongDateFormat: string;
  defaultShortDateFormat: string;
  defaultTimeFormat: string;
}

export enum DateSequence {
  DayMonthYear = 0, MonthDayYear = 1
}

export function parseDateSequence(candidate: string) {
  switch (candidate) {
    case "DayMonthYear":
      return DateSequence.DayMonthYear;
    case "MonthDayYear":
      return DateSequence.MonthDayYear;
    default:
      throw new Error("Cannot parse \"" + candidate + "\" to DateSequence")
  }
}

let _defaultDateFormats: IDefaultDateFormats | undefined;

export function getDefaultCsDateFormatDataFromCookie(): IDefaultDateFormats {
  if (!_defaultDateFormats) {
    const cookieValue = decodeURIComponent(getCookie("origamCurrentLocale"));
    try {
      const parameters = getCookieParameters(cookieValue);

      _defaultDateFormats = {
        defaultDateSeparator: getSeparator("defaultDateSeparator", parameters),
        defaultTimeSeparator: getSeparator("defaultTimeSeparator", parameters),
        defaultDateTimeSeparator: getSeparator("defaultDateTimeSeparator", parameters),
        defaultDateSequence: parseDateSequence(getParameter("defaultDateSequence", parameters)),
        defaultLongDateFormat: getParameter("defaultLongDateFormat", parameters),
        defaultShortDateFormat: getParameter("defaultShortDateFormat", parameters),
        defaultTimeFormat: getParameter("defaultTimeFormat", parameters),
      }
    } catch (error) {
      throw new Error("Could not parse locale cookie value \"" + cookieValue + "\". " + error);
    }
  }
  return _defaultDateFormats;
}

function getCookieParameters(cookieValue: string) {
  return cookieValue
    .split("|")
    .map(pair => pair.split("="))
    .reduce(function (map: { [key: string]: string }, pair) {
      map[pair[0]] = pair[1];
      return map;
    }, {});
}

function isValidLocalizationCookie(cookieValue: string) {
  if (!cookieValue) {
    return false;
  }
  try {
    getDefaultCsDateFormatDataFromCookie();
    return true;
  } catch (e) {
    console.warn("Error when parsing localization cookie:" + e); // eslint-disable-line no-console
    return false;
  }
}

function getSeparator(name: string, parameters: { [key: string]: string }) {
  let value = getParameter(name, parameters);
  return value === ""
    ? " "
    : value;
}

function getParameter(name: string, parameters: { [key: string]: string }) {
  let value = parameters[name];
  if (value === undefined || value === null) {
    throw new Error("Parameter named \"" + name + "\" was not found");
  }
  return value;
}

export async function initLocaleCookie(ctx: any) {
  const cookieValue = decodeURIComponent(getCookie("origamCurrentLocale"));
  if (isValidLocalizationCookie(cookieValue)) {
    return;
  }
  const api = getApi(ctx);
  document.cookie = "origamCurrentLocale=" + await api.defaultLocalizationCookie();

  const newCookieValue = decodeURIComponent(getCookie("origamCurrentLocale"));
  if (!isValidLocalizationCookie(newCookieValue)) {
    throw new Error("Could not parse localization cookie: \"" + newCookieValue + "\"");
  }
}
