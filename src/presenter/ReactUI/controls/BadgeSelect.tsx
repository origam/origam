import * as React from "react";

export interface IOption {
  value: string;
  label: string;
}

export interface IOptions {
  count: number;
  getItem(idx: number): IOption;
}

export interface IBadgeSelectProps {
  optAvailable: IOptions;
  optSelected: IOptions;
  onOptionAdd?(opt: IOption): void;
  onOptionDelete?(opt: IOption): void;
  onDropChange?(down: boolean): void;
}

class Badge extends React.Component {
  render() {
    return (
      <div className="badge">
        {this.props.children}
        <button className="remove-badge">
          <i className="fas fa-times" />
        </button>
      </div>
    );
  }
}

export class BadgeSelect extends React.Component<IBadgeSelectProps> {
  getSelectedItems() {
    const result = [];
    for (let i = 0; i < this.props.optSelected.count; i++) {
      const opt = this.props.optSelected.getItem(i);
      result.push(<Badge key={opt.value}>{opt.label}</Badge>);
    }
    return result;
  }

  getAvailableItems() {
    const result = [];
    for (let i = 0; i < this.props.optAvailable.count; i++) {
      const opt = this.props.optAvailable.getItem(i);
      result.push(
        <div className="list-item" key={opt.value}>
          {opt.label}
        </div>
      );
    }
    return result;
  }

  render() {
    return (
      <div className="badge-select">
        {this.getSelectedItems()}
        <div className="btn-add-container">
          <button className="dropdown-trigger">
            <i className="fas fa-plus-square" />
          </button>
          <div className="dropdown-list">{this.getAvailableItems()}</div>
        </div>
      </div>
    );
  }
}
