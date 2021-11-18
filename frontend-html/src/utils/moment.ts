import  { Moment } from "moment";

export function toOrigamServerString(date: Moment){
    if(!date){
        return date;
    }
    return date.toISOString(true).split(".")[0]
}
  