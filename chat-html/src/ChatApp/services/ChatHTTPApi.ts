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

//import axios from "axios";
import { config } from "../config";
import axiosLib from "axios";
import { PubSub } from "../modules/hashtagging/services/APIService";

export interface IGetPolledDataResult {
  messages: {
    id: string;
    authorId: string;
    authorName: string;
    authorAvatarUrl: string;
    timeSent: string;
    text: string;
    mentions: {
      id: string;
      name: string;
      avatarUrl: string;
    }[];
  }[];
  info: {
    topic: string;
    categoryName: string;
    referenceId: string;
  };
  participants: {
    id: string;
    name: string;
    avatarUrl: string;
    status: "online" | "away" | "offline" | "none";
  }[];
  localUser: {
    id: string;
    name: string;
    avatarUrl: string;
  };
}

export interface IGetUsersToInviteResult {
  users: {
    id: string;
    name: string;
    avatarUrl: string;
  }[];
}

export interface IInviteUsersArg {
  users: { userId: string }[];
}

export interface ISendMessageArg {
  id: string;
  text: string;
  mentions: {
    id: string;
  }[];
}

export class ChatHTTPApi {
  constructor(public chatroomId = "", public fakeUserId?: string) {}

  axios = axiosLib.create({});

  urlPrefix = config.apiUrlPrefix;

  testNum = 0;

  get authToken() {
    return sessionStorage.getItem("origamAuthToken") || config.authToken;
  }

  get headers() {
    let result: { [key: string]: any } = {};

    if (this.fakeUserId) {
      result = { ...result, "x-fake-user-id": this.fakeUserId };
    }

    if (this.authToken) {
      result = { ...result, Authorization: `Bearer ${this.authToken}` };
    }

    return result;
  }

  setChatroomId(chatroomId: string) {
    this.chatroomId = chatroomId;
  }

  *getUsersToInvite(
    searchPhrase: string,
    limit: number,
    offset: number
  ): Generator<any, IGetUsersToInviteResult> {
    const cancelSource = axiosLib.CancelToken.source();
    try {
      const response = yield this.axios.get(
        `${this.urlPrefix}/chatrooms/${this.chatroomId}/usersToInvite`,
        {
          params: { searchPhrase, limit, offset },
          headers: this.headers,
          cancelToken: cancelSource.token,
        }
      );
      return {
        users: (response as any).data,
      };
    } finally {
      cancelSource.cancel();
    }
  }

  *getUsersToInviteByReferences(
    searchPhrase: string,
    limit: number,
    offset: number,
    references: { [key: string]: any }
  ): Generator<any, IGetUsersToInviteResult> {
    const cancelSource = axiosLib.CancelToken.source();
    try {
      const response = yield this.axios.get(
        `${this.urlPrefix}/users/listToInvite`,
        {
          params: { searchPhrase, limit, offset, ...references },
          headers: this.headers,
          cancelToken: cancelSource.token,
        }
      );
      return {
        users: (response as any).data,
      };
    } finally {
      cancelSource.cancel();
    }
  }

  *getUsersToOutvite(
    searchPhrase: string,
    limit: number,
    offset: number
  ): Generator<any, IGetUsersToInviteResult> {
    const cancelSource = axiosLib.CancelToken.source();
    try {
      const response = yield this.axios.get(
        `${this.urlPrefix}/chatrooms/${this.chatroomId}/usersToOutvite`,
        {
          params: { searchPhrase, limit, offset },
          headers: this.headers,
          cancelToken: cancelSource.token,
        }
      );
      return {
        users: (response as any).data,
      };
    } finally {
      cancelSource.cancel();
    }
  }

  *getUsersToMention(
    searchPhrase: string,
    limit: number,
    offset: number
  ): Generator<any, IGetUsersToInviteResult> {
    const cancelSource = axiosLib.CancelToken.source();
    try {
      const response = yield this.axios.get(
        `${this.urlPrefix}/chatrooms/${this.chatroomId}/usersToMention`,
        {
          params: { searchPhrase, limit, offset },
          headers: this.headers,
          cancelToken: cancelSource.token,
        }
      );
      return {
        users: (response as any).data,
      };
    } finally {
      cancelSource.cancel();
    }
  }

  async createChatroom(
    references: { [key: string]: any },
    topic: string,
    inviteUsers: string[]
  ): Promise<{ chatroomId: any }> {
    const response = await this.axios.post(
      `${this.urlPrefix}/chatrooms/create`,
      {
        topic,
        ReferenceCategory: references.referenceCategory,
        ReferenceRecordId: references.referenceRecordId,
        inviteUsers: inviteUsers.map((userId) => ({ id: userId })),
      },
      { headers: this.headers }
    );
    return { chatroomId: response.data };
  }

  async inviteUsers(arg: IInviteUsersArg) {
    for (let user of arg.users) {
      await this.axios.post(
        `${this.urlPrefix}/chatrooms/${this.chatroomId}/inviteUser`,
        { userId: user.userId },
        { headers: this.headers }
      );
    }
  }

  async outviteUsers(arg: IInviteUsersArg) {
    for (let user of arg.users) {
      await this.axios.post(
        `${this.urlPrefix}/chatrooms/${this.chatroomId}/outviteUser`,
        { userId: user.userId },
        { headers: this.headers }
      );
    }
  }

  async getPolledData(
    afterIdIncluding?: string
  ): Promise<IGetPolledDataResult> {
    const response = await this.axios.get(
      `${this.urlPrefix}/chatrooms/${this.chatroomId}/polledData`,
      { params: { afterIdIncluding }, headers: this.headers }
    );

    return response.data;
  }

  async sendMessage(arg: ISendMessageArg) {
    await this.axios.post(
      `${this.urlPrefix}/chatrooms/${this.chatroomId}/messages`,
      {
        ...arg,
      },
      { headers: this.headers }
    );
  }

  async abandonChatroom() {
    await this.axios.post(
      `${this.urlPrefix}/chatrooms/${this.chatroomId}/abandon`,
      {},
      { headers: this.headers }
    );
  }

  async patchChatroomInfo(topic: string) {
    await this.axios.post(
      `${this.urlPrefix}/chatrooms/${this.chatroomId}/info`,
      { topic },
      { headers: this.headers }
    );
  }

  async getHashtagAvailableCategories() {
    return (
      await axiosLib.get(`../internalApi/DeepLink/categories`, {
        headers: this.headers,
      })
    ).data;
  }

  async getHashtagAvailableObjects(
    categoryId: string,
    pageNumber: number,
    pageSize: number,
    searchPhrase: string | undefined,
    chCancel?: PubSub
  ) {
    const source = axiosLib.CancelToken.source();
    const _disposer = chCancel?.subs(() => source.cancel());
    try {
      return (
        await axiosLib.get(`../internalApi/DeepLink/${categoryId}/objects`, {
          params: { searchPhrase, limit: pageSize, pageNumber: pageNumber },
          headers: this.headers,
          cancelToken: source.token,
        })
      ).data;
    } catch (e: any) {
      if (axiosLib.isCancel(e)) {
        (e as any).$isCancellation = true;
      }
      throw e;
    } finally {
      _disposer?.();
    }
  }

  async getHashtagLabels(categoryId: string, labelIds: string[]) {
    return (
      await axiosLib.post(
        `../internalApi/DeepLink/${categoryId}/labels`,
        { LabelIds: labelIds },
        {
          headers: this.headers,
          //cancelToken: source.token,
        }
      )
    ).data;
  }
}
