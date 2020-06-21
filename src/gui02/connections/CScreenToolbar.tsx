import { Icon } from "gui02/components/Icon/Icon";
import { ScreenToolbar } from "gui02/components/ScreenToolbar/ScreenToolbar";
import { ScreenToolbarAction } from "gui02/components/ScreenToolbar/ScreenToolbarAction";
import { ScreenToolbarPusher } from "gui02/components/ScreenToolbar/ScreenToolbarPusher";
import { MobXProviderContext } from "mobx-react";
import { IApplication } from "model/entities/types/IApplication";
import React, {
  createContext,
  useState,
  PropsWithChildren,
  useContext,
  useMemo,
  Fragment,
} from "react";
import { action } from "mobx";
import { onScreenToolbarLogoutClick } from "model/actions-ui/ScreenToolbar/onScreenToolbarLogoutClick";
import { ScreenToolbarActionGroup } from "gui02/components/ScreenToolbar/ScreenToolbarActionGroup";
import { getActiveScreen } from "model/selectors/getActiveScreen";
import { onSaveSessionClick } from "model/actions-ui/ScreenToolbar/onSaveSessionClick";
import { onRefreshSessionClick } from "model/actions-ui/ScreenToolbar/onRefreshSessionClick";
import { observer } from "mobx-react";
import { getActiveScreenActions } from "model/selectors/getActiveScreenActions";
import { getIsEnabledAction } from "model/selectors/Actions/getIsEnabledAction";

import uiActions from "model/actions-ui-tree";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { UserMenuDropdown } from "gui02/components/UserMenuDropdown/UserMenuDropdown";
import { UserMenuBlock } from "gui02/components/UserMenuDropdown/UserMenuBlock";
import { getLoggedUserName } from "model/selectors/User/getLoggedUserName";
import { DropdownItem } from "gui02/components/Dropdown/DropdownItem";
import { onReloadWebScreenClick } from "model/actions-ui/ScreenToolbar/onReloadWebScreen";
import { isIFormScreen, isIFormScreenEnvelope } from "model/entities/types/IFormScreen";
import { isIWebScreen } from "model/entities/types/IWebScreen";
import { getIsSuppressSave } from "model/selectors/FormScreen/getIsSuppressSave";
import _ from "lodash";
import { Dropdown } from "gui02/components/Dropdown/Dropdown";
import { IAction } from "model/entities/types/IAction";

class ResponsiveToolbar {
  constructor(private onChildrenSetUpdate?: (ids: Set<any>) => void) {
    this.refContainer = this.refContainer.bind(this);
    this.refChild = this.refChild.bind(this);
  }

  hiddenChildren = new Set<any>();

  childToKey = new Map<any, any>();
  keyToChildRec = new Map<
    any,
    { elmChild: any; width: number; order: number | undefined; setHidden: (state: boolean) => void }
  >();

  container: any;
  containerWidth = Number.MAX_SAFE_INTEGER;

  domObsv = new (window as any).ResizeObserver(this.someNodesResized.bind(this));

  recomputeSizesImm() {
    const keysAndChildren = Array.from(this.keyToChildRec);
    keysAndChildren.sort(([ak, ar], [bk, br]) => {
      if (ar.order === br.order) return 0;
      if (ar.order === undefined) return 1;
      if (br.order === undefined) return -1;
      return ar.order - br.order;
    });

    const hiddenChildrenPruned = new Set(this.hiddenChildren);
    this.hiddenChildren = new Set();
    let widthAcc = 0;
    for (let [k, v] of keysAndChildren) {
      widthAcc = widthAcc + v.width;
      if (widthAcc > this.containerWidth) {
        this.hiddenChildren.add(k);
        hiddenChildrenPruned.delete(k);
        v.setHidden(true);
      }
    }
    for (let k of hiddenChildrenPruned.keys()) {
      this.keyToChildRec.get(k)?.setHidden(false);
    }
    this.onChildrenSetUpdate?.(this.hiddenChildren);
  }

  recomputeSizesDeb = _.throttle(this.recomputeSizesImm.bind(this), 500);

  someNodesResized(entries: any[]) {
    for (let e of entries) {
      //console.log(e.target, e.contentRect.width);
      if (e.target === this.container) {
        this.containerWidth = e.contentRect.width;
        continue;
      }
      const key = this.childToKey.get(e.target);
      // Preserve width when hidden, otherwise it gets never shown again.
      if (this.hiddenChildren.has(key)) continue;
      const childRec = this.keyToChildRec.get(key);
      if (childRec) {
        childRec.width = e.target.getBoundingClientRect().width;
        continue;
      }
    }
    /*console.log(
      "chw",
      this.containerWidth,
      Array.from(this.keyToChildRec).map(([k, v]) => `${k}:${v.width}`)
    );*/
    this.recomputeSizesDeb();
  }

  refContainer(elm: any) {
    if (elm) {
      this.container = elm;
      this.domObsv.observe(elm);
    } else {
      this.domObsv.disconnect();
      this.container = elm;
    }
  }

  refChild(key: any, order: any, setHidden: (state: boolean) => void, elm: any) {
    if (elm) {
      this.childToKey.set(elm, key);
      this.keyToChildRec.set(key, { elmChild: elm, width: 0, order, setHidden });
      this.domObsv.observe(elm);
    } else {
      this.domObsv.unobserve(this.keyToChildRec.get(key)!.elmChild);
      this.childToKey.delete(elm);
      this.keyToChildRec.delete(key);
    }
  }

  isHiddenChild(key: any) {
    return this.hiddenChildren.has(key);
  }
}

const CtxResponsiveToolbar = createContext<ResponsiveToolbar>(new ResponsiveToolbar());

function ResponsiveChild(
  props: PropsWithChildren<{
    childKey: any;
    order?: any;
    children: (args: { refChild: any; isHidden: boolean }) => any;
  }>
) {
  const [isHidden, setHidden] = useState(false);
  const responsiveToolbar = useContext(CtxResponsiveToolbar);
  const refChild = useMemo(
    () => (elm: any) => {
      responsiveToolbar.refChild(props.childKey, props.order, setHidden, elm);
    },
    [props.childKey]
  );
  return props.children({ refChild, isHidden });
}

function ResponsiveContainer(
  props: PropsWithChildren<{ children: (args: { refChild: any }) => any }>
) {
  const responsiveToolbar = useContext(CtxResponsiveToolbar);
  return props.children({ refChild: responsiveToolbar.refContainer });
}

function GrovableArea(props: PropsWithChildren<{}>) {
  return <div className="grovableArea">{props.children}</div>;
}

@observer
export class CScreenToolbar extends React.Component<{}> {
  static contextType = MobXProviderContext;

  state = {
    hiddenActionIds: new Set<string>(),
  };
  responsiveToolbar = new ResponsiveToolbar((ids) => {
    this.setState({ ...this.state, hiddenActionIds: ids });
  });

  get application(): IApplication {
    return this.context.application;
  }

  @action.bound
  handleLogoutClick(event: any) {
    onScreenToolbarLogoutClick(this.application)(event);
  }

  getOverfullActionsDropdownContent(
    toolbarActions: Array<{
      section: string;
      actions: IAction[];
    }>
  ) {
    return toolbarActions
      .filter((actionGroup) => actionGroup.actions.length > 0)
      .map((actionGroup) => (
        <Fragment key={actionGroup.section}>
          {actionGroup.actions
            .filter(
              (action) => this.state.hiddenActionIds.has(action.id) && getIsEnabledAction(action)
            )
            .map((action, idx) => (
              <ScreenToolbarAction
                //icon={<Icon src="./icons/settings.svg" />}
                label={action.caption}
                onClick={(event) => uiActions.actions.onActionClick(action)(event, action)}
              />
            ))}
        </Fragment>
      ));
  }

  renderForFormScreen() {
    const activeScreen = getActiveScreen(this.application);
    if (activeScreen && !activeScreen.content) return null;
    const formScreen =
      activeScreen && !activeScreen.content.isLoading ? activeScreen.content.formScreen : undefined;
    const isDirty = formScreen && formScreen.isDirty;
    const toolbarActions = getActiveScreenActions(this.application);
    const userName = getLoggedUserName(this.application);
    return (
      <CtxResponsiveToolbar.Provider value={this.responsiveToolbar}>
        <ScreenToolbar>
          {formScreen ? (
            <>
              <ScreenToolbarActionGroup>
                {!getIsSuppressSave(formScreen) && (
                  <ScreenToolbarAction
                    onClick={onSaveSessionClick(formScreen)}
                    icon={
                      <Icon
                        src="./icons/save.svg"
                        className={isDirty ? "isRed isHoverGreen" : ""}
                      />
                    }
                    label="Save"
                  />
                )}
                <ScreenToolbarAction
                  onClick={onRefreshSessionClick(formScreen)}
                  icon={<Icon src="./icons/refresh.svg" />}
                  label="Refresh"
                />
              </ScreenToolbarActionGroup>
              <ResponsiveContainer>
                {({ refChild }) => (
                  <ScreenToolbarActionGroup grovable={true} domRef={refChild}>
                    {toolbarActions
                      .filter((actionGroup) => actionGroup.actions.length > 0)
                      .map((actionGroup) => (
                        <ScreenToolbarActionGroup key={actionGroup.section}>
                          {actionGroup.actions
                            .filter((action) => getIsEnabledAction(action))
                            .map((action, idx) => (
                              <ResponsiveChild key={action.id} childKey={action.id} order={idx}>
                                {({ refChild, isHidden }) => (
                                  <ScreenToolbarAction
                                    rootRef={refChild}
                                    isHidden={isHidden}
                                    //icon={<Icon src="./icons/settings.svg" />}
                                    label={action.caption}
                                    onClick={(event) =>
                                      uiActions.actions.onActionClick(action)(event, action)
                                    }
                                  />
                                )}
                              </ResponsiveChild>
                            ))}
                        </ScreenToolbarActionGroup>
                      ))}
                  </ScreenToolbarActionGroup>
                )}
              </ResponsiveContainer>
            </>
          ) : null}

          {this.state.hiddenActionIds.size > 0 && (
            <Dropdowner
              style={{ width: "auto" }}
              trigger={({ refTrigger, setDropped }) => (
                <ScreenToolbarAction
                  rootRef={refTrigger}
                  onClick={() => setDropped(true)}
                  //onClick={this.handleLogoutClick}
                  icon={<Icon src="./icons/dot-menu.svg" />}
                  label={userName}
                />
              )}
              content={() => (
                <Dropdown>{this.getOverfullActionsDropdownContent(toolbarActions)}</Dropdown>
              )}
            />
          )}
          <Dropdowner
            style={{ width: "auto" }} // TODO: Move to stylesheet
            trigger={({ refTrigger, setDropped }) => (
              <ScreenToolbarAction
                rootRef={refTrigger}
                onClick={() => setDropped(true)}
                //onClick={this.handleLogoutClick}
                icon={
                  <>
                    <Icon src="./icons/user.svg" />
                    {/*<ScreenToolbarAlertCounter>5</ScreenToolbarAlertCounter>*/}
                  </>
                }
                label={userName}
              />
            )}
            content={() => (
              <UserMenuDropdown>
                <UserMenuBlock
                  userName={userName || "Logged user"}
                  actionItems={
                    <>
                      <DropdownItem isDisabled={true}>My profile</DropdownItem>
                      <DropdownItem onClick={this.handleLogoutClick}>Log out</DropdownItem>
                    </>
                  }
                />
              </UserMenuDropdown>
            )}
          />
        </ScreenToolbar>
      </CtxResponsiveToolbar.Provider>
    );
  }

  renderForWebScreen() {
    const activeScreen = getActiveScreen(this.application);
    const userName = getLoggedUserName(this.application);
    return (
      <ScreenToolbar>
        <>
          <ScreenToolbarActionGroup>
            {/*<ScreenToolbarAction
                onClick={onSaveSessionClick(formScreen)}
                icon={
                  <Icon
                    src="./icons/save.svg"
                    className={isDirty ? "isRed isHoverGreen" : ""}
                  />
                }
              />*/}
            <ScreenToolbarAction
              onClick={onReloadWebScreenClick(activeScreen)}
              icon={<Icon src="./icons/refresh.svg" />}
            />
          </ScreenToolbarActionGroup>

          {/*<ScreenToolbarActionGroup>
              <ScreenToolbarAction
                icon={<Icon src="./icons/invoice.svg" />}
                label="Action 3"
              />
              <ScreenToolbarAction
                icon={<Icon src="./icons/word.svg" />}
                label="Action 4"
              />
            </ScreenToolbarActionGroup>*/}
        </>

        <ScreenToolbarPusher />
        {/*<ScreenToolbarAction
          icon={<Icon src="./icons/search.svg" />}
          label="Search"
        />*/}
        <Dropdowner
          style={{ width: "auto" }} // TODO: Move to stylesheet
          trigger={({ refTrigger, setDropped }) => (
            <ScreenToolbarAction
              rootRef={refTrigger}
              onClick={() => setDropped(true)}
              //onClick={this.handleLogoutClick}
              icon={
                <>
                  <Icon src="./icons/user.svg" />
                  {/*<ScreenToolbarAlertCounter>5</ScreenToolbarAlertCounter>*/}
                </>
              }
              label={userName}
            />
          )}
          content={() => (
            <UserMenuDropdown>
              <UserMenuBlock
                userName={userName || "Logged user"}
                actionItems={
                  <>
                    <DropdownItem isDisabled={true}>My profile</DropdownItem>
                    <DropdownItem onClick={this.handleLogoutClick}>Log out</DropdownItem>
                  </>
                }
              />
            </UserMenuDropdown>
          )}
        />
      </ScreenToolbar>
    );
  }

  renderDefault() {
    const userName = getLoggedUserName(this.application);
    return (
      <ScreenToolbar>
        <ScreenToolbarPusher />
        {/*<ScreenToolbarAction
          icon={<Icon src="./icons/search.svg" />}
          label="Search"
        />*/}
        <Dropdowner
          style={{ width: "auto" }} // TODO: Move to stylesheet
          trigger={({ refTrigger, setDropped }) => (
            <ScreenToolbarAction
              rootRef={refTrigger}
              onClick={() => setDropped(true)}
              //onClick={this.handleLogoutClick}
              icon={
                <>
                  <Icon src="./icons/user.svg" />
                  {/*<ScreenToolbarAlertCounter>5</ScreenToolbarAlertCounter>*/}
                </>
              }
              label={userName}
            />
          )}
          content={() => (
            <UserMenuDropdown>
              <UserMenuBlock
                userName={userName || "Logged user"}
                actionItems={
                  <>
                    <DropdownItem isDisabled={true}>My profile</DropdownItem>
                    <DropdownItem onClick={this.handleLogoutClick}>Log out</DropdownItem>
                  </>
                }
              />
            </UserMenuDropdown>
          )}
        />
      </ScreenToolbar>
    );
  }

  render() {
    const activeScreen = getActiveScreen(this.application);
    if (!activeScreen) {
      return this.renderDefault();
    }
    if (activeScreen.content && isIFormScreenEnvelope(activeScreen.content)) {
      return this.renderForFormScreen();
    }
    if (isIWebScreen(activeScreen)) {
      return this.renderForWebScreen();
    }
    return null;
  }
}
