/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import React from "react";
import { inject, observer } from "mobx-react";
import { getApi } from "model/selectors/getApi";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getSessionId } from "model/selectors/getSessionId";
import { IApi } from "model/entities/types/IApi";
import { IProperty } from "model/entities/types/IProperty";
import { action, flow, observable } from "mobx";
import S from "./BlobEditor.module.scss";
import { IProcessCRUDResult } from "model/actions/Actions/processActionResult";
import { processCRUDResult } from "model/actions/DataLoading/processCRUDResult";
import { getDialogStack } from "model/selectors/DialogStack/getDialogStack";
import { IDialogStack } from "model/entities/types/IDialogStack";
import { changeManyFields } from "model/actions-ui/DataView/TableView/onFieldChange";
import { flushCurrentRowData } from "model/actions/DataView/TableView/flushCurrentRowData";
import { handleError } from "model/actions/handleError";
import { IFocusable } from "model/entities/FormFocusManager";
import cx from "classnames";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { Dropdown } from "gui/Components/Dropdown/Dropdown";
import { DropdownItem } from "gui/Components/Dropdown/DropdownItem";
import { T } from "utils/translation";
import CS from "gui/Components/Dropdown/Dropdown.module.scss";
import { runGeneratorInFlowWithHandler, runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { ModalDialog } from "gui/Components/Dialog/ModalDialog";
import moment from "moment";
import { toOrigamServerString } from "utils/moment";

@inject(({property}: { property: IProperty }, {value}) => {
  return {
    api: getApi(property),
    processCRUDResult: (result: any) => processCRUDResult(property, result),
    changeManyFields: changeManyFields(property),
    flushCurrentRowData: flushCurrentRowData(property),
    handleError: handleError(property),
    dialogStack: getDialogStack(property),
    DataStructureEntityId: getDataStructureEntityId(property),
    Property: property.id,
    RowId: getSelectedRowId(property),
    menuItemId: getMenuItemId(property),
    Entity: getEntity(property),
    SessionFormIdentifier: getSessionId(property),
    parameters: property.parameters,
  };
})
@observer
export class BlobEditor extends React.Component<{
  value: string;
  api?: IApi;
  isReadOnly: boolean;
  processCRUDResult?: IProcessCRUDResult;
  changeManyFields?: (values: Array<{ fieldId: string; value: any }>) => Generator;
  flushCurrentRowData?: () => Generator;
  handleError?: (error: any) => Generator;
  dialogStack?: IDialogStack;
  DataStructureEntityId?: string;
  Property?: string;
  RowId?: string;
  menuItemId?: string;
  Entity?: string;
  SessionFormIdentifier?: string;
  parameters?: any;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  canUpload: boolean;
  onKeyDown?(event: any): void;
  onChange?(event: any, value: string): void;
  onEditorBlur?(event: any): void;
}> {
  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
  };

  @observable
  focused = false;

  componentDidMount() {
    if (this.elmInput && this.props.subscribeToFocusManager) {
      this.props.subscribeToFocusManager(this.elmInput);
    }
  }

  handleFileChange(event: any) {
    this.fileList = event.target.files;
    flow(this.upload.bind(this))();
  }

  @observable.ref fileList: any = [];
  @observable progressValue = 0;
  @observable speedValue = 0;
  @observable isUploading = false;

  async download(args: { isPreview: boolean }) {
    const token = await this.props.api!.getDownloadToken({
      SessionFormIdentifier: this.props.SessionFormIdentifier!,
      MenuId: this.props.menuItemId!,
      DataStructureEntityId: this.props.DataStructureEntityId!,
      Entity: this.props.Entity!,
      RowId: this.props.RowId!,
      Property: this.props.Property!,
      FileName: this.props.value,
      parameters: this.props.parameters,
      isPreview: args.isPreview,
    });
    await this.props.api!.getBlob({downloadToken: token});
  }

  *upload(): any {
    this.progressValue = 0;
    this.speedValue = 0;
    this.isUploading = true;
    try {
      if (this.fileList && this.fileList.length > 0) {
        for (let file of this.fileList) {
          const token = yield this.props.api!.getUploadToken({
            SessionFormIdentifier: this.props.SessionFormIdentifier!,
            MenuId: this.props.menuItemId!,
            DataStructureEntityId: this.props.DataStructureEntityId!,
            Entity: this.props.Entity!,
            RowId: this.props.RowId!,
            Property: this.props.Property!,
            FileName: this.props.value,
            parameters: this.props.parameters,
            DateCreated: toOrigamServerString(moment(file.lastModifiedDate)), // DateCreated is not available in the browser
            DateLastModified: toOrigamServerString(moment(file.lastModifiedDate)),
          });

          let lastTime: number | undefined;
          let lastSize: number = 0;
          yield this.props.api!.putBlob(
            {
              uploadToken: token,
              fileName: file.name,
              file,
            },
            action((event: any) => {
              this.progressValue = event.loaded / event.total;
              if (lastTime !== undefined) {
                this.speedValue = ((event.loaded - lastSize) / (event.timeStamp - lastTime)) * 1000;
              }
              lastTime = event.timeStamp;
              lastSize = event.loaded;
            })
          );
          const crudResult = yield this.props.api!.changes({
            SessionFormIdentifier: this.props.SessionFormIdentifier!,
            Entity: this.props.Entity!,
            RowId: this.props.RowId!,
          });
          yield*this.props.processCRUDResult!(crudResult);
        }
      }
    } catch (e) {
      yield*this.props.handleError!(e);
    } finally {
      this.isUploading = false;
      if (this.elmInput) this.elmInput.value = "";
    }
  }

  *delete(): any {
    if (
      yield new Promise(
        action((resolve: (value: boolean) => void) => {
          const closeDialog = this.props.dialogStack!.pushDialog(
            "",
            <ModalDialog
              title={T("Question", "question_title")}
              titleButtons={null}
              buttonsCenter={
                <>
                  <button
                    tabIndex={0}
                    autoFocus={true}
                    onClick={() => {
                      closeDialog();
                      resolve(true);
                    }}
                  >
                    {T("Yes", "button_yes")}
                  </button>
                  <button
                    tabIndex={0}
                    onClick={() => {
                      closeDialog();
                      resolve(false);
                    }}
                  >
                    {T("No", "button_no")}
                  </button>
                </>
              }
              buttonsLeft={null}
              buttonsRight={null}
            >
              <div className={S.dialogContent}>
                {T("Do you wish to delete {0}?", "blob_delete_confirmation", this.props.value)}
              </div>
            </ModalDialog>
          );
        })
      )
    ) {
      const {parameters} = this.props;
      const changeSet: Array<{ fieldId: string; value: any }> = [];
      changeSet.push({fieldId: this.props.Property!, value: null});
      if (parameters["AuthorMember"]) {
        changeSet.push({fieldId: parameters["AuthorMember"], value: null});
      }
      if (parameters["BlobMember"]) {
        changeSet.push({fieldId: parameters["BlobMember"], value: null});
      }
      if (parameters["CompressionStateMember"]) {
        changeSet.push({fieldId: parameters["CompressionStateMember"], value: null});
      }
      if (parameters["DateCreatedMember"]) {
        changeSet.push({fieldId: parameters["DateCreatedMember"], value: null});
      }
      if (parameters["DateLastModifiedMember"]) {
        changeSet.push({fieldId: parameters["DateLastModifiedMember"], value: null});
      }
      if (parameters["FileSizeMember"]) {
        changeSet.push({fieldId: parameters["FileSizeMember"], value: null});
      }
      if (parameters["OriginalPathMember"]) {
        changeSet.push({fieldId: parameters["OriginalPathMember"], value: null});
      }
      if (parameters["RemarkMember"]) {
        changeSet.push({fieldId: parameters["RemarkMember"], value: null});
      }
      if (parameters["ThumbnailMember"]) {
        changeSet.push({fieldId: parameters["ThumbnailMember"], value: null});
      }
      yield * this.props.changeManyFields!(changeSet);
      yield * this.props.flushCurrentRowData!();
    }
  }

  @observable displayImageEditor = false;
  imageObjectUrl: any;

  render() {
    return (
      <div className={S.editorContainer}>
        {this.renderInput()}
      </div>
    );
  }

  private onFocus() {
    this.focused = true;
  }

  private onBlur() {
    this.focused = false;
  }

  private renderInput() {
    if (this.props.isReadOnly && !this.props.value) {
      return (
        <div className={S.blobEditor}>
          <input
            readOnly={true}
            className={"fileName " + (this.focused ? S.focusedBorder : S.standardBorder)}
            value={this.props.value || ""}
          />
        </div>
      );
    }
    if (!this.props.value) {
      return (
        <div className={S.blobEditor}>
          <label className="customBtnChoose" title={"Upload new file."}>
            <input
              className="btnChooseFile"
              name="file"
              type="file"
              multiple={false}
              onChange={(event) => this.handleFileChange(event)}
              ref={this.refInput}
              autoComplete={"off"}
              onFocus={() => this.onFocus()}
              onBlur={() => this.onBlur()}
              onKeyDown={(event) => this.props.onKeyDown && this.props.onKeyDown(event)}
            />
            {T("Upload", "blob_upload")}
          </label>
          {this.isUploading && (
            <div className="progress">
              <div className="progressBar" style={{width: `${this.progressValue * 100}%`}}>
                <div className="progressBar" style={{width: `${this.progressValue * 100}%`}}>
                  {(this.progressValue * 100).toFixed(0)}%
                </div>
              </div>
            </div>
          )}
        </div>
      );
    }

    return (
      <div className={S.blobEditor + " " + CS.control}>
        <input
          className={"input " + (this.focused ? S.focusedBorder : S.standardBorder)}
          value={this.props.value || ""}
          disabled={this.props.isReadOnly}
          autoComplete={"off"}
          onChange={(event: any) =>
            !this.props.isReadOnly && this.props.onChange && this.props.onChange(event, event.target.value)
          }
          onBlur={event => !this.props.isReadOnly && this.props.onEditorBlur && this.props.onEditorBlur(event)}
        />
        <div>
          <Dropdowner
            trigger={({refTrigger, setDropped, isDropped}) => (
              <div className={CS.control} ref={refTrigger}>
                <div
                  className={cx("inputBtn", "lastOne")}
                  onClick={(event) => setDropped(true)}
                >
                  {!isDropped ? (
                    <i className="fas fa-caret-down"></i>
                  ) : (
                    <i className="fas fa-caret-up"></i>
                  )}
                </div>
              </div>
            )}
            content={({setDropped}) => (
              <Dropdown>
                <DropdownItem
                  onClick={(event: any) => {
                    setDropped(false);
                    runInFlowWithHandler({
                      ctx: this.props.Property!,
                      action: async () => await this.download({isPreview: false}),
                    });
                  }}
                >
                  {T("Download", "blob_download")}
                </DropdownItem>
                <DropdownItem
                  isDisabled={this.props.isReadOnly}
                  onClick={(event: any) => {
                    setDropped(false);
                    runGeneratorInFlowWithHandler({
                      ctx: this.props.Property!,
                      generator: this.delete.bind(this)(),
                    });
                  }}
                >
                  {T("Delete", "blob_delete")}
                </DropdownItem>
                <DropdownItem
                  onClick={(event: any) => {
                    setDropped(false);
                    runInFlowWithHandler({
                      ctx: this.props.Property!,
                      action: async () => await this.download({isPreview: true}),
                    });
                  }}
                >
                  {T("Preview", "blob_preview")}
                </DropdownItem>
              </Dropdown>
            )}
          />
        </div>
      </div>
    );
  }
}
