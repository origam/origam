import React from "react";
import axios from "axios";
import { getLocaleFromCookie, initLocaleCookie } from "./cookies";

const debugShowTranslations = window.localStorage.getItem("debugShowTranslations") === "true";

let translations = {} as { [k: string]: string };

export async function translationsInit(ctx: any) {
  await initLocaleCookie(ctx);
  const locale = getLocaleFromCookie();
  translations = (await axios.get(`locale/localization_${locale}.json`, {})).data;
}

export function T(defaultContent: any, translKey: string, ...p: any[]) {
  let result;
  let showingDefault = false;
  if (translations.hasOwnProperty(translKey)) {
    result = translations[translKey];
  } else {
    result = defaultContent;
    showingDefault = true;
  }
  for (let i = 0; i < p.length; i++) {
    result = result.replace(`{${i}}`, p[i]);
  }
  if (debugShowTranslations) {
    if (showingDefault) {
      console.error(`Could not find translation for: "${translKey}", showing default: "${result}"`);
      return (
        <span title={translKey} style={{ backgroundColor: "red" }}>
          {result}
        </span>
      );
    } else {
      return (
        <span title={translKey} style={{ backgroundColor: "green" }}>
          {result}
        </span>
      );
    }
  } else {
    return result;
  }
}
