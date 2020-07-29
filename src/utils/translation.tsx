import React from "react";
import axios from "axios";
import {getLocaleFromCookie} from "./cookies";
import PropertiesReader, {Value} from "properties-reader";

const DEBUG_SHOW_TRANSLATIONS = window.localStorage.getItem('debugShowTranslations') === "true";

let translations = {
  this_is_a_text: "This is {0} a text {1}",
} as {[k: string]: Value};



export async function translationsInit(){
  const locale = getLocaleFromCookie();
  const localeFolder = locale.replace("-","_");
  const translationData =  (
    await axios.get(`locale/${localeFolder}/asap.properties`, {
    })
  ).data;
  const propertiesReader = PropertiesReader("");
  propertiesReader.read(translationData);
  translations = propertiesReader.getAllProperties();
  console.log(translations);
}

export function T(defaultContent: any, translKey: string, ...p: any[]) {
  let result;
  if (translations.hasOwnProperty(translKey)) {
    result = translations[translKey];
  } else {
    result = defaultContent;
  }
  for (let i = 0; i < p.length; i++) {
    result = result.replace(`{${i}}`, p[i]);
  }
  if (DEBUG_SHOW_TRANSLATIONS) {
    return (
      <span title={translKey} style={{ backgroundColor: "red" }}>
    {result}
    </span>
  );
  } else {
    return result;
  }
}