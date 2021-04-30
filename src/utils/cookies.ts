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
    const cookieValue = unescape(getCookie("origamCurrentLocale"));
    const pattern = /c=([a-zA-Z-]+)\|/i;
    const results = cookieValue.match(pattern);
    if(results){
      _locale = results[1];
    }else{
      throw new Error("Locale cookie was not found. Was the function \"initLocaleCookie\" called?");
    }
  }
  return _locale;
}

export interface IDefaultDateFormats{
  defaultDateSeparator: string;
  defaultTimeSeparator: string;
  defaultDateTimeSeparator:string;
  defaultDateSequence: DateSequence;
  defaultLongDateFormat: string;
  defaultShortDateFormat: string;
  defaultTimeFormat: string;
}

export enum DateSequence{
  DayMonthYear = 0, MontDayYear = 1
}

export function parseDateSequence(candidate: string){
  switch (candidate){
    case "DayMonthYear": return DateSequence.DayMonthYear;
    case "MontDayYear": return DateSequence.MontDayYear;
    default: throw new Error("Cannot parse \""+candidate+"\" to DateSequence")
  }
}

let _defaultDateFormats: IDefaultDateFormats | undefined;

export function getDefaultCsDateFormatDataFromCookie(): IDefaultDateFormats {
  if (!_defaultDateFormats) {
    const cookieValue = unescape(getCookie("origamCurrentLocale"));
    try {
      const parameters = cookieValue
        .split("|")
        .map(pair => pair.split("="))
        .reduce(function (map: { [key: string]: string }, pair) {
          map[pair[0]] = pair[1];
          return map;
        }, {});

      _defaultDateFormats = {
        defaultDateSeparator: getParameter("defaultDateSeparator", parameters),
        defaultTimeSeparator: getParameter("defaultTimeSeparator", parameters),
        defaultDateTimeSeparator: getParameter("defaultDateTimeSeparator", parameters),
        defaultDateSequence: parseDateSequence(getParameter("defaultDateSequence", parameters)),
        defaultLongDateFormat: getParameter("defaultLongDateFormat", parameters),
        defaultShortDateFormat: getParameter("defaultShortDateFormat", parameters),
        defaultTimeFormat: getParameter("defaultTimeFormat", parameters),
      }
    }
    catch(error){
      throw new Error("Could not parse locale cookie value \""+cookieValue+"\". " + error);
    }
  }
  return _defaultDateFormats;
}

function getParameter(name: string, parameters: { [key: string]: string }){
  let value = parameters[name];
  if(!value){
    throw new Error("Parameter named \""+name+"\" was not found");
  }
  return value;
}

export async function initLocaleCookie(ctx: any) {
  const cookieValue = unescape(getCookie("origamCurrentLocale"));
  if (cookieValue) {
    return;
  }
  const api = getApi(ctx);
  document.cookie = "origamCurrentLocale=" + await api.defaultLocalizationCookie();
}
