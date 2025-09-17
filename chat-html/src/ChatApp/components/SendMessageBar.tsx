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

import React, { useContext, useEffect } from "react";
import { Editor } from "react-draft-wysiwyg";
import "react-draft-wysiwyg/dist/react-draft-wysiwyg.css";
import { CtxMentionUserWorkflow } from "../componentIntegrations/Contexts";
import { UserToMention } from "./Dialogs/MentionUserDialog";
import { Entity, Modifier, EditorState } from "draft-js";
import { CtxHashtagRootStore } from "../modules/hashtagging/components/Common";
import { buildReferenceLink } from "../../util/links";

export function SendMessageBar(props: {
  //onEditorKeyDown: any;
  localUserName: string;
  isWorking?: boolean;
  editorState: any;
  onEditorStateChange: any;
  onHandleReturn: any;
  onSendMessageClick: any;
}) {
  useEffect(() => {
    setTimeout(() => {}, 5000);
  }, []);

  function mentionUsers(users: UserToMention[]) {
    let editorState = props.editorState;
    for (let user of users) {
      let currentContent = editorState.getCurrentContent();
      let currentSelection = editorState.getSelection();
      const newEntityKey = Entity.create("MENTION", "IMMUTABLE", {
        text: `@${user.name}`,
        url: `@${user.name}`,
        value: `${user.id}`,
      });
      const textWithEntity = Modifier.insertText(
        currentContent,
        currentSelection,
        `@${user.name}`,
        undefined,
        newEntityKey
      );
      editorState = EditorState.push(
        editorState,
        textWithEntity,
        "insert-characters"
      );
      currentContent = editorState.getCurrentContent();
      currentSelection = editorState.getSelection();
      editorState = EditorState.push(
        editorState,
        Modifier.insertText(currentContent, currentSelection, " "),
        "insert-characters"
      );
    }
    props.onEditorStateChange?.(editorState);
  }

  function insertHashtags(
    tags: Array<{
      hashtagCategoryName: string;
      hashtagObjectId: any;
      hashtagLabel: string;
    }>
  ) {
    let editorState = props.editorState;
    for (let tag of tags) {
      let currentContent = editorState.getCurrentContent();
      let currentSelection = editorState.getSelection();
      const newEntityKey = Entity.create("LINK", "IMMUTABLE", {
        text: `#${tag.hashtagCategoryName} / ${tag.hashtagLabel}`,
        url: buildReferenceLink(tag.hashtagCategoryName, tag.hashtagObjectId),
        value: `${tag.hashtagObjectId}`,
      });
      const textWithEntity = Modifier.insertText(
        currentContent,
        currentSelection,
        `#${tag.hashtagCategoryName} / ${tag.hashtagLabel}`,
        undefined,
        newEntityKey
      );
      editorState = EditorState.push(
        editorState,
        textWithEntity,
        "insert-characters"
      );
      currentContent = editorState.getCurrentContent();
      currentSelection = editorState.getSelection();
      editorState = EditorState.push(
        editorState,
        Modifier.insertText(currentContent, currentSelection, " "),
        "insert-characters"
      );
    }
    props.onEditorStateChange?.(editorState);
  }

  return (
    <div className="sendMessageBar">
      <div className="sendMessageBar__userName">{props.localUserName}:</div>
      {/*<textarea
        onKeyDown={props.onEditorKeyDown}
        className="sendMessageBar__textarea"
        readOnly={props.isWorking}
      />*/}

      <Editor
        editorState={props.editorState}
        {...({
          handleReturn: props.onHandleReturn,
        } as any)}
        toolbarClassName="sendMessageEditorToolbar"
        wrapperClassName=""
        editorClassName=""
        toolbarOnFocus={true}
        toolbarCustomButtons={[
          <MentionButton onUsersMentioned={mentionUsers} />,
          <HashtagButton onTagsCreated={insertHashtags} />,
        ]}
        onEditorStateChange={props.onEditorStateChange}
        toolbar={{
          inline: {
            options: [
              "bold",
              "italic",
              "underline",
              "strikethrough",
              "monospace",
              //"subscript",
              //"superscript"
            ],
          },
          options: [
            "inline",
            //"blockType",
            //"fontSize",
            //"fontFamily",
            //"list",
            //"textAlign",
            //"colorPicker",
            "link",
            //"embedded",
            "emoji",
            //"image",
            "remove",
            "history",
          ],
        }}
      />
      <button
        className="sendMessageBar__sendBtn"
        onClick={props.onSendMessageClick}
      >
        <i className="far fa-paper-plane fa-lg" />
      </button>
    </div>
  );
}

export function MentionButton(props: {
  onUsersMentioned?: (users: UserToMention[]) => void;
}) {
  const mentionWorkflow = useContext(CtxMentionUserWorkflow);
  return (
    <button
      onClick={() =>
        mentionWorkflow.start((users) => props.onUsersMentioned?.(users))
      }
      className="mentionButton"
    >
      @
    </button>
  );
}

export function HashtagButton(props: {
  onTagsCreated: (
    tags: Array<{
      hashtagCategoryName: string;
      hashtagObjectId: any;
      hashtagLabel: string;
    }>
  ) => void;
}) {
  const hashtagRootStore = useContext(CtxHashtagRootStore);
  return (
    <button
      className="hashtagButton"
      onClick={() =>
        hashtagRootStore.screenProcess.start((ht) => {
          props.onTagsCreated?.(ht);
        })
      }
    >
      #
    </button>
  );
}
