import React from "react";
import S from "./LoginPage.module.scss";
import { action } from "mobx";
import { inject, observer } from "mobx-react";
import { getApplicationLifecycle } from "model/selectors/getApplicationLifecycle";

@observer
export class LoginPage extends React.Component<{
  formInfoText: string;
  isWorking: boolean;
  onSubmitLogin: (args: {
    event: any;
    userName: string;
    password: string;
  }) => void;
}> {
  refUserName = React.createRef<HTMLInputElement>();
  refPassword = React.createRef<HTMLInputElement>();

  @action.bound
  handleSubmit(event: any) {
    event.preventDefault();
    this.props.onSubmitLogin!({
      event,
      userName: this.refUserName.current!.value,
      password: this.refPassword.current!.value
    });
  }

  render() {
    return (
      <div className={S.loginPage}>
        <div className={S.loginPanel}>
          <img className={S.brandImg} src="/img/asap.png" />
          {this.props.formInfoText && (
            <div className={S.formInfoText}>{this.props.formInfoText}</div>
          )}
          <form onSubmit={this.handleSubmit} className={S.loginForm}>
            <div className={S.inputRow}>
              <input
                ref={this.refUserName}
                readOnly={this.props.isWorking}
                autoFocus={true}
                tabIndex={1}
                placeholder="User name"
                className={S.userNameInput}
                name="userName"
                type="text"
              />
            </div>
            <div className={S.inputRow}>
              <input
                readOnly={this.props.isWorking}
                ref={this.refPassword}
                tabIndex={2}
                placeholder="Password"
                className={S.passwordInput}
                name="password"
                type="password"
              />
            </div>
            <div className={S.spacer} />
            <div className={S.inputRow}>
              <div className={S.centered}>
                <button tabIndex={3} className={S.submitBtn} type="submit">
                  {this.props.isWorking ? (
                    <i className="fas fa-cog fa-spin" />
                  ) : (
                    "Login"
                  )}
                </button>
              </div>
            </div>
          </form>
        </div>
      </div>
    );
  }
}
