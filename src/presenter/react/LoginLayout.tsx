import style from "./LoginLayout.module.css";
import { observer } from "mobx-react";
import React from "react";
import { action } from "mobx";

interface IASubmitLogin {
  do(userName: string, password: string): void;
}

@observer
export class LoginLayout extends React.Component<{
  aSubmitLogin: IASubmitLogin;
  isWorking: boolean;
  formInfoText?: string;
}> {
  @action.bound
  handleSubmit(event: any) {
    event.preventDefault();
    this.props.aSubmitLogin.do(
      this.refUserName.current!.value,
      this.refPassword.current!.value
    );
  }

  refUserName = React.createRef<HTMLInputElement>();
  refPassword = React.createRef<HTMLInputElement>();

  render() {
    return (
      <div className={style.loginPage}>
        <div className={style.loginPanel}>
          <img className={style.brandImg} src="/img/asap.png" />
          {this.props.formInfoText && (
            <div className={style.formInfoText}>{this.props.formInfoText}</div>
          )}
          <form onSubmit={this.handleSubmit} className={style.loginForm}>
            <div className={style.inputRow}>
              <input
                ref={this.refUserName}
                readOnly={this.props.isWorking}
                autoFocus={true}
                tabIndex={1}
                placeholder="User name"
                className={style.userNameInput}
                name="userName"
                type="text"
              />
            </div>
            <div className={style.inputRow}>
              <input
                readOnly={this.props.isWorking}
                ref={this.refPassword}
                tabIndex={2}
                placeholder="Password"
                className={style.passwordInput}
                name="password"
                type="password"
              />
            </div>
            <div className={style.spacer} />
            <div className={style.inputRow}>
              <div className={style.centered}>
                <button tabIndex={3} className={style.submitBtn} type="submit">
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
