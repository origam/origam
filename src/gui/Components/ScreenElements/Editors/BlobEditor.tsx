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
/*import ImageEditor from "tui-image-editor";
import "tui-image-editor/dist/tui-image-editor.css";*/
import { IProcessCRUDResult } from "model/actions/Actions/processActionResult";
import { processCRUDResult } from "model/actions/DataLoading/processCRUDResult";
import { getDialogStack } from "model/selectors/DialogStack/getDialogStack";
import { IDialogStack } from "model/entities/types/IDialogStack";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
import { changeManyFields } from "model/actions-ui/DataView/TableView/onFieldChange";
import { flushCurrentRowData } from "model/actions/DataView/TableView/flushCurrentRowData";
import { handleError } from "model/actions/handleError";
import { IFocusable } from "model/entities/FocusManager";
import { Tooltip } from "react-tippy";

@inject(({ property }: { property: IProperty }, { value }) => {
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
  isInvalid: boolean;
  invalidMessage?: string;
  onKeyDown?(event: any): void;
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

  *download() {
    try {
      const token = yield this.props.api!.getDownloadToken({
        SessionFormIdentifier: this.props.SessionFormIdentifier!,
        MenuId: this.props.menuItemId!,
        DataStructureEntityId: this.props.DataStructureEntityId!,
        Entity: this.props.Entity!,
        RowId: this.props.RowId!,
        Property: this.props.Property!,
        FileName: this.props.value,
        parameters: this.props.parameters,
      });
      this.props.api!.getBlob({ downloadToken: token });
    } catch (e) {
      yield* this.props.handleError!(e);
    }
  }

  *upload() {
    this.progressValue = 0;
    this.speedValue = 0;
    this.isUploading = true;
    try {
      if (this.fileList && this.fileList.length > 0) {
        for (let file of this.fileList) {
          console.log(file);
          const token = yield this.props.api!.getUploadToken({
            SessionFormIdentifier: this.props.SessionFormIdentifier!,
            MenuId: this.props.menuItemId!,
            DataStructureEntityId: this.props.DataStructureEntityId!,
            Entity: this.props.Entity!,
            RowId: this.props.RowId!,
            Property: this.props.Property!,
            FileName: this.props.value,
            parameters: this.props.parameters,
            DateCreated: "2010-01-01",
            DateLastModified: "2010-01-01",
          });

          console.log("Uploading ", file.name, file.size);
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
                console.log(event.loaded - lastSize, event.timeStamp - lastTime);
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
          console.log(crudResult);
          yield* this.props.processCRUDResult!(crudResult);
        }
      }
    } catch (e) {
      yield* this.props.handleError!(e);
    } finally {
      this.isUploading = false;
      if (this.elmInput) this.elmInput.value = "";
    }
  }

  *delete() {
    try {
      if (
        yield new Promise(
          action((resolve: (value: boolean) => void) => {
            const closeDialog = this.props.dialogStack!.pushDialog(
              "",
              <ModalWindow
                title="Question"
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
                      OK
                    </button>
                    <button
                      tabIndex={0}
                      onClick={() => {
                        closeDialog();
                        resolve(false);
                      }}
                    >
                      Cancel
                    </button>
                  </>
                }
                buttonsLeft={null}
                buttonsRight={null}
              >
                <div className={S.dialogContent}>Do you wish to delete {this.props.value} ?</div>
              </ModalWindow>
            );
          })
        )
      ) {
        const { parameters } = this.props;
        console.log(parameters);
        const changeSet: Array<{ fieldId: string; value: any }> = [];
        changeSet.push({ fieldId: this.props.Property!, value: null });
        if (parameters["AuthorMember"]) {
          changeSet.push({ fieldId: parameters["AuthorMember"], value: null });
        }
        if (parameters["BlobMember"]) {
          changeSet.push({ fieldId: parameters["BlobMember"], value: null });
        }
        if (parameters["CompressionStateMember"]) {
          changeSet.push({ fieldId: parameters["CompressionStateMember"], value: null });
        }
        if (parameters["DateCreatedMember"]) {
          changeSet.push({ fieldId: parameters["DateCreatedMember"], value: null });
        }
        if (parameters["DateLastModifiedMember"]) {
          changeSet.push({ fieldId: parameters["DateLastModifiedMember"], value: null });
        }
        if (parameters["FileSizeMember"]) {
          changeSet.push({ fieldId: parameters["FileSizeMember"], value: null });
        }
        if (parameters["OriginalPathMember"]) {
          changeSet.push({ fieldId: parameters["OriginalPathMember"], value: null });
        }
        if (parameters["RemarkMember"]) {
          changeSet.push({ fieldId: parameters["RemarkMember"], value: null });
        }
        if (parameters["ThumbnailMember"]) {
          changeSet.push({ fieldId: parameters["ThumbnailMember"], value: null });
        }
        yield* this.props.changeManyFields!(changeSet);
        yield* this.props.flushCurrentRowData!();
      }
    } catch (e) {
      yield* this.props.handleError!(e);
    }
  }

  @observable displayImageEditor = false;
  imageObjectUrl: any;

  render() {
    return (
      <div className={S.editorContainer}>
        {this.renderInput()}
        {this.props.isInvalid && (
          <div className={S.notification}>
            <Tooltip html={this.props.invalidMessage} arrow={true}>
              <i className="fas fa-exclamation-circle red" />
            </Tooltip>
          </div>
        )}
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
    return (
      <div className={S.blobEditor}>
        {/*this.displayImageEditor && <ImageEditorCom imageUrl={this.imageObjectUrl} />*/}
        <input
          readOnly={true}
          className={"fileName " + (this.focused ? S.focusedBorder : S.standardBorder)}
          value={this.props.value || ""}
        />
        <div className="controls">
          <>
            {this.props.value && (
              <button
                className={
                  "btnDownload " + (this.props.isReadOnly ? "btnDownloadOnly" : "btnDownloadFirst")
                }
                onClick={flow(this.download.bind(this))}
                title={`Download: ${this.props.value}`}
              >
                <i className="fas fa-download"></i>
              </button>
            )}
            {this.props.value && !this.props.isReadOnly && (
              <button
                onClick={flow(this.delete.bind(this))}
                className="btnDelete"
                title={`Delete: ${this.props.value}`}
              >
                <i className="far fa-trash-alt"></i>
              </button>
            )}
          </>
          {!this.props.isReadOnly && (
            <label className="customBtnChoose" title={"Upload new file."}>
              <input
                className="btnChooseFile"
                name="file"
                type="file"
                multiple={false}
                onChange={(event) => this.handleFileChange(event)}
                ref={this.refInput}
                onFocus={() => this.onFocus()}
                onBlur={() => this.onBlur()}
                onKeyDown={(event) => this.props.onKeyDown && this.props.onKeyDown(event)}
              />
              <i className="fas fa-upload"></i>
            </label>
          )}
        </div>
        {this.isUploading && (
          <div className="progress">
            <div className="progressBar" style={{ width: `${this.progressValue * 100}%` }}>
              {(this.progressValue * 100).toFixed(0)}%
            </div>
          </div>
        )}
      </div>
    );
  }
}
/*
class ImageEditorCom extends React.Component<{ imageUrl: any }> {
  refImageEditor = (elm: any) => (this.elmImageEditor = elm);
  elmImageEditor: HTMLDivElement | null = null;

  componentDidMount() {
    const instance = new ImageEditor(this.elmImageEditor as any, {
      usageStatistics: false,
      //cssMaxWidth: 700,
      //cssMaxHeight: 500,
      selectionStyle: {
        cornerSize: 20,
        rotatingPointOffset: 70,
      },
      includeUI: {
        loadImage: { path: this.props.imageUrl, name: "Loaded image" },
        //loadImage: {
        //    path: 'img/sampleImage2.png',
        //    name: 'SampleImage'
        //},
        // theme: blackTheme, // or whiteTheme
        initMenu: "filter",
        menuBarPosition: "bottom",
      },
    });
  }

  render() {
    return (
      <div className={S.imageEditor}>
        <div ref={this.refImageEditor} className="imageEditorMountpoint" />
      </div>
    );
  }
}*/
