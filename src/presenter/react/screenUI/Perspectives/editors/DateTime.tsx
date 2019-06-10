import * as React from "react";
import { observer } from "mobx-react";
import styles from "./DateTime.module.css";
import { action, observable, computed, runInAction } from "mobx";
import moment from "moment";
import { Tooltip } from "react-tippy";

@observer
class CalendarWidget extends React.Component<{
  initialDisplayDate?: moment.Moment;
  selectedDay?: moment.Moment;
  onDayClick?(event: any, day: moment.Moment): void;
}> {
  @observable.ref displayedMonth = moment(
    this.props.initialDisplayDate
  ).startOf("month");

  getDayHeaders() {
    const result: any[] = [];
    for (
      let day = moment(this.displayedMonth).startOf("week"), i = 0;
      i < 7;
      day.add({ days: 1 }), i++
    ) {
      result.push(
        <div className={styles.calendarWidgetDayHeaderCell}>
          {day.format("dd")[0]}
        </div>
      );
    }
    return result;
  }

  getDates(weekInc: number) {
    const result: any[] = [];
    for (
      let day = moment(this.displayedMonth)
          .startOf("week")
          .add({ weeks: weekInc }),
        i = 0;
      i < 7;
      day.add({ days: 1 }), i++
    ) {
      const isNeighbourMonth = day.month() !== this.displayedMonth.month();
      const isSelectedDay = day.isSame(this.props.selectedDay, "day");
      const dayCopy = moment(day);
      result.push(
        <div
          className={
            styles.calendarWidgetCell +
            (isNeighbourMonth
              ? ` ${styles.calendarWidgetNeighbourMonthCell}`
              : "") +
            (isSelectedDay ? ` ${styles.calendarWidgetSelectedDay}` : "")
          }
          onClick={(event: any) => this.handleDayClick(event, dayCopy)}
        >
          {day.format("D")}
        </div>
      );
    }
    return result;
  }

  getDateRows() {
    const result: any[] = [];
    for (let i = 0; i < 6; i++) {
      result.push(
        <div className={styles.calendarWidgetRow}>{this.getDates(i)}</div>
      );
    }
    return result;
  }

  @action.bound
  handleMonthDecClick(event: any) {
    this.displayedMonth = moment(this.displayedMonth).subtract({ months: 1 });
  }

  @action.bound
  handleMonthIncClick(event: any) {
    this.displayedMonth = moment(this.displayedMonth).add({ months: 1 });
  }

  @action.bound
  handleYearDecClick(event: any) {
    this.displayedMonth = moment(this.displayedMonth).subtract({ years: 1 });
  }

  @action.bound
  handleYearIncClick(event: any) {
    this.displayedMonth = moment(this.displayedMonth).add({ years: 1 });
  }

  @action.bound handleDayClick(event: any, day: moment.Moment) {
    this.props.onDayClick && this.props.onDayClick(event, day);
  }

  render() {
    return (
      <div className={styles.calendarWidgetContainer}>
        <div className={styles.calendarWidgetDayTable}>
          <div className={styles.calendarWidgetRow}>
            <div className={styles.calendarWidgetHeader}>
              <div className={styles.calendarWidgetMonthControls}>
                <button
                  className={styles.calendarWidgetControlBtn}
                  onClick={this.handleMonthDecClick}
                >
                  <i className="fas fa-caret-left" />
                </button>
                <button
                  className={styles.calendarWidgetControlBtn}
                  onClick={this.handleMonthIncClick}
                >
                  <i className="fas fa-caret-right" />
                </button>
              </div>
              <div className={styles.calendarWidgetTitle}>
                {this.displayedMonth.format("MMMM YYYY")}
              </div>
              <div className={styles.calendarWidgetYearControls}>
                <button
                  className={styles.calendarWidgetControlBtn}
                  onClick={this.handleYearDecClick}
                >
                  <i className="fas fa-caret-down" />
                </button>
                <button
                  className={styles.calendarWidgetControlBtn}
                  onClick={this.handleYearIncClick}
                >
                  <i className="fas fa-caret-up" />
                </button>
              </div>
            </div>
          </div>
          <div className={styles.calendarWidgetRow}>{this.getDayHeaders()}</div>
          {this.getDateRows()}
        </div>
      </div>
    );
  }
}

@observer
export class DateTimeEditor extends React.Component<{
  value: string;
  inputFormat: string;
  outputFormat: string;
  isReadOnly: boolean;
  isInvalid: boolean;
  isFocused: boolean;
  onChange?: (event: any, isoDay: string) => void;
  refocuser?: (cb: () => void) => () => void;
}> {
  @observable isDroppedDown = false;

  @action.bound handleDropperClick(event: any) {
    event.stopPropagation();
    this.makeDroppedDown();
  }

  @action.bound makeDroppedDown() {
    if (!this.isDroppedDown) {
      this.isDroppedDown = true;
      window.addEventListener("click", this.handleWindowClick);
    }
  }

  @action.bound makeDroppedUp() {
    if (this.isDroppedDown) {
      this.isDroppedDown = false;
      window.removeEventListener("click", this.handleWindowClick);
    }
  }

  @action.bound handleWindowClick(event: any) {
    if (this.elmContainer && !this.elmContainer.contains(event.target)) {
      this.makeDroppedUp();
    }
  }

  disposers: any[] = [];

  componentDidMount() {
    this.props.refocuser &&
      this.disposers.push(this.props.refocuser(this.makeFocusedIfNeeded));
    this.makeFocusedIfNeeded();
  }

  componentWillUnmount() {
    this.disposers.forEach(d => d());
  }

  componentDidUpdate(prevProps: { isFocused: boolean }) {
    runInAction(() => {
      if (!prevProps.isFocused && this.props.isFocused) {
        this.makeFocusedIfNeeded();
      }
      /*if (prevProps.textualValue !== this.props.textualValue) {
        this.dirtyTextualValue = undefined;
        this.makeFocusedIfNeeded();
      }*/
    });
  }

  @action.bound
  makeFocusedIfNeeded() {
    if (this.props.isFocused) {
      console.log("--- MAKE FOCUSED ---");
      this.elmInput && this.elmInput.focus();
      setTimeout(() => {
        this.elmInput && this.elmInput.select();
      }, 10);
    }
  }

  refContainer = (elm: HTMLDivElement | null) => (this.elmContainer = elm);
  elmContainer: HTMLDivElement | null = null;
  refInput = (elm: HTMLInputElement | null) => (this.elmInput = elm);
  elmInput: HTMLInputElement | null = null;

  @observable dirtyTextualValue: string | undefined;

  @computed get momentValue() {
    return moment(this.props.value);
  }

  @computed get formattedMomentValue() {
    return this.momentValue.format("DD.MM.YYYY");
  }

  @computed get textfieldValue() {
    return this.dirtyTextualValue !== undefined
      ? this.dirtyTextualValue
      : this.momentValue.format("DD.MM.YYYY");
  }

  @computed get isTooltipShown() {
    return (
      this.textfieldValue !== undefined &&
      (!moment(this.textfieldValue) ||
        this.formattedMomentValue !== this.textfieldValue)
    );
  }

  @action.bound handleTextfieldChange(event: any) {
    this.dirtyTextualValue = event.target.value;
    const dirtyMomentValue = [
      moment(this.dirtyTextualValue, "DD.MM.YYYY", true),
      moment(this.dirtyTextualValue, "M")
    ].find(m => m.isValid());
    if (dirtyMomentValue) {
      this.props.onChange &&
        this.props.onChange(event, dirtyMomentValue.toISOString());
    } else if (
      this.dirtyTextualValue &&
      /^[A-Za-z]+$/.test(this.dirtyTextualValue)
    ) {
      this.props.onChange && this.props.onChange(event, moment().toISOString());
    }
  }

  @action.bound handleDayClick(event: any, day: moment.Moment) {
    this.dirtyTextualValue = undefined;
    this.props.onChange && this.props.onChange(event, day.toISOString());
  }

  render() {
    return (
      <div className="editor-container" ref={this.refContainer}>
        <Tooltip
          title={this.formattedMomentValue}
          position="top"
          arrow={true}
          trigger="manual"
          open={this.isTooltipShown}
        >
          <input
            className="editor"
            type="text"
            /*value={moment(this.props.value, this.props.inputFormat).format(
            this.props.outputFormat
          )}*/
            ref={this.refInput}
            value={this.textfieldValue}
            readOnly={this.props.isReadOnly}
            onChange={this.handleTextfieldChange}
          />
        </Tooltip>
        {this.props.isInvalid && (
          <div className="notification">
            <i className="fas fa-exclamation-circle red" />
          </div>
        )}
        {!this.props.isReadOnly && (
          <div
            className={styles.dropdownSymbol}
            onClick={this.handleDropperClick}
          >
            <i className="far fa-calendar-alt" />
          </div>
        )}
        {this.isDroppedDown && (
          <div className={styles.droppedPanelContainer}>
            <CalendarWidget
              onDayClick={this.handleDayClick}
              initialDisplayDate={this.momentValue}
              selectedDay={this.momentValue}
            />
          </div>
        )}
      </div>
    );
  }
}
