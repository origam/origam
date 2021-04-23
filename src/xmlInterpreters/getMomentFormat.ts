import {csToMomentFormat} from "../utils/dateFormatConversion";
import {T} from "../utils/translation";

export function getMomentFormat(propertyXmlNode: any) {
  const momentFormat = csToMomentFormat(propertyXmlNode.attributes.FormatterPattern)
  if (!momentFormat) {
    throw new Error(T("CustomFormat \"{0}\" of the field named: \"{1}\", id: \"{2}\" is not valid",
      "invalid_field_format",
      propertyXmlNode.attributes.FormatterPattern, propertyXmlNode.attributes.Name, propertyXmlNode.attributes.ModelInstanceId));
  }
  return momentFormat;
}