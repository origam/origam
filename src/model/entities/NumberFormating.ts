import {getLocaleFromCookie} from "../../utils/cookies";

export function formatNumberWithLocale(customNumericFormat: string | undefined, value: number, locale: string) {
  if (customNumericFormat) {
    const customFormat = new CustomNumericFormat(customNumericFormat)
    return value.toLocaleString(
      locale,
      {
        minimumFractionDigits: customFormat.minimumFractionDigits,
        maximumFractionDigits: customFormat.maximumFractionDigits,
        useGrouping: customFormat.useThousandsSeparator
      });
  } else {
    return value.toLocaleString(locale);
  }
}

export function getCurrentDecimalSeparator() {
  return getSeparator("decimal");
}

export function getCurrentGroupSeparator() {
  return getSeparator("group");
}

function getSeparator(separatorType: string) {
  const locale = getLocaleFromCookie();
  const numberWithDecimalSeparator = 1000.1;
  return Intl.NumberFormat(locale)
    .formatToParts(numberWithDecimalSeparator)
    .find(part => part.type === separatorType)!
    .value;
}

export function formatNumber(customNumericFormat: string | undefined, dataType: string, value: number){
  const locale = getLocaleFromCookie();
  if(customNumericFormat){
    return formatNumberWithLocale(customNumericFormat, value, locale);
  }
  if(dataType === "Currency" || dataType === "Float"){
    return formatNumberWithLocale("### ###.00", value, locale);
  }else{
    return formatNumberWithLocale("### ###", value, locale);
  }
}

class CustomNumericFormat{
  private format: string;
  private fractionFormat: string | undefined;
  constructor(format: string){
    const containsAllowedChar = RegExp('^[# 0.]+$').test(format)
    if(!containsAllowedChar){
      throw new Error("Custom format contains unimplemented characters: \""+format+"\"")
    }
    this.format = format.trim();
    this.fractionFormat = this.getFractionFormat(this.format);
  }

  getFractionFormat(format: string){
    const splitByPoint = format.split(".")
    return splitByPoint.length < 2
      ? undefined
      : splitByPoint[1];
  }

  get maximumFractionDigits(){
    return this.fractionFormat ? this.fractionFormat.length : undefined;
  }

  get minimumFractionDigits(){
    if(!this.fractionFormat){
      return undefined;
    }
    const minDigits = this.fractionFormat.split("0").length-1
    return minDigits === 0 ? undefined : minDigits;
  }

  get useThousandsSeparator(){
    return this.format.includes(" ");
  }
}