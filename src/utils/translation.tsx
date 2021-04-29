import React from "react";
import axios from "axios";
import { getLocaleFromCookie } from "./cookies";

const debugShowTranslations = window.localStorage.getItem("debugShowTranslations") === "true";

let translations = {} as { [k: string]: string };

export async function translationsInit(ctx: any) {
  const locale = getLocaleFromCookie();
  axios.get(`locale/localization_${locale}.json`, {})
    .then(result => translations = result.data)
    .catch(error => {
      if(error.response.status === 404){
        const localeParent = locale.split("-")[0]
        axios.get(`locale/localization_${localeParent}.json`, {})
          .then(result => translations = result.data)
          .catch(error2 => console.error(error2)) /* eslint-disable-line no-console */
      }
      console.error(error) /* eslint-disable-line no-console */
    })
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
      console.error(`Could not find translation for: "${translKey}", showing default: "${result}"`); // eslint-disable-line no-console
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
