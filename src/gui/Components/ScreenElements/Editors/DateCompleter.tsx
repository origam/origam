import moment from "moment";

export default class DateCompleter
{
  expectedFormat: string;
  dateSeparator: string;
  timeSeparator: string;
  dateTimeSeparator: string;
  expectedDateFormat: string
  timeNowFunc: () => moment.Moment;

    constructor( expectedFormat: string, dateSeparator: string,
         timeSeparator: string, dateTimeSeparator: string, timeNowFunc: () => moment.Moment)
    {
        this.expectedFormat = expectedFormat;
        this.dateSeparator = dateSeparator;
        this.timeSeparator = timeSeparator;
        this.dateTimeSeparator = dateTimeSeparator;
        this.expectedDateFormat = this.expectedFormat.split(this.dateTimeSeparator)[0]
        this.timeNowFunc = timeNowFunc;
    }

    autoComplete(text: string | undefined): moment.Moment | undefined
    {
        if(!text) {
          return undefined;
        }
        const dateAndTime = text.trim().split(this.dateTimeSeparator);
        const dateText = dateAndTime[0];
        let completeDate = this.autoCompleteDate(dateText);

        if(dateAndTime.length === 2)
        {
            const timeText = dateAndTime[1];
            const completeTime = this.autoCompleteTime(timeText);
            completeDate += this.dateTimeSeparator + completeTime;
        }
        return moment(completeDate, this.expectedFormat);
    }


    reformat(completeDateTime: string){
      const dateTime = moment(completeDateTime, this.expectedFormat)
      if (dateTime.hour() === 0 && dateTime.minute() === 0 && dateTime.second() === 0){
        return dateTime.format(this.expectedDateFormat)
      }
      return dateTime.format(this.expectedFormat)
    }

   autoCompleteTime(incompleteTime: string): string
    {
        if(incompleteTime.includes(this.timeSeparator))
        {
            return this.completeTimeWithSeparators(incompleteTime);
        }
        return this.completeTimeWithoutSeparators(incompleteTime);
    }

    completeTimeWithoutSeparators(incompleteTime: string): string
    {
        switch(incompleteTime.length)
        {
            case 1:
            case 2:
                return incompleteTime + this.timeSeparator +
                       "00" + this.timeSeparator +
                       "00";
            case 3:
            case 4:
                return incompleteTime.substring(0, 2) + this.timeSeparator +
                       incompleteTime.substring(2) + this.timeSeparator +
                       "00";
            default:
                return incompleteTime.substring(0, 2) + this.timeSeparator +
                       incompleteTime.substring(2, 4) + this.timeSeparator +
                       incompleteTime.substring(4);
        }
    }

    completeTimeWithSeparators(incompleteTime: string): string
    {
        const splitTime = incompleteTime.split(this.timeSeparator)
        if(splitTime.length === 2){
          return moment([2010, 1, 1, splitTime[0],splitTime[1], 0, 0]).format("hh:mm:ss A")
        }   
        if(splitTime.length === 3){
          return moment([2010, 1, 1, splitTime[0], splitTime[1], splitTime[2], 0]).format("hh:mm:ss A")
        }       
        return incompleteTime
    }

    autoCompleteDate(incompleteDate: string): string
    {
        return  incompleteDate.includes(this.dateSeparator)
          ? this.completeDateWithSeparators(incompleteDate)
          : this.completeDateWithoutSeparators(incompleteDate);
    }

    completeDateWithSeparators(incompleteDate: string): string
    {
        const splitDate = incompleteDate.split(this.dateSeparator)
        if(splitDate.length === 2){
          return incompleteDate + this.dateSeparator + this.timeNowFunc().year()
        }
        else
        {
          return incompleteDate
        }
    }

    completeDateWithoutSeparators(incompleteDate: string): string
    {
        switch(incompleteDate.length)
        {
            case 1:
            case 2:
                {
                    // assuming input is always a day.
                    return this.addMonthAndYear(incompleteDate);
                }
            case 3:
            case 4:
                {
                    // assuming input is day and month in order specified by
                    // current culture
                    return this.addYear(incompleteDate);
                }
            case 6:
                {
                    // assuming input is day and month in order specified by
                    // current culture followed by incomplete year (yy)
                    const incompleteWithSeparators = this.addSeparators(incompleteDate);
                    return incompleteWithSeparators;
                }
            default:
                return this.addSeparators(incompleteDate);
        }
    }

    addMonthAndYear(day: string): string
    {
        const now = this.timeNowFunc();
        const usDateString = (now.month()+1)+"/"+day+"/"+now.year();

        const date = moment(usDateString, "M/D/YYYY")

        return date ? date.format(this.expectedDateFormat) : day;
    }

    addYear(dayAndMonth: string): string
    {
        return dayAndMonth.substring(0, 2) + this.dateSeparator
              + dayAndMonth.substring(2) + this.dateSeparator
              + this.timeNowFunc().year();
    }

    addSeparators(incompleteDate: string): string
    {
        const format = this.getDoubleDayAndMonthFormat();

        const firstIndex = format.indexOf(this.dateSeparator);
        const secondIndex = format.lastIndexOf(this.dateSeparator);
        const dateLength = incompleteDate.length;

        if(firstIndex < dateLength && secondIndex >= dateLength)
        {
            return incompleteDate.substring(0, firstIndex)
                            + this.dateSeparator
                            + incompleteDate.substring(firstIndex);
        }
        if(firstIndex < dateLength && secondIndex < dateLength)
        {
            return incompleteDate.substring(0, firstIndex)
                            + this.dateSeparator
                            + incompleteDate.substring(firstIndex, firstIndex + 2)
                            + this.dateSeparator
                            + incompleteDate.substring(secondIndex - 1);
        }
        return incompleteDate;
    }

    getDoubleDayAndMonthFormat(): string
    {
        // dateFormat might be d/m/yyyy, this,
        // method makes sure we get dd/mm/yyyy
        const formatHasSingleDigitDayAndMonth = this.expectedDateFormat.length === 8;
        let format;
        if(formatHasSingleDigitDayAndMonth)
        {
            format = this.expectedFormat.toLowerCase().replace("d", "dd")
                .replace("m", "mm");
        }
        else
        {
            format = this.expectedFormat;
        }
        return format;
    }
}