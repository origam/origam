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
    public class OrigamChatParticipant
    {
        public OrigamChatParticipant(Guid id, string username, string avatarUrl, string status)
        {
            this.Id = id;
            Name = username;
            this.AvatarUrl = avatarUrl;
            this.Status = status;
        }

        public Guid Id { get; }
        public string Name { get; }
        public string AvatarUrl { get; }
        public string Status { get; }

        internal static List<OrigamChatParticipant> CreateJson(
            DataSet datasetParticipants,
            DataSet onlineUsers
        )
        {
            List<OrigamChatParticipant> messages = new List<OrigamChatParticipant>();
            foreach (DataRow row in datasetParticipants.Tables[name: "BusinessPartner"].Rows)
            {
                messages.Add(
                    item: new OrigamChatParticipant(
                        id: row.Field<Guid>(columnName: "Id"),
                        username: row.Field<string>(columnName: "Username"),
                        avatarUrl: row.Field<Guid>(columnName: "Id").ToString(),
                        status: GetStatus(
                            userName: row.Field<string>(columnName: "Username"),
                            onlineUsers: onlineUsers
                        )
                    )
                );
            }
            return messages;
        }

        private static string GetStatus(string userName, DataSet onlineUsers)
        {
            if (!string.IsNullOrEmpty(value: userName))
            {
                foreach (DataRow row in onlineUsers.Tables[index: 0].Rows)
                {
                    if (userName.Equals(value: row.Field<string>(columnName: "UserName")))
                    {
                        return "online";
                    }
                }
            }
            return "offline";
        }
    }
}
