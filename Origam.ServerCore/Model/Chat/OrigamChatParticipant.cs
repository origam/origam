#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

namespace Origam.ServerCore.Model.Chat
{
    public class OrigamChatParticipant
    {
        public OrigamChatParticipant(Guid guid1, string username, string avatarurl, string Status)
        {
            id = guid1;
            name = username;
            this.avatarUrl = avatarurl;
            this.status = status;
        }

        public Guid id { get; }
        public string name { get;  }
        public string avatarUrl { get; }
        public string status { get;  }
        

        internal static List<OrigamChatParticipant> CreateJson(DataSet datasetParticipants, DataSet onlineUsers)
        {
            List<OrigamChatParticipant> messages = new List<OrigamChatParticipant>();
            foreach (DataRow row in datasetParticipants.Tables["BusinessPartner"].Rows)
            {
                messages.Add(new OrigamChatParticipant(row.Field<Guid>("Id"), row.Field<string>("Username"), row.Field<Guid>("Id").ToString(), GetStatus(row.Field<Guid>("Id"),onlineUsers)));
            }
            return messages;
        }

        private static string GetStatus(Guid guid, DataSet onlineUsers)
        {
            foreach (DataRow row in onlineUsers.Tables[0].Rows)
            {
                Guid rowId = row.Field<Guid>("Id");
                if (rowId == guid)
                {
                    return "online";
                }
            }
            return "offline";
        }
    }

    public enum StatusEnum
    {
        online,
        away,
        offline,
        none
    }

}
