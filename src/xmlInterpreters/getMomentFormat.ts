import {csToMomentFormat} from "../utils/dateFormatConversion";
import {T} from "../utils/translation";
import {getDefaultCsDateFormatDataFromCookie} from "../utils/cookies";

export function replaceDefaultDateFormats(formatFromServer: string){
  if(formatFromServer === "long"){
    let defaultDateFormatData = getDefaultCsDateFormatDataFromCookie();
    return defaultDateFormatData.defaultLongDateFormat;
  }

  if(formatFromServer === "short"){
    let defaultDateFormatData = getDefaultCsDateFormatDataFromCookie();
    return defaultDateFormatData.defaultShortDateFormat;
  }

  if(formatFromServer === "time"){
    let defaultDateFormatData = getDefaultCsDateFormatDataFromCookie();
    return defaultDateFormatData.defaultTimeFormat;
  }
  return formatFromServer;
}


export function getMomentFormat(propertyXmlNode: any) {
  const nodeAttributes = propertyXmlNode.attributes;
  const csFormat = replaceDefaultDateFormats(nodeAttributes.FormatterPattern);
  const momentFormat = csToMomentFormat(csFormat)
  if (!momentFormat) {
    throw new Error(T("CustomFormat \"{0}\" of the field named: \"{1}\", id: \"{2}\" is not valid",
      "invalid_field_format",
      nodeAttributes.FormatterPattern, nodeAttributes.Name, nodeAttributes.ModelInstanceId));
  }
  return momentFormat;
}