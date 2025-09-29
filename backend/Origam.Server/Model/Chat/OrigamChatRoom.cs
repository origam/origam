#region license
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
#endregion

using System;
using System.Collections.Generic;
using System.Data;

namespace Origam.Server.Model.Chat
{
    public class OrigamChatRoom
    {
        public OrigamChatRoom(
            Guid id,
            string topic,
            int unreadMessageCount,
            string categoryName,
            Guid? referenceId
        )
        {
            this.Id = id;
            this.Topic = topic;
            this.UnreadMessageCount = unreadMessageCount;
            this.CategoryName = categoryName;
            this.ReferenceId = referenceId;
        }

        public Guid Id { get; set; }
        public string Topic { get; set; }
        public string CategoryName { get; set; }
        public Guid? ReferenceId { get; set; }
        public int UnreadMessageCount { get; private set; }

        internal static List<OrigamChatRoom> CreateJson(
            DataSet ChatRoomDataSet,
            Dictionary<Guid, int> unreadMessages
        )
        {
            List<OrigamChatRoom> chatRoom = new List<OrigamChatRoom>();
            DataTable table = ChatRoomDataSet.Tables["OrigamChatRoom"];
            foreach (DataRow row in table.Rows)
            {
                Guid chatRoomId = row.Field<Guid>("Id");
                chatRoom.Add(
                    new OrigamChatRoom(
                        row.Field<Guid>("Id"),
                        row.Field<string>("Name"),
                        unreadMessages.ContainsKey(chatRoomId) ? unreadMessages[chatRoomId] : 0,
                        row.Field<string>("ReferenceEntity"),
                        row.Field<Nullable<Guid>>("ReferenceId")
                    )
                );
            }
            return chatRoom;
        }
    }
}
